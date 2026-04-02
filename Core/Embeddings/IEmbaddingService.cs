namespace LetsDoc.Core.Embeddings;

public interface IEmbeddingService
{
    float[] Embed(string text);
}
