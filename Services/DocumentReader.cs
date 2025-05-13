using SemanticKernel.Models;

namespace SemanticKernel.Services;

/// <summary>
/// Parses C# source code files into semantically meaningful chunks (like methods or classes).
/// Reads all lines of a file.
/// Uses heuristics to identify where new sections of code begin.
/// Groups accumulated lines into a TextChunk.
/// Creates a TextChunk instance from collected lines.
/// </summary>
public class DocumentReader
{
    public static IEnumerable<TextChunk> ParseFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var docName = Path.GetFileName(filePath);
        var chunks = new List<TextChunk>();

        var currentChunk = new List<string>();
        int paragraphId = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Start a new chunk if we detect a method signature or class
            if (trimmed.StartsWith("public") || trimmed.StartsWith("private") || trimmed.StartsWith("internal") || trimmed.StartsWith("class"))
            {
                // Flush previous chunk
                if (currentChunk.Count > 0)
                {
                    AddChunk(chunks, currentChunk, docName, ref paragraphId);
                    currentChunk.Clear();
                }
            }

            currentChunk.Add(line);
        }

        // Add the last chunk
        if (currentChunk.Count > 0)
        {
            AddChunk(chunks, currentChunk, docName, ref paragraphId);
        }

        return chunks;
    }

    private static void AddChunk(List<TextChunk> chunks, List<string> lines, string docName, ref int id)
    {
        var text = string.Join("\n", lines).Trim();
        if (!string.IsNullOrWhiteSpace(text))
        {
            id++;
            chunks.Add(new TextChunk
            {
                Key = $"{docName}_{id}",
                DocumentName = docName,
                ParagraphId = id,
                Text = text,
                TextEmbedding = ReadOnlyMemory<float>.Empty
            });
        }
    }

}
