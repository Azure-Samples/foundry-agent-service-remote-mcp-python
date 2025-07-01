#!/bin/bash

# Simple validation script to test bicep templates
echo "Validating bicep templates..."

# Test individual agent modules
echo "Testing agent bicep modules..."
for file in infra/agent/*.bicep; do
    echo "Validating $file..."
    if command -v az &> /dev/null; then
        az bicep build --file "$file" --stdout > /dev/null 2>&1
        if [ $? -eq 0 ]; then
            echo "✓ $file is valid"
        else
            echo "✗ $file has errors"
        fi
    else
        echo "Azure CLI not available, skipping validation"
        break
    fi
done

# Check for required outputs in main.bicep
echo ""
echo "Checking required outputs in main.bicep..."
required_outputs=("PROJECT_ENDPOINT" "MODEL_DEPLOYMENT_NAME" "MCP_SERVER_URL" "SERVICE_API_NAME")

for output in "${required_outputs[@]}"; do
    if grep -q "output $output" infra/main.bicep; then
        echo "✓ $output output found"
    else
        echo "✗ $output output missing"
    fi
done

# Check for required parameters
echo ""
echo "Checking required parameters in main.bicep..."
required_params=("aiProjectName" "aiServicesName" "modelName" "modelFormat" "modelVersion")

for param in "${required_params[@]}"; do
    if grep -q "param $param" infra/main.bicep; then
        echo "✓ $param parameter found"
    else
        echo "✗ $param parameter missing"
    fi
done

# Check agent folder structure
echo ""
echo "Checking agent folder structure..."
agent_files=("standard-ai-project.bicep" "standard-dependent-resources.bicep" "standard-ai-project-capability-host.bicep" "standard-ai-project-role-assignments.bicep" "post-capability-host-role-assignments.bicep")

for file in "${agent_files[@]}"; do
    if [ -f "infra/agent/$file" ]; then
        echo "✓ infra/agent/$file exists"
    else
        echo "✗ infra/agent/$file missing"
    fi
done

echo ""
echo "Validation complete!"