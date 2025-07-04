#!/bin/bash

# Porting Validation Script
# This script validates that all components required for successful porting are present
# and properly documented in the Python reference implementation.

set -e

echo "🔍 Validating Python Reference Implementation for Multi-Language Porting"
echo "================================================================"

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counter for validation results
PASS_COUNT=0
FAIL_COUNT=0

# Function to report validation results
validate() {
    local test_name="$1"
    local condition="$2"
    
    if [ "$condition" = true ]; then
        echo -e "${GREEN}✅ PASS${NC}: $test_name"
        ((PASS_COUNT++))
    else
        echo -e "${RED}❌ FAIL${NC}: $test_name"
        ((FAIL_COUNT++))
    fi
}

echo -e "${BLUE}📋 Repository Structure Validation${NC}"
echo "-----------------------------------"

# Check core directories exist
validate "MCP Server source directory exists" "$([ -d "src/mcp_server" ] && echo true || echo false)"
validate "Agent client source directory exists" "$([ -d "src/agent" ] && echo true || echo false)"
validate "Infrastructure directory exists" "$([ -d "infra" ] && echo true || echo false)"
validate "AI Foundry agent modules exist" "$([ -d "infra/agent" ] && echo true || echo false)"

echo -e "\n${BLUE}📦 MCP Server Component Validation${NC}"
echo "-----------------------------------"

# Check MCP server implementation
validate "Function app implementation exists" "$([ -f "src/mcp_server/function_app.py" ] && echo true || echo false)"
validate "Function app has MCP tool triggers" "$(grep -q "mcpToolTrigger" src/mcp_server/function_app.py && echo true || echo false)"
validate "Hello MCP tool implemented" "$(grep -q "hello_mcp" src/mcp_server/function_app.py && echo true || echo false)"
validate "Get snippet tool implemented" "$(grep -q "get_snippet" src/mcp_server/function_app.py && echo true || echo false)"
validate "Save snippet tool implemented" "$(grep -q "save_snippet" src/mcp_server/function_app.py && echo true || echo false)"
validate "Blob storage integration present" "$(grep -q "blob" src/mcp_server/function_app.py && echo true || echo false)"
validate "MCP host.json uses experimental bundle" "$(grep -q "Experimental" src/mcp_server/host.json && echo true || echo false)"
validate "Function requirements.txt exists" "$([ -f "src/mcp_server/requirements.txt" ] && echo true || echo false)"

echo -e "\n${BLUE}🤖 Agent Client Component Validation${NC}"
echo "------------------------------------"

# Check agent client implementation
validate "Agent main.py exists" "$([ -f "src/agent/main.py" ] && echo true || echo false)"
validate "Uses Azure AI Projects SDK" "$(grep -q "azure.ai.projects" src/agent/main.py && echo true || echo false)"
validate "Uses DefaultAzureCredential" "$(grep -q "DefaultAzureCredential" src/agent/main.py && echo true || echo false)"
validate "Creates agents with MCP tools" "$(grep -q "create_agent" src/agent/main.py && echo true || echo false)"
validate "Has MCP server configuration" "$(grep -q "mcp" src/agent/main.py && echo true || echo false)"
validate "Agent requirements.txt exists" "$([ -f "src/agent/requirements.txt" ] && echo true || echo false)"
validate "Environment example file exists" "$([ -f "src/agent/.env.example" ] && echo true || echo false)"

echo -e "\n${BLUE}🏗️ Infrastructure Component Validation${NC}"
echo "--------------------------------------"

# Check infrastructure components
validate "Main Bicep template exists" "$([ -f "infra/main.bicep" ] && echo true || echo false)"
validate "API Bicep module exists" "$([ -f "infra/app/api.bicep" ] && echo true || echo false)"
validate "AI project modules exist" "$([ -f "infra/agent/standard-ai-project.bicep" ] && echo true || echo false)"
validate "Infrastructure has AI Foundry integration" "$(grep -q "ai.*project" infra/main.bicep && echo true || echo false)"
validate "Function app uses Flex Consumption" "$(grep -q "FlexConsumption" infra/app/api.bicep && echo true || echo false)"
validate "Managed identity configured" "$(grep -q "managedIdentities" infra/app/api.bicep && echo true || echo false)"
validate "Azure YAML configuration exists" "$([ -f "azure.yaml" ] && echo true || echo false)"

echo -e "\n${BLUE}📖 Documentation Validation${NC}"
echo "----------------------------"

