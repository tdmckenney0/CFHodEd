# Development Guide

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Any IDE: Visual Studio 2022, VS Code + C# extension, Rider

## Build & Run

```bash
# Build everything
dotnet build

# Run the editor
dotnet run --project src/CFHodEd.UI

# Run the console HOD test loader (useful for debugging format parsing)
dotnet run --project src/TestHOD -- path/to/file.hod
```

There is no solution file currently (`.slnx` was deleted). You can open individual `.csproj` files or the root folder directly in your IDE.

## Project Layout

```
src/
├── CFHodEd.UI/           Main Avalonia application
│   ├── App.axaml(.cs)    Application entry, theme init
│   ├── MainWindow.axaml(.cs)  Main window UI + file I/O handlers
│   ├── OpenGLViewport.cs  3D viewport control (software rasterizer)
│   └── ViewModels/
│       ├── MainWindowViewModel.cs   Central editor state
│       └── HierarchyItemViewModel.cs  Scene tree nodes
├── CFHodEd.Rendering/    Graphics abstraction
│   ├── IRenderDevice.cs
│   ├── OpenGLRenderDevice.cs
│   └── Shaders/          GLSL shaders (embedded resources)
├── CFHodEd.Math/         Math types (Vector, Matrix, Quaternion…)
├── HW2HOD/               HOD file format
│   ├── HOD.cs            Top-level Read/Write
│   ├── DTRM/             Skeleton, markers, engine effects
│   └── HVMD/             Meshes, materials, textures
├── HW2IFF/               IFF container format
│   ├── IFFReader/
│   └── IFFWriter/
├── HW2MAD/               Animation format (stubbed)
├── GenericMesh/          Generic mesh container
├── GenericMath/          Generic math operators
├── GMWavObjT/            Wavefront OBJ support
└── TestHOD/              Console test application
```

## How-To Guides

### Add a New Render Mode

1. Open `src/CFHodEd.UI/OpenGLViewport.cs`
2. Add a value to the `RenderMode` enum
3. Add a `case` to the per-triangle render switch inside `DrawMeshes()`
4. In `MainWindow.axaml`, add a toolbar toggle button
5. In `MainWindowViewModel`, add a command or property to set `OpenGLViewport.CurrentRenderMode`

### Add a New IFF Chunk Handler

Handlers are registered in `src/HW2HOD/HOD.cs` inside `Read()`:

```csharp
reader.RegisterHandler("MYCHUNK", (reader, size) =>
{
    // read exactly 'size' bytes
    var value = reader.ReadInt32();
    // store on the hod object being built
});
```

For nested Form chunks, register a handler for the Form type and use a nested `IFFReader` on the chunk payload.

### Add a New UI Property

1. **ViewModel:** Add a backing field and property with `INotifyPropertyChanged`:
   ```csharp
   private string _myProp = "";
   public string MyProp
   {
       get => _myProp;
       set { _myProp = value; OnPropertyChanged(); }
   }
   ```

2. **XAML binding:** In `MainWindow.axaml`, bind a control:
   ```xml
   <TextBox Text="{Binding MyProp}" />
   ```

3. **Data flow:** In the setter, write back to the underlying HOD object if editing data (not just display state).

### Add a New Shader (GPU path)

1. Create `src/CFHodEd.Rendering/Shaders/myeffect.vert` and `myeffect.frag`
2. Add both to `CFHodEd.Rendering.csproj` as `EmbeddedResource`
3. Load via `ShaderLoader.LoadFromEmbeddedResource("myeffect")`
4. Use `OpenGLRenderDevice.CreateShaderProgram(vertSrc, fragSrc)`

### Implement OBJ Import

The `GMWavObjT.WavefrontObject` class can parse `.obj` files. To wire it up:

1. Add an "Import OBJ" menu item to `MainWindow.axaml`
2. In the handler, open a file dialog for `.obj` files
3. Call `WavefrontObject.Load(path)` to get a `GBasicMesh`
4. Convert `GBasicMesh` vertices/indices into `HODVertex` + `ushort` arrays
5. Create a new `MeshLOD` and `Mesh`, add to `CurrentHod.Meshes`
6. Refresh the hierarchy tree and viewport

### Port a MAD Animation

1. Read the VB.NET reference at `refs/cfhoded/` to understand the binary layout
2. Implement `MADFormat.cs` in `src/HW2MAD/` following the IFF reader pattern
3. Wire `MAD.Read(stream)` into the HOD loader if MAD data is embedded, or as a separate file load
4. Add animation timeline UI (frames, playback controls) to `MainWindow.axaml`
5. Apply keyframe transforms to joints in the viewport render loop

## Debugging Tips

- **TestHOD console app** prints the HOD object tree after loading. Run it on a `.hod` file to verify parsing without the UI.
- **Status bar** in the main window shows error messages from file operations.
- **Camera overlay** in the viewport (bottom-left) shows current position and controls.
- For IFF parsing issues, add `Console.WriteLine` to chunk handlers in `HOD.cs` to trace which chunks are being read.
- The original VB.NET source at `refs/cfhoded/` is the ground truth for format behavior.
