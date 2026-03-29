# Development Guide

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Godot 4](https://godotengine.org/download/) with C# / .NET support (Mono build)
- Any IDE: VS Code + C# Dev Kit, Rider, or Visual Studio 2022

## Build & Run

```bash
# Run the editor — open src/CFHodEd.Godot/ in the Godot 4 editor, then press Play (F5).
# Godot builds the C# project automatically.

# Build core libraries only
dotnet build src/HW2HOD
```

There is no `.sln` file. Open individual `.csproj` files or the `src/` folder directly in your IDE for editing. Open `src/CFHodEd.Godot/` as a Godot project in the Godot editor.

## Project Layout

```
src/
├── CFHodEd.Godot/           Godot 4 editor application
│   ├── project.godot        Godot project config
│   ├── CFHodEd.Godot.csproj C# project (references core libs)
│   ├── scenes/
│   │   └── Main.tscn        Root scene (entire UI layout)
│   └── scripts/
│       ├── Main.cs          App root: file I/O, toolbar, wiring
│       ├── HODLoader.cs     HOD → Godot ArrayMesh + Skeleton3D
│       ├── HierarchyPanel.cs Tree control population
│       ├── PropertiesPanel.cs Joint/material/color property editor
│       └── CameraController.cs Orbit/pan/zoom camera
├── CFHodEd.Math/            Math types (Vector, Matrix, Quaternion…)
├── HW2HOD/                  HOD file format
│   ├── HOD.cs               Top-level Read/Write
│   ├── DTRM/                Skeleton, markers, engine effects
│   └── HVMD/                Meshes, materials, textures
├── HW2IFF/                  IFF container format
├── HW2MAD/                  Animation format (stubbed)
├── GenericMesh/             Generic mesh container
├── GenericMath/             Generic math operators
└── GMWavObjT/               Wavefront OBJ support
```

## How-To Guides

### Add a New Render Mode

1. Add a `Button` to the toolbar in `Main.tscn` with `unique_name_in_owner = true`
2. In `Main.cs._Ready()`, wire `GetNode<Button>("%MyButton").Pressed += OnMyModePressed`
3. In the handler, set `_viewport.DebugDraw` and call `SetAllMaterials()` as needed

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

1. **Scene:** Add the control node to `Main.tscn` with `unique_name_in_owner = true`
2. **Script:** In `PropertiesPanel._Ready()`, resolve it with `GetNode<T>("%MyControl")`
3. **Wire signal:** Connect a `ValueChanged` / `ColorChanged` / etc. handler
4. **Write back:** In the handler, update the appropriate field on the selected HOD object

### Add a Property to HODLoader

All conversion logic lives in `HODLoader.cs`. It is a static class — no state:

- To support a new texture slot (e.g. specular), add a block in `BuildMaterial()` mirroring the existing diffuse/normal/glow pattern
- To change how joints convert, update `BuildBoneTransform()`
- To support LOD switching, change `BuildScene()` to accept a lod index parameter

### Implement OBJ Import

The `GMWavObjT.WavefrontObject` class can parse `.obj` files. To wire it up:

1. Add an "Import OBJ" button to the toolbar in `Main.tscn`
2. In `Main.cs`, open a `FileDialog` for `.obj` files
3. Call `WavefrontObject.Load(path)` to get a `GBasicMesh`
4. Convert `GBasicMesh` vertices/indices into `HODVertex` + `ushort` arrays
5. Create a new `MeshLOD` and `Mesh`, add to `_currentHod.Meshes`
6. Call `_hierarchyPanel.Populate(_currentHod)` and rebuild the model node

### Port the MAD Animation System

1. Read the VB.NET reference at `refs/cfhoded/` to understand the binary layout
2. Implement `MADFormat.cs` in `src/HW2MAD/` following the IFF reader pattern
3. Add animation playback UI to `Main.tscn` (timeline, play/pause/stop buttons)
4. On playback tick, update `Skeleton3D` bone poses from keyframe data

## Debugging Tips

- **Status bar** shows file operation results and error messages.
- For IFF parsing issues, add `GD.Print(...)` to chunk handlers in `HOD.cs` — output appears in the Godot output panel.
- **Godot Remote Debugger** (Scene → Remote) lets you inspect the live node tree while the editor is running.
- The original VB.NET source at `refs/cfhoded/` is the ground truth for format behavior.

## Name Conflict Gotchas

Both Godot and HW2HOD define `Material` and `Mesh`. The Godot scripts use aliases to resolve this:

```csharp
using HodMaterial = HW2HOD.Material;
// Use global::Godot.Mesh for Godot's Mesh type
```

Any new script that imports both `using Godot` and `using HW2HOD` must add these aliases if it references either type.
