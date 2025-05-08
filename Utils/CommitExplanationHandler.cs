using Microsoft.SemanticKernel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SemanticKernel.Utils
{
    /// <summary>
    /// Uses the CommitExplainer plugin to interpret commit messages into plain English.
    /// Appends each explanation to EXPLAIN.md.
    /// </summary>
    public static class CommitExplanationHandler
    {
        private const string FileName = "EXPLAIN.md";

        public static async Task ExplainCommitAsync(Kernel kernel, string commitText)
        {
            var explainResult = await kernel.InvokeAsync("CommitExplainer", "ExplainCommit", new KernelArguments
            {
                ["commit"] = commitText
            });

            var explanation = explainResult.GetValue<string>() ?? "[No explanation generated]";
            Console.WriteLine($"[CommitExplainer] {explanation}");

            var outputDir = HelperFunctions.EnsureOutputDirectory();
            var fullPath = Path.Combine(outputDir, FileName);

            var content = $"# Commit Explanation\n\n**Commit:** {commitText}\n\n**Explanation:**\n{explanation}\n";

            File.AppendAllText(fullPath, content + "\n---\n");

            Console.WriteLine($"Explanation appended to: {fullPath}");
        }
    }
}
