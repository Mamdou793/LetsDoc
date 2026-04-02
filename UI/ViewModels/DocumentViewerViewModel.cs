using System;
using ReactiveUI;            
using LetsDoc.Core.Models;   

namespace LetsDoc.UI.ViewModels
{
    public class DocumentViewerViewModel : ReactiveObject
    {
        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        // --- ADD THESE TO FIX THE BUILD ERROR ---
        private string _sourceInfo = string.Empty;
        public string SourceInfo
        {
            get => _sourceInfo;
            set 
            {
                this.RaiseAndSetIfChanged(ref _sourceInfo, value);
                // Notify the UI that HasSourceInfo changed too
                this.RaisePropertyChanged(nameof(HasSourceInfo));
            }
        }

        public bool HasSourceInfo => !string.IsNullOrWhiteSpace(SourceInfo);
        // ------------------------------------------

        public void ShowChunk(SearchResult result)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            
            Content = result.Text ?? string.Empty;
            
            // Optional: If SearchResult has a filename or source, 
            // set it here so it shows up in your UI
            SourceInfo = result.Metadata?.ToString() ?? "Unknown Source"; 
        }
    }
}
