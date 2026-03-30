# SharpHodEditor — Codebase Analysis

## Overview

SharpHodEditor is a desktop 3D mesh editor for Homeworld 2 HOD (Homeworld Object Definition) files. It allows users to inspect, modify, and export 3D ship models and animations used by the Homeworld 2 game engine.

- **Language:** Visual Basic .NET
- **Runtime:** .NET Framework 3.5 SP1
- **UI framework:** Windows Forms
- **Graphics API:** Microsoft DirectX 9.0 (managed wrappers)
- **Build system:** MSBuild / Visual Studio 2010 solution
- **Architecture:** x86 (32-bit)
- **Platform:** Windows-only (as of initial analysis, March 2026)

---

## Project Structure

```
src/
├── CFHodEd/          # Main executable — WinForms UI
├── D3DHelper/        # DirectX 9 wrapper — device, camera, lights, config
├── GenericMesh/      # 3D mesh rendering abstractions
├── GenericMath/      # Vector/matrix math utilities
├── GMWavObjT/        # Wavefront OBJ import/export
├── HW2HOD/           # HOD file format parser + HLSL shader library
├── HW2IFF/           # IFF chunk-based container format
└── HW2MAD/           # Animation (MAD) file format parser
```

---

## Module Descriptions

### CFHodEd — Main Application
**Output type:** WinExe
**Entry point:** `CFHodEd.My.MyApplication` via `ApplicationEvents.vb`

Contains 13 Windows Forms:

| Form | Purpose |
|------|---------|
| `HODEditorA.vb` | Main editor window — 3D viewport + all panels |
| `LightEditor.vb` | Scene lighting controls |
| `MeshTransformer.vb` | Mesh transform operations |
| `MaterialSubstitute.vb` | Material replacement tool |
| `TexturePreview.vb` | Texture viewer |
| `HODType.vb` | HOD variant selector |
| `IOResult.vb` | Import/export results display |
| `Options.vb` | Application settings |
| `JointSelector.vb` | Animation joint picker |
| `JointTemplates.vb` | Joint template management |
| `HODBGTexGen.vb` | Background texture generator |
| `AboutBox.vb` | Application info |
| `ExceptionDisplay.vb` | Error dialog |

**Key dependencies:** D3DHelper, GenericMesh, GenericMath, GMWavObjT, HW2HOD, HW2MAD

---

### D3DHelper — DirectX 9 Wrapper
Manages the entire 3D rendering pipeline.

| File | Responsibility |
|------|---------------|
| `D3DHelper.vb` | Module init — loads DirectX assemblies via reflection |
| `D3DManager.vb` | Device lifecycle (create, reset, lost/recovered), render loop, threading |
| `D3DConfigurer.vb` | Adapter/display mode enumeration, device creation parameters |
| `Modules/Registry.vb` | Persists D3D device config to Windows Registry |
| `TextDisplay/FontCache.vb` | DirectX font rendering for FPS overlay |

**DirectX packages used:**
- `Microsoft.DirectX` v1.0.2902.0
- `Microsoft.DirectX.Direct3D` v1.0.2902.0
- `Microsoft.DirectX.Direct3DX` v1.0.2911.0

---

### GenericMesh — Mesh Rendering Abstractions
Wraps DirectX buffer and material types for mesh rendering.

| File | Notes |
|------|-------|
| `GMeshPart.vb` | Uses `Direct3D.VertexBuffer`, `Direct3D.IndexBuffer` |
| `GMeshTemplates.vb` | Uses DirectX vertex/index types |
| Other files | Platform-safe mesh data structures |

---

### HW2HOD — HOD File Format
Parses and writes the Homeworld 2 `.hod` mesh format. Most of this module is platform-safe binary I/O.

| File | Notes |
|------|-------|
| `ShaderLibrary.vb` | Loads and compiles HLSL shaders via `Direct3D.ShaderLoader` |
| `Shaders/*.ps` | HLSL pixel shaders (7 files) |
| `Shaders/2badge.vs` | HLSL vertex shader (1 file) |
| All other files | Platform-safe HOD parsing logic |

**HOD variants supported:**
- Homeworld2 Multi Mesh File
- Homeworld2 Variable Mesh File
- Homeworld2 Simple Mesh File
- Homeworld2 Wireframe Mesh File

---

### HW2IFF — IFF Container Format
**Platform-safe.** Pure .NET binary I/O implementing the IFF (Interchange File Format) chunk container used by Homeworld 2 files.

