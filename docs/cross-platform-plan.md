# Cross-Platform Migration Plan

## Goal

Make SharpHodEditor run natively on **Windows, macOS, and Linux** with minimum changes to business logic. Keep VB.NET; replace only the platform-specific layers.

See [codebase-analysis.md](codebase-analysis.md) for the full architecture and dependency inventory.

---

## Migration Stack

| Current | Replacement | Notes |
|---------|------------|-------|
| .NET Framework 3.5 | **.NET 8** | Cross-platform runtime; enables SDK-style projects |
| Microsoft.DirectX 9 COM | **OpenTK 4.x** | OpenGL wrapper; closest conceptual API to DirectX 9 |
| HLSL shaders | **GLSL shaders** | Translate 8 shader files; runtime-compiled via OpenTK |
| System.Windows.Forms | **Avalonia UI** | Best cross-platform .NET UI; VB.NET support; AXAML markup |
| Windows Registry settings | **JSON config file** | `System.Text.Json`; stored in `AppData`/`~/.config` |
| Hardcoded `\` paths | **`IO.Path.Combine()`** | 7 instances; trivial fix |

---

## What Changes vs. What Stays

### Must Change (platform-specific)

| Component | Files |
|-----------|-------|
| DirectX rendering pipeline | All of `src/D3DHelper/`; `GenericMesh/GMeshPart.vb`; `GenericMesh/GMeshTemplates.vb` |
| HLSL shader compiler | `src/HW2HOD/ShaderLibrary.vb`; `src/HW2HOD/Shaders/*.ps` / `.vs` |
| WinForms UI | All of `src/CFHodEd/Forms/`; `src/CFHodEd/ApplicationEvents.vb` |
| Registry config | `src/D3DHelper/Modules/Registry.vb` |
| Path separators | `src/CFHodEd/Forms/HODEditorA.vb` (7 instances) |
| Project files | All 8 `.vbproj` files |

### Stays Unchanged (platform-safe)

| Module | Reason |
|--------|--------|
| `src/HW2IFF/` | Pure binary I/O, no platform deps |
| `src/HW2MAD/` | Pure binary I/O, no platform deps |
| `src/HW2HOD/` (except ShaderLibrary.vb) | Pure file format parsing |
| `src/GenericMath/` | Pure math |
| `src/GMWavObjT/` | Text file I/O only |

---

## Phase Plan

### Phase 0 — Documentation & Audit (complete)
- [x] Create `docs/codebase-analysis.md`
- [x] Create `docs/cross-platform-plan.md`
- [ ] Audit full DirectX API surface used (enumerate every `Direct3D.*` type/method called across all files)

---

### Phase 1 — Project File Migration
**Risk:** Low. Windows build continues to work throughout.

**All 8 `.vbproj` files:**

1. Convert from legacy MSBuild format to SDK-style:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
   ```
2. Replace `<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>` with:
   ```xml
   <TargetFramework>net8.0-windows</TargetFramework>
   ```
   (`net8.0-windows` keeps implicit WinForms access on Windows during the transition)
3. Remove all `<Reference Include="Microsoft.DirectX.*" />` entries
4. Add OpenTK via NuGet:
   ```xml
   <PackageReference Include="OpenTK" Version="4.*" />
   ```
5. Add Avalonia packages to `CFHodEd.vbproj`:
   ```xml
   <PackageReference Include="Avalonia" Version="11.*" />
   <PackageReference Include="Avalonia.Desktop" Version="11.*" />
   <PackageReference Include="Avalonia.Themes.Fluent" Version="11.*" />
   ```
6. Change `<PlatformTarget>x86</PlatformTarget>` → `<PlatformTarget>AnyCPU</PlatformTarget>`
7. Remove bootstrapper packages and UAC manifest reference
8. Add `global.json` at repo root to pin .NET 8 SDK

---

### Phase 2 — Configuration Storage
**Risk:** Low. Self-contained module; no rendering dependencies.

**File:** `src/D3DHelper/Modules/Registry.vb`

Replace Windows Registry with a JSON config file:

- New file: `src/D3DHelper/Modules/JsonConfig.vb`
- Preserve the same public API surface (`SaveSetting`, `LoadSetting`, `DeleteSettings`) so all callers require no changes
- Config path: `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)` + `\SharpHodEditor\config.json`
- Use `System.Text.Json.JsonSerializer` for serialization
- Delete `Registry.vb`

---

### Phase 3 — Path Separator Fixes
**Risk:** Trivial.

**File:** `src/CFHodEd/Forms/HODEditorA.vb`

Replace all 7 instances of `& "\"` with `IO.Path.Combine()`:

| Line | Before | After |
|------|--------|-------|
| 91 | `Application.StartupPath & "\shaders"` | `IO.Path.Combine(AppContext.BaseDirectory, "shaders")` |
| 219 | `IO.Path.GetDirectoryName(filename) & "\" & ...` | `IO.Path.Combine(IO.Path.GetDirectoryName(filename), ...)` |
| 676 | `IO.Path.GetDirectoryName(lightFile) & "\" & ...` | `IO.Path.Combine(...)` |
| 719 | `IO.Path.GetDirectoryName(madFile) & "\" & ...` | `IO.Path.Combine(...)` |
| 807 | `IO.Path.GetDirectoryName(lightFile) & "\" & ...` | `IO.Path.Combine(...)` |
| 853 | `IO.Path.GetDirectoryName(madFile) & "\" & ...` | `IO.Path.Combine(...)` |
| 2346 | `FolderBrowserDialog.SelectedPath & "\" & ...` | `IO.Path.Combine(...)` |

Also in `src/HW2HOD/ShaderLibrary.vb`:
- Replace `vbCrLf` with `Environment.NewLine`
- Replace `Application.StartupPath` with `AppContext.BaseDirectory`

---

### Phase 4 — Shader Translation
**Risk:** Medium. Rendering correctness must be verified against original output.

**Files:** `src/HW2HOD/Shaders/*.ps`, `src/HW2HOD/Shaders/2badge.vs`

Translate each shader from HLSL (DirectX 9 assembly model) to GLSL (OpenGL 3.3 core):

| HLSL file | GLSL output | Notes |
|-----------|------------|-------|
| `2badge.vs` | `2badge.vert` | Vertex transform; map `mul(pos, WorldViewProj)` → uniform matrix |
| `background.ps` | `background.frag` | Texture sampling; `tex2D` → `texture()` |
| `badge.ps` | `badge.frag` | Team badge blending |
| `matte.ps` | `matte.frag` | Diffuse lighting |
| `mattealpha.ps` | `mattealpha.frag` | Diffuse + alpha test |
| `megalith.ps` | `megalith.frag` | Multi-texture megalith surface |
| `ship.ps` | `ship.frag` | Ship hull with team stripe compositing |
| `thruster.ps` | `thruster.frag` | Additive thruster glow |

Common translation notes:
- `tex2D(sampler, uv)` → `texture(sampler2D, uv)`
- `float4` → `vec4`, `float3` → `vec3`, `float2` → `vec2`
- `mul(v, M)` → `M * v` (column-major in GLSL)
- Semantics (`COLOR0`, `TEXCOORD0`) become explicit `in`/`out` declarations
- Constants (`cb[0]`, registers) become `uniform` variables

Update `src/HW2HOD/ShaderLibrary.vb`:
- Remove `Direct3D.ShaderLoader` calls
- Use `GL.CreateShader` / `GL.ShaderSource` / `GL.CompileShader` from OpenTK
- Load `.vert`/`.frag` files instead of `.vs`/`.ps`

---

### Phase 5 — Graphics Layer Rewrite
**Risk:** Highest. Core rendering must be preserved.

**Scope:** All of `src/D3DHelper/`, `src/GenericMesh/GMeshPart.vb`, `src/GenericMesh/GMeshTemplates.vb`

#### DirectX 9 → OpenTK/OpenGL Mapping

| DirectX 9 | OpenTK 4 / OpenGL 3.3 |
|-----------|----------------------|
| `Direct3D.Device` | OpenGL context (via `GameWindow` or `NativeWindow`) |
| `Device.CreateVertexBuffer()` | `GL.GenBuffers()` + `GL.BindBuffer(BufferTarget.ArrayBuffer)` |
| `Device.CreateIndexBuffer()` | `GL.GenBuffers()` + `GL.BindBuffer(BufferTarget.ElementArrayBuffer)` |
| `Device.CreateTexture()` | `GL.GenTextures()` + `GL.BindTexture()` |
| `Direct3D.Light` | Uniform structs in shaders |
| `Direct3D.Material` | Uniform `vec4` color/specularity in shaders |
| `Device.CreateVertexShader()` | `GL.CreateShader(ShaderType.VertexShader)` |
| `Device.CreatePixelShader()` | `GL.CreateShader(ShaderType.FragmentShader)` |
| `Device.BeginScene()` / `EndScene()` | No equivalent; just draw calls |
| `Device.Present()` | `SwapBuffers()` |
| Device Lost / Reset | Context loss recovery (less common in modern OpenGL) |
| `Direct3D.Font` | SkiaSharp or OpenTK's text rendering for FPS overlay |

#### Files to Rewrite

| Current File | Action | New File |
|-------------|--------|---------|
| `D3DManager.vb` | Rewrite | `GLManager.vb` — OpenGL context lifecycle, render loop |
| `D3DConfigurer.vb` | Rewrite | `GLConfigurer.vb` — display enumeration, MSAA, vsync |
| `D3DHelper.vb` | Update | Remove assembly reflection; initialize OpenTK |
| `TextDisplay/FontCache.vb` | Rewrite | Use SkiaSharp or OpenTK.Graphics text for FPS display |
| `GenericMesh/GMeshPart.vb` | Replace DirectX buffer types | Use `int` handle from `GL.GenBuffers()` |
| `GenericMesh/GMeshTemplates.vb` | Replace DirectX vertex types | Use `float[]` or custom structs with `StructLayout` |

#### Render Thread Model

The current render loop uses WinForms' `Control.Handle` (an `IntPtr` to the HWND) as the DirectX render target. With Avalonia + OpenTK:

- Use Avalonia's `NativeControlHost` to embed an OpenTK `NativeWindow`
- Or use Avalonia's `OpenGL` control (Avalonia.OpenGL) to get a GL surface
- The render thread continues on a background thread; synchronization with Avalonia via `Dispatcher.UIThread`

---

### Phase 6 — UI Layer Migration
**Risk:** High (surface area). Logic risk is low since UI is being restructured, not rewritten.

**Scope:** All of `src/CFHodEd/Forms/`

#### Avalonia Concepts

| WinForms | Avalonia |
|---------|---------|
| `Form` | `Window` |
| `UserControl` | `UserControl` |
| `Panel` | `Panel` / `Border` |
| `TreeView` | `TreeView` |
| `ListView` | `ListBox` / `DataGrid` |
| `MenuStrip` | `Menu` / `NativeMenu` |
| `ToolStrip` | `ToolBar` |
| `TabControl` | `TabControl` |
| `OpenFileDialog` | `StorageProvider.OpenFilePickerAsync()` |
| `SaveFileDialog` | `StorageProvider.SaveFilePickerAsync()` |
| `FolderBrowserDialog` | `StorageProvider.OpenFolderPickerAsync()` |
| `Application.StartupPath` | `AppContext.BaseDirectory` |
| `Application.Run(form)` | `AppBuilder.Configure<App>().UsePlatformDetect().StartWithClassicDesktopLifetime(args)` |
| `Control.Invoke(delegate)` | `Dispatcher.UIThread.InvokeAsync(delegate)` |

#### Migration Per Form

Each form gets:
1. A `.axaml` file for the visual layout (Avalonia XAML)
2. The existing `.vb` file becomes the code-behind (minimal changes to event handlers and logic)

Priority order (start with simpler dialogs, finish with main window):

1. `AboutBox.vb` — static info, trivial
2. `ExceptionDisplay.vb` — displays exception text, trivial
3. `IOResult.vb` — shows import/export result, simple
4. `HODType.vb` — radio buttons for HOD variant, simple
5. `Options.vb` — settings form
6. `JointSelector.vb` / `JointTemplates.vb` — list-based selection
7. `LightEditor.vb` — sliders and inputs
8. `MeshTransformer.vb` — numeric inputs
9. `MaterialSubstitute.vb` — list + replacement controls
10. `TexturePreview.vb` — needs image display control
11. `HODBGTexGen.vb` — texture generation UI
12. `HODEditorA.vb` — main window (largest; do last)

#### 3D Viewport Embedding

`HODEditorA.vb` currently hosts a WinForms `Panel` as the Direct3D render target. In Avalonia:

```vb
' Avalonia NativeControlHost embeds the OpenTK NativeWindow
Dim host As New NativeControlHost()
' Attach OpenTK context to host.Handle
```

---

### Phase 7 — Final Target Framework Switch
**Risk:** Low (final validation step).

Once all WinForms code is removed:

1. Change all `.vbproj` targets from `net8.0-windows` → `net8.0`
2. This removes the Windows-only WinForms assembly availability
3. Run `dotnet build` on all platforms to confirm no platform-specific APIs remain

---

## Testing Checklist

### Build Verification
- [ ] `dotnet build src/CFHodEd/CFHodEd.vbproj` succeeds on Windows
- [ ] `dotnet build src/CFHodEd/CFHodEd.vbproj` succeeds on macOS
- [ ] `dotnet build src/CFHodEd/CFHodEd.vbproj` succeeds on Linux

### Functional Verification (all platforms)
- [ ] Application launches without errors
- [ ] Open a `.hod` file — 3D model renders correctly
- [ ] All 8 shaders render with visual parity to the original Windows build
- [ ] Camera rotate, zoom, pan work in the 3D viewport
- [ ] Lights can be added, moved, and removed
- [ ] Save a modified `.hod` — file writes correctly and reloads
- [ ] Import a `.obj` file — geometry appears in viewport
- [ ] Export to `.obj` — file is valid and importable in Blender
- [ ] File open/save dialogs work (native dialogs on each OS)
- [ ] Settings (config JSON) persist across restarts
- [ ] Window layout and sizes restore correctly

### Regression Verification
- [ ] HOD files saved by the new build are loadable by Homeworld 2
- [ ] HOD files saved by the original Windows build load correctly in the new build

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|-----------|
| GLSL shader output differs visually from HLSL | Medium | High | Test against reference screenshots per shader; keep HLSL files as reference |
| Avalonia `NativeControlHost` OpenGL integration complexity | Medium | High | Prototype viewport embedding before migrating all forms |
| VB.NET + Avalonia tooling gaps (no designer) | High | Medium | Write AXAML by hand; reference Avalonia docs and community samples |
| OpenTK VBO/texture API mismatch with existing mesh data layout | Medium | Medium | Audit vertex buffer layouts in `GMeshPart.vb` before rewriting |
| macOS OpenGL deprecation (Apple prefers Metal) | Low | Medium | OpenTK supports OpenGL on macOS via compatibility context; acceptable for a dev tool |
| .NET 8 VB.NET compiler regressions from old code patterns | Low | Low | Fix any warnings/errors incrementally during Phase 1 |

---

## Reference Links

- [OpenTK 4.x documentation](https://opentk.net/learn/index.html)
- [Avalonia UI documentation](https://docs.avaloniaui.net/)
- [Avalonia VB.NET samples](https://github.com/AvaloniaUI/Avalonia-Samples)
- [.NET 8 migration guide from .NET Framework](https://learn.microsoft.com/en-us/dotnet/core/porting/)
- [HLSL to GLSL translation reference](https://anteru.net/blog/2016/mapping-between-hlsl-and-glsl/)
