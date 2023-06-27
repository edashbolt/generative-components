using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.ElementBasedNodes;
using Bentley.GenerativeComponents.GCScript;
using Bentley.GenerativeComponents.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GCCommunity.Utils;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
using Bentley.GenerativeComponents.ElementBasedNodes.Specific;
using GCCommunity.Extensions;
using Bentley.GenerativeComponents.DgnPlatformGC;
using Bentley.GenerativeComponents.MicroStation;

namespace GCCommunity
{
    [GCNamespace("User")]
    [GCNodeTypePaletteCategory("Custom")]
    [GCNodeTypeIcon("Resources/import.png")]
    [GCSummary("Import MicroStation Elements into GC")]

    public class Import : GeometricNode
    {
        [GCDefaultTechnique]
        [GCSummary("Import MicroStation geometry as GC Elements")]
        [GCParameter("ElementPaths", "The display path for the selected elements, to be consumed by GC nodes using the 'ByElement' technique")]
        public NodeUpdateResult ImportSelectedElements
        (
            NodeUpdateContext updateContext,
            [GCOut, GCInitiallyPinned] ref IGCObject[] ElementPaths
        )
        {
            try
            {
                //Get selected elements
                List<Element> selectedElements = Utils.Selection.GetSelectedElements();

                //If none selected return
                if (!selectedElements.Any())
                    return new NodeUpdateResult.SuccessButWithWarning(Ls.Literal("No elements selected"));

                //Clear previous values
                if (this.ReplicationIndex < 1)
                {
                    this.ClearReplicatedChildNodes();
                }

                //Instantiate variables
                List<IGCObject> importedElementPaths = new List<IGCObject>();
                List<Element> importedElements = new List<Element>();

                DgnModel model = Session.Instance.GetActiveDgnModel();
                Boxer boxer = GCEnvironment().Boxer;

                foreach (Element element in selectedElements)
                {
                    try
                    {
                        Element clone = element.Clone();
                        if (clone != null && clone.IsPersistent && NativeDgnTools.TryGetPersistentPathOfElement(out string persistentPath, clone))
                        {
                            importedElementPaths.Add(boxer.Box(persistentPath));
                            importedElements.Add(clone);
                        }
                    }
                    catch { }
                }

                //Set outputs
                ElementPaths = importedElementPaths.ToArray();
                this.SetElements(importedElements);
                this.SetUpdateDeferral(NodeUpdateDeferralOption.UntilUpdateAll);

                return NodeUpdateResult.Success;
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
        }
    }
}
