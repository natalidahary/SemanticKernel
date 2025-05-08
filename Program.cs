using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Plugins;
using SemanticKernel.Config;
using SemanticKernel.Utils;
using SemanticKernel.Constants;

// Load configuration values from appsettings.json
var config = new AppConfiguration();

// Create and configure the Semantic Kernel with Azure OpenAI model
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(config.ModelName, config.Endpoint, config.ApiKey);

// Create GitPlugin with the configured repository path
var gitPlugin = new GitPlugin(config.RepoPath);

// Register custom plugins (functions and prompt templates)
builder.Plugins.AddFromObject(gitPlugin, "GitPlugin");
builder.Plugins.AddFromPromptDirectory("Plugins/ReleaseNotesPlugin");
builder.Plugins.AddFromPromptDirectory("Plugins/CommitExplainer");

// Build the kernel which manages plugins and LLM communication
var kernel = builder.Build();

HelperFunctions.PrintLoadedPlugins(kernel);

// Retrieve the chat service for free-text AI conversations
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Define how the AI should behave (auto function selection, etc.)
AzureOpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Maintain conversation history (user + AI messages)
var chatHistory = new ChatHistory();

// Load optional system prompt if configured (helps steer AI responses)
var systemPrompt = config.SystemPrompt;
if (!string.IsNullOrWhiteSpace(systemPrompt))
{
    chatHistory.AddSystemMessage(systemPrompt);
}

HelperFunctions.ShowMenu();

while (true)
{
    // Prompt user for input
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Me > ");
    Console.ResetColor();

    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput)) continue;
    if (userInput == Commands.Exit) break;

    if (userInput == Commands.Help)
    {
        HelperFunctions.ShowMenu();
        continue;
    }

    //Handle plugin commands
    if (userInput.StartsWith(Commands.Commits))
    {
        await GitPluginHandler.ShowCommitsAsync(kernel);
        continue;
    }


    if (userInput.StartsWith(Commands.SetRepo + " "))
    {
        var path = userInput.Substring(Commands.SetRepo.Length).Trim();
        await GitPluginHandler.SetRepoPathAsync(kernel, path);
        continue;
    }

    if (userInput.StartsWith(Commands.ReleaseNotes))
    {
        var parts = userInput.Split(' ');
        int count = (parts.Length > 1 && int.TryParse(parts[1], out var parsed)) ? parsed : 10;

        await ReleaseNotesHandler.GenerateAndSaveReleaseNotesAsync(kernel, count);
        continue;
    }

    if (userInput.StartsWith(Commands.Explain + " "))
    {
        var commitText = userInput.Substring(Commands.Explain.Length).Trim('"');
        await CommitExplanationHandler.ExplainCommitAsync(kernel, commitText);
        continue;
    }


    if (userInput.StartsWith(Commands.Pull))
    {
        await GitPluginHandler.PullAsync(kernel);
        continue;
    }

    if (userInput.StartsWith(Commands.Commit))
    {
        var message = userInput.Substring(Commands.Commit.Length).Trim('"');
        await GitPluginHandler.CommitAsync(kernel, message);
        continue;
    }

    if (userInput.StartsWith(Commands.FindFixes))
    {
        await GitPluginHandler.FindFixesAsync(kernel);
        continue;
    }

    if (userInput.StartsWith(Commands.Diff))
    {
        var parts = userInput.Split(" ");
        if (parts.Length >= 3)
        {
            await GitPluginHandler.CompareCommitsAsync(kernel, parts[1], parts[2]);
        }
        else
        {
            Console.WriteLine($"Usage: {Commands.Diff} <sha1> <sha2>");
        }
        continue;
    }

    // If input is not a command, treat it as natural language and send to AI
    chatHistory.AddUserMessage(userInput);

    var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, openAiPromptExecutionSettings, kernel);
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine($"AI > {response.Content}");
    Console.ResetColor();

    if (!string.IsNullOrEmpty(response.Content))
    {
        chatHistory.AddAssistantMessage(response.Content);
    }

}