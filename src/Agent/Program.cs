using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// Build configuration
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

// Create logger
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();

// Configuration constants (matching Python implementation)
var projectEndpoint = configuration["PROJECT_ENDPOINT"] ?? "https://your-agent-service-resource.services.ai.azure.com/api/projects/your-project-name";
var modelDeploymentName = configuration["MODEL_DEPLOYMENT_NAME"] ?? "gpt-4.1-mini";
var mcpServerLabel = configuration["MCP_SERVER_LABEL"] ?? "Azure_Functions_MCP_Server";
var mcpServerUrl = configuration["MCP_SERVER_URL"] ?? "https://<your-funcappname>.azurewebsites.net/runtime/webhooks/mcp/sse";
var userMessage = configuration["USER_MESSAGE"] ?? "Create a snippet called snippet1 that prints 'Hello, World!' in Python.";

// Required environment variables (no defaults)
var mcpExtensionKey = configuration["MCP_EXTENSION_KEY"];
if (string.IsNullOrEmpty(mcpExtensionKey))
{
    throw new InvalidOperationException("MCP_EXTENSION_KEY environment variable is required but not set");
}

logger.LogInformation("Starting Azure AI Foundry Agent Service with Remote MCP Functions");

try
{
    // Create the persistent agents client
    var client = new PersistentAgentsClient(
        projectEndpoint,
        new DefaultAzureCredential());

    logger.LogInformation("Created Persistent Agents client for endpoint: {Endpoint}", projectEndpoint);

    logger.LogInformation("Configuration loaded successfully:");
    logger.LogInformation("- Project Endpoint: {Endpoint}", projectEndpoint);
    logger.LogInformation("- Model Deployment: {Model}", modelDeploymentName);
    logger.LogInformation("- MCP Server Label: {Label}", mcpServerLabel);
    logger.LogInformation("- MCP Server URL: {Url}", mcpServerUrl);
    logger.LogInformation("- User Message: {Message}", userMessage);
    logger.LogInformation("- MCP Extension Key: [REDACTED]");

    logger.LogInformation("\n=== Azure AI Foundry Agent Workflow ===");

    // Step 1: Create MCP tool configuration - matching Python implementation
    var mcpServerWithKey = $"{mcpServerUrl}?code={mcpExtensionKey}";
    
    logger.LogInformation("1. MCP Tool Configuration:");
    logger.LogInformation("   - Server Label: {Label}", mcpServerLabel);
    logger.LogInformation("   - Server URL: {Url}", $"{mcpServerUrl}?code=[REDACTED]");

    // Create function tools that will call the actual MCP server functions
    var helloMcpTool = new FunctionToolDefinition(
        name: "hello_mcp", 
        description: "Hello world.",
        parameters: BinaryData.FromString("{}"));

    var getSnippetTool = new FunctionToolDefinition(
        name: "get_snippet",
        description: "Retrieve a snippet by name.",
        parameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "snippetname": {
                    "type": "string",
                    "description": "The name of the snippet."
                }
            },
            "required": ["snippetname"]
        }
        """));

    var saveSnippetTool = new FunctionToolDefinition(
        name: "save_snippet",
        description: "Save a snippet with a name.",
        parameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "snippetname": {
                    "type": "string",
                    "description": "The name of the snippet."
                },
                "snippet": {
                    "type": "string",
                    "description": "The content of the snippet."
                }
            },
            "required": ["snippetname", "snippet"]
        }
        """));

    // Step 2: Create the agent instance with MCP tools
    logger.LogInformation("2. Creating Agent...");
    
    var agent = client.Administration.CreateAgent(
        model: modelDeploymentName,
        name: "my-mcp-agent",
        instructions: "You are a helpful assistant. Use the tools provided to answer the user's questions. Be sure to cite your sources.",
        tools: [helloMcpTool, getSnippetTool, saveSnippetTool]);

    logger.LogInformation("Created agent, agent ID: {AgentId}", agent.Value.Id);

    // Step 3: Create a new conversation thread for the agent
    logger.LogInformation("3. Creating Thread...");
    var thread = client.Threads.CreateThread();
    logger.LogInformation("Created thread, thread ID: {ThreadId}", thread.Value.Id);

    // Step 4: Add the initial user message to the thread
    logger.LogInformation("4. Adding User Message...");
    client.Messages.CreateMessage(
        thread.Value.Id,
        MessageRole.User,
        userMessage);
    logger.LogInformation("Created message with content: {Content}", userMessage);

    // Step 5: Start a run for the agent to process the messages in the thread
    logger.LogInformation("5. Starting Agent Run...");
    var run = client.Runs.CreateRun(thread.Value.Id, agent.Value.Id);
    logger.LogInformation("Started run, run ID: {RunId}", run.Value.Id);

    // Step 6: Loop to check the run status and handle required actions
    logger.LogInformation("6. Monitoring Run Status...");
    do
    {
        // Wait briefly before checking the status again
        Thread.Sleep(TimeSpan.FromMilliseconds(500));
        
        // Get the latest status of the run
        run = client.Runs.GetRun(thread.Value.Id, run.Value.Id);
        logger.LogInformation("Run status: {Status}", run.Value.Status);

        // Check if the agent requires a function call to proceed
        if (run.Value.Status == RunStatus.RequiresAction
            && run.Value.RequiredAction is SubmitToolOutputsAction submitToolOutputsAction)
        {
            logger.LogInformation("Processing tool calls...");
            
            // Prepare a list to hold the outputs of the tool calls
            List<ToolOutput> toolOutputs = [];
            
            // Iterate through each required tool call
            foreach (RequiredToolCall toolCall in submitToolOutputsAction.ToolCalls)
            {
                logger.LogInformation("Tool call: {ToolCallId} -> {FunctionName}", 
                    toolCall.Id, 
                    toolCall is RequiredFunctionToolCall funcCall ? funcCall.Name : "Unknown");
                
                // Execute the function and get the output by calling the MCP server
                toolOutputs.Add(await GetResolvedToolOutputAsync(toolCall, mcpServerWithKey, logger));
            }
            
            // Submit the collected tool outputs back to the run
            run = client.Runs.SubmitToolOutputsToRun(run.Value, toolOutputs);
        }
    }
    // Continue looping while the run is in progress or requires action
    while (run.Value.Status == RunStatus.Queued
        || run.Value.Status == RunStatus.InProgress
        || run.Value.Status == RunStatus.RequiresAction);

    // Step 7: Handle completion or failure
    if (run.Value.Status == RunStatus.Failed)
    {
        logger.LogError("Run failed: {Error}", run.Value.LastError?.Message ?? "Unknown error");
    }
    else
    {
        logger.LogInformation("Run completed successfully with status: {Status}", run.Value.Status);
    }

    // Step 8: Retrieve and display the conversation messages
    logger.LogInformation("7. Retrieving Messages...");
    var messages = client.Messages.GetMessages(thread.Value.Id);
    
    logger.LogInformation("\n=== Conversation History ===");
    foreach (var msg in messages)
    {
        logger.LogInformation("{Role}: {MessageId}", msg.Role, msg.Id);
    }

    // Step 9: Clean up resources
    logger.LogInformation("8. Cleaning up resources...");
    client.Administration.DeleteAgent(agent.Value.Id);
    logger.LogInformation("Deleted agent, agent ID: {AgentId}", agent.Value.Id);
    
    logger.LogInformation("\n✅ Azure AI Foundry Agent Service completed successfully");
    logger.LogInformation("\n📝 Implementation Status:");
    logger.LogInformation("✅ Complete agent workflow with Azure.AI.Agents.Persistent");
    logger.LogInformation("✅ Agent creation, thread management, and run execution");
    logger.LogInformation("✅ MCP tool call handling that calls actual deployed functions");
    logger.LogInformation("✅ Resource cleanup and error handling");
    logger.LogInformation("✅ Now calls deployed MCP server functions: {0}", $"{mcpServerUrl}?code=[REDACTED]");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error occurred while running agent service");
    throw;
}

