# CFHodEd.Rendering — Agent Guidance

This library provides a graphics abstraction layer and an OpenGL 3.3 backend. It is **complete but not yet active** — the viewport currently uses a software rasterizer in `CFHodEd.UI/OpenGLViewport.cs`.

## Status

| Component | Status |
|-----------|--------|
| `IRenderDevice` interface | Complete |
| `OpenGLRenderDevice` implementation | Complete |
| Shader files (GLSL 330) | Complete |
| Wiring to `OpenGLViewport` | **Not done** |

The next major rendering milestone is replacing `OpenGLViewport`'s software rasterizer with calls to `OpenGLRenderDevice`. Everything in this library is ready for that work.

## IRenderDevice

The abstraction interface. All rendering operations should go through it. Do not call Silk.NET or raw OpenGL APIs from outside this library.

Key capabilities:
- Viewport and clear
- Create/bind vertex buffers and index buffers
- Create/bind textures (from RGBA byte arrays)
- Create/bind shader programs
- Set uniforms (matrices, vectors, colors, samplers)
- Set blend/cull/fill modes
- Draw calls (DrawPrimitives, DrawIndexed)

To add a new graphics backend (e.g., Vulkan, Metal), implement `IRenderDevice`.

## OpenGLRenderDevice

Uses `Silk.NET.OpenGL` 2.21.0. Initialization requires an `IGL` context from Silk.NET.

Supports:
- **BlendMode**: Opaque, Alpha, Additive, Multiply
- **CullMode**: None, CW, CCW
- **FillMode**: Solid, Wireframe, Point
- **PrimitiveType**: Points, Lines, LineStrip, Triangles, TriangleStrip, TriangleFan
- **IndexType**: 16-bit (`ushort`), 32-bit (`uint`)

## Shaders

All shader files are in `Shaders/` and are **embedded resources**. When adding a new shader:

1. Create `Shaders/myname.vert` and `Shaders/myname.frag`
2. Add to `CFHodEd.Rendering.csproj`:
   ```xml
   <EmbeddedResource Include="Shaders\myname.vert" />
   <EmbeddedResource Include="Shaders\myname.frag" />
   ```
3. Load via `ShaderLoader.LoadFromEmbeddedResource("myname")` (matches by filename stem)

**Naming convention:** `purpose.vert` / `purpose.frag`

### Shader Inventory

| Shader | Vertex | Fragment | Notes |
|--------|--------|----------|-------|
| standard | `standard.vert` | — | Shared vertex shader for all ship materials |
| ship | `standard.vert` | `ship.frag` | Full HW2 ship: team/stripe/glow/specular |
| matte | `standard.vert` | `matte.frag` | Simple diffuse, no specular |
| background | `standard.vert` | `background.frag` | Unlit skybox surfaces |
| thruster | `standard.vert` | `thruster.frag` | Additive engine glow |

All shaders target **GLSL 330 core**. Do not target a higher version without verifying driver support on all platforms.

## Math Compatibility

`CFHodEd.Math.Matrix` is **row-major, left-handed** (DirectX convention). When passing matrices to GLSL uniforms via `OpenGLRenderDevice`, the matrix must be **transposed** before upload because GLSL expects column-major. Handle this in the `SetMatrix` uniform setter, not at the call site.

## StbImageSharp

`StbImageSharp 2.30.15` is available for loading images from files or streams. Use it for texture import from non-HOD sources (PNG, TGA, etc.).
