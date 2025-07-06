using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using McpServer.Models;
using static McpServer.Models.ToolsInformation;

namespace McpServer.Functions;

public class McpFunctions
{
    private readonly ILogger<McpFunctions> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    
    // Constants matching the Python implementation
    private const string BlobContainerName = "snippets";
    private const string BlobPath = "snippets/{mcptoolargs." + SnippetNamePropertyName + "}.json";

    public McpFunctions(ILogger<McpFunctions> logger, BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    [Function(nameof(HelloMcp))]
    public string HelloMcp(
        [McpToolTrigger(HelloMcpToolName, HelloMcpToolDescription)] ToolInvocationContext context)
    {
        _logger.LogInformation("hello_mcp function executed");
        return "Hello I am MCPTool!";
    }

    [Function(nameof(GetSnippet))]
    public async Task<string> GetSnippet(
        [McpToolTrigger(GetSnippetToolName, GetSnippetToolDescription)] ToolInvocationContext context,
        [McpToolProperty(SnippetNamePropertyName, PropertyType, SnippetNamePropertyDescription)] string snippetName,
        [BlobInput(BlobPath)] string? snippetContent)
    {
        try
        {
            if (string.IsNullOrEmpty(snippetName))
            {
                return "No snippet name provided";
            }

            // If blob binding didn't find content, try direct access
            if (string.IsNullOrEmpty(snippetContent))
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainerName);
                var blobClient = containerClient.GetBlobClient($"{snippetName}.json");

                if (await blobClient.ExistsAsync())
                {
                    var downloadResult = await blobClient.DownloadContentAsync();
                    snippetContent = downloadResult.Value.Content.ToString();
                }
                else
                {
                    snippetContent = "Snippet not found";
                }
            }

            _logger.LogInformation("Retrieved snippet: {SnippetContent}", snippetContent);
            return snippetContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving snippet");
            return "Error retrieving snippet";
        }
    }

    [Function(nameof(SaveSnippet))]
    [BlobOutput(BlobPath)]
    public string SaveSnippet(
        [McpToolTrigger(SaveSnippetToolName, SaveSnippetToolDescription)] ToolInvocationContext context,
        [McpToolProperty(SnippetNamePropertyName, PropertyType, SnippetNamePropertyDescription)] string snippetName,
        [McpToolProperty(SnippetPropertyName, PropertyType, SnippetPropertyDescription)] string snippet)
    {
        try
        {
            if (string.IsNullOrEmpty(snippetName))
            {
                return "No snippet name provided";
            }

            if (string.IsNullOrEmpty(snippet))
            {
                return "No snippet content provided";
            }

            // The BlobOutput attribute will handle the actual saving
            _logger.LogInformation("Saved snippet: {Snippet}", snippet);
            return $"Snippet '{snippet}' saved successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving snippet");
            return "Error saving snippet";
        }
    }
}