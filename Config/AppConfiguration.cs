using Microsoft.Extensions.Configuration;

namespace SemanticKernel.Config;

/// <summary>
/// Loads and provides access to configuration values from appsettings.json.
/// These values include OpenAI API credentials, repository path, and system prompt settings.
/// </summary>
public class AppConfiguration
{
    private readonly IConfigurationRoot _configuration;

    public AppConfiguration()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    }

    public string ModelName => _configuration["ModelName"]
        ?? throw new ApplicationException("ModelName not found");

    public string Endpoint => _configuration["Endpoint"]
        ?? throw new ApplicationException("Endpoint not found");

    public string ApiKey => _configuration["ApiKey"]
        ?? throw new ApplicationException("ApiKey not found");

    public string RepoPath => _configuration["RepoPath"]
        ?? throw new Exception("RepoPath not configured");

    public string? SystemPrompt =>
    File.Exists("SystemPrompt.txt") ? File.ReadAllText("SystemPrompt.txt") : null;

    public string EmbeddingModel => _configuration["EmbeddingModel"]
    ?? throw new ApplicationException("EmbeddingModel not found");

    public string CodebasePath => _configuration["CodebasePath"]
    ?? throw new Exception("CodebasePath not configured");

}
