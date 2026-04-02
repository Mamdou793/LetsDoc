using System.Collections.Generic;
using LetsDoc.Core.Models;

namespace LetsDoc.Core.Retrieval;

/// <summary>
/// Responsible for transforming a natural language question into a vector 
/// and finding the most relevant document chunks.
/// </summary>
public interface IQueryEngine
{
    /// <summary>
    /// Searches the vector store for the most relevant pieces of information.
    /// </summary>
    /// <param name="question">The user's query (e.g., "What is the project deadline?")</param>
    /// <param name="topK">How many relevant chunks to return.</param>
    List<SearchResult> Query(string question, int topK = 5);
}