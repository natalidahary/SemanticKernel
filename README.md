# SemanticKernel Dev Assistant
A developer-focused AI CLI powered by Microsoft Semantic Kernel and Azure OpenAI. This tool lets you interact with your Git repository and documentation using natural language—making commit explanation, release note generation, and codebase Q&A intuitive and automated.

## Features:
- Explain commits in clear, simple language
- Auto-generate release notes grouped by intent
- Git utilities: pull, commit, diff, and keyword search
- Write to disk: RELEASE_NOTES.md, VERSION, EXPLAIN.md
- Conversational AI with a customizable system prompt
- Plugin-driven architecture for easy extension
- Documentation QA – index .cs files and ask questions about their content

## How Documentation Q&A Works
!indexcode indexes all .cs files under the CodebasePath directory using Azure OpenAI embeddings.
This populates an in-memory vector store with chunks of your code.
Then, you can use !askcode <your question> to retrieve relevant snippets and insights from your codebase.
- inMemory connector
  You used the Microsoft.SemanticKernel.Connectors.InMemory package.
- Vector Store ingest data
  Done through UploadToVectorStoreAsync() with proper embeddings.
- Text search plugin
  Implemented through semantic search using ITextEmbeddingGenerationService + vector store collection.

## Command Description:
- !help	Show available commands
- !commits	Show the latest Git commits
- !indexcode	Index .cs files in your codebase for question-answering
- !askcode <question>	Ask questions about the indexed documentation
- !setrepo <path>	Set the current Git repository
- !releasenotes [n]	Generate release notes from last n commits
- !explain "commit msg"	Explain a commit in simple English
- !pull	Pull latest changes from remote
- !commit "message"	Stage all & commit with message
- !findfixes	Show commits that include the word fix
- !diff <sha1> <sha2>	Compare two commits and show code differences
- exit	Quit the assistant

## Usage Examples:
### Interact naturally with the assistant using commands:

Me > !indexcode
[Info] Found 14 .cs files to index:
 - /Users/natalidahary/Desktop/SemanticKernel/Program.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Config/AppInitializer.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Config/KernelConfigurator.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Config/AppConfiguration.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Plugins/GitPlugin.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Constants/Commands.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Utils/CommitExplanationHandler.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Utils/AskCodeHandler.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Utils/GitPluginHandler.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Utils/HelperFunctions.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Utils/ReleaseNotesHandler.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Models/TextChunk.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Services/CodeMemoryService.cs
 - /Users/natalidahary/Desktop/SemanticKernel/Services/DocumentReader.cs
[Chunking] 1 chunks from Program.cs
[Chunking] 3 chunks from AppInitializer.cs
[Chunking] 3 chunks from KernelConfigurator.cs
....
[Memory] Indexed 86 code chunks from 14 files.

Me > !askcode List the async methods in GitPluginHandler.
[Debug] Code context:

public GitPlugin(string repoPath)
    {
        _repoPath = repoPath;
    }

    [KernelFunction, Description("Returns the latest Git commit messages from the current repository.")]
---
public static async Task ShowCommitsAsync(Kernel kernel)
    {
        var result = await kernel.InvokeAsync("GitPlugin", "GetLatestCommits", new KernelArguments
        {
            ["count"] = 5
        });

        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
    }
.....
info: Microsoft.SemanticKernel.KernelFunction[0]
      Function CodeMemoryPlugin-AskCodebase invoking.
info: Microsoft.SemanticKernel.Connectors.AzureOpenAI.AzureOpenAIChatCompletionService[0]
      Prompt tokens: 1377. Completion tokens: 97. Total tokens: 1474.
info: Microsoft.SemanticKernel.KernelFunction[0]
      Function CodeMemoryPlugin-AskCodebase succeeded.
info: Microsoft.SemanticKernel.KernelFunction[0]
      Function CodeMemoryPlugin-AskCodebase completed. Duration: 1.348157s
AI > Based on the code snippets provided, the async methods in GitPluginHandler are:
1. ShowCommitsAsync(Kernel kernel)
2. PullAsync(Kernel kernel)
3. CommitAsync(Kernel kernel, string message)
4. FindFixesAsync(Kernel kernel)
5. CompareCommitsAsync(Kernel kernel, string fromSha, string toSha)
6. SetRepoPathAsync(Kernel kernel, string path)
These are all marked as static async Task methods.

Me > !commits
[GitPlugin] - Fix typo in README (natalidahary, 2025-05-07)
            - Add controller for events (natalidahary, 2025-05-06)
            - Initial commit (natalidahary, 2025-05-05)

Me > !setrepo /Users/natalidahary/Desktop/Project
[GitPlugin] Repository path updated to: /Users/natalidahary/Desktop/Project

Me > !releasenotes 5
[DEBUG] Current version: 1.0.2 → New version: 1.0.3
v1.0.3 release notes written to RELEASE_NOTES.md

Me > !explain fix race condition in data loader
[CommitExplainer] This commit resolves a timing issue in the data loader that could lead to inconsistent results. It ensures operations are now safely synchronized.

Me > !pull
[GitPlugin] Pulled successfully: Fast-forward

Me > !commit "add unit tests for auth module"
[GitPlugin] Committed: 8f4c9d1 - add unit tests for auth module

Me > !findfixes
[GitPlugin] - fix null ref in login (natalidahary, 2025-05-04)
            - hotfix: prevent crash on bad config (natalidahary, 2025-05-03)

Me > !diff abc123 def456
[GitPlugin] UserService.cs: 12++ / 3--  
            Program.cs: 5++ / 0--

Me > !help
Available commands:
  !commits         - Show the latest Git commits
  !setrepo <path>  - Set the current Git repository path
  ...


## Prerequisites:
- .NET 9 SDK
- Azure OpenAI API key + model deployment
- A local Git repository (initialized and accessible)

## Output Files:
### File ->	Purpose
- RELEASE_NOTES.md -> Appended with new changelogs
- VERSION	 -> Tracks current version (semver patch)
- EXPLAIN.md	-> Appends explanations of commit messages

## Setup:
- Clone this repository - git clone https://github.com/yourname/SemanticKernel.git
- cd SemanticKernel
- Fill in appsettings.json
1. Copy the provided `appsettings.template.json` to `appsettings.json`.
2. Fill in your own OpenAI credentials and repo path.
3. This file is ignored by Git to keep secrets secure.
{
  "ModelName": "gpt-4",
  "Endpoint": "https://your-openai-endpoint.openai.azure.com/",
  "ApiKey": "your-azure-openai-key",
  "RepoPath": "/absolute/path/to/your/git/repo",
  "CodebasePath": "/absolute/path/to/cs",
  "EmbeddingModel": "text-embedding-3-large"
}
- Run the application - dotnet run

