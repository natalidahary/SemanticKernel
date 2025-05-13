using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernel.Models;

namespace SemanticKernel.Services;

/// <summary>
/// Handles reading, embedding, indexing, and searching source code using vector storage.
/// Uploads text chunks with embeddings into the vector store.
/// Generates an embedding for each chunk of code text.
/// Indexes all .cs files from the given root folder.
/// Performs a vector similarity search based on a question or keyword.
/// </summary>
#pragma warning disable SKEXP0001
public class CodeMemoryService
{
    private readonly IVectorStore _vectorStore;
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public CodeMemoryService(IVectorStore vectorStore, ITextEmbeddingGenerationService embeddingService)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
    }

    public async Task UploadToVectorStoreAsync(string collectionName, IEnumerable<TextChunk> textChunks)
    {
        var collection = _vectorStore.GetCollection<string, TextChunk>(collectionName);
        await collection.CreateCollectionIfNotExistsAsync();

        foreach (var chunk in textChunks)
        {
            chunk.TextEmbedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Text);
            await collection.UpsertAsync(chunk);
        }
    }

    public async Task IndexCodebaseAsync(string rootFolder)
    {
        var files = Directory.GetFiles(rootFolder, "*.cs", SearchOption.AllDirectories)
                            .Where(f => !f.Contains("/obj/") && !f.Contains("/bin/"))
                            .ToArray();

        Console.WriteLine($"\n[Info] Found {files.Length} .cs files to index:");
        foreach (var file in files)
        {
            Console.WriteLine(" - " + file);
        }

        var allChunks = new List<TextChunk>();

        // Parse files into text chunks
        foreach (var file in files)
        {
            var chunks = DocumentReader.ParseFile(file).ToList();
            allChunks.AddRange(chunks);
            Console.WriteLine($"[Chunking] {chunks.Count} chunks from {Path.GetFileName(file)}");
        }

        // Generate all embeddings in batch
        var texts = allChunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(texts); // ensure this method exists

        for (int i = 0; i < allChunks.Count; i++)
        {
            allChunks[i].TextEmbedding = embeddings[i];
        }

        // Upload all chunks to vector store in parallel
        var collection = _vectorStore.GetCollection<string, TextChunk>("codebase-memory");
        await collection.CreateCollectionIfNotExistsAsync();

        var uploadTasks = allChunks.Select(chunk => collection.UpsertAsync(chunk));
        await Task.WhenAll(uploadTasks);

        Console.WriteLine($"[Memory] Indexed {allChunks.Count} code chunks from {files.Length} files.");
    }


    public async Task<List<string>> SearchAsync(string query, int topK = 15)
    {
        var results = new List<string>();
        var collection = _vectorStore.GetCollection<string, TextChunk>("codebase-memory");

        // Generate the embedding for the query
        var embedding = await _embeddingService.GenerateEmbeddingAsync(query);

        // Perform the vector search
        var searchResults = collection.SearchEmbeddingAsync(embedding, top: topK);

        await foreach (var result in searchResults)
        {
            results.Add(result.Record.Text);
        }

        return results;
    }
}
