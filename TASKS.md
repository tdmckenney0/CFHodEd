# CFHodEd — Godot 4 Editor

## Complete ✅
- All core libraries ported from VB.NET to C#
- Godot 4 project with C# scripting (`src/CFHodEd.Godot/`)
- HOD file reading and writing
- Joint/skeleton hierarchy (DTRM) parsing (fixed byte vs int32 bug)
- HODLoader: HOD → Godot ArrayMesh + Skeleton3D with LH→RH coordinate conversion
- Hierarchy panel: Tree control populated from Joints/Meshes/Materials/Markers/Effects
- Properties panel: Joint transforms, Material shader/textures, Team/Stripe colors
- Orbit/pan/zoom camera controller (CameraController.cs)
- Render mode switching: Wireframe / Solid / Textured via toolbar
- File dialogs: Open / Save / Save As
- DXT1/3/5 and raw RGBA texture decompression → Godot ImageTexture

## Testing ✅
- xUnit project `CFHodEd.Tests` — Math, IFF, HOD, texture decompression (`dotnet test src/CFHodEd.Tests`)
- gdUnit4 project `CFHodEd.Tests.Godot` — HODLoader coordinate conversion, HierarchyPanel population

## Remaining
- [ ] OBJ import/export (GMWavObjT library exists; wire up to File menu)
- [ ] Animation playback (HW2MAD library is stubbed — see `src/HW2MAD/MADFormat.cs`)
- [ ] Live viewport update when joint transforms are edited in properties panel

## Run

Open `src/CFHodEd.Godot/` in the Godot 4 editor and press Play.

