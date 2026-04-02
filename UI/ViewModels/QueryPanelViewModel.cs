using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.Threading;
using Avalonia.ReactiveUI;    // Added: Fixes 'AvaloniaScheduler' red line
using LetsDoc.Core.Models;    // Existing: Fixes 'SearchResult' red line
using LetsDoc.UI;             // Added: Fixes 'ServiceLocator' red line

namespace LetsDoc.UI.ViewModels;

public class QueryPanelViewModel : ReactiveObject
{

    private bool _isDocumentLoaded; 
    // Backing field added — fixes red underline (field was missing)
    public bool IsDocumentLoaded
    {
        get => _isDocumentLoaded;
        set => this.RaiseAndSetIfChanged(ref _isDocumentLoaded, value);
    }

    private string? _loadedDocumentName;
    // Backing field added — fixes red underline
    public string? LoadedDocumentName
    {
        get => _loadedDocumentName;
        set => this.RaiseAndSetIfChanged(ref _loadedDocumentName, value);
    }

    private string _question = string.Empty;
    // Backing field added — fixes red underline
    public string Question
    {
        get => _question;
        set => this.RaiseAndSetIfChanged(ref _question, value);
    }

    private List<SearchResult> _results = new();
    // Backing field added — fixes red underline
    public List<SearchResult> Results
    {
        get => _results;
        set => this.RaiseAndSetIfChanged(ref _results, value);
    }

    private SearchResult? _selectedResult;
    // Backing field added — fixes red underline
    public SearchResult? SelectedResult
    {
        get => _selectedResult;
        set => this.RaiseAndSetIfChanged(ref _selectedResult, value);
    }

    private string _generatedAnswer = string.Empty;
    // Backing field added — fixes red underline
    public string GeneratedAnswer
    {
        get => _generatedAnswer;
        set => this.RaiseAndSetIfChanged(ref _generatedAnswer, value);
    }

    public ReactiveCommand<Unit, Unit> AskCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadDocumentCommand { get; }

    public QueryPanelViewModel()
    {
   
        AskCommand = ReactiveCommand.CreateFromTask(
            ExecuteQueryAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );

        // Same fix for LoadDocumentCommand
        LoadDocumentCommand = ReactiveCommand.CreateFromTask(
            LoadDocumentAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );
    }


    private async Task ExecuteQueryAsync()
    {
        // FIX: UI updates are safe because command runs on UI thread
        if (!IsDocumentLoaded)
        {
            GeneratedAnswer = "Please load a document first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Question))
        {
            GeneratedAnswer = "Ask a question about your document.";
            return;
        }

        try
        {
            GeneratedAnswer = "Searching...";

            // FIX: Heavy work moved to background thread
            var searchResults = await Task.Run(() =>
                ServiceLocator.QueryEngine.Query(Question)
            );

            Results = searchResults;

            if (Results.Count == 0)
            {
                GeneratedAnswer = "No results found.";
                return;
            }

            GeneratedAnswer = "Thinking...";

            // FIX: LLM call also offloaded to background thread
            var finalAnswer = await Task.Run(() =>
                ServiceLocator.Rag.AskAsync(Question)
            );

            GeneratedAnswer = finalAnswer;
        }
        catch (Exception ex)
        {
            // FIX: Safe UI update
            GeneratedAnswer = $"Error: {ex.Message}";
        }
    }


    private async Task LoadDocumentAsync()
    {
        // Placeholder — UI update is safe
        GeneratedAnswer = "Loading feature triggered.";
        await Task.CompletedTask;
    }

    public void MarkDocumentLoaded(string fileName)
    {
        // FIX: Proper state update
        IsDocumentLoaded = true;
        LoadedDocumentName = fileName;
    }
}
