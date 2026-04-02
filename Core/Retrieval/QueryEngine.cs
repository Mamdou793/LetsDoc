using System;
using System.Collections.Generic;
using System.Linq;
using LetsDoc.Core.Embeddings;
using LetsDoc.Core.VectorStore;
using LetsDoc.Core.Models;

namespace LetsDoc.Core.Retrieval;

public class QueryEngine : IQueryEngine
{
    private readonly IEmbeddingService _embeddings;
    private readonly IVectorStore _vectorStore;

    public QueryEngine(IEmbeddingService embeddings, IVectorStore vectorStore)
    {
        _embeddings = embeddings;
        _vectorStore = vectorStore;
    }

    /// <summary>
    /// Executes the RAG retrieval step: Question -> Vector -> Top-K Results.
    /// </summary>
    public List<SearchResult> Query(string question, int topK = 5)
    {
        if (string.IsNullOrWhiteSpace(question))
            return new List<SearchResult>();

        // 1. Embed the question (This is the most CPU-intensive step)
        var queryVec = _embeddings.Embed(question);

        // 2. Perform the Vector Search (This happens in SQLite via sqlite-vec)
        var matches = _vectorStore.Search(queryVec, topK);

        // 3. Map to SearchResult and ensure we are returning the "best" matches
        // Note: For sqlite-vec (L2 distance), smaller numbers are BETTER matches.
        return matches
            .OrderBy(m => m.Distance) 
            .Select(m => new SearchResult
            {
                Text = m.Text,
                Score = m.Distance,
                DocumentId = m.DocumentId,
                PageNumber = m.PageNumber
            })
            .ToList();
    }
}