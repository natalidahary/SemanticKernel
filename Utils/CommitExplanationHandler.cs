using Microsoft.SemanticKernel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SemanticKernel.Utils
{
    public static class CommitExplanationHandler
    {
        public static async Task ExplainCommitAsync(Kernel kernel, string commitText)
        {
            var explainResult = await kernel.InvokeAsync("CommitExplainer", "ExplainCommit", new KernelArguments
            {
                ["commit"] = commitText
            });

            var explanation = explainResult.GetValue<string>() ?? "[No explanation generated]";
            Console.WriteLine($"[CommitExplainer] {explanation}");

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"EXPLAIN_{timestamp}.md";
            File.WriteAllText(fileName, $"# Commit Explanation\n\n**Commit:** {commitText}\n\n**Explanation:**\n{explanation}\n");

            Console.WriteLine($"Explanation saved to: {fileName}");
        }
    }
}
