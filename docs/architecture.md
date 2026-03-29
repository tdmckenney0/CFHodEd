# Architecture

SharpHodEditor is a multi-project .NET 8 solution. Each library has a single, well-defined responsibility. This document maps all projects, their roles, and how they connect.

## Layered Overview

```
┌──────────────────────────────────────────────────────────────┐
│  CFHodEd.UI                                                   │
│  Avalonia desktop application                                 │
│  • MainWindow (XAML layout + file I/O)                        │
│  • OpenGLViewport (3D rendering control — software rasterizer)│
│  • ViewModels/ (MVVM state management)                        │
└───────────────────────────┬──────────────────────────────────┘
                            │ uses
           ┌────────────────┼─────────────────┐
           ▼                ▼                 ▼
      ┌─────────┐    ┌──────────┐     ┌──────────────┐
      │ HW2HOD  │    │ HW2MAD   │     │CFHodEd.      │
      │ (model) │    │(animation│     │Rendering     │
      └────┬────┘    │ stubbed) │     │(IRenderDevice│
           │         └──────────┘     │+ OpenGL impl)│
           │ uses                     └──────┬───────┘
      ┌────▼────┐                            │ uses
      │ HW2IFF  │                     ┌──────▼───────┐
      │(IFF I/O)│                     │CFHodEd.Math  │
      └─────────┘                     │(Vector/Matrix│
                                      │/Quaternion)  │
                                      └──────────────┘

Supporting utilities (no external deps except CFHodEd.Math):
  GenericMesh   — flexible mesh container
  GenericMath   — generic numeric operators (reflection-based)
  GMWavObjT     — Wavefront OBJ import/export (not yet wired up)
  TestHOD       — console test harness for HOD loading
```

## Project Reference

| Project | Output | Role |
|---------|--------|------|
| `CFHodEd.UI` | WinExe | Desktop editor application (Avalonia) |
| `CFHodEd.Rendering` | Library | Graphics abstraction + OpenGL backend |
| `CFHodEd.Math` | Library | DirectX-compatible math types |
| `HW2HOD` | Library | Homeworld 2 model file format |
| `HW2IFF` | Library | IFF binary container format |
| `HW2MAD` | Library | Homeworld 2 animation format (stubbed) |
| `GenericMesh` | Library | Generic mesh data structures |
| `GenericMath` | Library | Generic math operators via reflection |
| `GMWavObjT` | Library | Wavefront OBJ format support |
| `TestHOD` | Exe | Console test app for HOD loading |

## Dependency Graph

```
CFHodEd.UI
  ├── Avalonia 11.2.3
  ├── Silk.NET.OpenGL 2.21.0
  ├── CFHodEd.Math
  ├── CFHodEd.Rendering
  ├── HW2HOD
  ├── HW2IFF
  ├── HW2MAD
  └── GMWavObjT

CFHodEd.Rendering
  ├── Silk.NET.OpenGL 2.21.0
  ├── StbImageSharp 2.30.15
  └── CFHodEd.Math

HW2HOD
  ├── CFHodEd.Math
  ├── HW2IFF
  └── HW2MAD

HW2IFF   — no project dependencies
HW2MAD   — no project dependencies (stubbed)
GenericMesh, GenericMath, GMWavObjT — no project dependencies
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
├── List<Material>               ← shader + texture binding
├── List<Texture>                ← DXT or raw RGBA pixel data
├── List<Marker>                 ← hardpoints/attachment points
├── EngineGlows/Burns/NavLights  ← visual effect descriptors
└── TeamColor / StripeColor      ← ColorValue (RGBA float)
```

### UI / ViewModel

```
MainWindowViewModel              ← central editor state (ViewModels/)
├── CurrentHod: HOD              ← loaded model
├── CurrentFilePath: string
├── HierarchyItems               ← ObservableCollection bound to tree view
├── SelectedItem                 ← currently selected hierarchy node
├── Transform properties         ← Position/Rotation/Scale XYZ (9 floats)
├── Material properties          ← ShaderName, texture names
└── TeamColor / StripeColor

HierarchyItemViewModel           ← one node in the hierarchy tree
├── Name, ItemType, Data         ← display + backing HOD object
├── Children                     ← ObservableCollection (recursive)
└── IsExpanded, IsSelected
```

### Rendering

```
IRenderDevice                    ← graphics abstraction interface (CFHodEd.Rendering)
└── OpenGLRenderDevice           ← OpenGL 3.3+ implementation (complete, not wired yet)
    ├── OpenGLVertexBuffer
    ├── OpenGLIndexBuffer
    ├── OpenGLTexture
    └── OpenGLShaderProgram

OpenGLViewport                   ← Avalonia control (CFHodEd.UI)
    Uses WriteableBitmap for software rasterization.
    Does NOT use IRenderDevice/OpenGLRenderDevice yet.
```

## MVVM Data Flow

```
File Open
    → MainWindow.axaml.cs reads stream
    → HOD.Read(stream) parses binary
    → MainWindowViewModel.CurrentHod = hod
    → HierarchyItemViewModel.BuildFrom(hod) populates tree
    → Avalonia bindings update UI panels

Selection Changed
    → HierarchyItemViewModel.IsSelected = true
    → MainWindowViewModel.SelectedItem changes
    → Property panel bindings update (Position, Material, etc.)

Edit Property
    → User edits field in property panel
    → ViewModel property setter updates HOD data object
    → OpenGLViewport re-renders on next frame tick

Save
    → MainWindow.axaml.cs calls HOD.Write(stream)
    → IFFWriter serializes all chunks to binary
```

## UI Layout

The main window is a 1280×720 three-panel layout:

```
┌─────────────────────────────────────────────────┐
│ Menu bar  (File / Edit / View / Tools / Help)    │
├─────────────────────────────────────────────────┤
│ Toolbar   (Open / Save / Render mode buttons)    │
├──────────┬──────────────────────────┬────────────┤
│ Hierarchy│                          │ Properties │
│ tree     │   3D Viewport            │ panel      │
│          │   (OpenGLViewport)       │ (Transform │
│ Joints   │                          │  Material  │
│ Meshes   │                          │  Colors)   │
│ Materials│                          │            │
│ Markers  │                          │            │
│ Effects  │                          │            │
├──────────┴──────────────────────────┴────────────┤
│ Status bar                                       │
└─────────────────────────────────────────────────┘
```
