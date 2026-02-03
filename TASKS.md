# CFHodEd Cross-Platform Port

## Decisions
- [x] **Language**: C# (port from VB.NET)
- [x] **UI Framework**: Avalonia UI
- [x] **Graphics API**: Silk.NET + OpenGL
- [x] **Scope**: Full UI port required

## Phase 1: Foundation ✅
- [x] Create .NET 8 solution (`SharpHodEditor.slnx`)
- [x] GenericMath → C# (14 files) ✅
- [x] HW2IFF → C# (9 files) ✅

## Phase 2: Math & Mesh Abstraction ✅
- [x] CFHodEd.Math: Vector2/3/4, Matrix, Quaternion, Plane, BoundingBox, ColorValue ✅
- [x] GenericMesh core interfaces: IVertex, IMaterial, VertexFormats, IRenderDevice ✅
- [x] GenericMesh classes: GVertexGroup, GPrimitiveGroup, GMeshPart, GBasicMesh ✅
- [x] Material field interfaces: IMaterialTexture, IMaterialName, IMaterialAttributes ✅
- [x] Standard Vertex and Material ✅
- [x] GMWavObjT: Full OBJ read/write translator ✅
- [/] HW2MAD: Stub - requires more porting

## Phase 3: Rendering Abstraction
- [ ] CFHodEd.Rendering with Silk.NET
- [ ] IRenderDevice OpenGL implementation
- [ ] Convert HW2HOD
- [ ] Port shaders to GLSL

## Phase 4: UI Migration
- [ ] CFHodEd.UI with Avalonia
- [ ] Port MainWindow & dialogs
- [ ] 3D viewport with Silk.NET
- [ ] JSON settings (replace Registry)

## Build Status ✅
```
SharpHodEditor.slnx - 6 projects
├── GenericMath ✅
├── HW2IFF ✅
├── CFHodEd.Math ✅
├── GenericMesh ✅ (complete)
├── GMWavObjT ✅ (complete)
└── HW2MAD ✅ (stub)

Build succeeded: 0 Warning(s) 0 Error(s)
```
