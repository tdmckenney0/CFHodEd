# Architecture

SharpHodEditor is a multi-project .NET 8 codebase. The editor UI runs inside Godot 4; all file-format and math work lives in standalone class libraries with no Godot dependency.

## Layered Overview

```
┌──────────────────────────────────────────────────────────────┐
│  CFHodEd.Godot  (src/CFHodEd.Godot/)                         │
│  Godot 4 desktop application                                  │
│  • Main.cs          — toolbar, file dialogs, render modes     │
│  • HODLoader.cs     — HOD → Godot ArrayMesh + Skeleton3D      │
│  • HierarchyPanel.cs — Tree control population                │
│  • PropertiesPanel.cs — joint/material/color property editor  │
│  • CameraController.cs — orbit/pan/zoom Camera3D              │
└───────────────────────────┬──────────────────────────────────┘
                            │ references
           ┌────────────────┼───────────────┐
           ▼                ▼               ▼
      ┌─────────┐    ┌──────────┐    ┌─────────────┐
      │ HW2HOD  │    │ HW2MAD   │    │ CFHodEd.Math│
      │ (model) │    │(animation│    │ (Vector /   │
      └────┬────┘    │ stubbed) │    │  Matrix /   │
           │         └──────────┘    │  Quaternion)│
           │ uses                    └─────────────┘
      ┌────▼────┐
      │ HW2IFF  │
      │(IFF I/O)│
      └─────────┘

Supporting utilities:
  GenericMesh   — flexible mesh container
  GenericMath   — generic numeric operators (reflection-based)
  GMWavObjT     — Wavefront OBJ import/export (not yet wired up)
  TestHOD       — console test harness for HOD loading
```

## Project Reference

