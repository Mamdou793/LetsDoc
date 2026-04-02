using System;
using System.Collections.Generic;
namespace LetsDoc.Core.Models;

public class DocumentChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; } // Foreign key to Document
    public string Text { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public float[]? Embedding { get; set; } // The vector representation
}