# Porting Guide: Azure AI Foundry Agent Service with Remote MCP Functions

## Overview

This guide provides comprehensive instructions for porting the Python-based "Getting Started with Agent Service and Remote MCP Servers" repository to **.NET** and **JavaScript/TypeScript**. The goal is to maintain exact functional parity while adapting to language-specific best practices and ecosystems.

## Target Repositories

- **.NET/C# Version**: https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-dotnet
- **JavaScript/TypeScript Version**: https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-javascript

## Architecture Overview

The repository implements a complete Azure AI Foundry integration with remote MCP (Model Context Protocol) servers using:

1. **Azure Functions MCP Server**: Remote MCP server with three tools for snippet management
2. **AI Foundry Agent Client**: Client that connects to Azure AI Foundry and uses the MCP server
3. **Infrastructure as Code**: Bicep templates for complete Azure resource deployment
4. **Development Workflow**: Local debugging, testing, and cloud deployment with azd

## Core Components to Port

### 1. MCP Server (Azure Functions)

**Current Implementation**: `src/mcp_server/function_app.py`

**Key Features**:
- Azure Functions with Flex Consumption plan
- MCP tool triggers using experimental extension bundle
- Three MCP tools: `hello_mcp`, `get_snippet`, `save_snippet`
- Azure Blob Storage integration for persistence
- Managed identity authentication
- Structured logging and error handling

**Python Implementation Details**:
```python
# Tool definition with properties
@app.generic_trigger(
    arg_name="context",
    type="mcpToolTrigger",
    toolName="save_snippet",
    description="Save a snippet with a name.",
    toolProperties=tool_properties_save_snippets_json,
)
@app.generic_output_binding(arg_name="file", type="blob", connection="AzureWebJobsStorage", path=_BLOB_PATH)
def save_snippet(file: func.Out[str], context) -> str:
    # Implementation
```

**Porting Requirements**:
- Maintain exact same MCP tool interface and behavior
- Use language-appropriate Azure Functions SDK
- Preserve blob storage path patterns: `snippets/{mcptoolargs.snippetname}.json`
- Maintain same error handling and logging patterns
- Use same host.json configuration with experimental extension bundle

### 2. Agent Client

**Current Implementation**: `src/agent/main.py`

**Key Features**:
- Azure AI Projects SDK integration
- Environment variable configuration
- Agent creation with MCP server registration
- Thread and message management
- Polling for run completion
- Comprehensive error handling and logging

**Python Implementation Pattern**:
```python
project_client = AIProjectClient(
    endpoint=PROJECT_ENDPOINT,
    credential=DefaultAzureCredential()
)

agent = project_client.agents.create_agent(
    model=MODEL_DEPLOYMENT_NAME,
    name="my-mcp-agent",
    instructions="You are a helpful assistant...",
    tools=[{
        "type": "mcp",
        "server_label": MCP_SERVER_LABEL,
        "server_url": MCP_SERVER_URL + "?code=" + MCP_EXTENSION_KEY,
        "require_approval": "never"
    }]
)
```

**Porting Requirements**:
- Use appropriate Azure AI SDK for target language
- Maintain same environment variable patterns
- Preserve exact agent configuration
- Implement same run polling and message handling logic
- Maintain same logging and error patterns

### 3. Infrastructure (Bicep Templates)

**Key Files**:
- `infra/main.bicep` - Main deployment template
- `infra/app/api.bicep` - Function app configuration
- `infra/agent/` - AI Foundry resource modules

**Critical Features**:
- AI Foundry project and capability hosts
- Flex Consumption Function App
- Managed identity and RBAC configuration
- Storage accounts, AI Services, Search, Cosmos DB
- Optional VNET and private endpoints
- Location restrictions for AI services

**Porting Requirements**:
- Maintain exact same Bicep infrastructure
- Update runtime configurations for target language
- Preserve all security and networking configurations
- Maintain same parameter structure and outputs

### 4. Development Experience

**Key Elements**:
- Azure Developer CLI (azd) integration
- VS Code configuration and debugging
- Local development and testing
- Deployment and validation scripts

## Language-Specific Porting Instructions

### .NET/C# Version

**Target Repository**: https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-dotnet

#### MCP Server Implementation

