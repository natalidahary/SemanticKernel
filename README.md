# SemanticKernel Dev Assistant
A developer-focused AI CLI powered by Microsoft Semantic Kernel and Azure OpenAI. This tool lets you interact with your Git repository and documentation using natural language—making commit explanation, release note generation, and codebase Q&A intuitive and automated.

## Features:
- Explain commits in clear, simple language
- Auto-generate release notes grouped by intent
- Git utilities: pull, commit, diff, and keyword search
- Write to disk: RELEASE_NOTES.md, VERSION, EXPLAIN.md
- Conversational AI with a customizable system prompt
- Plugin-driven architecture for easy extension
- Documentation QA – index .txt files and ask questions about their content

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
- !indexcode	Index .txt files in your codebase for question-answering
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
[Memory] Indexed 145 code chunks from 5 files.

Me > !askcode what is the SystemPrompt ?
Top Results:
- /Users/natalidahary/Desktop/SemanticKernel/bin/Debug/net9.0/System.ClientModel.dll
- /Users/natalidahary/Desktop/SemanticKernel/SemanticKernel/bin/Debug/net9.0/System.ClientModel.dll
- Answer the question using relevant code context.

Me > what is the SystemPrompt ?
info: Microsoft.SemanticKernel.Connectors.AzureOpenAI.AzureOpenAIChatCompletionService[0]
      Prompt tokens: 426. Completion tokens: 142. Total tokens: 568.
AI > SystemPrompt generally refers to a special instruction or message given to an AI model (like ChatGPT) that sets the tone, behavior, and constraints for how it should respond to user inputs. It acts as a set of guidelines or a scenario that the AI should follow when interacting with users.
For example, a SystemPrompt might instruct the AI to answer politely, act as a programming assistant, provide concise answers, or follow a specific format. It is not visible to end-users but is used by application developers or service providers to define and control the AI’s behavior in different contexts.
In summary:  
A SystemPrompt is an underlying directive or setup that guides how an AI assistant responds to user requests.

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
  "CodebasePath": "/absolute/path/to/txt",
  "EmbeddingModel": "text-embedding-3-large"
}
- Run the application - dotnet run

