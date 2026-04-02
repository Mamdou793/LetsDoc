namespace LetsDoc.Core.Embeddings;

public interface ITextTokenizer
{
    TokenizationResult Tokenize(string text, int maxLength);
}

public record TokenizationResult(long[] InputIds, long[] AttentionMask);
