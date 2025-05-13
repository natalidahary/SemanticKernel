using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernel.Services;

namespace SemanticKernel.Config;

#pragma warning disable SKEXP0001
public static class AppInitializer
{
    public static CodeMemoryService SetupCodeMemory(AppConfiguration config, Kernel kernel)
    {
        var vectorStore = kernel.GetRequiredService<IVectorStore>();
        var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        var memoryService = new CodeMemoryService(vectorStore, embeddingService);

        return memoryService;
    }
}

