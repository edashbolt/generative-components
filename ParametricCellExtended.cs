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
    public class ParametricCellExtended : ElementBasedNode
    {
        [GCDefaultTechnique]
        [GCSummary("Parametric Cell Node extended techniques")]
        public NodeUpdateResult DropParametricCellToOrphanCell
        (
            NodeUpdateContext updateContext,
            [GCDgnModelProvider, GCReplicatable] ParametricCellNode ParametricCell,
            //[GCOut, GCReplicatable] ref CellNode Cells
            [GCOut, GCReplicatable] ref string CellElementPaths
        )
        {
            try
            {
                //Process graphics for cell
                Element cell = ParametricCell.Element();
                GraphicsProcessor processor = new GraphicsProcessor();
                ElementGraphicsOutput.Process(cell, processor);

                //Create cell
                if (processor.output.Count > 0)
                {
                    CellHeaderElement group = CellHeaderElement.CreateOrphanCellElement(Session.Instance.GetActiveDgnModel(), this.NamePath(ScriptingLanguage.None).BasicName, processor.output.Select(x => x.Element).ToList());
                    if (group != null)
                    {
                        this.SetElement(group);
                        /*
                        Cells = CreateConstituentNode<CellNode>(this, "Cells", true);
                        Cells.SetElement(group);
                        Cells.LatestUpdateResult = NodeUpdateResult.Success;
                        */
                        NativeDgnTools.TryGetPersistentPathOfElement(out string persistentPath, this.Element());
                        CellElementPaths = persistentPath;
                    }
                    else
                        throw new Exception("An error occured creating the orphan cell");
                }
                else
                    throw new Exception("Unable to extract elements from the parametric cell");
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }

            return NodeUpdateResult.Success;
        }
        /*
        [GCTechnique]
        [GCSummary("Parametric Cell Node extended techniques")]
        public NodeUpdateResult ExtractSolids
        (
            NodeUpdateContext updateContext,
            [GCDgnModelProvider, GCReplicatable] ParametricCellNode ParametricCell,
            [GCOut, GCReplicatable] ref SolidNode Solids
        )
        {
            try
            {
                //Process graphics for cell
                Element cell = ParametricCell.Element();
                GraphicsProcessor processor = new GraphicsProcessor();
                ElementGraphicsOutput.Process(cell, processor);

                //Create cell
                if (processor.output.Count > 0)
                {
                    Solids = CreateConstituentNode<SolidNode>(this, "Solids", true);
                    Solids.SetElements(processor.output.Where(x => x.Type == ElementType.Solid).Select(x => x.Element));
                    Solids.LatestUpdateResult = NodeUpdateResult.Success;
                }
                else
                    throw new Exception("Unable to extract elements from the parametric cell");
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }

            return NodeUpdateResult.Success;
        }*/
    }
}
