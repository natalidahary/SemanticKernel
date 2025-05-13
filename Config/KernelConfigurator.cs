using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Plugins;

namespace SemanticKernel.Config;

#pragma warning disable SKEXP0010
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