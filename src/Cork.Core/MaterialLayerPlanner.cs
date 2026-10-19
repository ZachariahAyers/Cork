namespace Cork.Core;

/// <summary>
/// Decides how MaterialsToLayers sorts solids onto layers named after their materials.
/// </summary>
public static class MaterialLayerPlanner
{
    private static readonly string[] InheritedMaterialNames = ["ByLayer", "ByBlock"];

    /// <summary>Plans the layer for every solid.</summary>
    /// <param name="solids">The solids to sort.</param>
    /// <param name="existingLayers">Layers already in the drawing.</param>
    /// <param name="isValidLayerName">Returns true when a name can be used as a layer name.</param>
    /// <returns>The layers to create and one change per solid.</returns>
    /// <remarks>Layer names are compared case-insensitively, as AutoCAD compares symbol names.</remarks>
    public static MaterialLayerPlan Plan(
        IReadOnlyList<SolidMaterial> solids,
        IEnumerable<ExistingLayer> existingLayers,
        Func<string, bool> isValidLayerName)
    {
        ArgumentNullException.ThrowIfNull(solids);
        ArgumentNullException.ThrowIfNull(existingLayers);
        ArgumentNullException.ThrowIfNull(isValidLayerName);

        var layerMaterials = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var layer in existingLayers)
        {
            layerMaterials.TryAdd(layer.Name, layer.MaterialName);
        }

        var newLayers = new List<NewLayer>();
        var changes = new List<SolidChange>(solids.Count);

        for (var i = 0; i < solids.Count; i++)
        {
            var solid = solids[i];
            var name = solid.MaterialName;

            if (solid.IsOnLockedLayer)
            {
                changes.Add(new SolidChange(SolidOutcome.SkippedLockedLayer, null, false));
            }
            else if (InheritedMaterialNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                changes.Add(new SolidChange(SolidOutcome.SkippedInheritedMaterial, null, false));
            }
            else if (!isValidLayerName(name))
            {
                changes.Add(new SolidChange(SolidOutcome.SkippedInvalidName, null, false));
            }
            else
            {
                if (layerMaterials.TryAdd(name, name))
                {
                    newLayers.Add(new NewLayer(name, i));
                }

                var layerCarriesMaterial = string.Equals(layerMaterials[name], name, StringComparison.OrdinalIgnoreCase);
                changes.Add(new SolidChange(SolidOutcome.Moved, name, layerCarriesMaterial));
            }
        }

        return new MaterialLayerPlan(newLayers, changes);
    }
}
