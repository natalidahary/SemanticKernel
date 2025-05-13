using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// using Microsoft.KernelMemory;
// using Microsoft.KernelMemory.Configuration;
// using Microsoft.KernelMemory.SemanticKernelPlugin;
using Microsoft.SemanticKernel;
using Plugins;
using Microsoft.Extensions.Logging; // Needed for LogLevel


namespace SemanticKernel.Config;

#pragma warning disable SKEXP0010
public static class KernelConfigurator
{
    public static Kernel Configure(AppConfiguration config)
    {

        //Approach 1 – using SK Vector Store, read documentation

        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.ModelName, config.Endpoint, config.ApiKey)
            .AddAzureOpenAITextEmbeddingGeneration(config.EmbeddingModel, config.Endpoint, config.ApiKey)
            .AddInMemoryVectorStore();

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        // //Approach 2 - use Kernel Memory
        // var builder = Kernel.CreateBuilder()
        //         .AddAzureOpenAIChatCompletion(config.ModelName, config.Endpoint, config.ApiKey);

        // var memory = new KernelMemoryBuilder()
        //     .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig
        //     {
        //         APIKey = config.ApiKey,
        //         Endpoint = config.Endpoint,
        //         Deployment = config.EmbeddingModel,
        //         Auth = AzureOpenAIConfig.AuthTypes.APIKey
        //     })
        //     .WithAzureOpenAITextGeneration(new AzureOpenAIConfig
        //     {
        //         APIKey = config.ApiKey,
        //         Endpoint = config.Endpoint,
        //         Deployment = config.ModelName,
        //         Auth = AzureOpenAIConfig.AuthTypes.APIKey
        //     })
        //     .WithCustomTextPartitioningOptions(new TextPartitioningOptions
        //     {
        //         MaxTokensPerParagraph = 600,
        //         OverlappingTokens = 50
        //     })
        //     .Build<MemoryServerless>();

        //     builder.Plugins.AddFromType<MemoryPlugin>();
        //     builder.Services.AddSingleton<IKernelMemory>(memory);

        //     // Add KernelMemoryPlugin
        //     builder.Plugins.AddFromObject(new KernelMemoryPlugin(memory), "KernelMemoryPlugin");

        builder.Plugins.AddFromObject(new GitPlugin(config.RepoPath), "GitPlugin");
            builder.Plugins.AddFromPromptDirectory("Plugins/ReleaseNotesPlugin");
            builder.Plugins.AddFromPromptDirectory("Plugins/CommitExplainer");


        return builder.Build();
    }
}