---

### HW2MAD — Animation Format
**Platform-safe.** Parses `.mad` animation files. No platform-specific dependencies.

---

### GenericMath — Math Utilities
**Platform-safe.** Vector, matrix, and quaternion math for 3D operations. No platform-specific dependencies.

---

### GMWavObjT — Wavefront OBJ Support
**Platform-safe.** Text-based OBJ mesh import/export using `My.Computer.FileSystem` (standard .NET I/O).

---

## File Format Reference

| Extension | Format | Library |
|-----------|--------|---------|
| `.hod` | Homeworld 2 Object Definition (3D mesh) | HW2HOD |
| `.mad` | Homeworld 2 Animation | HW2MAD |
| `.iff` | IFF chunk container | HW2IFF |
| `.obj` | Wavefront OBJ | GMWavObjT |

---

## Shader Inventory

HLSL shaders in `src/HW2HOD/Shaders/`:

| File | Type | Purpose |
|------|------|---------|
| `2badge.vs` | Vertex shader | Team badge geometry transform |
| `background.ps` | Pixel shader | Background/skybox rendering |
| `badge.ps` | Pixel shader | Team badge texture application |
| `matte.ps` | Pixel shader | Matte (opaque) surface |
| `mattealpha.ps` | Pixel shader | Matte surface with alpha |
| `megalith.ps` | Pixel shader | Megalith-style surface |
| `ship.ps` | Pixel shader | Ship hull with team stripe support |
| `thruster.ps` | Pixel shader | Engine thruster glow |

---

## Current Platform Dependencies

### Critical (block all non-Windows platforms)

| Dependency | Location | Description |
|-----------|----------|-------------|
| `Microsoft.DirectX.*` | D3DHelper, GenericMesh, HW2HOD | Legacy DirectX 9 COM wrappers — Windows-only |
| `System.Windows.Forms` | CFHodEd (all forms) | WinForms — not available on Linux/macOS in any .NET version |

### High

| Dependency | Location | Description |
|-----------|----------|-------------|
| `Microsoft.Win32.Registry` | `D3DHelper/Modules/Registry.vb` | Windows Registry for persistent settings |
| `Direct3D.ShaderLoader` | `HW2HOD/ShaderLibrary.vb` | HLSL compilation — DirectX-specific API |

### Medium

| Issue | Location | Details |
|-------|----------|---------|
| Hardcoded `\` path separators | `CFHodEd/Forms/HODEditorA.vb` (7 instances) | Uses `& "\"` instead of `IO.Path.Combine()` |
| x86 PlatformTarget | `CFHodEd.vbproj` line 6 | Forces 32-bit; cross-platform .NET defaults to 64-bit |
| `vbCrLf` hardcoded | `HW2HOD/ShaderLibrary.vb` | Should use `Environment.NewLine` |
| Windows UAC manifest | `CFHodEd/My Project/app.manifest` | Windows-only security directive |

---

## Build Configuration

**Solution:** `src/CFHodEd/CFHodEd.sln` (Visual Studio 2010 format)

All projects:
- Target .NET Framework 3.5
- Use legacy MSBuild `.vbproj` format (non-SDK-style)
- Reference DirectX assemblies from the GAC (Global Assembly Cache)
- No NuGet packages (predates modern package management)

**Build configurations:** Debug | AnyCPU, Release | AnyCPU
(Despite AnyCPU config, `PlatformTarget` is `x86` in CFHodEd.vbproj)

---

## Rendering Architecture

The render loop runs on a dedicated background thread managed by `D3DManager.vb`:

1. `D3DManager` creates a DirectX device bound to the WinForms panel handle
2. On each frame: clear → render scene → present
3. Device-lost events trigger reset/recreation
4. Frame rate is throttled to 15 FPS when the window is inactive
5. Shaders are compiled from HLSL source at startup by `ShaderLibrary.vb`
6. Geometry is uploaded to DirectX vertex/index buffers via `GMeshPart.vb`

---

## Settings Storage

Application settings are split across two mechanisms:

1. **`My.Settings`** (`.NET` application settings) — stored in user's `AppData` folder — used for UI state (window sizes, colors, etc.)
2. **Windows Registry** (`HKCU\Software\{Company}\{App}\D3DHelper`) — used for DirectX device configuration (adapter, AA level, device type, resolution)
