# CFHodEd Cross-Platform Port

## Phase 1-3: Complete ✅
- GenericMath, HW2IFF, CFHodEd.Math, GenericMesh, GMWavObjT, HW2MAD
- CFHodEd.Rendering (Silk.NET OpenGL), HW2HOD

## Phase 4: UI Migration ✅
- [x] CFHodEd.UI project (Avalonia 11.2.3) ✅
- [x] MainWindow layout (menu, toolbar, panels) ✅
- [x] OpenGLViewport control ✅
  - Software-rendered 3D viewport
  - XZ grid, XYZ axes
  - Orbit camera (LMB rotate, MMB/RMB pan, scroll zoom)
  - WASD + R keyboard controls
  - 3 render modes (wireframe/solid/textured)
- [x] HOD file open/save ✅
- [x] Hierarchy tree view ✅
- [ ] Property bindings
- [ ] Settings dialog
- [ ] OBJ import/export

## Build Status ✅
```
9 projects (0 errors)
└── CFHodEd.UI ✅
```

## To Run
```bash
cd /home/stella/Projects/CFHodEd-master
dotnet run --project src/CFHodEd.UI
```
