using System;
using System.Collections.Generic;
using System.IO;
using SemanticKernel.Models;

namespace SemanticKernel.Services;
public class DocumentReader
{
    public static IEnumerable<TextChunk> ParseFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var docName = Path.GetFileName(filePath);

        var chunks = new List<TextChunk>();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            var paragraphId = i + 1;
            var key = $"{docName}_{paragraphId}";

            chunks.Add(new TextChunk
            {
                Key = key,
                DocumentName = docName,
                ParagraphId = paragraphId,
                Text = line,
                TextEmbedding = ReadOnlyMemory<float>.Empty
            });
        }

        return chunks;
    }
}
