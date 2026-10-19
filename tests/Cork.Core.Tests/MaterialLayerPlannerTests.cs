using Cork.Core;

namespace Cork.Core.Tests;

public sealed class MaterialLayerPlannerTests
{
    private static readonly ExistingLayer[] NoLayers = [];

    private static bool AnyName(string name) => true;

    [Fact]
    public void CreatesOneLayerPerMaterialFromTheFirstSolidThatUsesIt()
    {
        SolidMaterial[] solids =
        [
            new("Plywood", false),
            new("Steel", false),
            new("plywood", false),
        ];

        var plan = MaterialLayerPlanner.Plan(solids, NoLayers, AnyName);

        NewLayer[] expectedLayers = [new("Plywood", 0), new("Steel", 1)];
        string?[] expectedTargets = ["Plywood", "Steel", "plywood"];
        Assert.Equal(expectedLayers, plan.NewLayers);
        Assert.Equal(expectedTargets, plan.Changes.Select(change => change.Layer));
        Assert.All(plan.Changes, change => Assert.Equal(SolidOutcome.Moved, change.Outcome));
        Assert.All(plan.Changes, change => Assert.True(change.UseLayerMaterial));
    }

    [Fact]
    public void ReusesAnExistingLayerWithoutCreatingIt()
    {
        SolidMaterial[] solids = [new("Steel", false)];
        ExistingLayer[] layers = [new("STEEL", "Steel")];

        var plan = MaterialLayerPlanner.Plan(solids, layers, AnyName);

        Assert.Empty(plan.NewLayers);
        Assert.Equal(new SolidChange(SolidOutcome.Moved, "Steel", true), plan.Changes[0]);
    }

    [Fact]
    public void KeepsTheSolidMaterialWhenTheExistingLayerCarriesAnother()
    {
        SolidMaterial[] solids = [new("Steel", false)];
        ExistingLayer[] layers = [new("Steel", "Global")];

        var plan = MaterialLayerPlanner.Plan(solids, layers, AnyName);

        Assert.Equal(new SolidChange(SolidOutcome.Moved, "Steel", false), plan.Changes[0]);
    }

    [Fact]
    public void LeavesSolidsOnLockedLayersAlone()
    {
        SolidMaterial[] solids = [new("Steel", true)];

        var plan = MaterialLayerPlanner.Plan(solids, NoLayers, AnyName);

        Assert.Empty(plan.NewLayers);
        Assert.Equal(new SolidChange(SolidOutcome.SkippedLockedLayer, null, false), plan.Changes[0]);
    }

    [Theory]
    [InlineData("ByLayer")]
    [InlineData("BYBLOCK")]
    public void SkipsSolidsThatInheritTheirMaterial(string materialName)
    {
        SolidMaterial[] solids = [new(materialName, false)];

        var plan = MaterialLayerPlanner.Plan(solids, NoLayers, AnyName);

        Assert.Empty(plan.NewLayers);
        Assert.Equal(SolidOutcome.SkippedInheritedMaterial, plan.Changes[0].Outcome);
    }

    [Fact]
    public void SkipsMaterialNamesThatAreNotValidLayerNames()
    {
        SolidMaterial[] solids = [new("Steel <raw>", false), new("Steel", false)];

        var plan = MaterialLayerPlanner.Plan(solids, NoLayers, name => !name.Contains('<', StringComparison.Ordinal));

        NewLayer[] expectedLayers = [new("Steel", 1)];
        Assert.Equal(expectedLayers, plan.NewLayers);
        Assert.Equal(SolidOutcome.SkippedInvalidName, plan.Changes[0].Outcome);
        Assert.Equal(SolidOutcome.Moved, plan.Changes[1].Outcome);
    }

    [Fact]
    public void SummaryReportsMovesNewLayersAndEachSkipReason()
    {
        SolidMaterial[] solids =
        [
            new("Plywood", false),
            new("Steel", true),
            new("ByLayer", false),
        ];

        var plan = MaterialLayerPlanner.Plan(solids, NoLayers, AnyName);

        Assert.Equal(
            "Moved 1 of 3 solids to material layers (1 new). Skipped 1 on a locked layer. Skipped 1 with a ByLayer or ByBlock material.",
            plan.Summary());
    }

    [Fact]
    public void SummaryOmitsSkipReasonsThatDidNotHappen()
    {
        SolidMaterial[] solids = [new("Plywood", false)];

        var plan = MaterialLayerPlanner.Plan(solids, NoLayers, AnyName);

        Assert.Equal("Moved 1 of 1 solids to material layers (1 new).", plan.Summary());
    }
}
