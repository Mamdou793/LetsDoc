using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;                    // Added: Fixes Path.GetFileName
using Avalonia.Threading;           // Added: Fixes Threading/Dispatcher
using LetsDoc.Core.Models;          // Existing: Models
using LetsDoc.UI;                   // Added: Fixes ServiceLocator error

namespace LetsDoc.UI.ViewModels;

public class DocumentListViewModel : ReactiveObject
{
    public ObservableCollection<Document> Documents { get; } = new();

    // --- PROPERTIES FOR XAML BINDING ---
    private Document? _selectedDocument;
    public Document? SelectedDocument
    {
        get => _selectedDocument;
        set 
        {
            this.RaiseAndSetIfChanged(ref _selectedDocument, value);
            // Notify UI that the helper properties depend on this change
            this.RaisePropertyChanged(nameof(Content));
            this.RaisePropertyChanged(nameof(SourceInfo));
            this.RaisePropertyChanged(nameof(HasSourceInfo));
        }
    }

    public string Content => SelectedDocument?.Content ?? "No document selected.";
    public string SourceInfo => SelectedDocument?.FileName ?? "";
    public bool HasSourceInfo => !string.IsNullOrEmpty(SourceInfo);
    // ------------------------------------------

    public ReactiveCommand<string, Unit> ImportDocumentCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public DocumentListViewModel()
    {
        // Use the built-in ReactiveUI scheduler to keep the command on the UI thread
        ImportDocumentCommand = ReactiveCommand.CreateFromTask<string>(
            ImportDocumentAsync, 
            outputScheduler: RxApp.MainThreadScheduler);
    }

    private async Task ImportDocumentAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var docId = Guid.NewGuid().ToString();
        var fileName = Path.GetFileName(filePath);

        try
        {
            IsBusy = true;

            // Perform heavy indexing work on a background thread
            await Task.Run(() => ServiceLocator.Indexer.IndexDocumentAsync(docId, filePath));

            var newDoc = new Document 
            { 
                Id = Guid.Parse(docId), 
                FileName = fileName, 
                FilePath = filePath,
                Content = $"Document '{fileName}' indexed successfully." 
            };
            
            // IMPORTANT: UI Collections must be updated on the Main Thread
            await Dispatcher.UIThread.InvokeAsync(() => 
            {
                Documents.Add(newDoc);
                SelectedDocument = newDoc; 
            });
        }
        catch (Exception ex)
        {
            // Update UI with the error message safely
            await Dispatcher.UIThread.InvokeAsync(() => 
            {
                Console.WriteLine($"Import failed: {ex.Message}");
            });
        }
        finally
        {
            IsBusy = false;
        }
    }
}