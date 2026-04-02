using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Required for Async
using LetsDoc.Core.Ingestion;
using LetsDoc.Core.Chunking;
using LetsDoc.Core.Embeddings;
using LetsDoc.Core.VectorStore;
using LetsDoc.Core.Models;

namespace LetsDoc.Core.Pipeline;

public class IndexingService
{
    private readonly IDocumentParser _parser;
    private readonly IChunker _chunker;
    private readonly IEmbeddingService _embeddings;
    private readonly IVectorStore _vectorStore;

    public IndexingService(
        IDocumentParser parser,
        IChunker chunker,
        IEmbeddingService embeddings,
        IVectorStore vectorStore)
    {
        _parser = parser;
        _chunker = chunker;
        _embeddings = embeddings;
        _vectorStore = vectorStore;
    }

    public async Task IndexDocumentAsync(string docId, string filePath)
    {
        // 1. Run parsing on a background thread (CPU intensive)
        var text = await Task.Run(() => _parser.Parse(filePath));
        
        // 2. Chunking is usually fast, but we'll keep it in the background task
        var chunks = _chunker.Chunk(text);

        // 3. Embedding (The most time-consuming part)
        var vectors = new List<EmbeddingVector>();
        
        await Task.Run(() => {
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunkText = chunks[i];
                vectors.Add(new EmbeddingVector
                {
                    DocumentId = docId,
                    ChunkIndex = i,
                    PageNumber = 1, // Defaulting to 1 since most parsers start at 1
                    Text = chunkText,
                    Vector = _embeddings.Embed(chunkText)
                });
            }
        });

        // 4. Save to SQLite (I/O intensive)
        _vectorStore.AddVectors(vectors);
    }
}
