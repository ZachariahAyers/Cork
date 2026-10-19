# Changelog

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [2.0.0] - 2026-10-19

AutoCAD 2027 add-in with two commands: `Floor` drops parts onto the work plane, and `MaterialsToLayers` sorts 3D solids onto layers named after their materials.

### Added

- Targets AutoCAD 2027 and .NET 10, built from Autodesk's AutoCAD.NET packages.
- One add-in (`Cork`) and one logic library (`Cork.Core`), with unit tests.
- `MaterialsToLayers` assigns the material to each layer it creates, so solids switched to ByLayer
  keep rendering with their material.
- `MaterialsToLayers` reports what it moved and what it skipped, and why. It skips a solid on a
  locked layer and continues, and never creates a layer named ByLayer or ByBlock or from a material
  name that is not a valid layer name.
- `Floor` makes no change and says so when the picked point is already on the XY plane.

[2.0.0]: https://github.com/ZachariahAyers/Cork/tree/v2.0.0
