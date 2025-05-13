using Microsoft.Extensions.VectorData;

namespace SemanticKernel.Models;

/// <summary>
/// Represents a chunk of source code that can be embedded and stored
/// in a vector database for semantic search.
/// </summary>
public record TextChunk
{
    [VectorStoreRecordKey]
    public required string Key { get; init; }

    [VectorStoreRecordData]
    public required string DocumentName { get; init; }

    [VectorStoreRecordData]
    public required int ParagraphId { get; init; }

    [VectorStoreRecordData]
    public required string Text { get; init; }

    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}
