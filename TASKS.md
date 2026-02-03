# CFHodEd Cross-Platform Port

## Decisions
- [x] **Language**: C# (port from VB.NET)
- [x] **UI Framework**: Avalonia UI
- [x] **Graphics API**: Silk.NET + OpenGL
- [x] **Scope**: Full UI port required

## Planning
- [x] Analyze codebase structure and dependencies
- [x] Map DirectX usage across all 8 projects  
- [x] Identify clean (DirectX-free) libraries
- [x] Create implementation plan

## Phase 1: Foundation (.NET 8 + VB→C# Conversion)
- [x] Create new .NET 8 solution structure (`SharpHodEditor.slnx`)
- [x] Convert GenericMath to C# / .NET 8 (14 files) ✅ Building
- [x] Convert HW2IFF to C# / .NET 8 (9 files) ✅ Building
- [x] Verify builds on Linux ✅

## Phase 2: Math Abstraction
- [x] Create CFHodEd.Math project with System.Numerics wrappers
- [x] Implement Vector2, Vector3, Vector4, Matrix, Quaternion, ColorValue, Plane, BoundingBox ✅
- [ ] Convert GenericMesh to C# and use new math types
- [ ] Convert GMWavObjT to C# and use new math types
- [ ] Convert HW2MAD to C# and use new math types

## Phase 3: Rendering Abstraction  
- [ ] Create CFHodEd.Rendering project with Silk.NET
- [ ] Implement IRenderDevice interface
- [ ] Implement vertex/index buffer abstractions
- [ ] Port DirectX shaders to GLSL
- [ ] Convert HW2HOD to C# and use rendering abstraction
- [ ] Port D3DHelper functionality to new rendering layer

## Phase 4: UI Migration
- [ ] Create CFHodEd.UI project with Avalonia
- [ ] Port MainWindow (HODEditorA - 10,712 lines)
- [ ] Port dialogs and secondary windows (12 forms)
- [ ] Implement 3D render control with Silk.NET
- [ ] Replace Registry with JSON settings
- [ ] Test on Linux, macOS, Windows
