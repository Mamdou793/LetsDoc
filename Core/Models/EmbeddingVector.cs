namespace LetsDoc.Core.Models;

public class EmbeddingVector
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentId { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public int PageNumber { get; set; }
    public float[] Vector { get; set; } = Array.Empty<float>();
    public string Text { get; set; } = string.Empty;
    public double Distance { get; set; }  // filled on search
}