| Project | Output | Role |
|---------|--------|------|
| `CFHodEd.Godot` | Godot app | Desktop editor (Godot 4 + C#) |
| `CFHodEd.Math` | Library | DirectX-compatible math types |
| `HW2HOD` | Library | Homeworld 2 model file format |
| `HW2IFF` | Library | IFF binary container format |
| `HW2MAD` | Library | Homeworld 2 animation format (stubbed) |
| `GenericMesh` | Library | Generic mesh data structures |
| `GenericMath` | Library | Generic math operators via reflection |
| `GMWavObjT` | Library | Wavefront OBJ format support |
| `TestHOD` | Exe | Console test app for HOD loading (legacy) |
| `CFHodEd.Tests` | xUnit | Unit tests: Math, IFF, HOD, textures (`dotnet test`) |
| `CFHodEd.Tests.Godot` | gdUnit4 | HOD model contracts, coordinate system rules (`dotnet test`) |

## Dependency Graph

```
CFHodEd.Godot
  ├── Godot.NET.Sdk (GodotSharp API)
  ├── HW2HOD
  ├── HW2IFF
  ├── HW2MAD
  ├── CFHodEd.Math
  ├── GenericMesh
  ├── GenericMath
  └── GMWavObjT

HW2HOD
  ├── CFHodEd.Math
  ├── HW2IFF
  ├── HW2MAD
  └── GenericMesh

HW2IFF   — no project dependencies
HW2MAD   — no project dependencies (stubbed)
GenericMesh, GenericMath, GMWavObjT — CFHodEd.Math / GenericMath only

CFHodEd.Tests  (xUnit)
  ├── CFHodEd.Math
  ├── HW2IFF
  ├── HW2HOD
  └── GenericMesh

CFHodEd.Tests.Godot  (gdUnit4, Microsoft.NET.Sdk)
  └── HW2HOD
```

## Key Classes

### Data Model

```
HOD                              ← root model container (HW2HOD/HOD.cs)
├── Joint (root)                 ← skeleton root node
│   └── Joint (children…)       ← recursive tree
├── List<Mesh>                   ← geometry containers
│   └── Mesh
│       └── List<MeshLOD>        ← level-of-detail array
│           ├── List<HODVertex>  ← Position, Normal, Tangent, TexCoords
│           └── List<ushort>     ← triangle indices
├── List<Material>               ← shader name + texture parameter bindings
├── List<Texture>                ← DXT or raw RGBA pixel data
├── List<Marker>                 ← hardpoints / attachment points
├── EngineGlows/Burns/NavLights  ← visual effect descriptors
└── TeamColor / StripeColor      ← ColorValue (RGBA float)
```

### Godot Scripts

```
Main : Control                   ← root scene script (scripts/Main.cs)
├── LoadHOD(path)                ← reads stream → HODLoader.BuildScene → viewport
├── SaveHOD(path)                ← HOD.Write(stream)
├── OnWireframe/Solid/Textured   ← sets viewport.DebugDraw, material mode
└── wires HierarchyPanel ↔ PropertiesPanel via SelectionChanged event

HODLoader (static)               ← scripts/HODLoader.cs
├── BuildScene(hod) → Node3D     ← Skeleton3D + MeshInstance3D subtree
├── BuildMesh(lod)  → ArrayMesh  ← vertex/normal/uv/index arrays with Z-flip
├── BuildMaterial   → StandardMaterial3D  ← loads Godot ImageTexture from DXT data
└── BuildSkeleton                ← recursive Joint → bone rest transforms

HierarchyPanel : VBoxContainer   ← scripts/HierarchyPanel.cs
├── Populate(hod)                ← fills Godot Tree control
└── SelectionChanged event       ← fires with the selected HOD object

PropertiesPanel : VBoxContainer  ← scripts/PropertiesPanel.cs
└── ShowProperties(item, hod)    ← updates SpinBoxes / OptionButton / ColorPickers

CameraController : Camera3D      ← scripts/CameraController.cs
├── Orbit  (left drag)
├── Pan    (middle/right drag)
├── Zoom   (scroll wheel)
└── FocusOnBounds(Aabb)
```

## Scene Graph (Main.tscn)

```
Main (Control)
└── VBoxContainer
    ├── ToolBar (HBoxContainer)
    │   └── Open / Save / SaveAs / [sep] / Wireframe / Solid / Textured
    ├── WorkArea (HSplitContainer)
    │   ├── HierarchyPanel (VBoxContainer) [%HierarchyPanel]
    │   │   └── Tree [child of HierarchyPanel]
    │   ├── ViewportContainer (SubViewportContainer)
    │   │   └── SubViewport [%SubViewport]
    │   │       ├── Camera3D [%Camera3D, CameraController script]
    │   │       ├── DirectionalLight3D
    │   │       ├── WorldEnvironment
    │   │       └── HodModelRoot (Node3D) [%HodModelRoot]
    │   └── PropertiesContainer (ScrollContainer)
    │       └── PropertiesPanel (VBoxContainer) [%PropertiesPanel]
    │           ├── TransformSection [%TransformSection]
    │           │   └── SpinBox × 9 [%PosX … %ScaleZ]
    │           ├── MaterialSection [%MaterialSection]
    │           │   ├── OptionButton [%ShaderOption]
    │           │   └── LineEdit × 3 [%DiffuseEdit, %GlowEdit, %NormalEdit]
    │           └── TeamColorsSection [%TeamColorsSection]
    │               └── ColorPickerButton × 2 [%TeamColorPicker, %StripeColorPicker]
    └── StatusBar (Label) [%StatusBar]
```

## Data Flow

```
File Open
    → Main.LoadHOD(path)
    → HOD.Read(stream) parses binary
    → HODLoader.BuildScene(hod) → Node3D subtree
    → subtree added to HodModelRoot in SubViewport
    → HierarchyPanel.Populate(hod) fills Tree control
    → CameraController.FocusOnBounds() frames model

Selection Changed
    → HierarchyPanel.SelectionChanged fires
    → PropertiesPanel.ShowProperties(item, hod)
    → SpinBoxes / OptionButton / ColorPickers populated

Edit Joint Transform
    → SpinBox.ValueChanged → OnTransformChanged()
    → joint.Position/Rotation/Scale updated on HOD object

Save
    → Main.SaveHOD(path)
    → HOD.Write(stream) → IFFWriter serializes all chunks
```
