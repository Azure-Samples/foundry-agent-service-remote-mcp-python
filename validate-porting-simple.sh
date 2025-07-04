#!/bin/bash

# Simplified Porting Validation Script
# This script validates that all components required for successful porting are present

echo "🔍 Validating Python Reference Implementation for Multi-Language Porting"
echo "================================================================"

# Color codes for output
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counter for validation results
PASS_COUNT=0
FAIL_COUNT=0

# Function to report validation results
validate() {
    local test_name="$1"
    local test_command="$2"
    
    if eval "$test_command" &>/dev/null; then
        echo -e "${GREEN}✅ PASS${NC}: $test_name"
        ((PASS_COUNT++))
    else
        echo -e "${RED}❌ FAIL${NC}: $test_name"
        ((FAIL_COUNT++))
    fi
}

echo -e "${BLUE}📋 Repository Structure Validation${NC}"
echo "-----------------------------------"

validate "MCP Server source directory exists" "[ -d 'src/mcp_server' ]"
validate "Agent client source directory exists" "[ -d 'src/agent' ]"
validate "Infrastructure directory exists" "[ -d 'infra' ]"
validate "AI Foundry agent modules exist" "[ -d 'infra/agent' ]"

echo -e "\n${BLUE}📦 MCP Server Component Validation${NC}"
echo "-----------------------------------"

validate "Function app implementation exists" "[ -f 'src/mcp_server/function_app.py' ]"
validate "Function app has MCP tool triggers" "grep -q 'mcpToolTrigger' src/mcp_server/function_app.py"
validate "Hello MCP tool implemented" "grep -q 'hello_mcp' src/mcp_server/function_app.py"
validate "Get snippet tool implemented" "grep -q 'get_snippet' src/mcp_server/function_app.py"
validate "Save snippet tool implemented" "grep -q 'save_snippet' src/mcp_server/function_app.py"
validate "MCP host.json uses experimental bundle" "grep -q 'Experimental' src/mcp_server/host.json"
validate "Function requirements.txt exists" "[ -f 'src/mcp_server/requirements.txt' ]"

echo -e "\n${BLUE}🤖 Agent Client Component Validation${NC}"
echo "------------------------------------"

validate "Agent main.py exists" "[ -f 'src/agent/main.py' ]"
validate "Uses Azure AI Projects SDK" "grep -q 'azure.ai.projects' src/agent/main.py"
validate "Uses DefaultAzureCredential" "grep -q 'DefaultAzureCredential' src/agent/main.py"
validate "Creates agents with MCP tools" "grep -q 'create_agent' src/agent/main.py"
validate "Agent requirements.txt exists" "[ -f 'src/agent/requirements.txt' ]"
validate "Environment example file exists" "[ -f 'src/agent/.env.example' ]"

echo -e "\n${BLUE}🏗️ Infrastructure Component Validation${NC}"
echo "--------------------------------------"

validate "Main Bicep template exists" "[ -f 'infra/main.bicep' ]"
validate "API Bicep module exists" "[ -f 'infra/app/api.bicep' ]"
validate "AI project modules exist" "[ -f 'infra/agent/standard-ai-project.bicep' ]"
validate "Infrastructure has AI integration" "grep -q 'ai.*project\|aiproject\|aiProject' infra/main.bicep"
validate "Function app uses Flex Consumption" "grep -qi 'flex.*consumption' infra/app/api.bicep"
validate "Azure YAML configuration exists" "[ -f 'azure.yaml' ]"

echo -e "\n${BLUE}📖 Documentation Validation${NC}"
echo "----------------------------"

validate "Main README exists" "[ -f 'README.md' ]"
validate "AI Foundry integration docs exist" "[ -f 'AI-FOUNDRY-INTEGRATION.md' ]"
validate "Porting prompt created" "[ -f '.github/prompts/PORTING_PROMPT.md' ]"
validate "Multi-language README created" "[ -f '.github/prompts/MULTI_LANGUAGE_README.md' ]"

echo -e "\n${BLUE}🔍 Environment Variables Validation${NC}"
echo "-----------------------------------"

validate "PROJECT_ENDPOINT documented" "grep -q 'PROJECT_ENDPOINT' src/agent/.env.example"
validate "MODEL_DEPLOYMENT_NAME documented" "grep -q 'MODEL_DEPLOYMENT_NAME' src/agent/.env.example"
validate "MCP_SERVER_URL documented" "grep -q 'MCP_SERVER_URL' src/agent/.env.example"
validate "MCP_EXTENSION_KEY documented" "grep -q 'MCP_EXTENSION_KEY' src/agent/.env.example"

echo -e "\n${BLUE}📝 Porting Documentation Validation${NC}"
echo "-----------------------------------"

validate "Porting prompt has .NET section" "grep -q '\.NET\|dotnet' .github/prompts/PORTING_PROMPT.md"
validate "Porting prompt has JavaScript section" "grep -q 'JavaScript\|TypeScript' .github/prompts/PORTING_PROMPT.md"
validate "Architecture overview documented" "grep -q 'Architecture' .github/prompts/PORTING_PROMPT.md"
validate "Infrastructure guidance included" "grep -q 'Infrastructure' .github/prompts/PORTING_PROMPT.md"

echo -e "\n================================================================"
echo -e "${BLUE}📋 Validation Summary${NC}"
echo "================================================================"

TOTAL_TESTS=$((PASS_COUNT + FAIL_COUNT))

echo "   📊 Total Tests: $TOTAL_TESTS"
echo "   ✅ Passed: $PASS_COUNT"
echo "   ❌ Failed: $FAIL_COUNT"

if [ $FAIL_COUNT -eq 0 ]; then
    echo -e "\n${GREEN}🎉 ALL VALIDATIONS PASSED${NC}"
    echo -e "${GREEN}✨ The Python reference implementation is ready for multi-language porting!${NC}"
    exit 0
else
    echo -e "\n${RED}⚠️  Some validations failed. Please address the issues above.${NC}"
    exit 1
fi