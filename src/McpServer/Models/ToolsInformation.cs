namespace McpServer.Models;

internal sealed class ToolsInformation
{
    // Tool names matching Python implementation
    public const string HelloMcpToolName = "hello_mcp";
    public const string HelloMcpToolDescription = "Hello world.";
    
    public const string SaveSnippetToolName = "save_snippet";
    public const string SaveSnippetToolDescription = "Save a snippet with a name.";
    
    public const string GetSnippetToolName = "get_snippet";
    public const string GetSnippetToolDescription = "Retrieve a snippet by name.";
    
    // Property names and descriptions matching Python implementation
    public const string SnippetNamePropertyName = "snippetname";
    public const string SnippetPropertyName = "snippet";
    public const string SnippetNamePropertyDescription = "The name of the snippet.";
    public const string SnippetPropertyDescription = "The content of the snippet.";
    public const string PropertyType = "string";
}