# CFHodEd Cross-Platform Port

## Complete ✅
- Phase 1-3: GenericMath, HW2IFF, CFHodEd.Math, GenericMesh, GMWavObjT, HW2MAD, CFHodEd.Rendering, HW2HOD

## Phase 4: UI ✅
- [x] CFHodEd.UI (Avalonia 11.2.3)
- [x] MainWindow layout
- [x] OpenGLViewport (software-rendered 3D, grid, axes, camera)
- [x] MVVM Property Bindings ✅
  - ViewModelBase (INotifyPropertyChanged)
  - HierarchyItemViewModel (tree view)
  - MainWindowViewModel (transform, material, colors)
  - ColorToBrushConverter
- [ ] OBJ import/export
- [ ] Settings dialog

## Build: 9 projects ✅

## Run
```bash
dotnet run --project src/CFHodEd.UI
```
