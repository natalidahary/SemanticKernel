
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Plugins;

// loads appsettings.json (if present) so you can access settings like the OpenAI key and endpoint
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var modelName = configuration["ModelName"] ?? throw new ApplicationException("ModelName not found");
var endpoint = configuration["Endpoint"] ?? throw new ApplicationException("Endpoint not found");
var apiKey = configuration["ApiKey"] ?? throw new ApplicationException("ApiKey not found");

//creates the Semantic Kernel instance and connects it to Azure OpenAI's chat completion API
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(modelName, endpoint, apiKey);

var repoPath = configuration["RepoPath"] ?? throw new Exception("RepoPath not configured");
var gitPlugin = new GitPlugin(repoPath);

// Register the plugin
builder.Plugins.AddFromObject(gitPlugin, "GitPlugin");
builder.Plugins.AddFromPromptDirectory("Plugins/ReleaseNotesPlugin");

var kernel = builder.Build();

foreach (var plugin in kernel.Plugins)
{
    Console.WriteLine($"[DEBUG] Loaded plugin: {plugin.Name}");
    foreach (var function in plugin)
    {
        Console.WriteLine($"         ↳ Function: {function.Name}");
    }
}

//retrieves the chat service from the kernel, which will actually send/receive AI responses
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// //tell Semantic Kernel how to choose functions if any are defined
AzureOpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var chatHistory = new ChatHistory();

var systemPrompt = configuration["SystemPrompt"];
if (!string.IsNullOrWhiteSpace(systemPrompt))
{
    chatHistory.AddSystemMessage(systemPrompt);
}

while (true)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Me > ");
    Console.ResetColor();

    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput)) continue;
    if (userInput == "exit") break;

    if (userInput == "!help")
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  !commits             - Show the latest Git commits");
        Console.WriteLine("  !setrepo <path>      - Set the current Git repository path");
        Console.WriteLine("  !releasenotes [n]    - Generate release notes from the last [n] commits (default 10)");
        Console.WriteLine("  !pull                - Pull latest changes from remote");
        Console.WriteLine("  !commit \"message\"   - Stage all and commit with given message");
        Console.WriteLine("  !findfixes           - Show commits containing the word 'fix'");
        Console.WriteLine("  !diff <sha1> <sha2>  - Compare two commits and list changes");
        Console.WriteLine("  !help                - Show this help message");
        Console.WriteLine("  exit                 - Quit the app");
        Console.WriteLine("  [anything else]      - Sent to AI via Azure OpenAI");
        continue;
    }

    //Handle plugin commands
    if (userInput.StartsWith("!commits"))
    {
        var result = await kernel.InvokeAsync("GitPlugin", "GetLatestCommits", new KernelArguments
        {
            ["count"] = 3
        });
        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
        continue;
    }

    if (userInput.StartsWith("!setrepo "))
    {
        var path = userInput.Substring("!setrepo ".Length);
        var result = await kernel.InvokeAsync("GitPlugin", "SetRepositoryPath", new KernelArguments
        {
            ["newPath"] = path
        });
        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
        continue;
    }

    if (userInput.StartsWith("!releasenotes"))
    {
        var parts = userInput.Split(' ');
        int count = (parts.Length > 1 && int.TryParse(parts[1], out var parsed)) ? parsed : 10;

        //Read current version
        var currentVersion = File.Exists("VERSION") ? File.ReadAllText("VERSION").Trim() : "0.0.0";
        var newVersion = IncrementPatchVersion(currentVersion);

        Console.WriteLine($"[DEBUG] Current version: {currentVersion} → New version: {newVersion}");

        //Get commits
        var commitsResult = await kernel.InvokeAsync("GitPlugin", "GetLatestCommits", new KernelArguments
        {
            ["count"] = count
        });

        var commits = commitsResult.GetValue<string>() ?? "";

        //Generate release notes
        var releaseResult = await kernel.InvokeAsync("ReleaseNotesPlugin", "GenerateReleaseNotes", new KernelArguments
        {
            ["commits"] = commits
        });

        var notes = releaseResult.GetValue<string>() ?? "[No release notes generated]";

        //Build full changelog entry
        var changelog = $"## v{newVersion} - {DateTime.Now:yyyy-MM-dd}\n\n{notes}\n";

        //Save release notes to file
        File.AppendAllText("RELEASE_NOTES.md", changelog);
        File.WriteAllText("VERSION", newVersion);

        Console.WriteLine($"v{newVersion} release notes written to RELEASE_NOTES.md");
        continue;
    }

    if (userInput.StartsWith("!pull"))
    {
        var result = await kernel.InvokeAsync("GitPlugin", "PullRepository");
        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
        continue;
    }

    if (userInput.StartsWith("!commit "))
    {
        var message = userInput.Substring("!commit ".Length).Trim('"');
        var result = await kernel.InvokeAsync("GitPlugin", "CommitAllChanges", new KernelArguments
        {
            ["message"] = message
        });
        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
        continue;
    }

    if (userInput.StartsWith("!findfixes"))
    {
        var result = await kernel.InvokeAsync("GitPlugin", "FindCommitsByKeyword", new KernelArguments
        {
            ["keyword"] = "fix"
        });
        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
        continue;
    }

    if (userInput.StartsWith("!diff "))
    {
        var parts = userInput.Split(" ");
        if (parts.Length >= 3)
        {
            var result = await kernel.InvokeAsync("GitPlugin", "CompareCommits", new KernelArguments
            {
                ["fromSha"] = parts[1],
                ["toSha"] = parts[2]
            });
            Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
        }
        else
        {
            Console.WriteLine("Usage: !diff <sha1> <sha2>");
        }
        continue;
    }

    //Send to LLM as a chat message
    chatHistory.AddUserMessage(userInput);

    var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, openAiPromptExecutionSettings, kernel);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"AI > {response.Content}");
    Console.ResetColor();

    if (!string.IsNullOrEmpty(response.Content))
    {
        chatHistory.AddAssistantMessage(response.Content);
    }

}

string IncrementPatchVersion(string currentVersion)
{
    var parts = currentVersion.Split('.');
    if (parts.Length != 3) return "0.0.1"; // fallback

    int major = int.Parse(parts[0]);
    int minor = int.Parse(parts[1]);
    int patch = int.Parse(parts[2]) + 1;

    return $"{major}.{minor}.{patch}";
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
