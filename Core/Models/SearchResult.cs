namespace LetsDoc.Core.Models;

public class SearchResult
{
    public string Text { get; set; } = string.Empty;
    public double Score { get; set; }   // lower distance = better
    public string DocumentId { get; set; } = string.Empty;
    public object? Metadata { get; set; }
    public int PageNumber { get; set; }
}
