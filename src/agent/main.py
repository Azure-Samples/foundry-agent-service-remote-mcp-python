import json
import os
import time

from azure.ai.agents import AgentsClient
from azure.ai.agents.models import MessageTextContent, ListSortOrder
from azure.core.rest import HttpRequest
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

# Configuration constants
PROJECT_ENDPOINT = os.getenv("PROJECT_ENDPOINT", "https://your-agent-service-resource.services.ai.azure.com/api/projects/your-project-name")
MODEL_DEPLOYMENT_NAME = os.getenv("MODEL_DEPLOYMENT_NAME", "gpt-4.1-mini")
MCP_SERVER_LABEL = os.getenv("MCP_SERVER_LABEL", "Azure_Functions_MCP_Server")
MCP_SERVER_URL = os.getenv("MCP_SERVER_URL", "https://<your-funcappname>.azurewebsites.net/runtime/webhooks/mcp")
USER_MESSAGE = os.getenv("USER_MESSAGE", "Create a snippet called snippet1 that prints 'Hello, World!' in Python.")

# Required environment variables (no defaults)
MCP_EXTENSION_KEY = os.getenv("MCP_EXTENSION_KEY")
if not MCP_EXTENSION_KEY:
    raise ValueError("MCP_EXTENSION_KEY environment variable is required but not set")


agents_client = AgentsClient(
    endpoint=PROJECT_ENDPOINT,
    credential=DefaultAzureCredential()
)

with agents_client:
    agent = agents_client.create_agent(
        model=MODEL_DEPLOYMENT_NAME,
        name="my-mcp-agent",
        instructions="You are a helpful assistant. Use the tools provided to answer the user's questions. Be sure to cite your sources.",
        tools=[
            {
                "type": "mcp",
                "server_label": MCP_SERVER_LABEL,
                "server_url": MCP_SERVER_URL + "?code=" + MCP_EXTENSION_KEY,
                "allowed_tools": ["hello_mcp", "save_snippet", "get_snippet"]
            }
        ],
        tool_resources=None
    )

    # Create a Thread, Message and Run
    print(f"Created agent, agent ID: {agent.id}")

    thread = agents_client.threads.create()
    print(f"Created thread, thread ID: {thread.id}")

    message = agents_client.messages.create(
        thread_id=thread.id,
        role="user",
        content=USER_MESSAGE,
    )
    print(f"Created message, message ID: {message.id}")
    print(f"Message content: {message.content[-1].text.value if message.content else 'No content'}")

    run = agents_client.runs.create(
        thread_id=thread.id,
        agent_id=agent.id
    )

    # Poll the run, auto-approving any MCP tool calls
    while run.status in ["queued", "in_progress", "requires_action"]:
        time.sleep(1)
        run = agents_client.runs.get(
            thread_id=thread.id,
            run_id=run.id
        )
        print(f"Run status: {run.status}")

        if run.status == "requires_action":
            action = run.required_action
            if "submit_tool_approval" in action:
                tool_calls = action["submit_tool_approval"]["tool_calls"]
                approvals = [{"tool_call_id": tc["id"], "approve": True} for tc in tool_calls]
                request = HttpRequest(
                    method="POST",
                    url=f"/threads/{thread.id}/runs/{run.id}/submit_tool_outputs?api-version=2025-05-15-preview",
                    json={"tool_approvals": approvals}
                )
                agents_client.send_request(request)
                print(f"Auto-approved {len(approvals)} MCP tool call(s)")

    if run.status == "failed":
        print(f"Run error: {run.last_error}")

    run_steps = agents_client.run_steps.list(
        thread_id=thread.id,
        run_id=run.id
    )
    for step in run_steps:
        print(f"Run step: {step.id}, status: {step.status}, type: {step.type}")
        if step.type == "tool_calls":
            print("Tool call details:")
            for tool_call in step.step_details.tool_calls:
                print(json.dumps(tool_call.as_dict(), indent=2))

    messages = agents_client.messages.list(
        thread_id=thread.id,
        order=ListSortOrder.ASCENDING
    )
    for data_point in messages:
        last_message_content = data_point.content[-1]
        if isinstance(last_message_content, MessageTextContent):
            print(f"{data_point.role}: {last_message_content.text.value}")

    # Clean up the agent
    agents_client.delete_agent(agent.id)
    print(f"Deleted agent, agent ID: {agent.id}")
