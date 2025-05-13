using Microsoft.SemanticKernel;
using SemanticKernel.Services;


namespace SemanticKernel.Utils
{
    /// <summary>
    /// Handles questions about the indexed codebase by:
    /// Retrieving relevant code snippets.
    /// Sending them with the user's question to the AI plugin.
    /// </summary>
    public static class AskCodeHandler
    {
        public static async Task AskCodebaseQuestionAsync(Kernel kernel, CodeMemoryService memoryService, string question)
        {
            var memoryResults = await memoryService.SearchAsync(question);
            var codeContext = string.Join("\n---\n", memoryResults);

            Console.WriteLine("[Debug] Code context:");
            Console.WriteLine(codeContext);

            var answer = await kernel.InvokeAsync("CodeMemoryPlugin", "AskCodebase", new KernelArguments
            {
                ["question"] = question,
                ["code"] = codeContext
            });

            Console.WriteLine("AI > " + answer.GetValue<string>());
        }
    }
}
