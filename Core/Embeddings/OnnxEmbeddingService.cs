using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LetsDoc.Core.Embeddings;

public class OnnxEmbeddingService : IEmbeddingService
{
    private readonly InferenceSession _session;
    private readonly ITextTokenizer _tokenizer;
    private readonly int _maxLength;

    public OnnxEmbeddingService(string modelPath, ITextTokenizer tokenizer, int maxLength = 256)
    {
        // PERFORMANCE TIP: For MacBook Air (M1/M2/M3), use CoreML for hardware acceleration
        var options = new SessionOptions();
        try {
            options.AppendExecutionProvider_CoreML(CoreMLFlags.COREML_FLAG_ONLY_ENABLE_DEVICE_WITH_ANE);
        } catch {
            // Fallback to CPU if CoreML isn't available
        }

        _session = new InferenceSession(modelPath, options);
        _tokenizer = tokenizer;
        _maxLength = maxLength;
    }

    public float[] Embed(string text)
    {
        var tokens = _tokenizer.Tokenize(text, _maxLength);
        
        // 1. Create Tensors with explicit dimensions [BatchSize, SequenceLength]
        var inputIdsTensor = new DenseTensor<long>(new[] { 1, tokens.InputIds.Length });
        var maskTensor = new DenseTensor<long>(new[] { 1, tokens.AttentionMask.Length });

        for (int i = 0; i < tokens.InputIds.Length; i++)
        {
            inputIdsTensor[0, i] = tokens.InputIds[i];
            maskTensor[0, i] = tokens.AttentionMask[i];
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor)
        };

        // 2. Run Inference
        using var results = _session.Run(inputs);
        
        // The model output (last_hidden_state) is usually the first result
        var outputTensor = results.First().AsTensor<float>();
        
        // Shape: [Batch(1), SeqLen, HiddenSize(384)]
        int seqLen = (int)outputTensor.Dimensions[1];
        int hiddenSize = (int)outputTensor.Dimensions[2];

        // 3. Manual Mean Pooling (Average the token vectors)
        var embedding = new float[hiddenSize];
        int validTokens = 0;

        for (int i = 0; i < seqLen; i++)
        {
            if (tokens.AttentionMask[i] == 0) continue; 
            validTokens++;

            for (int h = 0; h < hiddenSize; h++)
            {
                // Access the tensor at [Batch 0, Token i, Feature h]
                embedding[h] += outputTensor[0, i, h];
            }
        }

        // Normalize the average
        float divisor = Math.Max(1, validTokens);
        for (int h = 0; h < hiddenSize; h++)
        {
            embedding[h] /= divisor;
        }

        return embedding;
    }
}