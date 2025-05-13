using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Plugins;

namespace SemanticKernel.Config;

#pragma warning disable SKEXP0010
/// <summary>
/// Configures and constructs the Semantic Kernel, adds OpenAI models, logging, and plugins.
/// Adds Azure OpenAI chat model and embedding model to the kernel.
/// Uses in-memory vector store for semantic search.
/// Loads several custom plugins from folders and one (GitPlugin) from a class instance.
/// </summary>
public static class KernelConfigurator
{
    public static Kernel Configure(AppConfiguration config)
    {

        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.ModelName, config.Endpoint, config.ApiKey)
            .AddAzureOpenAITextEmbeddingGeneration(config.EmbeddingModel, config.Endpoint, config.ApiKey)
            .AddInMemoryVectorStore();

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        builder.Plugins.AddFromPromptDirectory("Plugins/CodeMemoryPlugin");
        builder.Plugins.AddFromObject(new GitPlugin(config.RepoPath), "GitPlugin");
        builder.Plugins.AddFromPromptDirectory("Plugins/ReleaseNotesPlugin");
        builder.Plugins.AddFromPromptDirectory("Plugins/CommitExplainer");

        return builder.Build();
    }
}