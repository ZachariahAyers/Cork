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

## Browser demo

[Explore the material-layer planner](https://ayers.repair/opensource/cork/). Edit a small drawing
sample and inspect the moves, new layers, and skipped solids produced by `Cork.Core` in WebAssembly.
The Floor calculator uses the same Z-offset calculation as the AutoCAD command.

The demo does not open or modify DWG files. AutoCAD supplies its layer-name validator in the add-in;
the browser lets you mark sample names valid or invalid explicitly. No sample data is sent to a server.

Build it with `dotnet publish src/Cork.Demo/Cork.Demo.csproj -c Release -o demo-dist`, then serve
`demo-dist/wwwroot` with a static web server. CI deploys that folder after the existing build and tests.
