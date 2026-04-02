using System;
using System.Text;
using UglyToad.PdfPig; // <--- Change 'using PdfPig' to this

namespace LetsDoc.Core.Ingestion;

public class PdfParser : IDocumentParser
{
    public string Parse(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return string.Empty;

        // Change this line to use the full name as well
        using var doc = UglyToad.PdfPig.PdfDocument.Open(filePath); 
        var text = new StringBuilder();

        foreach (var page in doc.GetPages())
        {
            text.AppendLine(page.Text);
        }

        return text.ToString();
    }
}