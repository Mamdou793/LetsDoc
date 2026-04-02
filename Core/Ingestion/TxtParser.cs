namespace LetsDoc.Core.Ingestion;

public class TxtParser : IDocumentParser
{
    public string Parse(string filePath)
    {
        return File.ReadAllText(filePath);
    }
}
