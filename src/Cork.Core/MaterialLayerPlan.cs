using System.Globalization;

namespace Cork.Core;

/// <summary>The result of planning MaterialsToLayers for a set of solids.</summary>
/// <param name="NewLayers">Layers to create, in the order their materials were first seen.</param>
/// <param name="Changes">One change per input solid, in input order.</param>
public sealed record MaterialLayerPlan(IReadOnlyList<NewLayer> NewLayers, IReadOnlyList<SolidChange> Changes)
{
    /// <summary>Counts the solids with the given outcome.</summary>
    /// <param name="outcome">The outcome to count.</param>
    /// <returns>The number of solids planned with that outcome.</returns>
    public int Count(SolidOutcome outcome) => Changes.Count(change => change.Outcome == outcome);

    /// <summary>A one-line report for the command line.</summary>
    /// <returns>How many solids moved, how many layers were created and why any solid was skipped.</returns>
    public string Summary()
    {
        var parts = new List<string>
        {
            string.Create(
                CultureInfo.InvariantCulture,
                $"Moved {Count(SolidOutcome.Moved)} of {Changes.Count} solids to material layers ({NewLayers.Count} new)."),
        };

        AddSkipped(parts, SolidOutcome.SkippedLockedLayer, "on a locked layer");
        AddSkipped(parts, SolidOutcome.SkippedInheritedMaterial, "with a ByLayer or ByBlock material");
        AddSkipped(parts, SolidOutcome.SkippedInvalidName, "whose material name is not a valid layer name");

        return string.Join(' ', parts);
    }

    private void AddSkipped(List<string> parts, SolidOutcome outcome, string reason)
    {
        var count = Count(outcome);
        if (count > 0)
        {
            parts.Add(string.Create(CultureInfo.InvariantCulture, $"Skipped {count} {reason}."));
        }
    }
}
