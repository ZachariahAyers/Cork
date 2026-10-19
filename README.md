# Cork

Cork is an AutoCAD add-in for 3D models of fabricated parts. It adds two commands that keep a
model of solids tidy: one sets parts down on the work plane, the other sorts every solid onto a
layer named after its material.

Built by Zachariah Ayers - [ayers.repair](https://ayers.repair) - [GitHub](https://github.com/ZachariahAyers)

## Why

A shop model built from 3D solids carries its materials on the solids: plywood, MDF, steel plate,
laminate. Layers are what AutoCAD's filtering, freezing and plotting work from, so Cork turns those
materials into layers, one per material, coloured like the material. `Floor` handles the other chore
of modelling parts: dropping a group of objects so a chosen point sits on the XY plane.

## Requirements

- AutoCAD 2027 on 64-bit Windows. AutoCAD 2027 loads add-ins built for .NET 10
  ([Autodesk developer blog](https://blog.autodesk.io/autocad-2027-sdk-what-every-plugin-developer-needs-to-know/)).
  AutoCAD LT has no .NET API and cannot load Cork.
- To build: the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), on Windows, Linux
  or macOS. The AutoCAD assemblies come from Autodesk's
  [AutoCAD.NET](https://www.nuget.org/packages/AutoCAD.NET) packages, so no AutoCAD install is needed
  to compile.

## Build

```sh
dotnet build Cork.slnx --configuration Release
dotnet test --solution Cork.slnx --configuration Release
```

The add-in is written to `src/Cork/bin/Release/net10.0-windows/`: `Cork.dll` and `Cork.Core.dll`.

## Use

1. Copy `Cork.dll` and `Cork.Core.dll` into one folder.
2. In AutoCAD, run `NETLOAD` and pick `Cork.dll`.
3. Run either command.

### FLOOR

Select objects, then pick a point. The selection moves along the Z axis of the current UCS until
that point is on the UCS XY plane. Objects on locked layers and objects outside the current space
are not selectable.

### MATERIALSTOLAYERS

Finds every 3D solid in the drawing and moves each one to a layer named after its material.

- A missing layer is created with the material's diffuse colour, and the material is assigned to
  the layer.
- When the layer carries the solid's material, the solid's material is set to ByLayer, so it renders
  exactly as before.
- When an existing layer of that name carries a different material, the solid moves to the layer and
  keeps its own material.
- Solids on locked layers, solids whose material is ByLayer or ByBlock, and materials whose names
  are not valid layer names are left alone and counted in the report.
- Solids nested inside blocks are not changed.

The command line reports how many solids moved, how many layers were created and what was skipped.
If AutoCAD rejects any change, the command reports the error and the drawing is left unchanged.

## Structure

| Path | What it is |
|---|---|
| `src/Cork.Core` | The decisions, in plain .NET with no AutoCAD dependency |
| `src/Cork` | The AutoCAD commands: prompts, selection and the drawing transaction |
| `tests/Cork.Core.Tests` | xUnit tests for `Cork.Core` |
| `docs/ARCHITECTURE.md` | How the pieces fit and why |

## License

GPL-3.0. See [LICENSE](LICENSE). Copyright Ayers Electronics Inc.
