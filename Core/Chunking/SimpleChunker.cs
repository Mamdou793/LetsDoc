using System;
using System.Collections.Generic;

namespace LetsDoc.Core.Chunking;

public class SimpleChunker : IChunker
{
    public List<string> Chunk(string text, int chunkSize = 500, int overlap = 100)
    {
        // 1. Safety Check: If overlap >= chunkSize, the 'start' index will 
        // never move forward, causing an infinite loop.
        if (overlap >= chunkSize) 
            overlap = chunkSize / 2; 

        var chunks = new List<string>();
        
        // 2. Handle empty strings immediately
        if (string.IsNullOrWhiteSpace(text)) return chunks;

        int start = 0;
        while (start < text.Length)
        {
            int remainingLength = text.Length - start;
            int currentChunkSize = Math.Min(chunkSize, remainingLength);
            
            chunks.Add(text.Substring(start, currentChunkSize));

            // 3. Move the pointer
            start += (chunkSize - overlap);

            // 4. Break if we've reached the end to avoid redundant small chunks
            if (start >= text.Length || remainingLength <= (chunkSize - overlap))
                break;
        }

        return chunks;
    }
}
