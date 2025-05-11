using Microsoft.SemanticKernel;
using System;
using System.Threading.Tasks;

namespace SemanticKernel.Utils;

/// <summary>
/// Wraps all interactions with the GitPlugin into helper methods 
/// for fetching commits, setting repo path, pulling, committing, etc.
/// Keeps the main program loop clean and modular.
/// </summary>
public static class GitPluginHandler
{
    public static async Task ShowCommitsAsync(Kernel kernel)
    {
        var result = await kernel.InvokeAsync("GitPlugin", "GetLatestCommits", new KernelArguments
        {
            ["count"] = 5
        });

        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
    }

    public static async Task SetRepoPathAsync(Kernel kernel, string path)
    {
        var result = await kernel.InvokeAsync("GitPlugin", "SetRepositoryPath", new KernelArguments
        {
            ["newPath"] = path
        });

        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
    }

    public static async Task PullAsync(Kernel kernel)
    {
        var result = await kernel.InvokeAsync("GitPlugin", "PullRepository");
        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
    }

    public static async Task CommitAsync(Kernel kernel, string message)
    {
        var result = await kernel.InvokeAsync("GitPlugin", "CommitAllChanges", new KernelArguments
        {
            ["message"] = message
        });

        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
    }

    public static async Task FindFixesAsync(Kernel kernel)
    {
        var result = await kernel.InvokeAsync("GitPlugin", "FindCommitsByKeyword", new KernelArguments
        {
            ["keyword"] = "fix"
        });

        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
    }

    public static async Task CompareCommitsAsync(Kernel kernel, string fromSha, string toSha)
    {
        var result = await kernel.InvokeAsync("GitPlugin", "CompareCommits", new KernelArguments
        {
            ["fromSha"] = fromSha,
            ["toSha"] = toSha
        });

        Console.WriteLine($"[GitPlugin] {result.GetValue<string>()}");
    }
}
