using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernel.Services;

namespace SemanticKernel.Config;

#pragma warning disable SKEXP0001
/// <summary>
/// Sets up services related to code memory using vector store and embeddings.
/// Retrieves required services (IVectorStore, ITextEmbeddingGenerationService) 
/// from the kernel and initializes CodeMemoryService.
/// </summary>
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

