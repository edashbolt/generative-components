using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.GCScript;
using Bentley.GenerativeComponents.View;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
using Bentley.DgnPlatformNET.DgnEC;
using Bentley.GenerativeComponents.ElementBasedNodes;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GenerativeComponents.GCScript.GCTypes;
using Bentley.GenerativeComponents.GeneralPurpose.Collections;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.ScriptEditor.Controls.ExpressionEditor;
using Bentley.GenerativeComponents.ElementBasedNodes.Specific;
using GCCommunity.Utils;
using Bentley.GenerativeComponents.DgnPlatformGC;
using Bentley.GenerativeComponents.MicroStation;
using GCCommunity.Objects;

namespace GCCommunity
{
    [GCNamespace("User")]
    [GCNodeTypePaletteCategory("Custom")]
    [GCNodeTypeIcon("Resources/ParametricCell.png")]
    [GCSummary("Parametric Cell Node extensions")]
    public class ParametricCellUtils : ElementBasedNode
    {
        [GCDefaultTechnique]
        [GCSummary("Parametric Cell Node extensions")]
        [GCParameter("ParametricCell", "The Parametric Cell from which to extract geometry")]
        [GCParameter("SolidElementPaths", "The display path for all solid elements found within the parametric cell, to be consumed by a SolidNode using the 'ByElement' technique")]
        [GCParameter("SurfaceElementPaths", "The display path for all surface elements found within the parametric cell, to be consumed by a BSplineSurfaceNode using the 'ByElement' technique")]
        [GCParameter("MeshElementPaths", "The display path for all mesh elements found within the parametric cell, to be consumed by a MeshNode using the 'ByElement' technique")]
        [GCParameter("CurveElementPaths", "The display path for all curve elements found within the parametric cell, to be consumed by a CurveNode using the 'ByElement' technique")]
        public NodeUpdateResult DropParametricCellToOrphanCell
        (
            NodeUpdateContext updateContext,
            [GCDgnModelProvider, GCReplicatable] ParametricCellNode ParametricCell,
            [GCOut, GCReplicatable, GCInitiallyPinned] ref IGCObject[] SolidElementPaths,
            [GCOut, GCReplicatable, GCInitiallyPinned] ref IGCObject[] SurfaceElementPaths,
            [GCOut, GCReplicatable, GCInitiallyPinned] ref IGCObject[] MeshElementPaths,
            [GCOut, GCReplicatable, GCInitiallyPinned] ref IGCObject[] CurveElementPaths
        )
        {
            //Clear output properties for first replicated index
            if (this.ReplicationIndex < 1)
                this.ClearReplicatedChildNodes();
                        
            try
            {
                //Process graphics for cell
                Element cell = ParametricCell.Element();
                GraphicsProcessor processor = new GraphicsProcessor();
                ElementGraphicsOutput.Process(cell, processor);

                //Instantiate type lists
                List<IGCObject> solids = new List<IGCObject>();
                List<IGCObject> surfaces = new List<IGCObject>();
                List<IGCObject> meshes = new List<IGCObject>();
                List<IGCObject> curves = new List<IGCObject>();
                
                List<Element> importedElements = new List<Element>();
                Boxer boxer = GCEnvironment().Boxer;

                foreach (ElementGraphics output in processor.output)
                {
                    if (output.Element.IsPersistent && NativeDgnTools.TryGetPersistentPathOfElement(out string persistentPath, output.Element))
                    {
                        //Add to imported list
                        importedElements.Add(output.Element);

                        //Add to specific element type path lists
                        switch (output.Type)
                        {
                            case ElementType.Solid:
                                solids.Add(boxer.Box(persistentPath));
                                break;
                            case ElementType.Surface:
                                surfaces.Add(boxer.Box(persistentPath));
                                break;
                            case ElementType.Mesh:
                                meshes.Add(boxer.Box(persistentPath));
                                break;
                            case ElementType.Curve:
                                curves.Add(boxer.Box(persistentPath));
                                break;
                        }
                    }
                }

                //Assign to node outputs
                SolidElementPaths = solids.ToArray();
                SurfaceElementPaths = surfaces.ToArray();
                MeshElementPaths = meshes.ToArray();
                CurveElementPaths = curves.ToArray();

                //Set elements in node
                this.SetElements(importedElements);
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }

            return NodeUpdateResult.Success;
        }
    }
}
