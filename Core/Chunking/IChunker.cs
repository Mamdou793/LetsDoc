using System.Collections.Generic; // <--- Add this line!

namespace LetsDoc.Core.Chunking;

public interface IChunker
{
    List<string> Chunk(string text, int chunkSize = 500, int overlap = 100);
}