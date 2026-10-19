# Cork

[![CI](https://github.com/ZachariahAyers/Cork/actions/workflows/ci.yml/badge.svg)](https://github.com/ZachariahAyers/Cork/actions/workflows/ci.yml)

An AutoCAD add-in that keeps 3D models of fabricated parts tidy.

## Features

- `FLOOR` moves the selected objects along Z until a chosen point sits on the XY plane
- `MATERIALSTOLAYERS` moves every 3D solid to a layer named after its material
- Creates missing layers in the material's colour
- Reports how many solids moved, how many layers were created and what was skipped

## Tech stack

C#, .NET 10, AutoCAD .NET API, xUnit.

## Getting started

Requires AutoCAD 2027 on 64-bit Windows to run the add-in, and the .NET 10 SDK to build it.

```bash
git clone https://github.com/ZachariahAyers/Cork.git
cd Cork
dotnet build Cork.slnx --configuration Release
```

The build puts `Cork.dll` and `Cork.Core.dll` side by side in `src/Cork/bin/Release/net10.0-windows/`. In AutoCAD, run `NETLOAD`, pick `Cork.dll` from that folder, then run a command.

## Development

```bash
dotnet restore Cork.slnx
dotnet format Cork.slnx --verify-no-changes
dotnet build Cork.slnx --configuration Release
dotnet test --solution Cork.slnx --configuration Release
```

How it is put together: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## License

GPL-3.0. See [LICENSE](LICENSE).

---

Built by [Zachariah Ayers](https://ayers.repair).
