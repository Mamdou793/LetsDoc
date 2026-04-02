namespace LetsDoc.Core.Ingestion;

public interface IDocumentParser
{
    string Parse(string filePath);
}
