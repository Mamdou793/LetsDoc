using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LetsDoc.Core.LLM;

public class OllamaBackend : ILlmBackend
{
    private static readonly HttpClient _http = new() 
    { 
        BaseAddress = new Uri("http://localhost:11434/"),
        Timeout = TimeSpan.FromMinutes(2) // Local LLMs can be slow on first load
    };
    
    private readonly string _model;

    public OllamaBackend(string model = "llama3")
    {
        _model = model;
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var request = new
        {
            model = _model,
            prompt = prompt,
            stream = false
        };

        try
        {
            var response = await _http.PostAsJsonAsync("api/generate", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<OllamaResponse>();
            return json?.Response ?? "Error: LLM returned empty response.";
        }
        catch (HttpRequestException ex)
        {
            return $"Error: Could not connect to Ollama. Is it running? ({ex.Message})";
        }
        catch (TaskCanceledException)
        {
            return "Error: The request timed out. The model might be too large for current memory.";
        }
    }

    private class OllamaResponse
    {
        // Ollama JSON property is lowercase 'response'
        public string response { get; set; } = "";
        
        // This helper makes it work regardless of JSON casing
        public string Response => response;
    }
}
