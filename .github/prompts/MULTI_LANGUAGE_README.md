# Azure AI Foundry Agent Service - Multi-Language Sample Repository

## Repository Purpose

This repository serves as the **Python reference implementation** for integrating Azure AI Foundry Agent Service with remote MCP (Model Context Protocol) servers using Azure Functions. It provides a complete, production-ready template that demonstrates best practices for:

- **Remote MCP Server Implementation**: Azure Functions-based MCP server with blob storage persistence
- **AI Foundry Agent Integration**: Client application using Azure AI Projects SDK
- **Infrastructure as Code**: Complete Bicep templates for Azure resource deployment
- **Development Workflow**: Local debugging, testing, and cloud deployment patterns

## Multi-Language Ecosystem

This Python implementation is part of a multi-language sample ecosystem:

### 🐍 **Python** (This Repository)
- **Repository**: [foundry-agent-service-remote-mcp-python](https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-python)
- **Status**: ✅ **Reference Implementation** - Complete and actively maintained
- **Features**: Full Azure Functions MCP server + AI Foundry agent client

### 🔷 **.NET/C#** (Target Repository)
- **Repository**: [foundry-agent-service-remote-mcp-dotnet](https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-dotnet)
- **Status**: 🚧 **Porting in Progress** - Based on this Python reference
- **Target Features**: Identical functionality using .NET 8.0 + Azure Functions v4

### 🟨 **JavaScript/TypeScript** (Target Repository)
- **Repository**: [foundry-agent-service-remote-mcp-javascript](https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-javascript)
- **Status**: 🚧 **Porting in Progress** - Based on this Python reference
- **Target Features**: Identical functionality using Node.js 20.x + TypeScript

## Porting Documentation

### 📋 **Comprehensive Porting Guide**
The complete technical specification for porting this repository to other languages is available in:

**[PORTING_PROMPT.md](./PORTING_PROMPT.md)**

This document provides:
- ✅ **Architecture Analysis**: Complete breakdown of components and patterns
- ✅ **Implementation Specifications**: Language-specific coding requirements
- ✅ **Infrastructure Guidelines**: Bicep template adaptation instructions
- ✅ **Development Workflow**: VS Code, debugging, and deployment setup
- ✅ **Quality Assurance**: Testing and validation requirements
- ✅ **Best Practices**: Language-specific conventions and patterns

### 🎯 **Porting Objectives**

Each language port must achieve:

1. **Functional Parity**: Identical MCP server behavior and agent client functionality
2. **Infrastructure Consistency**: Same Azure resources and security model
3. **Development Experience**: Language-appropriate tooling and debugging
4. **Documentation Quality**: Complete setup and troubleshooting guides
5. **Performance Equivalence**: Similar resource usage and response times

### 🔧 **Technical Requirements**

**MCP Server (Azure Functions)**:
- Three identical MCP tools: `hello_mcp`, `get_snippet`, `save_snippet`
- Azure Blob Storage integration with same path patterns
- Managed identity authentication
- Experimental MCP extension bundle support

**Agent Client**:
- Azure AI Projects SDK integration
- Environment variable configuration
- Agent lifecycle management
- Thread and message handling

**Infrastructure**:
- Complete AI Foundry resource deployment
- Flex Consumption Function App
- RBAC and security configurations
- Optional VNET and private endpoints

## Getting Started with This Python Version

For complete setup and usage instructions for this Python implementation, see the main [README.md](./README.md).

## Contributing to the Multi-Language Ecosystem

### For Python Repository (This Repository)
- **Purpose**: Maintain as the reference implementation
- **Changes**: Should improve patterns that benefit all language ports
- **Testing**: Ensure changes maintain backward compatibility

### For Language Ports
- **Process**: Follow the specifications in [PORTING_PROMPT.md](./PORTING_PROMPT.md)
- **Validation**: Test against the behavior of this Python reference
- **Documentation**: Update language-specific documentation while maintaining structural consistency

### Cross-Repository Coordination
- **Issues**: Report cross-language issues in the relevant language repository
- **Features**: Propose new features in this Python reference repository first
- **Documentation**: Keep porting documentation synchronized across repositories

## Architecture Overview

```
┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐
│   Agent Client      │    │  Azure AI Foundry   │    │   MCP Server        │
│   (src/agent/)      │───▶│   Agent Service     │───▶│   (Azure Functions) │
│                     │    │                     │    │                     │
│ - AI Projects SDK   │    │ - GPT-4.1-mini      │    │ - hello_mcp         │
│ - DefaultAzureAuth  │    │ - Agent Management  │    │ - get_snippet       │
│ - Thread Management │    │ - MCP Integration   │    │ - save_snippet      │
└─────────────────────┘    └─────────────────────┘    └─────────────────────┘
                                                                   │
                                                                   ▼
                                                       ┌─────────────────────┐
                                                       │   Azure Blob        │
                                                       │   Storage           │
                                                       │                     │
                                                       │ - Snippet Storage   │
                                                       │ - Managed Identity  │
                                                       │ - HTTPS/Auth        │
                                                       └─────────────────────┘
```

## Support and Resources

- **Documentation**: [Azure AI Foundry Agents](https://learn.microsoft.com/en-us/azure/ai-foundry/agents/)
- **Azure Functions MCP**: [Functions MCP Bindings](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-mcp)
- **Community**: GitHub Issues in respective language repositories
- **Samples**: [Azure AI Samples](https://github.com/Azure-Samples/)

---

**Note**: This documentation is maintained as part of the Python reference implementation. Each language port should adapt this structure while maintaining consistency across the ecosystem.