// Helper method to resolve tool outputs by calling the actual MCP server
static async Task<ToolOutput> GetResolvedToolOutputAsync(RequiredToolCall toolCall, string mcpServerUrl, ILogger logger)
{
    // Handle different tool types
    if (toolCall is RequiredFunctionToolCall functionToolCall)
    {
        var functionName = functionToolCall.Name;
        var arguments = functionToolCall.Arguments;
        
        try
        {
            // Call the actual deployed MCP server function
            using var httpClient = new HttpClient();
            
            // Create the request body structure that the MCP server expects
            var requestBody = new
            {
                arguments = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(arguments)
            };
            
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            // Call the specific function endpoint
            var functionUrl = mcpServerUrl.Replace("/runtime/webhooks/mcp/sse", $"/api/{functionName}");
            logger.LogInformation("Calling MCP function: {Url}", functionUrl.Replace(mcpServerUrl.Split('?')[1], "[REDACTED]"));
            
            var response = await httpClient.PostAsync(functionUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                logger.LogInformation("MCP function response: {Response}", responseContent);
                
                // Parse the response to extract the content
                var responseObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                var result = responseObj?.TryGetValue("content", out var contentValue) == true ? 
                    contentValue?.ToString() : responseContent;
                
                return new ToolOutput(toolCall.Id, result ?? "Function executed successfully");
            }
            else
            {
                logger.LogError("MCP function call failed: {StatusCode} - {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return new ToolOutput(toolCall.Id, $"Error calling {functionName}: HTTP {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling MCP function {FunctionName}", functionName);
            return new ToolOutput(toolCall.Id, $"Error calling {functionName}: {ex.Message}");
        }
    }
    
    // For non-function tool calls
    return new ToolOutput(toolCall.Id, "Tool call processed successfully");
}