**Requirements**:
- Use .NET 8.0 (LTS) with Azure Functions v4
- Implement isolated worker model
- Use Azure.Storage.Blobs for blob operations
- Use Microsoft.Extensions.Logging for structured logging

**Key Implementation Points**:
```csharp
[Function("SaveSnippet")]
[BlobOutput("snippets/{mcptoolargs.snippetname}.json", Connection = "AzureWebJobsStorage")]
public async Task<string> SaveSnippet([McpToolTrigger] McpToolContext context, 
    ILogger logger)
{
    // Implementation maintaining exact same behavior
}
```

**Dependencies** (`*.csproj`):
- Microsoft.Azure.Functions.Worker
- Microsoft.Azure.Functions.Worker.Sdk
- Microsoft.Azure.Functions.Worker.Extensions.Storage.Blobs
- Microsoft.Extensions.Azure
- Azure.Storage.Blobs
- Azure.Identity

#### Agent Client Implementation

**Requirements**:
- Use Azure.AI.Projects NuGet package
- Use Microsoft.Extensions.Configuration for settings
- Use Microsoft.Extensions.Logging
- Implement async/await patterns throughout

**Key Implementation Points**:
```csharp
var client = new AIProjectClient(
    new Uri(projectEndpoint),
    new DefaultAzureCredential());

var agent = await client.CreateAgentAsync(
    model: modelDeploymentName,
    name: "my-mcp-agent",
    instructions: "You are a helpful assistant...",
    tools: new[] {
        new McpTool {
            Type = "mcp",
            ServerLabel = mcpServerLabel,
            ServerUrl = $"{mcpServerUrl}?code={mcpExtensionKey}",
            RequireApproval = "never"
        }
    });
```

#### Infrastructure Updates

**Runtime Configuration** (`infra/app/api.bicep`):
```bicep
runtime: {
  name: 'dotnet'
  version: '8.0'
}
```

**Project Configuration** (`azure.yaml`):
```yaml
services:
  api:
    project: ./src/McpServer
    language: dotnet
    host: function
```

#### Development Files

**Required Files**:
- `src/McpServer/McpServer.csproj`
- `src/McpServer/Program.cs` - Function app initialization
- `src/McpServer/Functions/McpFunctions.cs` - MCP tool implementations
- `src/McpServer/Models/` - Data models
- `src/Agent/Agent.csproj`
- `src/Agent/Program.cs` - Agent client implementation
- `.vscode/launch.json` - Updated for .NET debugging
- `.gitignore` - .NET specific patterns

### JavaScript/TypeScript Version

**Target Repository**: https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-javascript

#### MCP Server Implementation

**Requirements**:
- Use Node.js 20.x LTS with Azure Functions v4
- Use TypeScript for type safety
- Use @azure/functions for Functions SDK
- Use @azure/storage-blob for blob operations
- Use @azure/identity for authentication

**Key Implementation Points**:
```typescript
import { app, InvocationContext } from '@azure/functions';

app.generic('saveSnippet', {
    trigger: {
        type: 'mcpToolTrigger',
        name: 'save_snippet',
        description: 'Save a snippet with a name.',
        toolProperties: JSON.stringify(toolPropertiesSaveSnippets)
    },
    outputBinding: {
        type: 'blob',
        name: 'file',
        connection: 'AzureWebJobsStorage',
        path: 'snippets/{mcptoolargs.snippetname}.json'
    },
    handler: async (context: InvocationContext): Promise<string> => {
        // Implementation maintaining exact same behavior
    }
});
```

**Dependencies** (`package.json`):
```json
{
  "dependencies": {
    "@azure/functions": "^4.0.0",
    "@azure/storage-blob": "^12.25.0",
    "@azure/identity": "^4.0.0"
  },
  "devDependencies": {
    "@types/node": "^20.0.0",
    "typescript": "^5.0.0",
    "@azure/functions-core-tools": "^4.0.0"
  }
}
```

#### Agent Client Implementation

**Requirements**:
- Use @azure/ai-projects npm package
- Use @azure/identity for authentication
- Use dotenv for environment configuration
- Implement proper async/await patterns

