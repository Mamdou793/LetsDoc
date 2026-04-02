using System.Threading.Tasks;
namespace LetsDoc.Core.LLM;

public interface ILlmBackend
{
    Task<string> GenerateAsync(string prompt);
}
