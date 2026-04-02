using System;
using System.Collections.Generic;
namespace LetsDoc.Core.Models;

public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}