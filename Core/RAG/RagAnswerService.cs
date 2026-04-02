using System;
using System.Linq;
using System.Threading.Tasks;
using LetsDoc.Core.Retrieval;
using LetsDoc.Core.LLM;

namespace LetsDoc.Core.RAG; // Matches your 4:11 PM folder structure

public class RagAnswerService
{
    private readonly IQueryEngine _retrieval;
    private readonly ILlmBackend _llm;

    public RagAnswerService(IQueryEngine retrieval, ILlmBackend llm)
    {
        _retrieval = retrieval;
        _llm = llm;
    }

    public async Task<string> AskAsync(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            return "Please enter a question.";

        // 1. Retrieve relevant chunks (Top 5 is a good balance for MiniLM)
        var results = _retrieval.Query(question, topK: 5);

        if (!results.Any())
            return "I couldn't find any relevant information in your documents to answer that.";

        // 2. Build a structured RAG prompt
        // Added a "Source" label to help the LLM distinguish between chunks
        var context = string.Join("\n\n", results.Select((c, i) => $"[Source {i+1}]: {c.Text}"));

        var prompt = 
$@"Instructions: You are a helpful AI assistant. Use the provided context to answer the question. 
If the answer isn't in the context, say you don't know—don't make things up.

### Context:
{context}

### Question:
{question}

### Answer:";

        // 3. Generate answer via Ollama
        try 
        {
            return await _llm.GenerateAsync(prompt);
        }
        catch (Exception ex)
        {
            return $"Sorry, I encountered an error while generating the answer: {ex.Message}";
        }
    }
}