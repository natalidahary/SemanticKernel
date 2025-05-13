// using Microsoft.KernelMemory;
// using Microsoft.SemanticKernel;
// using System.ComponentModel;

// namespace Plugins;

// public class KernelMemoryPlugin
// {
//     private readonly IKernelMemory _memory;

//     public KernelMemoryPlugin(IKernelMemory memory)
//     {
//         _memory = memory;
//     }

//     [KernelFunction, Description("Search the indexed codebase using Kernel Memory.")]
//     public async Task<string> AskCodebaseAsync([Description("Question about the codebase")] string question)
//     {
//         Console.WriteLine("[DEBUG] AskCodebaseAsync was triggered!"); 
//         var result = await _memory.SearchAsync(question, index: "code", minRelevance: 0.7);

//         if (result.Results.Count == 0) return "No relevant code segments found.";

//         var response = string.Join("\n---\n", result.Results
//             .SelectMany(r => r.Partitions)
//             .Take(5)
//             .Select(p => p.Text.Trim()));

//         return response;
//     }
// }
