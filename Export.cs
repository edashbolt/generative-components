using Bentley.DgnPlatformNET.Elements;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.DgnPlatformGC;
using Bentley.GenerativeComponents.ElementBasedNodes;
using Bentley.GenerativeComponents.GCScript;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.GeneralPurpose.Atomic;
using Bentley.GenerativeComponents.ScriptEditor;
using Bentley.GenerativeComponents.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GCCommunity
{
    [GCNamespace("User")]
    [GCNodeTypePaletteCategory("Custom")]
    [GCNodeTypeIcon("Resources/export.png")]
    [GCSummary("Export GC Elements as generic MicroStation Elements, removing GC control.")]

    public class Export : GeometricNode
    {
        /*
        [GCDefaultTechnique]
        [GCSummary("Copies the input geometry nodes into the active model, removing GC control over the elements.")]
        [GCParameter("GeometryToBake", "Input geometry to bake into the active model")]

        public NodeUpdateResult ExportElementsIntoNewFile
        (
            NodeUpdateContext updateContext,
            [GCIn, GCSingularOrPlural, GCDgnModelProvider] GeometricNode[] GeometryToExport,
            [GCIn, GCFileBrowser(
                mode: FileBrowserMode.OutputFile,
                buttonToolTip: "Save location of the newly exported dgn file",
                dialogTitle: "Export file save location",
                fileFilterMask: "*.dgn")] string ExportFile,
            [GCIn, GCFileBrowser(
                mode: FileBrowserMode.InputFile, 
                buttonToolTip: "Select a dgn seed file that will be used to create the new export file", 
                dialogTitle: "Select a seed file",
                fileFilterMask: "*.dgn")] string SeedFile,
            [GCIn] string ModelName,
            [GCIn] bool CreateModelIfNotFound
        )
        {   
            try
            {
                //Check for valid seed file
                if (!File.Exists(SeedFile))
                    throw new Exception("Seed file does not exist: Please check the node inputs before continuing");

                //Copy seed file and rename
                File.Copy(SeedFile, ExportFile, true);
                if (!File.Exists(ExportFile))
                    throw new Exception($"Failed to copy the seed file, please check that you have the correct permissions for writing into the selected export location");

                //Load the dgn
                foreach (GeometricNode node in GeometryToExport)
                {
                    if (!node.Export(this.GCDgnModel(), isExternalDesignFile: false, null))
                        throw new Exception($"Failed to export elements for node '{node.Name}'");
                }
                return NodeUpdateResult.Success;
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
        }


        [GCTechnique]
        [GCSummary("Copies the input geometry nodes into the active model, removing GC control over the elements.")]
        [GCParameter("GeometryToBake", "Input geometry to bake into the active model")]
        public NodeUpdateResult ExportElementsIntoExistingFile
        (
            NodeUpdateContext updateContext,
            [GCIn, GCSingularOrPlural, GCDgnModelProvider] GeometricNode[] GeometryToExport
        )
        {
            try
            {
                
                return NodeUpdateResult.Success;
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
        }
        */

        [GCTechnique]
        [GCSummary("Copies the input geometry nodes into the active model, removing GC control over the elements.")]
        [GCParameter("GeometryToBake", "Input geometry to bake into the active model")]
        public NodeUpdateResult BakeElementsIntoActiveModel
        (
            NodeUpdateContext updateContext,
            [GCIn, GCSingularOrPlural, GCDgnModelProvider] GeometricNode[] GeometryToBake
        )
        {
            try
            {
                DialogResult dlg = MessageBox.Show($"Do you want to bake the geometry from {GeometryToBake.Length} nodes into the active model?", $"{this.Name} - Bake Elements", MessageBoxButtons.YesNo);
                if (dlg == DialogResult.Yes)
                {
                    foreach (GeometricNode node in GeometryToBake)
                    {
                        if (!node.Export(this.GCDgnModel(), isExternalDesignFile: false, null))
                            throw new Exception($"Failed to bake elements for node '{node.Name}'");
                    }
                }
                return NodeUpdateResult.Success;
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
        }
    }
}