# Check documentation
validate "Main README exists" "$([ -f "README.md" ] && echo true || echo false)"
validate "AI Foundry integration docs exist" "$([ -f "AI-FOUNDRY-INTEGRATION.md" ] && echo true || echo false)"
validate "Porting prompt created" "$([ -f ".github/prompts/PORTING_PROMPT.md" ] && echo true || echo false)"
validate "Multi-language README created" "$([ -f ".github/prompts/MULTI_LANGUAGE_README.md" ] && echo true || echo false)"
validate "README mentions other language versions" "$(grep -q "\.NET\|JavaScript\|TypeScript" README.md && echo true || echo false)"

echo -e "\n${BLUE}🔧 Development Environment Validation${NC}"
echo "-------------------------------------"

# Check development setup
validate "VS Code launch configuration exists" "$([ -f ".vscode/launch.json" ] && echo true || echo false)"
validate "Python project configuration exists" "$([ -f "pyproject.toml" ] && echo true || echo false)"
validate "Gitignore file exists" "$([ -f ".gitignore" ] && echo true || echo false)"

echo -e "\n${BLUE}🔍 Environment Variables Validation${NC}"
echo "-----------------------------------"

# Check environment variable structure
REQUIRED_VARS=("PROJECT_ENDPOINT" "MODEL_DEPLOYMENT_NAME" "MCP_SERVER_LABEL" "MCP_SERVER_URL" "USER_MESSAGE" "MCP_EXTENSION_KEY")

for var in "${REQUIRED_VARS[@]}"; do
    validate "Environment variable $var documented" "$(grep -q "$var" src/agent/.env.example && echo true || echo false)"
done

echo -e "\n${BLUE}📝 Porting Documentation Validation${NC}"
echo "-----------------------------------"

# Check porting documentation completeness
validate "Porting prompt has .NET section" "$(grep -q "\.NET/C#" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"
validate "Porting prompt has JavaScript section" "$(grep -q "JavaScript/TypeScript" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"
validate "Architecture overview documented" "$(grep -q "Architecture Overview" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"
validate "Infrastructure porting guidance included" "$(grep -q "Infrastructure Updates" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"
validate "Development experience documented" "$(grep -q "Development Experience" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"
validate "Quality assurance requirements specified" "$(grep -q "Quality Assurance" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"

echo -e "\n${BLUE}🔗 Repository Link Validation${NC}"
echo "-----------------------------"

# Check target repository links
validate "Mentions .NET target repository" "$(grep -q "foundry-agent-service-remote-mcp-dotnet" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"
validate "Mentions JavaScript target repository" "$(grep -q "foundry-agent-service-remote-mcp-javascript" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"

echo -e "\n${BLUE}📊 Implementation Detail Validation${NC}"
echo "-----------------------------------"

# Check implementation details are captured
validate "Blob path pattern documented" "$(grep -q "snippets/{mcptoolargs" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"
validate "Tool properties pattern documented" "$(grep -q "toolProperties" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"
validate "Authentication patterns documented" "$(grep -q "DefaultAzureCredential\|managed.identity" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"
validate "Runtime versions specified" "$(grep -q "version.*8\.0\|version.*20" .github/prompts/PORTING_PROMPT.md && echo true || echo false)"

echo -e "\n================================================================"
echo -e "${BLUE}📋 Validation Summary${NC}"
echo "================================================================"

TOTAL_TESTS=$((PASS_COUNT + FAIL_COUNT))

if [ $FAIL_COUNT -eq 0 ]; then
    echo -e "${GREEN}🎉 ALL VALIDATIONS PASSED${NC}"
    echo -e "   ✅ Total Tests: $TOTAL_TESTS"
    echo -e "   ✅ Passed: $PASS_COUNT"
    echo -e "   ❌ Failed: $FAIL_COUNT"
    echo -e "\n${GREEN}✨ The Python reference implementation is ready for multi-language porting!${NC}"
    exit 0
else
    echo -e "${RED}⚠️  SOME VALIDATIONS FAILED${NC}"
    echo -e "   📊 Total Tests: $TOTAL_TESTS"
    echo -e "   ✅ Passed: $PASS_COUNT"
    echo -e "   ❌ Failed: $FAIL_COUNT"
    echo -e "\n${YELLOW}🔧 Please address the failed validations before proceeding with porting.${NC}"
    exit 1
fi