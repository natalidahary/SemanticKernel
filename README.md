# SemanticKernel
A developer-friendly AI CLI tool powered by Microsoft Semantic Kernel and Azure OpenAI, built to streamline Git workflows with natural language. Easily generate release notes, explain commits in plain English, and manage repositories with intelligent command support.

## Features:
- Explain commits in clear, simple language
- Auto-generate release notes grouped by intent
- Git utilities: pull, commit, diff, and keyword search
- Write to disk: RELEASE_NOTES.md, VERSION, EXPLAIN.md
- Conversational AI with a customizable system prompt
- Plugin-driven architecture for easy extension

## Command Description:
- !help	Show available commands
- !commits	Show the latest Git commits
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
  "SystemPrompt": "You are an AI assistant that helps developers work with Git. Be concise, helpful, and clear."
}
- Run the application - dotnet run

