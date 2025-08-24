<!--
---
name: Remote MCP with Azure Functions (.NET)
description: Run a remote MCP server on Azure functions.  
page_type: sample
languages:
- csharp
- bicep
- azdeveloper
products:
- azure-functions
- azure
- entra-id
- azure-openai
- ai-foundry
urlFragment: foundry-agent-service-remote-mcp-dotnet
---
-->

# Getting Started with Agent Service and Remote MCP Servers (.NET)

This is a quickstart template to easily run an [Azure AI Foundry Agent Service](https://learn.microsoft.com/en-us/azure/ai-foundry/agents/) client and then add a custom remote MCP server to the cloud using [Azure Functions Remote MCP](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-mcp?pivots=programming-language-csharp). You can clone/restore/run on your local machine with debugging, and `azd up` to have it in the cloud in a couple minutes. The MCP server is secured by design using keys and HTTPS, and allows more options for OAuth using built-in auth and/or [API Management](https://aka.ms/mcp-remote-apim-auth) as well as network isolation using VNET.

If you're looking for this sample in more languages check out the [Python](https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-python) and [Node.js/TypeScript](https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-typescript) versions.

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/Azure-Samples/foundry-agent-service-remote-mcp-dotnet)

Below is the architecture diagram for the Remote MCP Server using Azure Functions with Foundry Agent Service:

![Architecture Diagram](architecture-diagram.png)

## Prerequisites

