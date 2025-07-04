using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// Build configuration
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
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
    // Create AI Projects client
    var client = new AIProjectClient(
        new Uri(projectEndpoint),
        new DefaultAzureCredential());

    logger.LogInformation("Created AI Project client for endpoint: {Endpoint}", projectEndpoint);

    // Create agent with MCP server tool
    var agentData = BinaryData.FromObjectAsJson(new
    {
        model = modelDeploymentName,
        name = "my-mcp-agent",
        instructions = "You are a helpful assistant. Use the tools provided to answer the user's questions. Be sure to cite your sources.",
        tools = new[]
        {
            new
            {
                type = "mcp",
                server_label = mcpServerLabel,
                server_url = mcpServerUrl + "?code=" + mcpExtensionKey,
                require_approval = "never"
            }
        }
    });

    var agent = client.Agents.CreateAgent(agentData);
    var agentId = JsonDocument.Parse(agent.Value.ToStream()).RootElement.GetProperty("id").GetString();

    logger.LogInformation("Created agent, agent ID: {AgentId}", agentId);

    // Create thread
    var thread = client.Agents.CreateThread();
    var threadId = JsonDocument.Parse(thread.Value.ToStream()).RootElement.GetProperty("id").GetString();
    logger.LogInformation("Created thread, thread ID: {ThreadId}", threadId);

    // Create message
    var messageData = BinaryData.FromObjectAsJson(new
    {
        role = "user",
        content = userMessage
    });

    var message = client.Agents.CreateMessage(threadId!, messageData);
    var messageId = JsonDocument.Parse(message.Value.ToStream()).RootElement.GetProperty("id").GetString();
    
    logger.LogInformation("Created message, message ID: {MessageId}", messageId);
    logger.LogInformation("Message content: {Content}", userMessage);

    // Create and run
    var runData = BinaryData.FromObjectAsJson(new
    {
        assistant_id = agentId
    });

    var run = client.Agents.CreateRun(threadId!, runData);
    var runDoc = JsonDocument.Parse(run.Value.ToStream());
    var runId = runDoc.RootElement.GetProperty("id").GetString();
    var runStatus = runDoc.RootElement.GetProperty("status").GetString();

    logger.LogInformation("Created run, run ID: {RunId}", runId);

    // Poll the run until completion
    while (runStatus == "queued" || runStatus == "in_progress" || runStatus == "requires_action")
    {
        await Task.Delay(1000);
        var runResponse = client.Agents.GetRun(threadId!, runId!);
        var updatedRunDoc = JsonDocument.Parse(runResponse.Value.ToStream());
        runStatus = updatedRunDoc.RootElement.GetProperty("status").GetString();
        logger.LogInformation("Run status: {Status}", runStatus);
    }

    if (runStatus == "failed")
    {
        var runResponse = client.Agents.GetRun(threadId!, runId!);
        var failedRunDoc = JsonDocument.Parse(runResponse.Value.ToStream());
        var errorMessage = failedRunDoc.RootElement.TryGetProperty("last_error", out var errorProp) 
            ? errorProp.GetProperty("message").GetString() ?? "Unknown error"
            : "Unknown error";
        logger.LogError("Run failed: {Error}", errorMessage);
    }

    // Get run steps
    var runSteps = client.Agents.GetRunSteps(threadId!, runId!);
    var runStepsDoc = JsonDocument.Parse(runSteps.Value.ToStream());
    
    foreach (var step in runStepsDoc.RootElement.GetProperty("data").EnumerateArray())
    {
        var stepId = step.GetProperty("id").GetString();
        var stepStatus = step.GetProperty("status").GetString();
        var stepType = step.GetProperty("type").GetString();
        
        logger.LogInformation("Run step: {StepId}, status: {Status}, type: {Type}", 
            stepId, stepStatus, stepType);
        
        if (stepType == "tool_calls" && step.TryGetProperty("step_details", out var stepDetails) &&
            stepDetails.TryGetProperty("tool_calls", out var toolCalls))
        {
            logger.LogInformation("Tool call details:");
            logger.LogInformation(JsonSerializer.Serialize(toolCalls, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    // Get final messages
    var messages = client.Agents.GetMessages(threadId!);
    var messagesDoc = JsonDocument.Parse(messages.Value.ToStream());
    
    foreach (var dataPoint in messagesDoc.RootElement.GetProperty("data").EnumerateArray())
    {
        var role = dataPoint.GetProperty("role").GetString();
        if (dataPoint.TryGetProperty("content", out var contentArray) && contentArray.GetArrayLength() > 0)
        {
            var firstContent = contentArray[0];
            if (firstContent.TryGetProperty("text", out var textProp) &&
                textProp.TryGetProperty("value", out var valueProp))
            {
                var content = valueProp.GetString();
                logger.LogInformation("{Role}: {Content}", role, content);
            }
        }
    }

    // Clean up
    client.Agents.DeleteAgent(agentId!);
    logger.LogInformation("Deleted agent, agent ID: {AgentId}", agentId);

    logger.LogInformation("Agent Service completed successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error occurred while running agent service");
    throw;
}
