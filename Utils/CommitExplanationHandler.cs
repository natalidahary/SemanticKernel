using Microsoft.SemanticKernel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SemanticKernel.Utils
{
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

            var content = $"# Commit Explanation\n\n**Commit:** {commitText}\n\n**Explanation:**\n{explanation}\n";

            File.AppendAllText(FileName, content + "\n---\n");

            Console.WriteLine($"Explanation appended to: {FileName}");
        }
    }
}
