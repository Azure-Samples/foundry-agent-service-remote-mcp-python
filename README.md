<!--
---
name: Remote MCP with Azure Functions (Python)
description: Run a remote MCP server on Azure functions.  
page_type: sample
languages:
- python
- bicep
- azdeveloper
products:
- azure-functions
- azure
- entra-id
- azure-openai
- ai-foundry
urlFragment: foundry-agent-service-remote-mcp-python
---
-->

# Getting Started with Agent Service and Remote MCP Servers (Python)

This is a quickstart template to easily run an [Azure AI Foundry Agent Service](https://learn.microsoft.com/en-us/azure/ai-foundry/agents/) client and then add a custom remote MCP server to the cloud using [Azure Functions Remote MCP](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-mcp?pivots=programming-language-python). You can clone/restore/run on your local machine with debugging, and `azd up` to have it in the cloud in a couple minutes. The MCP server is secured by design using keys and HTTPS, and allows more options for OAuth using built-in auth and/or [API Management](https://aka.ms/mcp-remote-apim-auth) as well as network isolation using VNET.

If you're looking for this sample in more languages check out the [.NET/C#](https://github.com/Azure-Samples/remote-mcp-functions-dotnet) and [Node.js/TypeScript](https://github.com/Azure-Samples/remote-mcp-functions-typescript) versions.

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/Azure-Samples/remote-mcp-functions-python)

Below is the architecture diagram for the Remote MCP Server using Azure Functions with Foundry Agent Service:

![Architecture Diagram](architecture-diagram.png)

## Prerequisites

