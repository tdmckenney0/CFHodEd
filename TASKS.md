# CFHodEd Cross-Platform Port

## Decisions
- [x] **Language**: C# (port from VB.NET)
- [x] **UI Framework**: Avalonia UI
- [x] **Graphics API**: Silk.NET + OpenGL
- [x] **Scope**: Full UI port required

## Phase 1: Foundation (.NET 8 + VB→C# Conversion) ✅
- [x] Create .NET 8 solution (`SharpHodEditor.slnx`)
- [x] GenericMath → C# (14 files) ✅
- [x] HW2IFF → C# (9 files) ✅

## Phase 2: Math Abstraction ✅
- [x] CFHodEd.Math: Vector2/3/4, Matrix, Quaternion, Plane, BoundingBox, ColorValue ✅
- [x] GenericMesh core: IVertex, IMaterial, VertexFormats, RenderDevice interfaces ✅
- [/] GenericMesh full: GBasicMesh, GPrimitiveGroup, GVertexGroup (requires more work)
- [/] GMWavObjT: Stub created, requires GBasicMesh
- [/] HW2MAD: Stub created, requires full port

## Phase 3: Rendering Abstraction
- [ ] CFHodEd.Rendering with Silk.NET
- [ ] IRenderDevice OpenGL implementation
- [ ] Convert HW2HOD (requires GenericMesh + D3DHelper)
- [ ] Port shaders to GLSL

## Phase 4: UI Migration
- [ ] CFHodEd.UI with Avalonia
- [ ] Port MainWindow & dialogs
- [ ] 3D viewport with Silk.NET
- [ ] JSON settings (replace Registry)

## Current Build Status
```
SharpHodEditor.slnx - 6 projects
├── GenericMath ✅
├── HW2IFF ✅
├── CFHodEd.Math ✅
├── GenericMesh ✅ (core types)
├── GMWavObjT ✅ (stub)
└── HW2MAD ✅ (stub)
```
