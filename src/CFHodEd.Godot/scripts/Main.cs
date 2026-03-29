using Godot;
using HW2HOD;

namespace CFHodEd.Godot;

/// <summary>
/// Root script for the editor window. Wires toolbar buttons, file dialogs,
/// render-mode switching, and the hierarchy ↔ properties panel communication.
///
/// Expected scene layout (attached to the root Control named "Main"):
///   Main (Control)
///     VBoxContainer
///       ToolBar (HBoxContainer)
///         OpenButton, SaveButton, SaveAsButton, [VSeparator]
///         WireframeButton, SolidButton, TexturedButton
///       WorkArea (HSplitContainer)
///         HierarchyPanel  ← unique name, HierarchyPanel script
///         ViewportContainer (SubViewportContainer)
///           SubViewport   ← unique name
///             Camera3D    ← unique name, CameraController script
///             HodModelRoot ← unique name
///         PropertiesPanel ← unique name, PropertiesPanel script
///       StatusBar (Label) ← unique name
/// </summary>
public partial class Main : Control
{
    private HOD?   _currentHod;
    private string? _currentFilePath;
    private Node3D? _currentModelNode;

    // Node references resolved in _Ready
    private Label            _statusBar      = null!;
    private Node3D           _hodModelRoot   = null!;
    private HierarchyPanel   _hierarchyPanel = null!;
    private PropertiesPanel  _propertiesPanel = null!;
    private CameraController _camera         = null!;
    private SubViewport      _viewport       = null!;

    public override void _Ready()
    {
        _statusBar       = GetNode<Label>("%StatusBar");
        _hodModelRoot    = GetNode<Node3D>("%HodModelRoot");
        _hierarchyPanel  = GetNode<HierarchyPanel>("%HierarchyPanel");
        _propertiesPanel = GetNode<PropertiesPanel>("%PropertiesPanel");
        _camera          = GetNode<CameraController>("%Camera3D");
        _viewport        = GetNode<SubViewport>("%SubViewport");

        // Wire toolbar buttons
        GetNode<Button>("%OpenButton").Pressed    += OnOpenPressed;
        GetNode<Button>("%SaveButton").Pressed    += OnSavePressed;
        GetNode<Button>("%SaveAsButton").Pressed  += OnSaveAsPressed;
        GetNode<Button>("%WireframeButton").Pressed += OnWireframePressed;
        GetNode<Button>("%SolidButton").Pressed   += OnSolidPressed;
        GetNode<Button>("%TexturedButton").Pressed += OnTexturedPressed;

        // Wire hierarchy → properties
        _hierarchyPanel.SelectionChanged += item =>
            _propertiesPanel.ShowProperties(item, _currentHod);

        SetStatus("Ready — open a .hod file to begin");

        // Headless screenshot mode: --screenshot <output.png> [--hod <file.hod>]
        var userArgs = OS.GetCmdlineUserArgs();
        if (userArgs.Contains("--screenshot"))
            TakeScreenshot(userArgs);
    }

    // -------------------------------------------------------------------------
    // Screenshot mode  (invoked via: godot --path src/CFHodEd.Godot -- --screenshot out.png)
    // -------------------------------------------------------------------------

    private async void TakeScreenshot(string[] args)
    {
        string? hodPath    = null;
        // Default to temp/screenshots/ in the repo root (already gitignored)
        string  outputPath = System.IO.Path.GetFullPath(
            System.IO.Path.Combine(
                ProjectSettings.GlobalizePath("res://"), "..", "..", "temp", "screenshots", "screenshot.png"));

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--hod")        hodPath    = args[i + 1];
            if (args[i] == "--screenshot") outputPath = args[i + 1];
        }

        // Default HOD: the test fixture in test/meg_starjumper.hod
        hodPath ??= System.IO.Path.GetFullPath(
            System.IO.Path.Combine(
                ProjectSettings.GlobalizePath("res://"), "..", "..", "test", "meg_starjumper.hod"));

        if (System.IO.File.Exists(hodPath))
            LoadHOD(hodPath);

        // Ensure the SubViewport renders every frame while we wait
        _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;

