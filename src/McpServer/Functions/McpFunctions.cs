using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using McpServer.Models;

namespace McpServer.Functions;

public class McpFunctions
{
    private readonly ILogger<McpFunctions> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    
    // Constants matching the Python implementation
    private const string SnippetNamePropertyName = "snippetname";
    private const string SnippetPropertyName = "snippet";
    private const string BlobContainerName = "snippets";

    public McpFunctions(ILogger<McpFunctions> logger, BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    [Function("hello_mcp")]
    public HttpResponseData HelloMcp([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("hello_mcp function executed");
        
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        
        var result = new { content = "Hello I am MCPTool!" };
        response.WriteString(JsonSerializer.Serialize(result));
        
        return response;
    }

    [Function("get_snippet")]
    public async Task<HttpResponseData> GetSnippet([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var content = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
            
            if (!content.TryGetValue("arguments", out var argumentsObj) ||
                argumentsObj is not JsonElement argumentsElement ||
                !argumentsElement.TryGetProperty(SnippetNamePropertyName, out var snippetNameElement))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.WriteString(JsonSerializer.Serialize(new { content = "No snippet name provided" }));
                return errorResponse;
            }

            var snippetName = snippetNameElement.GetString();
            if (string.IsNullOrEmpty(snippetName))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.WriteString(JsonSerializer.Serialize(new { content = "No snippet name provided" }));
                return errorResponse;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainerName);
            var blobClient = containerClient.GetBlobClient($"{snippetName}.json");

            string snippetContent = "Snippet not found";
            if (await blobClient.ExistsAsync())
            {
                var downloadResult = await blobClient.DownloadContentAsync();
                snippetContent = downloadResult.Value.Content.ToString();
            }

            _logger.LogInformation($"Retrieved snippet: {snippetContent}");
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            var result = new { content = snippetContent };
            response.WriteString(JsonSerializer.Serialize(result));
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving snippet");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.WriteString(JsonSerializer.Serialize(new { content = "Error retrieving snippet" }));
            return errorResponse;
        }
    }

    [Function("save_snippet")]
    public async Task<HttpResponseData> SaveSnippet([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var content = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
            
            if (!content.TryGetValue("arguments", out var argumentsObj) ||
                argumentsObj is not JsonElement argumentsElement)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.WriteString(JsonSerializer.Serialize(new { content = "No arguments provided" }));
                return errorResponse;
            }

            if (!argumentsElement.TryGetProperty(SnippetNamePropertyName, out var snippetNameElement) ||
                !argumentsElement.TryGetProperty(SnippetPropertyName, out var snippetContentElement))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.WriteString(JsonSerializer.Serialize(new { content = "Missing required arguments" }));
                return errorResponse;
            }

            var snippetName = snippetNameElement.GetString();
            var snippetContentStr = snippetContentElement.GetString();

            if (string.IsNullOrEmpty(snippetName))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.WriteString(JsonSerializer.Serialize(new { content = "No snippet name provided" }));
                return errorResponse;
            }

            if (string.IsNullOrEmpty(snippetContentStr))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.WriteString(JsonSerializer.Serialize(new { content = "No snippet content provided" }));
                return errorResponse;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainerName);
            await containerClient.CreateIfNotExistsAsync();
            
            var blobClient = containerClient.GetBlobClient($"{snippetName}.json");
            await blobClient.UploadAsync(BinaryData.FromString(snippetContentStr), overwrite: true);

            _logger.LogInformation($"Saved snippet: {snippetContentStr}");
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            var result = new { content = $"Snippet '{snippetContentStr}' saved successfully" };
            response.WriteString(JsonSerializer.Serialize(result));
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving snippet");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.WriteString(JsonSerializer.Serialize(new { content = "Error saving snippet" }));
            return errorResponse;
        }
    }
}