+ [.NET SDK](https://dotnet.microsoft.com/download) version 8.0 or higher
+ [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local?pivots=programming-language-csharp#install-the-azure-functions-core-tools) >= `4.0.7030`
+ [Azure Developer CLI](https://aka.ms/azd)
+ To use Visual Studio Code to run and debug locally:
  + [Visual Studio Code](https://code.visualstudio.com/)
  + [Azure Functions extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
  + [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)


## Deploy Remote MCP Server to Azure

Run this [azd](https://aka.ms/azd) command to provision the function app, with any required Azure resources including AI Foundry Agent Service, and deploy your code:

```shell
azd up
```

> **Note**: You'll be prompted to specify an `agentLocation` during deployment. This must be one of the AI Foundry supported regions: `westus`, `westus2`, `uaenorth`, `southindia`, or `switzerlandnorth`. This location is used specifically for AI resources (AI Services, Search, Cosmos DB) and can be different from your main deployment location.

Additionally, [API Management]() can be used for improved security and policies over your MCP Server, and [App Service built-in authentication](https://learn.microsoft.com/azure/app-service/overview-authentication-authorization) can be used to set up your favorite OAuth provider including Entra.  

## Connect to your *remote* MCP server function app from a client

Your client will need a key in order to invoke the new hosted SSE endpoint, which will be of the form `https://<funcappname>.azurewebsites.net/runtime/webhooks/mcp/sse`. The hosted function requires a system key by default which can be obtained from the [portal](https://learn.microsoft.com/azure/azure-functions/function-keys-how-to?tabs=azure-portal) or the CLI (`az functionapp keys list --resource-group <resource_group> --name <function_app_name>`). Obtain the system key named `mcp_extension`.

### Foundry Agent Service Client

1. Change to the agent folder in a new terminal window:

   ```shell
   cd src/agent
   ```

2. Create an `appsettings.json` file based on the example provided. Copy the `appsettings.example.json` file:

   ```shell
   # Windows
   copy appsettings.example.json appsettings.json
   
   # Linux/macOS
   cp appsettings.example.json appsettings.json
   ```

3. Edit the `appsettings.json` file with your deployed function app details:

   ```json
   {
     "AzureAI": {
       "ProjectEndpoint": "https://your-agent-service-resource.services.ai.azure.com/api/projects/your-project-name",
       "ModelDeploymentName": "gpt-4.1-mini"
     },
     "MCP": {
       "ServerLabel": "Azure_Functions_MCP_Server",
       "ServerUrl": "https://<your-funcappname>.azurewebsites.net/runtime/webhooks/mcp/sse",
       "ExtensionKey": "your_mcp_extension_system_key_here"
     },
     "UserMessage": "Create a snippet called snippet1 that prints 'Hello, World!' in C#."
   }
   ```

   > **Note**: Replace the following values with outputs from your `azd up` deployment:
   > - `ProjectEndpoint`: Your Azure AI Project endpoint (from azd deployment output)
   > - `<your-funcappname>`: Your function app name (from azd deployment output)
   > - `your_mcp_extension_system_key_here`: The `mcp_extension` system key obtained from the Azure portal or CLI

4. Restore .NET dependencies for the agent:

   ```shell
   dotnet restore
   ```

5. Run the agent service:

   ```shell
   dotnet run
   ```

   The agent will connect to your remote MCP server and execute the message specified in the `UserMessage` configuration, demonstrating the integration between Azure AI Foundry and your deployed MCP server.

### Connect to remote MCP server in MCP Inspector
For MCP Inspector, you can include the key in the URL: 
```plaintext
https://<funcappname>.azurewebsites.net/runtime/webhooks/mcp/sse?code=<your-mcp-extension-system-key>
```

## Redeploy your code

You can run the `azd up` command as many times as you need to both provision your Azure resources and deploy code updates to your function app.

>[!NOTE]
>Deployed code files are always overwritten by the latest deployment package.

## Clean up resources

When you're done working with your function app and related resources, you can use this command to delete the function app and its related resources from Azure and avoid incurring any further costs:

```shell
azd down
```

## Helpful Azure Commands

Once your application is deployed, you can use these commands to manage and monitor your application:

```bash
# Get your function app name from the environment file
FUNCTION_APP_NAME=$(cat .azure/$(cat .azure/config.json | jq -r '.defaultEnvironment')/env.json | jq -r '.FUNCTION_APP_NAME')
echo $FUNCTION_APP_NAME

# Get resource group 
RESOURCE_GROUP=$(cat .azure/$(cat .azure/config.json | jq -r '.defaultEnvironment')/env.json | jq -r '.AZURE_RESOURCE_GROUP')
echo $RESOURCE_GROUP

# View function app logs
az webapp log tail --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP

# Redeploy the application without provisioning new resources
azd deploy
```

## Debugging MCP server function locally

An Azure Storage Emulator is needed for this particular sample because we will save and get snippets from blob storage.

1. Start Azurite

    ```shell
    docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 \
        mcr.microsoft.com/azure-storage/azurite
    ```

>**Note** if you use Azurite coming from VS Code extension you need to run `Azurite: Start` now or you will see errors.

## Run your MCP Server locally from the terminal

1. Change to the src/MCPServer folder in a new terminal window:

   ```shell
   cd src/MCPServer
   ```

2. Restore .NET dependencies:

   ```shell
   dotnet restore
   ```

>**Note** it is a best practice to use .NET solution files to manage dependencies across multiple projects.

3. Start the Functions host locally:

   ```shell
   func start
   ```

> **Note** by default this will use the webhooks route: `/runtime/webhooks/mcp/sse`.  Later we will use this in Azure to set the key on client/host calls: `/runtime/webhooks/mcp/sse?code=<system_key>`

## Connect to the *local* MCP server from a client/host

### Foundry Agent Service Client

The Foundry Agent Service is a cloud service that expects MCP tools that are also in the cloud (e.g. same VNET or on public internet).  Proceed to the steps around deploying the Azure for Remote MCP.

### MCP Inspector

1. In a **new terminal window**, install and run MCP Inspector

    ```shell
    npx @modelcontextprotocol/inspector
    ```

2. CTRL click to load the MCP Inspector web app from the URL displayed by the app (e.g. http://0.0.0.0:5173/#resources)
3. Set the transport type to `SSE`
4. Set the URL to your running Function app's SSE endpoint and **Connect**:

    ```shell
    http://0.0.0.0:7071/runtime/webhooks/mcp/sse
    ```

>**Note** this step will not work in CodeSpaces.  Please move on to Deploy to Remote MCP.  


## Source Code

The function code for the `get_snippet` and `save_snippet` endpoints are defined in the C# files in the `src/MCPServer` directory. The MCP function annotations expose these functions as MCP Server tools.

The Program.cs file sets up the Azure Functions host:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
```

Here's the actual code from the FunctionApp.cs file:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MCPServer
{
    public class FunctionApp
    {
        private readonly ILogger _logger;
        private const string BlobPath = "snippets/{name}.txt";
        private const string SnippetNameProperty = "name";
        private const string SnippetContentProperty = "snippet";

        public FunctionApp(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FunctionApp>();
        }

        [Function("Hello")]
        [MCPToolTrigger("hello", "Hello world.", "[]")]
        public string HelloMcp([MCPToolTrigger] object context)
        {
            /// <summary>
            /// A simple function that returns a greeting message.
            /// </summary>
            /// <param name="context">The trigger context (not used in this function).</param>
            /// <returns>A greeting message.</returns>
            return "Hello I am MCPTool!";
        }

        [Function("GetSnippet")]
        [MCPToolTrigger("getsnippet", "Retrieve a snippet by name.", 
            @"[{""name"": ""name"", ""type"": ""string"", ""description"": ""The name of the snippet to retrieve"", ""required"": true}]")]
        public string GetSnippet(
            [MCPToolTrigger] object context,
            [BlobInput(BlobPath, Connection = "AzureWebJobsStorage")] string file)
        {
            /// <summary>
            /// Retrieves a snippet by name from Azure Blob Storage.
            /// </summary>
            /// <param name="file">The input binding to read the snippet from Azure Blob Storage.</param>
            /// <param name="context">The trigger context containing the input arguments.</param>
            /// <returns>The content of the snippet or an error message.</returns>
            
            if (string.IsNullOrEmpty(file))
            {
                return "Snippet not found";
            }

            _logger.LogInformation($"Retrieved snippet: {file}");
            return file;
        }

        [Function("SaveSnippet")]
        [MCPToolTrigger("savesnippet", "Save a snippet with a name.",
            @"[{""name"": ""name"", ""type"": ""string"", ""description"": ""The name of the snippet"", ""required"": true}, 
               {""name"": ""snippet"", ""type"": ""string"", ""description"": ""The content of the snippet"", ""required"": true}]")]
        public string SaveSnippet(
            [MCPToolTrigger] object context,
            [BlobOutput(BlobPath, Connection = "AzureWebJobsStorage")] out string file)
        {
            var content = JsonSerializer.Deserialize<JsonElement>(context.ToString());
            var arguments = content.GetProperty("arguments");
            
            string snippetName = arguments.TryGetProperty(SnippetNameProperty, out var nameElement) 
                ? nameElement.GetString() : string.Empty;
            string snippetContent = arguments.TryGetProperty(SnippetContentProperty, out var contentElement) 
                ? contentElement.GetString() : string.Empty;

            if (string.IsNullOrEmpty(snippetName))
            {
                file = null;
                return "No snippet name provided";
            }

            if (string.IsNullOrEmpty(snippetContent))
            {
                file = null;
                return "No snippet content provided";
            }

            file = snippetContent;
            _logger.LogInformation($"Saved snippet: {snippetContent}");
            return $"Snippet '{snippetName}' saved successfully";
        }
    }
}
```

Note that the `host.json` file also includes a reference to the experimental bundle, which is required for apps using this feature:

```json
{
  "version": "2.0",
  "extensionBundle": {
    "id": "Microsoft.Azure.Functions.ExtensionBundle.Experimental",
    "version": "[4.*, 5.0.0)"
  },
  "functionTimeout": "00:05:00"
}
```

## Next Steps

- Add [API Management](https://aka.ms/mcp-remote-apim-auth) to your MCP server (auth, gateway, policies, more!)
- Add [built-in auth](https://learn.microsoft.com/en-us/azure/app-service/overview-authentication-authorization) to your MCP server
- Enable VNET using VNET_ENABLED=true flag
- Learn more about [related MCP efforts from Microsoft](https://github.com/microsoft/mcp/tree/main/Resources)