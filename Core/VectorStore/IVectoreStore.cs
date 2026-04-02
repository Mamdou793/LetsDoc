using System.Collections.Generic;
using LetsDoc.Core.Models;

namespace LetsDoc.Core.VectorStore;

public interface IVectorStore
{
    void AddVector(EmbeddingVector vector);
    void AddVectors(IEnumerable<EmbeddingVector> vectors);
    // Ensure the parameter name is 'limit' to match the class
    List<EmbeddingVector> Search(float[] query, int limit);
}