using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Plugins;
using SemanticKernel.Config;
using SemanticKernel.Utils;
using SemanticKernel.Constants;


var config = new AppConfiguration();

// Create and configure the kernel
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(config.ModelName, config.Endpoint, config.ApiKey);

var gitPlugin = new GitPlugin(config.RepoPath);

// Register the plugin
builder.Plugins.AddFromObject(gitPlugin, "GitPlugin");
builder.Plugins.AddFromPromptDirectory("Plugins/ReleaseNotesPlugin");
builder.Plugins.AddFromPromptDirectory("Plugins/CommitExplainer");

var kernel = builder.Build();


HelperFunctions.PrintLoadedPlugins(kernel);

//retrieves the chat service from the kernel, which will actually send/receive AI responses
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

//tell Semantic Kernel how to choose functions if any are defined
AzureOpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var chatHistory = new ChatHistory();

var systemPrompt = config.SystemPrompt;
if (!string.IsNullOrWhiteSpace(systemPrompt))
{
    chatHistory.AddSystemMessage(systemPrompt);
}

HelperFunctions.ShowMenu();

while (true)
{
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

    //Send to LLM as a chat message
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




// do
// {
//     Console.ForegroundColor = ConsoleColor.Cyan;
//     //Reads your message from the terminal
//     Console.Write("Me > ");
//     Console.ResetColor();

//     var userInput = Console.ReadLine();
//     if (userInput == "exit")
//     {
//         break;
//     }

//     //Adds your message to the ongoing conversation history
//     history.AddUserMessage(userInput!);

//     //sends your entire chat history to the AI and begins streaming the response in real time.
//     var streamingResponse =
//         chatCompletionService.GetStreamingChatMessageContentsAsync(
//             history,
//             openAiPromptExecutionSettings,
//             kernel);

//     Console.ForegroundColor = ConsoleColor.Green;
//     Console.Write("Agent > ");
//     Console.ResetColor();

//     var fullResponse = "";
//     //prints each chunk immediately to the terminal
//     await foreach (var chunk in streamingResponse)
//     {
//         Console.ForegroundColor = ConsoleColor.Yellow;
//         Console.Write(chunk.Content);
//         Console.ResetColor();
//         fullResponse += chunk.Content;
//     }
//     Console.WriteLine();

//     //Adds the AI's full response to the chat history so it has memory for the next message
//     history.AddMessage(AuthorRole.Assistant, fullResponse);


// } while (true);


// AzureOpenAIPromptExecutionSettings settings = new()
// {
//     MaxTokens = 100,
//     Temperature = 0.7
// };

// var singleTurnHistory = new ChatHistory();
// singleTurnHistory.AddUserMessage("Write a funny joke");

// var response = await chatCompletionService.GetChatMessageContentAsync(
//     singleTurnHistory,
//     settings,
//     kernel
// );

// Console.WriteLine("AI > " + response.Content);
