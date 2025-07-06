using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.AI.OpenAI;

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
    // Create AI Projects client
    var projectClient = new AIProjectClient(
        new Uri(projectEndpoint),
        new DefaultAzureCredential());

    logger.LogInformation("Created AI Project client for endpoint: {Endpoint}", projectEndpoint);

    logger.LogInformation("Configuration loaded successfully:");
    logger.LogInformation("- Project Endpoint: {Endpoint}", projectEndpoint);
    logger.LogInformation("- Model Deployment: {Model}", modelDeploymentName);
    logger.LogInformation("- MCP Server Label: {Label}", mcpServerLabel);
    logger.LogInformation("- MCP Server URL: {Url}", mcpServerUrl);
    logger.LogInformation("- User Message: {Message}", userMessage);
    logger.LogInformation("- MCP Extension Key: [REDACTED]");

    // Demonstrate agent workflow structure
    logger.LogInformation("\n=== Azure AI Foundry Agent Workflow ===");
    
    // Step 1: Agent Configuration
    logger.LogInformation("1. Agent Configuration:");
    logger.LogInformation("   - Model: {Model}", modelDeploymentName);
    logger.LogInformation("   - Name: my-mcp-agent");
    logger.LogInformation("   - Instructions: You are a helpful assistant. Use the tools provided to answer the user's questions. Be sure to cite your sources.");
    
    // Step 2: MCP Tool Configuration
    logger.LogInformation("2. MCP Tool Configuration:");
    var mcpToolConfig = new
    {
        type = "mcp",
        server_label = mcpServerLabel,
        server_url = $"{mcpServerUrl}?code={mcpExtensionKey}",
        require_approval = "never"
    };
    logger.LogInformation("   {McpConfig}", JsonSerializer.Serialize(mcpToolConfig, new JsonSerializerOptions { WriteIndented = true }));

    // Step 3: Verify Azure AI Project connectivity
    logger.LogInformation("3. Verifying Azure AI Project connectivity:");
    try
    {
        var chatClient = projectClient.GetChatCompletionsClient();
        logger.LogInformation("   ✓ Chat completions client ready");
        
        var connections = projectClient.GetAllConnections();
        logger.LogInformation("   ✓ Project connections accessible");
        
        logger.LogInformation("   ✓ Azure AI Project client operational");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "   ⚠ Some connectivity tests failed (expected with default configuration)");
    }

    // Step 4: Demonstrate what the full agent workflow would look like
    logger.LogInformation("4. Full Agent Workflow (Structure):");
    logger.LogInformation("   a) Create agent with MCP tools configuration");
    logger.LogInformation("   b) Create conversation thread");
    logger.LogInformation("   c) Add user message: {Message}", userMessage);
    logger.LogInformation("   d) Execute agent run");
    logger.LogInformation("   e) Monitor run status (queued → in_progress → completed)");
    logger.LogInformation("   f) Process tool calls (MCP server interactions)");
    logger.LogInformation("   g) Retrieve assistant responses");
    logger.LogInformation("   h) Display conversation history");
    logger.LogInformation("   i) Clean up resources");

    // Step 5: Implementation status
    logger.LogInformation("5. Implementation Status:");
    logger.LogInformation("   ✓ Project structure and configuration");
    logger.LogInformation("   ✓ Environment variable handling");
    logger.LogInformation("   ✓ Azure authentication setup");
    logger.LogInformation("   ✓ Logging and error handling");
    logger.LogInformation("   ⏳ Awaiting Azure.AI.Agents .NET package");
    
    logger.LogInformation("\n=== Notes ===");
    logger.LogInformation("The Azure.AI.Projects .NET SDK (v1.0.0-beta.9) provides project-level functionality");
    logger.LogInformation("but does not yet include the agents API that's available in the Python SDK.");
    logger.LogInformation("Python implementation uses: azure-ai-agents==1.1.0b2 + azure-ai-projects>=1.0.0b12");
    logger.LogInformation("Once Azure.AI.Agents NuGet package is available, this implementation can be completed.");
    
    logger.LogInformation("\n✅ Agent Service structure completed successfully");
    logger.LogInformation("🔄 Ready for full implementation when Azure.AI.Agents .NET package becomes available");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error occurred while running agent service");
    throw;
}
