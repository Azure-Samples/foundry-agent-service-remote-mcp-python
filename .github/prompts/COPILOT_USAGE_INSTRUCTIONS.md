# How to Use Porting Prompts with GitHub Copilot

## Quick Start for Copilot Coding Agent

### 1. Choose Your Target Language

**For .NET/C# Porting:**
```
Target Repository: https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-dotnet
```

**For JavaScript/TypeScript Porting:**
```
Target Repository: https://github.com/Azure-Samples/foundry-agent-service-remote-mcp-javascript
```

### 2. Copy the Complete Porting Prompt

**Primary Prompt File**: [PORTING_PROMPT.md](./PORTING_PROMPT.md)

#### How to Use with Copilot:

1. **Open the target repository** (dotnet or javascript)
2. **Start a new Copilot chat session**
3. **Copy the entire contents** of `PORTING_PROMPT.md` from this repository
4. **Paste it into Copilot** with this prefix:

```
I need to port the Azure AI Foundry Agent Service with Remote MCP Functions from Python to [.NET/JavaScript]. 

Please implement this following the complete specification below:

[PASTE ENTIRE PORTING_PROMPT.md CONTENTS HERE]

Start with Phase 1: Project Setup for [.NET/JavaScript] as specified in the prompt.
```

### 3. Execution Phases

Follow these phases in order with Copilot:

#### Phase 1: Project Setup
```
"Start with Phase 1 from the porting prompt - set up the project structure, dependencies, and basic configuration for [.NET/JavaScript] as specified."
```

#### Phase 2: MCP Server Implementation  
```
"Implement Phase 2 from the porting prompt - create the Azure Functions MCP server with the three tools (hello_mcp, get_snippet, save_snippet) using the exact specifications provided."
```

#### Phase 3: Agent Client Implementation
```
"Implement Phase 3 from the porting prompt - create the AI Foundry agent client with Azure AI Projects SDK integration as specified."
```

#### Phase 4: Infrastructure Updates
```
"Implement Phase 4 from the porting prompt - update the Bicep templates for the target language runtime as specified."
```

#### Phase 5: Development Experience
```
"Implement Phase 5 from the porting prompt - set up VS Code configuration, debugging, and local development experience as specified."
```

#### Phase 6: Documentation
```
"Create the documentation as specified in Phase 6 of the porting prompt - README, setup instructions, and troubleshooting guides."
```

### 4. Validation Commands

After each phase, use these validation commands:

```bash
# Check implementation completeness
./validate-porting-simple.sh

# Test local development
# (Follow language-specific commands from the prompt)

# Test deployment
azd up
```

### 5. Key Reference Points

During implementation, reference these specific sections of the porting prompt:

**For .NET Developers:**
- Section: "Language-Specific Porting Instructions > .NET/C# Version"
- Code examples for Azure Functions Worker model
- NuGet package requirements
- C# coding patterns

**For JavaScript/TypeScript Developers:**  
- Section: "Language-Specific Porting Instructions > JavaScript/TypeScript Version"
- Code examples for Azure Functions Node.js model
- npm package requirements
- TypeScript/JavaScript patterns

### 6. Common Copilot Prompts During Implementation

**When stuck on Azure Functions:**
```
"Based on the porting prompt specification, show me how to implement the [tool_name] Azure Function using [.NET/JavaScript] with the exact same behavior as the Python version."
```

**When implementing the agent client:**
```
"Using the agent client specifications from the porting prompt, implement the Azure AI Projects SDK integration for [.NET/JavaScript] with the same environment variables and behavior."
```

**When updating infrastructure:**
```
"Update the Bicep templates according to the infrastructure section of the porting prompt for [.NET/JavaScript] runtime configuration."
```

### 7. Troubleshooting with Copilot

**If implementation doesn't match specification:**
```
"Compare my current implementation with the requirements in the porting prompt section [X]. What needs to be adjusted to match the specification exactly?"
```

**If tests fail:**
```
"Based on the functional parity requirements in the porting prompt, help me debug why [specific functionality] isn't working as expected."
```

### 8. Final Validation

Before considering the port complete, verify with Copilot:

```
"Review my implementation against the 'Success Criteria' section of the porting prompt. Verify that all requirements are met for functional parity with the Python reference implementation."
```

## Complete Prompt Template

Here's the exact template to use with Copilot:

```
I need to implement a complete port of the Azure AI Foundry Agent Service with Remote MCP Functions from Python to [.NET/JavaScript/TypeScript].

This must achieve 100% functional parity with the Python reference implementation. Please follow the comprehensive specification below and implement each phase systematically:

[COPY AND PASTE THE ENTIRE CONTENT OF PORTING_PROMPT.md HERE]

Please start with Phase 1: Project Setup for [target language] and guide me through each phase to completion. Ask me to confirm before proceeding to the next phase.
```

## Tips for Success

1. **Use the complete prompt** - Don't break it into smaller pieces initially
2. **Follow phases sequentially** - Don't skip ahead 
3. **Validate each phase** - Test before moving to next phase
4. **Reference the Python code** - Use it as the behavioral specification
5. **Test functional parity** - Ensure identical behavior throughout

The porting prompt contains over 13,000 words of detailed specifications - using it completely with Copilot will ensure accurate and complete implementation.