        // Wait for the GPU to flush the rendered frames
        for (int i = 0; i < 3; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        var image = _viewport.GetTexture().GetImage();

        if (!System.IO.Path.IsPathRooted(outputPath))
            outputPath = System.IO.Path.GetFullPath(outputPath);

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath)!);
        image.SavePng(outputPath);
        GD.Print($"[Screenshot] {image.GetWidth()}x{image.GetHeight()} → {outputPath}");

        GetTree().Quit();
    }

    // -------------------------------------------------------------------------
    // File dialogs
    // -------------------------------------------------------------------------

    private void OnOpenPressed()
    {
        var dialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access   = FileDialog.AccessEnum.Filesystem,
        };
        dialog.AddFilter("*.hod", "Homeworld 2 HOD Files");
        dialog.FileSelected += path => { dialog.QueueFree(); LoadHOD(path); };
        dialog.Canceled     += () => dialog.QueueFree();
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    private void OnSavePressed()
    {
        if (_currentHod == null) return;
        if (_currentFilePath == null) { OnSaveAsPressed(); return; }
        SaveHOD(_currentFilePath);
    }

    private void OnSaveAsPressed()
    {
        if (_currentHod == null) return;
        var dialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Access   = FileDialog.AccessEnum.Filesystem,
        };
        dialog.AddFilter("*.hod", "Homeworld 2 HOD Files");
        dialog.FileSelected += path => { dialog.QueueFree(); SaveHOD(path); };
        dialog.Canceled     += () => dialog.QueueFree();
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    // -------------------------------------------------------------------------
    // Load / save
    // -------------------------------------------------------------------------

    private void LoadHOD(string path)
    {
        try
        {
            var hod = new HOD();
            using (var stream = System.IO.File.OpenRead(path))
                hod.Read(stream);

            _currentHod      = hod;
            _currentFilePath = path;

            // Replace model in viewport
            _currentModelNode?.QueueFree();
            _currentModelNode = HODLoader.BuildScene(hod);
            _hodModelRoot.AddChild(_currentModelNode);

            // Populate hierarchy
            _hierarchyPanel.Populate(hod);

            // Show team colors in properties immediately
            _propertiesPanel.ShowProperties(null, hod);

            // Frame the model
            var bounds = ComputeBounds(_currentModelNode);
            _camera.FocusOnBounds(bounds);

            SetStatus($"Loaded: {System.IO.Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            SetStatus($"Error loading: {ex.Message}");
            GD.PrintErr(ex);
        }
    }

    private void SaveHOD(string path)
    {
        if (_currentHod == null) return;
        try
        {
            using var stream = System.IO.File.Create(path);
            _currentHod.Write(stream);
            _currentFilePath = path;
            SetStatus($"Saved: {System.IO.Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            SetStatus($"Error saving: {ex.Message}");
            GD.PrintErr(ex);
        }
    }

    // -------------------------------------------------------------------------
    // Render modes
    // -------------------------------------------------------------------------

    private void OnWireframePressed()
    {
        _viewport.DebugDraw = Viewport.DebugDrawEnum.Wireframe;
    }

    private void OnSolidPressed()
    {
        _viewport.DebugDraw = Viewport.DebugDrawEnum.Disabled;
        SetAllMaterials(shaded: true, textured: false);
    }

    private void OnTexturedPressed()
    {
        _viewport.DebugDraw = Viewport.DebugDrawEnum.Disabled;
        SetAllMaterials(shaded: true, textured: true);
    }

    private void SetAllMaterials(bool shaded, bool textured)
    {
        if (_currentModelNode == null) return;

        foreach (var node in _currentModelNode.FindChildren("*", "MeshInstance3D", true, false))
        {
            if (node is not MeshInstance3D mi) continue;
            if (mi.GetActiveMaterial(0) is not StandardMaterial3D mat) continue;

            mat.ShadingMode = shaded
                ? BaseMaterial3D.ShadingModeEnum.PerPixel
                : BaseMaterial3D.ShadingModeEnum.Unshaded;

            // Hide texture in solid mode by temporarily overriding albedo
            if (!textured)
                mat.AlbedoColor = new Color(0.7f, 0.7f, 0.7f);
            else
                mat.AlbedoColor = Colors.White;
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Aabb ComputeBounds(Node3D root)
    {
        var aabb = new Aabb(Vector3.Zero, Vector3.Zero);
        bool first = true;

        foreach (var node in root.FindChildren("*", "MeshInstance3D", true, false))
        {
            if (node is MeshInstance3D mi)
            {
                var b = mi.GetAabb();
                aabb = first ? b : aabb.Merge(b);
                first = false;
            }
        }

        return first ? new Aabb(Vector3.Zero, new Vector3(5, 5, 5)) : aabb;
    }

    private void SetStatus(string message) => _statusBar.Text = message;
}
