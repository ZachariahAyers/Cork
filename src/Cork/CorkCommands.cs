using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Cork;
using Cork.Core;
using AcadException = Autodesk.AutoCAD.Runtime.Exception;

[assembly: CommandClass(typeof(CorkCommands))]

namespace Cork;

/// <summary>
/// The AutoCAD commands. Each one reads the drawing, asks Cork.Core what to change, and applies it
/// in a single transaction, so a failure leaves the drawing untouched.
/// </summary>
public sealed class CorkCommands
{
    /// <summary>Moves the selected objects so a picked point lands on the XY plane of the current UCS.</summary>
    [CommandMethod("Floor", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
    public static void AlignToFloor()
    {
        var document = Application.DocumentManager.MdiActiveDocument;
        var editor = document.Editor;

        var selection = editor.GetSelection(new PromptSelectionOptions
        {
            MessageForAdding = "\nSelect objects to move to the XY plane: ",
            MessageForRemoval = "\nSelect objects to remove from the selection: ",
            RejectPaperspaceViewport = true,
            RejectObjectsOnLockedLayers = true,
            RejectObjectsFromNonCurrentSpace = true,
        });
        if (selection.Status != PromptStatus.OK)
        {
            return;
        }

        var point = editor.GetPoint(new PromptPointOptions("\nPick the point to place on the XY plane: ") { AllowNone = false });
        if (point.Status != PromptStatus.OK)
        {
            return;
        }

        var zOffset = FloorAlignment.ZOffset(point.Value.Z);
        if (zOffset == 0)
        {
            editor.WriteMessage("\nThat point is already on the XY plane.\n");
            return;
        }

        var displacement = Matrix3d.Displacement(new Vector3d(0, 0, zOffset).TransformBy(editor.CurrentUserCoordinateSystem));

        using var transaction = document.Database.TransactionManager.StartTransaction();
        foreach (var id in selection.Value.GetObjectIds())
        {
            if (transaction.GetObject(id, OpenMode.ForWrite) is Entity entity)
            {
                entity.TransformBy(displacement);
            }
        }

        transaction.Commit();
        editor.WriteMessage("\nSelection aligned with the XY plane.\n");
    }

    /// <summary>
    /// Puts every 3D solid in the drawing on a layer named after its material, creating each layer
    /// with the material's diffuse colour and the material itself.
    /// </summary>
    [CommandMethod("MaterialsToLayers", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
    public static void MaterialsToLayers()
    {
        var document = Application.DocumentManager.MdiActiveDocument;
        var database = document.Database;
        var editor = document.Editor;

        var found = editor.SelectAll(new SelectionFilter([new TypedValue((int)DxfCode.Start, "3DSOLID")]));
        if (found.Status != PromptStatus.OK)
        {
            editor.WriteMessage("\nNo 3D solids found. Solids inside blocks are not included.\n");
            return;
        }

        try
        {
            using var transaction = database.TransactionManager.StartTransaction();

            var ids = found.Value.GetObjectIds();
            var solids = new Solid3d[ids.Length];
            var materials = new Material[ids.Length];
            var inputs = new SolidMaterial[ids.Length];
            for (var i = 0; i < ids.Length; i++)
            {
                solids[i] = (Solid3d)transaction.GetObject(ids[i], OpenMode.ForRead);
                materials[i] = (Material)transaction.GetObject(solids[i].MaterialId, OpenMode.ForRead);
                var layer = (LayerTableRecord)transaction.GetObject(solids[i].LayerId, OpenMode.ForRead);
                inputs[i] = new SolidMaterial(materials[i].Name, layer.IsLocked);
            }

            var layers = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);
            var plan = MaterialLayerPlanner.Plan(inputs, ReadLayers(transaction, layers), IsValidLayerName);

            if (plan.NewLayers.Count > 0)
            {
                layers.UpgradeOpen();
            }

            foreach (var newLayer in plan.NewLayers)
            {
                var material = materials[newLayer.SourceSolid];
                var record = new LayerTableRecord
                {
                    Name = newLayer.Name,
                    Color = Color.FromEntityColor(material.Diffuse.Color.Color),
                    MaterialId = material.ObjectId,
                };
                layers.Add(record);
                transaction.AddNewlyCreatedDBObject(record, true);
            }

            for (var i = 0; i < solids.Length; i++)
            {
                var change = plan.Changes[i];
                if (change.Outcome != SolidOutcome.Moved)
                {
                    continue;
                }

                solids[i].UpgradeOpen();
                solids[i].Layer = change.Layer;
                if (change.UseLayerMaterial)
                {
                    solids[i].Material = "ByLayer";
                }
            }

            transaction.Commit();
            editor.WriteMessage("\n" + plan.Summary() + "\n");
        }
        catch (AcadException ex)
        {
            Application.ShowAlertDialog("MaterialsToLayers made no changes: " + ex.Message);
        }
    }

    private static List<ExistingLayer> ReadLayers(Transaction transaction, LayerTable layers)
    {
        var result = new List<ExistingLayer>();
        foreach (ObjectId id in layers)
        {
            var layer = (LayerTableRecord)transaction.GetObject(id, OpenMode.ForRead);
            var material = (Material)transaction.GetObject(layer.MaterialId, OpenMode.ForRead);
            result.Add(new ExistingLayer(layer.Name, material.Name));
        }

        return result;
    }

    private static bool IsValidLayerName(string name)
    {
        try
        {
            SymbolUtilityServices.ValidateSymbolName(name, false);
            return true;
        }
        catch (AcadException)
        {
            return false;
        }
    }
}
