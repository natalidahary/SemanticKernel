// using Microsoft.KernelMemory;

// namespace SemanticKernel.Services;

// public class KernelMemoryService
// {
//     private readonly IKernelMemory _memory;

//     public KernelMemoryService(IKernelMemory memory)
//     {
//         _memory = memory;
//     }

//      public async Task ImportCodebaseAsync(string folderPath)
//     {
//         var files = Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories);
//         foreach (var file in files)
//         {
//             var docId = Path.GetFileNameWithoutExtension(file);

//             Console.WriteLine($"[Import] Indexing {file}");
//             try
//             {
//                 await _memory.ImportDocumentAsync(
//                     filePath: file,
//                     documentId: docId,
//                     index: "code"
//                 );

//                 Console.WriteLine($"[Success] Indexed {file}");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"[Error] Failed to index {file}: {ex.Message}");
//             }
//         }
//     }



//     public async Task<List<string>> SearchAsync(string query)
//     {
//         var result = await _memory.SearchAsync(query, index: "code", minRelevance: 0.7);

//         Console.WriteLine($"[Debug] Found {result.Results.Count} citations");
//         foreach (var r in result.Results)
//         {
//             Console.WriteLine($"[Debug] Document: {r.SourceName}");
//             foreach (var p in r.Partitions)
//             {
//                 Console.WriteLine($"[Text] {p.Text}");
//             }
//         }

//         return result.Results
//             .SelectMany(r => r.Partitions.Select(p => p.Text))
//             .Take(5)
//             .ToList();
//     }

// }

