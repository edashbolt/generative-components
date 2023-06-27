using Bentley.DgnPlatformNET.Elements;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.DgnPlatformGC;
using Bentley.GenerativeComponents.ElementBasedNodes;
using Bentley.GenerativeComponents.GCScript;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.GeneralPurpose.Atomic;
using Bentley.GenerativeComponents.View;
using System;
using System.Collections.Generic;
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

    public class Bake : GeometricNode
    {
        [GCDefaultTechnique]
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
