using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HW2HOD;

namespace CFHodEd.UI;

public partial class MainWindow : Window
{
    private HOD? _currentHod;
    private string? _currentFilePath;

    public MainWindow()
    {
        InitializeComponent();
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        string fileName = _currentFilePath != null ? System.IO.Path.GetFileName(_currentFilePath) : "Untitled";
        Title = $"CFHodEd - {fileName}";
    }

    private void SetStatus(string message)
    {
        StatusText.Text = message;
    }

    private void NewFile_Click(object? sender, RoutedEventArgs e)
    {
        _currentHod = new HOD();
        _currentHod.Initialize();
        _currentFilePath = null;
        UpdateTitle();
        RefreshHierarchy();
        SetStatus("New file created");
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
                _currentHod = new HOD();
                _currentHod.Read(stream);
                _currentFilePath = file.Path.LocalPath;
                UpdateTitle();
                RefreshHierarchy();
                SetStatus($"Loaded: {System.IO.Path.GetFileName(_currentFilePath)}");
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
            }
        }
    }

    private async void SaveFile_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentHod == null)
        {
            SetStatus("Nothing to save");
            return;
        }

        if (_currentFilePath == null)
        {
            SaveFileAs_Click(sender, e);
            return;
        }

        try
        {
            await using var stream = System.IO.File.Create(_currentFilePath);
            _currentHod.Write(stream);
            SetStatus($"Saved: {System.IO.Path.GetFileName(_currentFilePath)}");
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
        }
    }

    private async void SaveFileAs_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentHod == null)
        {
            SetStatus("Nothing to save");
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
                _currentHod.Write(stream);
                _currentFilePath = result.Path.LocalPath;
                UpdateTitle();
                SetStatus($"Saved: {System.IO.Path.GetFileName(_currentFilePath)}");
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
            }
        }
    }

    private void ImportOBJ_Click(object? sender, RoutedEventArgs e)
    {
        SetStatus("OBJ import not yet implemented");
    }

    private void ExportOBJ_Click(object? sender, RoutedEventArgs e)
    {
        SetStatus("OBJ export not yet implemented");
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResetCamera_Click(object? sender, RoutedEventArgs e)
    {
        SetStatus("Camera reset");
    }

    private void WireframeMode_Click(object? sender, RoutedEventArgs e)
    {
        SetStatus("Wireframe mode");
    }

    private void SolidMode_Click(object? sender, RoutedEventArgs e)
    {
        SetStatus("Solid mode");
    }

    private void TexturedMode_Click(object? sender, RoutedEventArgs e)
    {
        SetStatus("Textured mode");
    }

    private void Settings_Click(object? sender, RoutedEventArgs e)
    {
        SetStatus("Settings not yet implemented");
    }

    private void About_Click(object? sender, RoutedEventArgs e)
    {
        SetStatus("CFHodEd - Cross-platform Homeworld 2 Model Editor");
    }

    private void RefreshHierarchy()
    {
        HierarchyTree.Items.Clear();

        if (_currentHod == null)
        {
            var item = new TreeViewItem { Header = "Root", IsExpanded = true };
            item.Items.Add(new TreeViewItem { Header = "(No model loaded)" });
            HierarchyTree.Items.Add(item);
            return;
        }

        // Build joint hierarchy
        var rootItem = BuildJointTree(_currentHod.Root);
        HierarchyTree.Items.Add(rootItem);

        // Add meshes
        if (_currentHod.Meshes.Count > 0)
        {
            var meshesItem = new TreeViewItem { Header = $"Meshes ({_currentHod.Meshes.Count})", IsExpanded = true };
            foreach (var mesh in _currentHod.Meshes)
            {
                var meshItem = new TreeViewItem { Header = mesh.Name };
                foreach (var lod in mesh.LODs)
                    meshItem.Items.Add(new TreeViewItem { Header = $"LOD: {lod.VertexCount} verts" });
                meshesItem.Items.Add(meshItem);
            }
            HierarchyTree.Items.Add(meshesItem);
        }

        // Add materials
        if (_currentHod.Materials.Count > 0)
        {
            var materialsItem = new TreeViewItem { Header = $"Materials ({_currentHod.Materials.Count})" };
            foreach (var mat in _currentHod.Materials)
                materialsItem.Items.Add(new TreeViewItem { Header = mat.Name });
            HierarchyTree.Items.Add(materialsItem);
        }

        // Add markers
        if (_currentHod.Markers.Count > 0)
        {
            var markersItem = new TreeViewItem { Header = $"Markers ({_currentHod.Markers.Count})" };
            foreach (var marker in _currentHod.Markers)
                markersItem.Items.Add(new TreeViewItem { Header = marker.Name });
            HierarchyTree.Items.Add(markersItem);
        }
    }

    private TreeViewItem BuildJointTree(Joint joint)
    {
        var item = new TreeViewItem { Header = joint.Name, IsExpanded = true };
        foreach (var child in joint.Children)
            item.Items.Add(BuildJointTree(child));
        return item;
    }
}
