using LibGit2Sharp;
using Microsoft.SemanticKernel;
using System.Text;

namespace Plugins;

/// <summary>
/// Provides Git-related functionality for use within Semantic Kernel plugins.
/// Allows querying commit history, pulling, committing changes, and comparing commits.
/// </summary>
public class GitPlugin
{
    private string _repoPath;

    /// <summary>
    /// Initializes the plugin with a path to a Git repository.
    /// </summary>
    /// <param name="repoPath">Path to the local Git repository.</param>
    public GitPlugin(string repoPath)
    {
        _repoPath = repoPath;
    }

    /// <summary>
    /// Returns the most recent Git commits from the current repository.
    /// </summary>
    /// <param name="count">Number of recent commits to return (default: 5).</param>
    /// <returns>Formatted string of commit messages with author and date.</returns>
    [KernelFunction]
    public string GetLatestCommits(int count = 5)
    {
        if (!Repository.IsValid(_repoPath))
        {
            return $"Invalid Git repository path: {_repoPath}";
        }

        var sb = new StringBuilder();

        using var repo = new Repository(_repoPath);
        var commits = repo.Commits.Take(count);

        foreach (var commit in commits)
        {
            sb.AppendLine($"- {commit.MessageShort} ({commit.Author.Name}, {commit.Author.When.LocalDateTime})");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Updates the internal Git repository path used by the plugin.
    /// </summary>
    /// <param name="newPath">New path to a Git repository.</param>
    /// <returns>Status message indicating success or failure.</returns>
    [KernelFunction]
    public string SetRepositoryPath(string newPath)
    {
        if (!Directory.Exists(newPath))
        {
            return $"Directory does not exist: {newPath}";
        }

        _repoPath = newPath;
        return $"Repository path updated to: {_repoPath}";
    }

    /// <summary>
    /// Performs a Git pull from the origin remote to update the local repository.
    /// </summary>
    /// <returns>Status message with result of the pull operation.</returns>
    [KernelFunction]
    public string PullRepository()
    {
        using var repo = new Repository(_repoPath);
        var remote = repo.Network.Remotes["origin"];
        var options = new PullOptions();
        var signature = new Signature("AI Bot", "ai@localhost", DateTimeOffset.Now);

        try
        {
            var result = Commands.Pull(repo, signature, options);
            return $"Pulled successfully: {result.Status}";
        }
        catch (Exception ex)
        {
            return $"Pull failed: {ex.Message}";
        }
    }

    /// <summary>
    /// Stages all changes and commits them to the current repository.
    /// </summary>
    /// <param name="message">The commit message to use.</param>
    /// <returns>Summary of the created commit including SHA and message.</returns>
    [KernelFunction]
    public string CommitAllChanges(string message)
    {
        using var repo = new Repository(_repoPath);

        Commands.Stage(repo, "*");
        var author = new Signature("AI Bot", "ai@localhost", DateTimeOffset.Now);
        var commit = repo.Commit(message, author, author);

        return $"Committed: {commit.Sha.Substring(0, 7)} - {commit.MessageShort}";
    }

    /// <summary>
    /// Searches for commits containing a specific keyword in the message.
    /// </summary>
    /// <param name="keyword">Keyword to search for (case-insensitive).</param>
    /// <param name="count">Maximum number of matching commits to return.</param>
    /// <returns>Formatted list of matching commits or a not-found message.</returns>
    [KernelFunction]
    public string FindCommitsByKeyword(string keyword, int count = 10)
    {
        using var repo = new Repository(_repoPath);
        var matching = repo.Commits
            .Where(c => c.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .Take(count)
            .Select(c => $"- {c.MessageShort} ({c.Author.Name}, {c.Author.When.LocalDateTime})");

        return matching.Any()
            ? string.Join("\n", matching)
            : $"No commits found containing: {keyword}";
    }

    /// <summary>
    /// Compares two Git commits and shows a summary of added and deleted lines per file.
    /// </summary>
    /// <param name="fromSha">SHA of the base commit.</param>
    /// <param name="toSha">SHA of the target commit to compare against.</param>
    /// <returns>List of file changes with line additions and deletions, or a message if no changes.</returns>
    [KernelFunction]
    public string CompareCommits(string fromSha, string toSha)
    {
        using var repo = new Repository(_repoPath);

        var from = repo.Lookup<Commit>(fromSha);
        var to = repo.Lookup<Commit>(toSha);

        if (from == null || to == null)
            return "One or both SHAs not found.";

        var changes = repo.Diff.Compare<Patch>(from.Tree, to.Tree);
        return changes.Any()
            ? string.Join("\n", changes.Select(c => $"{c.Path}: {c.LinesAdded}++ / {c.LinesDeleted}--"))
            : "No changes between the specified commits.";
    }
}
