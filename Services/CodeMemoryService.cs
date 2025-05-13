using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernel.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemanticKernel.Services;


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
        var files = Directory.GetFiles(rootFolder, "*.txt", SearchOption.AllDirectories);
        var allChunks = new List<TextChunk>();

        foreach (var file in files)
        {
            var chunks = DocumentReader.ParseFile(file);
            allChunks.AddRange(chunks);
        }

        await UploadToVectorStoreAsync("codebase-memory", allChunks);
        Console.WriteLine($"[Memory] Indexed {allChunks.Count} code chunks from {files.Length} files.");
    }

    public async Task<List<string>> SearchAsync(string query, int topK = 3)
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
