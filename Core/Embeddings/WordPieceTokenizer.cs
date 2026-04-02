using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LetsDoc.Core.Embeddings;

public class WordPieceTokenizer : ITextTokenizer
{
    private readonly Dictionary<string, int> _vocab;
    private readonly int _unkId, _clsId, _sepId, _padId;
    private const string WordpiecePrefix = "##";

    public WordPieceTokenizer(string vocabPath)
    {
        // Path matches your Models/ folder
        _vocab = File.ReadAllLines(vocabPath)
                     .Select((t, i) => new { Token = t.Trim(), Index = i })
                     .Where(x => !string.IsNullOrWhiteSpace(x.Token))
                     .ToDictionary(x => x.Token, x => x.Index);

        _unkId = GetId("[UNK]");
        _clsId = GetId("[CLS]");
        _sepId = GetId("[SEP]");
        _padId = GetId("[PAD]");
    }

    private int GetId(string token) => _vocab.TryGetValue(token, out var id) ? id : 0;

    public TokenizationResult Tokenize(string text, int maxLength)
    {
        text = text.ToLowerInvariant();
        var words = Regex.Split(text, @"\s+").Where(w => !string.IsNullOrWhiteSpace(w));

        var tokens = new List<int> { _clsId };

        foreach (var word in words)
        {
            WordPieceTokenizeWord(word, tokens);
        }

        // Add SEP and enforce maxLength
        tokens.Add(_sepId);
        if (tokens.Count > maxLength) tokens = tokens.Take(maxLength).ToList();

        // Build Mask and Pad
        var attention = tokens.Select(_ => 1L).ToList();
        while (tokens.Count < maxLength)
        {
            tokens.Add(_padId);
            attention.Add(0L);
        }

        return new TokenizationResult(
            tokens.Select(i => (long)i).ToArray(),
            attention.ToArray()
        );
    }

    private void WordPieceTokenizeWord(string word, List<int> output)
    {
        int start = 0;
        var subTokens = new List<int>();

        while (start < word.Length)
        {
            int end = word.Length;
            int curId = -1;

            while (start < end)
            {
                var substr = word.Substring(start, end - start);
                
                // FIX: Only add ## if we are NOT at the beginning of the word
                if (start > 0) 
                    substr = WordpiecePrefix + substr;

                if (_vocab.TryGetValue(substr, out var id))
                {
                    curId = id;
                    break;
                }
                end--;
            }

            if (curId == -1)
            {
                output.Add(_unkId);
                return; // Entire word becomes UNK
            }

            subTokens.Add(curId);
            start = end;
        }
        output.AddRange(subTokens);
    }
}