using System;
using System.Reactive.Linq;
using ReactiveUI;
using LetsDoc.Core.Models;

namespace LetsDoc.UI.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    public DocumentListViewModel DocumentList { get; } = new();
    public QueryPanelViewModel QueryPanel { get; } = new();
    public DocumentViewerViewModel Viewer { get; } = new();

    public MainWindowViewModel()
    {
        // 1. SEARCH SYNC: When a search result is clicked, show it in the Viewer
        this.WhenAnyValue(x => x.QueryPanel.SelectedResult)
            .Where(result => result != null)
            .Subscribe(result => Viewer.ShowChunk(result!));

        // 2. LIST SYNC: When a document is clicked in the left sidebar, show it in the Viewer
        this.WhenAnyValue(x => x.DocumentList.SelectedDocument)
            .Where(doc => doc != null)
            .Subscribe(doc => 
            {
                Viewer.Content = doc!.Content ?? "No content available.";
                Viewer.SourceInfo = doc.FileName;
            });

        // 3. STARTUP DATA: Add a dummy document so the UI isn't empty on launch
        DocumentList.Documents.Add(new Document 
        { 
            Id = Guid.NewGuid(),
            FileName = "Welcome_to_LetsDoc.pdf", 
            Content = "System initialized. Your MacBook Air is ready to index documents!",
            FilePath = "/System/Initial/Setup"
        });
    }
}