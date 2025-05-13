using Microsoft.SemanticKernel;
using SemanticKernel.Services;

namespace SemanticKernel.Utils
{
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
