# CFHodEd.UI — Agent Guidance

This is the Avalonia desktop application project. It owns the main window, 3D viewport, and all MVVM bindings.

## Key Files

| File | Role |
|------|------|
| `Program.cs` | Entry point — builds Avalonia AppBuilder |
| `App.axaml(.cs)` | App lifecycle, theme (Fluent), resource init |
| `MainWindow.axaml` | UI layout (3-panel: hierarchy / viewport / properties) |
| `MainWindow.axaml.cs` | Code-behind: file dialogs, menu handlers, event wiring ONLY |
| `OpenGLViewport.cs` | Custom Avalonia control — the entire software rasterizer lives here |
| `ViewModels/MainWindowViewModel.cs` | Central editor state: current HOD, selection, all property panels |
| `ViewModels/HierarchyItemViewModel.cs` | Scene tree node — builds from HOD and supports selection |

## MVVM Rules

- **All state goes in ViewModels.** Never put mutable state in code-behind.
- **Code-behind is for wiring only:** subscribing to UI events that can't bind declaratively (file dialogs, keyboard shortcuts, viewport mouse events).
- Bindings use standard Avalonia `{Binding PropertyName}` syntax.
- For observable properties, implement `INotifyPropertyChanged` manually (call `OnPropertyChanged()` in the setter). There is no CommunityToolkit.Mvvm dependency — do not add it without discussion.
- Collections exposed to the UI use `ObservableCollection<T>`.

## OpenGLViewport.cs

This is the most complex file in the project (~680 lines). Key things to know:

- It is **not** using OpenGL. Despite the name, it renders via `WriteableBitmap` (CPU scanline rasterizer).
- The name reflects the *intent* — it will eventually use `OpenGLRenderDevice`. That wiring does not exist yet.
- The render loop is `RenderToBuffer()`. It runs on the UI thread; keep it fast.
- Camera state (`Yaw`, `Pitch`, `Distance`, `Target`) is maintained as fields, not ViewModel properties (no need to bind camera state to the property panel).
- Mouse events are handled in `OnPointerPressed/Moved/Released` and `OnPointerWheelChanged`.
- `FocusOnModel()` is called after loading a HOD to center the camera.

## Adding a Property Panel Field

1. Add the backing property to `MainWindowViewModel.cs`
2. Add the control (TextBox, ColorPicker, etc.) to the properties panel section of `MainWindow.axaml`
3. Bind it: `Text="{Binding MyProperty}"`
4. In the ViewModel setter, propagate the value to the underlying HOD object if needed

## File I/O

File open/save/new happens in `MainWindow.axaml.cs`:
- Uses Avalonia `StorageProvider` (not `System.Windows.Forms.OpenFileDialog`)
- HOD read: `HOD.Read(stream)` → assign to `ViewModel.CurrentHod`
- HOD write: `HOD.Write(stream)` from `ViewModel.CurrentHod`
- Always set status bar text (`ViewModel.StatusText`) after file operations

## Hierarchy Tree

`HierarchyItemViewModel.BuildFrom(hod)` creates the full scene tree from a loaded HOD. Call this after loading or significantly mutating the HOD. The tree has these top-level categories:
- Joints (recursive tree from root joint)
- Meshes
- Materials
- Markers
- Engine Effects
- Textures

Selection of a hierarchy item updates `MainWindowViewModel.SelectedItem`, which drives which properties are shown in the right panel.
