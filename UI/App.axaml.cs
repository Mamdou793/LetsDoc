using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LetsDoc.UI.ViewModels; // To find the ViewModels
using LetsDoc.UI.Views;       // To find the Views

namespace LetsDoc.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        try
        {
            AvaloniaXamlLoader.Load(this);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Primary XAML load failed: " + ex);

            try
            {
                // Fallback: attempt to load the App.axaml by avares URI (runtime load)
                AvaloniaXamlLoader.Load(new Uri("avares://LetsDoc.UI/App.axaml"));
            }
            catch (Exception fallbackEx)
            {
                Console.Error.WriteLine("Fallback XAML load failed: " + fallbackEx);
                throw;
            }
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Connects the View to its ViewModel
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
