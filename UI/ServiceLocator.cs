using System;
using System.IO;
using LetsDoc.Core.Ingestion;
using LetsDoc.Core.Chunking;
using LetsDoc.Core.Pipeline; // Most likely location for IndexingService
using LetsDoc.Core.RAG;      // Most likely location for RagAnswerService
using LetsDoc.UI.ViewModels;
using LetsDoc.Core.Embeddings;
using LetsDoc.Core.VectorStore;
using LetsDoc.Core.Retrieval;
using LetsDoc.Core.LLM;
using LetsDoc.Core.Models;

namespace LetsDoc.UI;

public static class ServiceLocator
{
    // AppContext.BaseDirectory is the most reliable path on macOS/ARM64
    private static readonly string BaseDir = AppContext.BaseDirectory;

    private static string ResolveFile(params string[] candidates)
    {
        foreach (var relativePath in candidates)
        {
            // Ensure we are checking an absolute path
            string fullPath = Path.GetFullPath(relativePath, BaseDir);
            if (File.Exists(fullPath))
                return fullPath;
        }
        return candidates.Length > 0 ? Path.GetFullPath(candidates[0], BaseDir) : string.Empty;
    }

    // Paths: Matching your screenshot where 'models' is lowercase in UI
    private static readonly string VocabPath = ResolveFile(
        "models/vocab.txt",
        "Models/vocab.txt",
        "../Core/Models/vocab.txt"
    );

    private static readonly string OnnxModelPath = ResolveFile(
        "models/minilm.onnx",
        "Models/minilm.onnx",
        "../Core/Models/minilm.onnx"
    );

    private static readonly string SqliteDbPath = Path.Combine(BaseDir, "letsdoc.db");

    private static readonly string SqliteVecExtensionPath = ResolveFile(
        "sqlite-vec.dylib",
        "Native/sqlite-vec.dylib",
        "../Native/sqlite-vec.dylib"
    );

    // --- Services ---
    public static readonly IDocumentParser PdfParser = new PdfParser();
    public static readonly IDocumentParser TxtParser = new TxtParser();
    public static readonly IChunker Chunker = new SimpleChunker();

    // The Tokenizer and Embeddings will now have the CORRECT paths resolved
    public static readonly ITextTokenizer Tokenizer = new WordPieceTokenizer(VocabPath);
    public static readonly IEmbeddingService Embeddings = new OnnxEmbeddingService(OnnxModelPath, Tokenizer);

    public static readonly IVectorStore VectorStore = new SqliteVectorStore(SqliteDbPath, SqliteVecExtensionPath);

    public static readonly IndexingService Indexer = new IndexingService(PdfParser, Chunker, Embeddings, VectorStore);
    public static readonly IQueryEngine QueryEngine = new QueryEngine(Embeddings, VectorStore);

    public static readonly ILlmBackend LlmBackend = new OllamaBackend("llama3");
    public static readonly RagAnswerService Rag = new RagAnswerService(QueryEngine, LlmBackend);
}