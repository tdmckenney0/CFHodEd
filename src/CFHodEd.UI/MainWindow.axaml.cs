using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CFHodEd.UI.ViewModels;
using HW2HOD;

namespace CFHodEd.UI;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void NewFile_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.NewFile();
        Viewport.ResetCamera();
    }

    private async void OpenFile_Click(object? sender, RoutedEventArgs e)
    {
        var options = new FilePickerOpenOptions
        {
            Title = "Open HOD File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("HOD Files") { Patterns = new[] { "*.hod" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            }
        };

        var result = await StorageProvider.OpenFilePickerAsync(options);
        if (result.Count > 0)
        {
            var file = result[0];
            try
            {
                await using var stream = await file.OpenReadAsync();
                var hod = new HOD();
                hod.Read(stream);
                
                _viewModel.CurrentHod = hod;
                _viewModel.CurrentFilePath = file.Path.LocalPath;
                _viewModel.StatusText = $"Loaded: {System.IO.Path.GetFileName(file.Path.LocalPath)}";
                Viewport.Model = hod;
            }
            catch (Exception ex)
            {
                _viewModel.StatusText = $"Error: {ex.Message}";
            }
        }
    }

    private async void SaveFile_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.CurrentHod == null)
        {
            _viewModel.StatusText = "Nothing to save";
            return;
        }

        if (_viewModel.CurrentFilePath == null)
        {
            SaveFileAs_Click(sender, e);
            return;
        }

        try
        {
            await using var stream = System.IO.File.Create(_viewModel.CurrentFilePath);
            _viewModel.CurrentHod.Write(stream);
            _viewModel.StatusText = $"Saved: {System.IO.Path.GetFileName(_viewModel.CurrentFilePath)}";
        }
        catch (Exception ex)
        {
            _viewModel.StatusText = $"Error: {ex.Message}";
        }
    }

    private async void SaveFileAs_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.CurrentHod == null)
        {
            _viewModel.StatusText = "Nothing to save";
            return;
        }

        var options = new FilePickerSaveOptions
        {
            Title = "Save HOD File",
            DefaultExtension = "hod",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("HOD Files") { Patterns = new[] { "*.hod" } }
            }
        };

        var result = await StorageProvider.SaveFilePickerAsync(options);
        if (result != null)
        {
            try
            {
                await using var stream = await result.OpenWriteAsync();
                _viewModel.CurrentHod.Write(stream);
                _viewModel.CurrentFilePath = result.Path.LocalPath;
                _viewModel.StatusText = $"Saved: {System.IO.Path.GetFileName(result.Path.LocalPath)}";
            }
            catch (Exception ex)
            {
                _viewModel.StatusText = $"Error: {ex.Message}";
            }
        }
    }

    private void ImportOBJ_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.StatusText = "OBJ import not yet implemented";
    }

    private void ExportOBJ_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.StatusText = "OBJ export not yet implemented";
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResetCamera_Click(object? sender, RoutedEventArgs e)
    {
        Viewport.ResetCamera();
        _viewModel.StatusText = "Camera reset";
    }

    private void WireframeMode_Click(object? sender, RoutedEventArgs e)
    {
        Viewport.Mode = RenderMode.Wireframe;
        _viewModel.StatusText = "Wireframe mode";
    }

    private void SolidMode_Click(object? sender, RoutedEventArgs e)
    {
        Viewport.Mode = RenderMode.Solid;
        _viewModel.StatusText = "Solid mode";
    }

    private void TexturedMode_Click(object? sender, RoutedEventArgs e)
    {
        Viewport.Mode = RenderMode.Textured;
        _viewModel.StatusText = "Textured mode";
    }

    private void Settings_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.StatusText = "Settings not yet implemented";
    }

    private void About_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.StatusText = "CFHodEd - Cross-platform Homeworld 2 Model Editor";
    }

    private void TeamColor_Click(object? sender, PointerPressedEventArgs e)
    {
        // Color picker would go here
        _viewModel.StatusText = "Color picker not yet implemented";
    }

    private void StripeColor_Click(object? sender, PointerPressedEventArgs e)
    {
        // Color picker would go here
        _viewModel.StatusText = "Color picker not yet implemented";
    }
}
