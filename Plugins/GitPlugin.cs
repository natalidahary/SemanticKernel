using LibGit2Sharp;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Plugins;

/// <summary>
/// Provides Git-related functionality for use within Semantic Kernel plugins.
/// Allows querying commit history, pulling, committing changes, and comparing commits.
/// </summary>
public class GitPlugin
{
    private string _repoPath;

    public GitPlugin(string repoPath)
    {
        _repoPath = repoPath;
    }

    [KernelFunction, Description("Returns the latest Git commit messages from the current repository.")]
    public string GetLatestCommits([Description("The number of recent commits to return. Default is 5.")] int count = 5)
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

    [KernelFunction, Description("Updates the path to the Git repository being used.")]
    public string SetRepositoryPath([Description("New path to a local Git repository.")] string newPath)
    {
        if (!Directory.Exists(newPath))
        {
            return $"Directory does not exist: {newPath}";
        }

        _repoPath = newPath;
        return $"Repository path updated to: {_repoPath}";
    }

    [KernelFunction, Description("Pulls the latest changes from the origin remote repository.")]
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

    [KernelFunction, Description("Stages all changes and creates a Git commit with the given message.")]
    public string CommitAllChanges([Description("Commit message describing the changes.")] string message)
    {
        using var repo = new Repository(_repoPath);

        Commands.Stage(repo, "*");
        var author = new Signature("AI Bot", "ai@localhost", DateTimeOffset.Now);
        var commit = repo.Commit(message, author, author);

        return $"Committed: {commit.Sha.Substring(0, 7)} - {commit.MessageShort}";
    }

    [KernelFunction, Description("Finds commits that contain a given keyword in their message.")]
    public string FindCommitsByKeyword(
        [Description("Keyword to search for in commit messages.")] string keyword,
        [Description("Maximum number of matching commits to return. Default is 10.")] int count = 10)
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

    [KernelFunction, Description("Compares two Git commits and summarizes changes per file (lines added/removed).")]
    public string CompareCommits(
        [Description("SHA of the base commit to compare from.")] string fromSha,
        [Description("SHA of the target commit to compare to.")] string toSha)
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
