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
- [x] CFHodEd.Math ✅
- [x] GenericMesh (complete with mesh classes) ✅
- [x] GMWavObjT (OBJ translator) ✅
- [x] HW2MAD (animation format) ✅

## Phase 3: Rendering Abstraction 🔄
- [x] CFHodEd.Rendering project ✅
  - [x] IRenderDevice interface ✅
  - [x] OpenGLRenderDevice implementation ✅
  - [x] OpenGLResources (VB, IB, Texture, Shader) ✅
  - [x] ShaderLoader utility ✅
- [x] GLSL Shaders (5 files) ✅
  - [x] standard.vert - vertex transforms
  - [x] ship.frag - team/stripe colors, glow, specular
  - [x] matte.frag - simple diffuse
  - [x] thruster.frag - glow effects
  - [x] background.frag - skybox
- [ ] Port HW2HOD (37+ files)
  - [ ] HOD, Joint, Mesh data classes
  - [ ] HODRender → OpenGL rendering

## Phase 4: UI Migration
- [ ] CFHodEd.UI with Avalonia (13 forms)
- [ ] 3D viewport with Silk.NET
- [ ] JSON settings

## Build Status ✅
```
7 projects building (0 errors, 0 warnings)
├── GenericMath ✅
├── HW2IFF ✅
├── CFHodEd.Math ✅
├── GenericMesh ✅
├── GMWavObjT ✅
├── HW2MAD ✅
└── CFHodEd.Rendering ✅ (NEW)
```
