# Plan: Migrate SharpHodEditor to Godot 4

## Context

SharpHodEditor currently uses Avalonia UI for its desktop shell and a CPU-based software rasterizer (`OpenGLViewport.cs`) for 3D display. This is complex to maintain and limits rendering quality. The goal is to replace both the UI layer and the rendering layer with Godot 4.6, leveraging its built-in 3D renderer, GPU acceleration, and rich UI widget system — while keeping all the core C# format libraries unchanged.

## What Gets Removed

- `src/CFHodEd.UI/` — entire Avalonia project (MainWindow, ViewModels, OpenGLViewport, rasterizer)
- `src/CFHodEd.Rendering/` — IRenderDevice abstraction and OpenGL backend (Godot's renderer replaces this entirely)
- Avalonia and Silk.NET NuGet dependencies

## What Gets Added

- `src/CFHodEd.Godot/` — Godot 4 project with C# scripting

## What Stays Unchanged

All core libraries: `HW2HOD`, `HW2IFF`, `HW2MAD`, `GenericMesh`, `GenericMath`, `GMWavObjT`, `CFHodEd.Math`

---

## Critical Technical Issue: Coordinate System Conversion

HOD/DirectX uses **left-handed** coordinates (camera looks toward +Z).
Godot 4 uses **right-handed** coordinates (camera looks toward -Z).

When converting HOD mesh data to Godot `ArrayMesh`:
- **Negate Z** on all vertex `Position` and `Normal` values
- **Flip winding order**: swap index[1] and index[2] in every triangle
- **Joint matrices** from `Joint.LocalTransform` need the same Z-flip applied

---

## Project Structure

```
src/
  CFHodEd.Godot/               ← New Godot 4 project
    project.godot
    CFHodEd.Godot.csproj       ← References core library projects
    scenes/
      Main.tscn               ← Root scene
      HierarchyPanel.tscn
      PropertiesPanel.tscn
      Viewport3D.tscn
    scripts/
      Main.cs                 ← App root: file menu, status bar, wires panels together
      HODLoader.cs            ← Converts HOD → Godot ArrayMesh, Skeleton3D, materials
      HierarchyPanel.cs       ← Populates Tree control from HOD data
      PropertiesPanel.cs      ← Reads/writes transform + material properties
      CameraController.cs     ← Orbit/pan/zoom on SubViewport camera
```

### .csproj Integration

`CFHodEd.Godot.csproj` uses `<ProjectReference>` to reference the core libs:

```xml
<ItemGroup>
  <ProjectReference Include="../HW2HOD/HW2HOD.csproj" />
  <ProjectReference Include="../HW2IFF/HW2IFF.csproj" />
  <ProjectReference Include="../HW2MAD/HW2MAD.csproj" />
  <ProjectReference Include="../GenericMesh/GenericMesh.csproj" />
  <ProjectReference Include="../CFHodEd.Math/CFHodEd.Math.csproj" />
  <!-- etc. -->
</ItemGroup>
```

Godot 4's C# uses .NET 6+ runtime; .NET 8 class libraries are compatible.

---

## Godot Scene Hierarchy

```
Main (Control)
├── MenuBar                          ← File / Edit / View / Tools
├── ToolBar (HBoxContainer)          ← Open, Save, Wireframe, Solid, Textured, Reset Camera
├── WorkArea (HSplitContainer)
│   ├── HierarchyPanel (VBoxContainer, 250px)
│   │   └── Tree                    ← Scene tree: Joints, Meshes, Materials, Markers, Effects
│   ├── ViewportContainer (SubViewportContainer, flex)
│   │   └── SubViewport
│   │       ├── Camera3D
│   │       ├── DirectionalLight3D
│   │       ├── WorldEnvironment
│   │       └── HodModelRoot (Node3D)   ← MeshInstance3D + Skeleton3D go here
│   └── PropertiesPanel (VBoxContainer, 280px)
│       ├── TransformSection (GridContainer)
│       │   └── SpinBox × 9 (Pos/Rot/Scale XYZ)
│       ├── MaterialSection (GridContainer)
│       │   ├── OptionButton (shader)
│       │   └── LineEdit + Button × 3 (Diffuse/Glow/Normal textures)
│       └── TeamColorsSection
│           └── ColorPickerButton × 2 (Team, Stripe)
└── StatusBar (Label)
```

---

## HODLoader.cs

Responsible for converting a loaded `HOD` object into Godot scene nodes.

```csharp
public static class HODLoader
{
    // Returns a Node3D subtree ready to add to HodModelRoot
    public static Node3D BuildScene(HOD hod);

    // Converts one MeshLOD to an ArrayMesh surface
    private static ArrayMesh BuildMesh(MeshLOD lod, HOD hod);

    // Applies Z-flip coordinate conversion to all vertices
    private static Vector3 ConvertVertex(CFHodEd.Math.Vector3 v);

    // Converts HOD.Material → Godot StandardMaterial3D
    private static StandardMaterial3D BuildMaterial(HW2HOD.Material mat, HOD hod);

    // Walks Joint hierarchy → Skeleton3D bones
    private static void BuildSkeleton(Skeleton3D skeleton, Joint joint, int parentIdx);
}
```

**Mesh construction** uses `SurfaceTool` or direct `ArrayMesh` arrays:
- `Mesh.ARRAY_VERTEX` ← HODVertex.Position with Z negated
- `Mesh.ARRAY_NORMAL` ← HODVertex.Normal with Z negated
- `Mesh.ARRAY_TEX_UV` ← HODVertex.TexCoords
- `Mesh.ARRAY_INDEX` ← Indices with winding flipped (swap [i*3+1] ↔ [i*3+2])

Each `Mesh` in the HOD becomes a `MeshInstance3D` child of `HodModelRoot`, attached to the appropriate joint via `Skeleton3D` bone transforms.

---

## HierarchyPanel.cs

Populates a Godot `Tree` control from the loaded HOD. Mirrors the existing hierarchy exactly:

```
Model (root)
├── Joints          → recursive Joint children
├── Meshes          → each Mesh, each LOD as child
├── Materials       → each Material
├── Markers         → each Marker
└── Effects         → EngineGlows, EngineBurns, NavLights
```

Each `TreeItem` stores the underlying HOD object in its metadata. On selection change, emits a signal to `PropertiesPanel` to update displayed values.

---

## PropertiesPanel.cs

**On selection changed:**
- If a `Joint` is selected: populate Position/Rotation/Scale SpinBoxes from `Joint.Position`, `Joint.Rotation`, `Joint.Scale`
- If a `Material` is selected: populate shader OptionButton + texture fields from `Material`
- TeamColor / StripeColor read from `HOD.TeamColor` / `HOD.StripeColor` → `ColorPickerButton.Color`

**On value changed:**
- SpinBox edits write back to the HOD object and call `HODLoader.RebuildMesh()` to refresh the viewport

Godot's built-in `ColorPickerButton` replaces the unimplemented color picker from Avalonia.

---

## CameraController.cs

Attached to a `Camera3D` inside the `SubViewport`. Replicates existing behavior:

| Input | Action |
|-------|--------|
| Left mouse drag | Orbit (yaw/pitch) |
| Middle/Right mouse drag | Pan |
| Scroll wheel | Zoom (FOV or distance) |
| W/A/S/D | Pan |
| R | Reset camera |
| F | Cycle render modes |

Uses Godot's `_Input(InputEvent)` override. Camera uses spherical coordinates (distance, yaw, pitch) identical to the existing system.

---

## Render Modes

Godot supports render mode switching without a custom rasterizer:

| Mode | Implementation |
|------|---------------|
| **Wireframe** | `get_viewport().debug_draw = Viewport.DebugDrawEnum.Wireframe` |
| **Solid** | Normal rendering with unshaded or basic lit `StandardMaterial3D` |
| **Textured** | Normal rendering with full `StandardMaterial3D` + texture |

---

## File I/O

- **Open HOD**: `FileDialog` (Godot built-in) → `FileDialog.FileSelected` signal → read file with `HW2HOD.HOD.Read(stream)` → `HODLoader.BuildScene(hod)` → add to viewport
- **Save / Save As**: `FileDialog` → `HOD.Write(stream)` (unchanged from current)

---

## Implementation Steps

1. **Setup** — Create `src/CFHodEd.Godot/` Godot 4 project; configure `.csproj` with project references to core libs; verify build
2. **Main scene** — Build `Main.tscn` with `HSplitContainer` layout, menu, toolbar, status bar
3. **HODLoader** — Implement mesh + material conversion with coordinate flip; test with a real .hod file
4. **Viewport** — Wire `HODLoader.BuildScene()` output into `SubViewport` node tree; verify model appears
5. **Camera** — Implement `CameraController.cs` with orbit/pan/zoom
6. **Render modes** — Wire toolbar buttons to `Viewport.DebugDraw` and material mode switching
7. **Hierarchy panel** — Populate `Tree` from HOD; wire selection signal
8. **Properties panel** — Wire SpinBoxes, OptionButton, ColorPickerButtons to selected HOD object
9. **File dialogs** — Open/Save/SaveAs using Godot `FileDialog`
10. **Cleanup** — Delete `src/CFHodEd.UI/`, `src/CFHodEd.Rendering/`; update solution `.sln`

---

## Verification

- Open a Homeworld 2 `.hod` file → model appears in viewport with correct orientation
- Hierarchy tree shows correct Joint/Mesh/Material/Marker structure
- Selecting a Joint shows its transform in the properties panel; editing SpinBoxes updates the model
- Wireframe / Solid / Textured modes all work via toolbar
- Camera orbits, pans, zooms; R resets; F cycles modes
- Save writes a valid `.hod` that the original game/editor can read
- Team/Stripe color pickers open and apply color to HOD object
