using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Azure;
using Azure.Identity;
using static McpServer.Models.ToolsInformation;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add Azure Blob Storage client
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true");
    clientBuilder.UseCredential(new DefaultAzureCredential());
});

// Enable MCP tool metadata
builder.EnableMcpToolMetadata();

// Configure MCP tools with properties matching Python implementation
builder
    .ConfigureMcpTool(GetSnippetToolName)
    .WithProperty(SnippetNamePropertyName, PropertyType, SnippetNamePropertyDescription);

builder
    .ConfigureMcpTool(SaveSnippetToolName)
    .WithProperty(SnippetNamePropertyName, PropertyType, SnippetNamePropertyDescription)
    .WithProperty(SnippetPropertyName, PropertyType, SnippetPropertyDescription);

builder.Build().Run();
