# Godot Migration — Completed

The Avalonia + software-rasterizer stack has been replaced with Godot 4. This document records what was done and why, for historical reference.

## What Was Removed

| Removed | Replaced by |
|---------|------------|
| `src/CFHodEd.UI/` (Avalonia app, MVVM, OpenGLViewport, software rasterizer) | `src/CFHodEd.Godot/` |
| `src/CFHodEd.Rendering/` (IRenderDevice, OpenGLRenderDevice, Silk.NET) | Godot's built-in Forward Plus renderer |

## What Was Added

`src/CFHodEd.Godot/` — Godot 4 project with C# scripting:

```
scenes/
  Main.tscn               ← Full UI layout (toolbar, panels, SubViewport)
scripts/
  Main.cs                 ← App root: file dialogs, toolbar, render modes
  HODLoader.cs            ← HOD → ArrayMesh + Skeleton3D + StandardMaterial3D
  HierarchyPanel.cs       ← Godot Tree control population
  PropertiesPanel.cs      ← Joint transform / material / team-color editing
  CameraController.cs     ← Orbit/pan/zoom Camera3D
```

## What Was Unchanged

All core libraries: `HW2HOD`, `HW2IFF`, `HW2MAD`, `GenericMesh`, `GenericMath`, `GMWavObjT`, `CFHodEd.Math`.

The unused `CFHodEd.Rendering` project reference was also removed from `HW2HOD.csproj`.

---

## Critical Technical Detail: Coordinate System

HOD/DirectX uses **left-handed** coordinates (camera toward +Z).
Godot 4 uses **right-handed** coordinates (camera toward −Z).

`HODLoader.cs` applies on every piece of geometry:
- **Negate Z** on all vertex `Position` and `Normal` values
- **Flip winding order**: swap `index[i*3+1]` ↔ `index[i*3+2]` in every triangle
- **Joint Euler rotation**: negate the Z component before `Basis.FromEuler()`

---

## Known Remaining Gaps

- **Team color shader**: The full HW2 multi-texture masking (team/stripe/glow) is not yet implemented. A custom `ShaderMaterial` is needed to replicate the game's look.
- **Live skeleton update**: Editing a joint transform in the Properties panel writes to the HOD object but does not yet update the `Skeleton3D` bone pose in real time.
- **OBJ import/export**: `GMWavObjT` is referenced but not wired to any menu action.
- **Animation (HW2MAD)**: Stubbed; no MAD playback.
