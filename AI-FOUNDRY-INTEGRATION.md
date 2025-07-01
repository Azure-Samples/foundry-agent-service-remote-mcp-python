# AI Foundry Integration Summary

This document summarizes the changes made to integrate Azure AI Foundry Standard resources into the existing Remote MCP Functions Python repository.

## Changes Made

### 1. Added `/infra/agent/` Folder
Copied 5 bicep modules from the reference repository:
- `standard-ai-project.bicep` - Creates AI projects with connections
- `standard-dependent-resources.bicep` - Creates AI services, search, cosmos DB, storage
- `standard-ai-project-capability-host.bicep` - Configures agent capability hosts
- `standard-ai-project-role-assignments.bicep` - Sets up RBAC for AI resources
- `post-capability-host-role-assignments.bicep` - Additional RBAC post-deployment

### 2. Updated `main.bicep`
- Added AI Foundry parameters (project name, services, model config, etc.)
- Added variables for AI project naming and unique suffix
- Integrated AI modules in correct dependency order
- Added proper outputs for environment variables

### 3. Environment Variables
The following environment variables are now automatically provided by `azd up`:

| Variable | Source | Example Value |
|----------|--------|---------------|
| `PROJECT_ENDPOINT` | AI Project endpoint | `https://ai-services-xxx.services.ai.azure.com/api/projects/project-demo-xxx` |
| `MODEL_DEPLOYMENT_NAME` | Model name parameter | `gpt-4.1-mini` |
| `MCP_SERVER_URL` | Function app URL | `https://func-api-xxx.azurewebsites.net/runtime/webhooks/mcp/sse` |
| `SERVICE_API_NAME` | Function app name | `func-api-xxx` |

### 4. Updated Source Files
- Updated `src/agent/.env.example` with correct model name
- Updated `src/agent/main.py` default model name
- Updated `README.md` with correct model name

### 5. Location Restrictions
Updated allowed locations to match AI Foundry requirements:
- `australiaeast`
- `eastus`
- `eastus2`  
- `southcentralus`
- `southeastasia`
- `uksouth`

## Deployment

The infrastructure now deploys:

1. **Existing Resources** (unchanged):
   - Azure Function App (Python 3.12, Flex Consumption)
   - Storage Account for function app
   - Application Insights and Log Analytics
   - User-assigned managed identity
   - App Service Plan
   - RBAC assignments
   - Optional: Virtual Network and Private Endpoints

2. **New AI Foundry Resources**:
   - AI Services account with model deployment (gpt-4.1-mini)
   - AI Search service
   - Cosmos DB account for thread storage
   - AI Storage account for artifacts
   - AI Project with connections to all services
   - Account and Project capability hosts for agents
   - Comprehensive RBAC configuration

## Usage

After `azd up` deployment:

1. Copy the output environment variables to `src/agent/.env`
2. Get the MCP extension key: `az functionapp keys list --resource-group <rg> --name <function-app>`
3. Add `MCP_EXTENSION_KEY` to `.env`
4. Run the agent: `python src/agent/main.py`

The agent will now connect to the deployed AI Foundry project and use the remote MCP server for enhanced capabilities.

## Testing

Run `./validate-setup.sh` to verify all components are correctly configured.