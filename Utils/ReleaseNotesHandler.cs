using Microsoft.SemanticKernel;
using System;

namespace SemanticKernel.Utils;

/// <summary>
/// Handles generating release notes from recent Git commits using the ReleaseNotesPlugin.
/// Also manages versioning and writing output to RELEASE_NOTES.md and VERSION file.
/// </summary>
public static class ReleaseNotesHandler
{
    public static async Task GenerateAndSaveReleaseNotesAsync(Kernel kernel, int count = 10)
    {
        var outputDir = HelperFunctions.EnsureOutputDirectory();

        var versionFile = Path.Combine(outputDir, "VERSION");
        var releaseFile = Path.Combine(outputDir, "RELEASE_NOTES.md");

        var currentVersion = File.Exists(versionFile) ? File.ReadAllText(versionFile).Trim() : "0.0.0";
        var newVersion = HelperFunctions.IncrementPatchVersion(currentVersion);

        Console.WriteLine($"[DEBUG] Current version: {currentVersion} → New version: {newVersion}");

        var commitsResult = await kernel.InvokeAsync("GitPlugin", "GetLatestCommits", new KernelArguments
        {
            ["count"] = count
        });

        var commits = commitsResult.GetValue<string>() ?? "";

        var releaseResult = await kernel.InvokeAsync("ReleaseNotesPlugin", "GenerateReleaseNotes", new KernelArguments
        {
            ["commits"] = commits
        });

        var notes = releaseResult.GetValue<string>() ?? "[No release notes generated]";
        var changelog = $"## v{newVersion} - {DateTime.Now:yyyy-MM-dd}\n\n{notes}\n";

        File.AppendAllText(releaseFile, changelog);
        File.WriteAllText(versionFile, newVersion);

        Console.WriteLine($"v{newVersion} release notes written to: {releaseFile}");
    }
}
