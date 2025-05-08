using Microsoft.SemanticKernel;

namespace SemanticKernel.Utils;

/// <summary>
/// Utility class that provides helper methods used across the app,
/// such as displaying the command menu, showing loaded plugins,
/// and incrementing semantic versions.
/// </summary>
public static class HelperFunctions
{
    public static void PrintLoadedPlugins(Kernel kernel)
    {
        foreach (var plugin in kernel.Plugins)
        {
            Console.WriteLine($"[DEBUG] Loaded plugin: {plugin.Name}");
            foreach (var function in plugin)
            {
                Console.WriteLine($"     Function: {function.Name}");
            }
        }
    }

    public static void ShowMenu()
    {
        Console.WriteLine("\n\nAvailable commands:");
        Console.WriteLine("  !commits                   - Show the latest Git commits");
        Console.WriteLine("  !setrepo <path>            - Set the current Git repository path");
        Console.WriteLine("  !releasenotes [n]          - Generate release notes from the last [n] commits (default 10)");
        Console.WriteLine("  !explain \"commit msg\"      - Explain a commit in plain English");
        Console.WriteLine("  !pull                      - Pull latest changes from remote");
        Console.WriteLine("  !commit \"message\"          - Stage all and commit with given message");
        Console.WriteLine("  !findfixes                 - Show commits containing the word 'fix'");
        Console.WriteLine("  !diff <sha1> <sha2>        - Compare two commits and list changes");
        Console.WriteLine("  !help                      - Show this help message again");
        Console.WriteLine("  exit                       - Quit the app");
        Console.WriteLine("  [anything else]            - Sent to AI via Azure OpenAI");
        Console.WriteLine();
    }

    public static string IncrementPatchVersion(string currentVersion)
    {
        var parts = currentVersion.Split('.');
        if (parts.Length != 3) return "0.0.1"; // fallback

        int major = int.Parse(parts[0]);
        int minor = int.Parse(parts[1]);
        int patch = int.Parse(parts[2]) + 1;

        return $"{major}.{minor}.{patch}";
    }
}