+ [Python](https://www.python.org/downloads/) version 3.13 or higher
+ [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local?pivots=programming-language-python#install-the-azure-functions-core-tools) >= `4.8.0`
+ [Azure Developer CLI](https://aka.ms/azd)
+ To use Visual Studio Code to run and debug locally:
  + [Visual Studio Code](https://code.visualstudio.com/)
  + [Azure Functions extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)


## Deploy Remote MCP Server to Azure

Run this [azd](https://aka.ms/azd) command to provision the function app, with any required Azure resources including AI Foundry Agent Service, and deploy your code:

```shell
azd up
```

> **Note**: You'll be prompted to specify an `agentLocation` during deployment. This must be one of the [AI Foundry supported regions](https://learn.microsoft.com/en-us/azure/foundry/reference/region-support#foundry-projects). This location is used specifically for AI resources (AI Services, Search, Cosmos DB) and can be different from your main deployment location.

> **⚠️ Region Capacity**: Some regions may not have capacity for all required resources. For example:
> - `eastus2` may be out of capacity for Azure AI Search
> - `eastus` may be out of capacity for Cosmos DB (availability zone accounts)
>
> If you encounter `InsufficientResourcesAvailable` or `ServiceUnavailable` errors during provisioning, try a different region (e.g., `swedencentral`, `westeurope`).

Additionally, [API Management]() can be used for improved security and policies over your MCP Server, and [App Service built-in authentication](https://learn.microsoft.com/azure/app-service/overview-authentication-authorization) can be used to set up your favorite OAuth provider including Entra.  

## Connect to your *remote* MCP server function app from a client

Your client will need a key in order to invoke the new hosted MCP endpoint, which will be of the form `https://<funcappname>.azurewebsites.net/runtime/webhooks/mcp`. The hosted function requires a system key by default which can be obtained from the [portal](https://learn.microsoft.com/azure/azure-functions/function-keys-how-to?tabs=azure-portal) or the CLI (`az functionapp keys list --resource-group <resource_group> --name <function_app_name>`). Obtain the system key named `mcp_extension`.

### Foundry Agent Service Client

1. Change to the agent folder in a new terminal window:

   ```shell
   cd src/agent
   ```

2. Create a `.env` file based on the example provided. Copy the `.env.example` file:

   ```shell
   cp .env.example .env
   ```

3. Edit the `.env` file with your deployed function app details:

   ```env
   # Azure AI Project Configuration
   PROJECT_ENDPOINT=https://your-agent-service-resource.services.ai.azure.com/api/projects/your-project-name
   MODEL_DEPLOYMENT_NAME=gpt-4.1-mini
   MCP_SERVER_LABEL=Azure_Functions_MCP_Server
   MCP_SERVER_URL=https://<your-funcappname>.azurewebsites.net/runtime/webhooks/mcp
   USER_MESSAGE=Create a snippet called snippet1 that prints 'Hello, World!' in Python.

   # Required: Azure Functions extension key for MCP server authentication
   MCP_EXTENSION_KEY=your_mcp_extension_system_key_here
   ```

   > **Note**: Replace the following values with outputs from your `azd up` deployment:
   > - `PROJECT_ENDPOINT`: Your Azure AI Project endpoint (from azd deployment output)
   > - `<your-funcappname>`: Your function app name (from azd deployment output)
   > - `your_mcp_extension_system_key_here`: The `mcp_extension` system key obtained from the Azure portal or CLI

4. Install Python dependencies for the agent:

   ```shell
   pip install -r requirements.txt
   ```

   > **Note**: The agent client uses `AgentsClient` from the `azure-ai-agents` package directly (not via `AIProjectClient`). MCP tool calls require explicit approval — the sample auto-approves them by submitting tool approvals when the run enters `requires_action` status.

5. Run the agent service:

   ```shell
   python main.py
   ```

   The agent will connect to your remote MCP server and execute the message specified in the `USER_MESSAGE` environment variable, demonstrating the integration between Azure AI Foundry and your deployed MCP server.

### Connect to remote MCP server in MCP Inspector
For MCP Inspector, you can include the key in the URL: 
```plaintext
https://<funcappname>.azurewebsites.net/runtime/webhooks/mcp?code=<your-mcp-extension-system-key>
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

## More MCP Server Samples

For additional samples showcasing the latest MCP server features on Azure Functions (including prompts, resources, and more tools), see the [remote-mcp-functions-python](https://github.com/Azure-Samples/remote-mcp-functions-python) repository.

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

1. Change to the src/mcp_server folder in a new terminal window:

   ```shell
   cd src/mcp_server
   ```

1. Install Python dependencies:

   ```shell
   pip install -r requirements.txt
   ```

>**Note** it is a best practice to create a Virtual Environment before doing the `pip install` to avoid dependency issues/collisions, or if you are running in CodeSpaces.  See [Python Environments in VS Code](https://code.visualstudio.com/docs/python/environments#_creating-environments) for more information.

1. Start the Functions host locally:

   ```shell
   func start
   ```

> **Note** by default this will use the webhooks route: `/runtime/webhooks/mcp`.  Later we will use this in Azure to set the key on client/host calls: `/runtime/webhooks/mcp?code=<system_key>`

## Connect to the *local* MCP server from a client/host

### Foundry Agent Service Client

The Foundry Agent Service is a cloud service that expects MCP tools that are also in the cloud (e.g. same VNET or on public internet).  Proceed to the steps around deploying the Azure for Remote MCP.

### MCP Inspector

1. In a **new terminal window**, install and run MCP Inspector

    ```shell
    npx @modelcontextprotocol/inspector
    ```

2. CTRL click to load the MCP Inspector web app from the URL displayed by the app (e.g. http://0.0.0.0:5173/#resources)
3. Set the transport type to `Streamable HTTP`
4. Set the URL to your running Function app's MCP endpoint and **Connect**:

    ```shell
    http://0.0.0.0:7071/runtime/webhooks/mcp
    ```

>**Note** this step will not work in CodeSpaces.  Please move on to Deploy to Remote MCP.  


## Source Code

The function code for the `get_snippet` and `save_snippet` endpoints are defined in the Python files in the `src/mcp_server` directory. The MCP function annotations expose these functions as MCP Server tools.

Here's the actual code from the function_app.py file:

```python

@app.mcp_tool()
def hello_mcp() -> str:
    """Hello world."""
    return "Hello I am MCPTool!"


@app.mcp_tool()
@app.mcp_tool_property(arg_name="snippetname", description="The name of the snippet.")
@app.blob_input(arg_name="file", connection="AzureWebJobsStorage", path=_BLOB_PATH)
def get_snippet(file: func.InputStream, snippetname: str) -> str:
    """Retrieve a snippet by name from Azure Blob Storage."""
    snippet_content = file.read().decode("utf-8")
    logging.info(f"Retrieved snippet: {snippet_content}")
    return snippet_content


@app.mcp_tool()
@app.mcp_tool_property(arg_name="snippetname", description="The name of the snippet.")
@app.mcp_tool_property(arg_name="snippet", description="The content of the snippet.")
@app.blob_output(arg_name="file", connection="AzureWebJobsStorage", path=_BLOB_PATH)
def save_snippet(file: func.Out[str], snippetname: str, snippet: str) -> str:
    """Save a snippet with a name to Azure Blob Storage."""
    if not snippetname:
        return "No snippet name provided"

    if not snippet:
        return "No snippet content provided"

    file.set(snippet)
    logging.info(f"Saved snippet: {snippet}")
    return f"Snippet '{snippet}' saved successfully"
```

The MCP decorators automatically:
- Infer tool properties from function signatures and type hints
- Handle JSON serialization
- Expose the functions as MCP tools without manual configuration

Note that the `host.json` file includes the extension bundle configuration:

```json
"extensionBundle": {
  "id": "Microsoft.Azure.Functions.ExtensionBundle",
  "version": "[4.*, 5.0.0)"
}
```

## Next Steps

- Add [API Management](https://aka.ms/mcp-remote-apim-auth) to your MCP server (auth, gateway, policies, more!)
- Add [built-in auth](https://learn.microsoft.com/en-us/azure/app-service/overview-authentication-authorization) to your MCP server
- Enable VNET using VNET_ENABLED=true flag
- Learn more about [related MCP efforts from Microsoft](https://github.com/microsoft/mcp/tree/main/Resources)