**Key Implementation Points**:
```typescript
import { AIProjectClient } from '@azure/ai-projects';
import { DefaultAzureCredential } from '@azure/identity';

const client = new AIProjectClient(
    projectEndpoint,
    new DefaultAzureCredential()
);

const agent = await client.agents.createAgent({
    model: modelDeploymentName,
    name: 'my-mcp-agent',
    instructions: 'You are a helpful assistant...',
    tools: [{
        type: 'mcp',
        serverLabel: mcpServerLabel,
        serverUrl: `${mcpServerUrl}?code=${mcpExtensionKey}`,
        requireApproval: 'never'
    }]
});
```

#### Infrastructure Updates

**Runtime Configuration** (`infra/app/api.bicep`):
```bicep
runtime: {
  name: 'node'
  version: '20'
}
```

**Project Configuration** (`azure.yaml`):
```yaml
services:
  api:
    project: ./src/mcp-server
    language: javascript
    host: function
```

#### Development Files

**Required Files**:
- `src/mcp-server/package.json`
- `src/mcp-server/tsconfig.json`
- `src/mcp-server/src/functions/mcpFunctions.ts`
- `src/mcp-server/src/models/` - Type definitions
- `src/agent/package.json`
- `src/agent/tsconfig.json`
- `src/agent/src/main.ts`
- `.vscode/launch.json` - Updated for Node.js debugging
- `.gitignore` - Node.js specific patterns

## Cross-Language Requirements

### Environment Variables

Maintain exact same environment variable structure:
```
PROJECT_ENDPOINT=https://your-agent-service-resource.services.ai.azure.com/api/projects/your-project-name
MODEL_DEPLOYMENT_NAME=gpt-4.1-mini
MCP_SERVER_LABEL=Azure_Functions_MCP_Server
MCP_SERVER_URL=https://<your-funcappname>.azurewebsites.net/runtime/webhooks/mcp/sse
USER_MESSAGE=Create a snippet called snippet1 that prints 'Hello, World!' in Python.
MCP_EXTENSION_KEY=your_function_key_here
```

### Documentation

**Required Documentation Updates**:
- Update README.md with language-specific prerequisites and commands
- Update architecture diagram if needed
- Create language-specific troubleshooting sections
- Update VS Code configuration documentation

**Prerequisites by Language**:

**.NET Requirements**:
- .NET 8.0 SDK
- Azure Functions Core Tools v4
- C# extension for VS Code

**JavaScript Requirements**:
- Node.js 20.x LTS
- npm or yarn
- Azure Functions Core Tools v4
- TypeScript

### Testing and Validation

**Validation Requirements**:
1. Deploy infrastructure successfully with `azd up`
2. Function app responds to MCP tool calls
3. Agent client successfully connects and creates agents
4. End-to-end snippet save/retrieve workflow works
5. All three MCP tools function identically to Python version
6. Local debugging works in VS Code
7. Environment variable configuration works correctly

### Error Handling and Logging

**Consistency Requirements**:
- Maintain same error message formats
- Use structured logging with same log levels
- Preserve same exception handling patterns
- Maintain same return value formats for MCP tools

### Security and Authentication

**Critical Requirements**:
- Preserve managed identity authentication patterns
- Maintain same RBAC role assignments
- Keep same function-level authentication
- Preserve blob storage security model
- Maintain same network security options

## Implementation Priority

### Phase 1: Core MCP Server
1. Set up Azure Functions project structure
2. Implement three MCP tools with exact same behavior
3. Configure blob storage integration
4. Test MCP tool responses

### Phase 2: Agent Client
1. Set up agent client project
2. Implement AI Foundry connection
3. Implement agent creation and management
4. Test end-to-end integration

### Phase 3: Infrastructure
1. Update Bicep templates for runtime
2. Update azd configuration
3. Test deployment pipeline
4. Validate all Azure resources

### Phase 4: Development Experience
1. Configure VS Code debugging
2. Update documentation
3. Test local development workflow
4. Create validation scripts

## Quality Assurance

### Functional Testing
- All MCP tools return identical responses
- Error conditions handled identically
- Performance characteristics maintained
- Resource consumption similar

### Integration Testing
- Azure AI Foundry integration works identically
- Blob storage operations function the same
- Authentication and authorization preserved
- Networking and security maintain same behavior

### Documentation Testing
- README instructions work correctly
- Prerequisites are accurate and complete
- Troubleshooting guides are effective
- Code samples run without modification

This comprehensive porting guide ensures that both .NET and JavaScript versions maintain complete functional parity with the Python original while following language-specific best practices and conventions.