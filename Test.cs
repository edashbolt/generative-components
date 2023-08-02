using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.GCScript;
//using Bentley.GenerativeComponents.GCScript.NameScopes;
//using Bentley.GenerativeComponents.GeneralPurpose;
//using Bentley.GenerativeComponents.MicroStation;
using Bentley.GenerativeComponents.View;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
using Bentley.DgnPlatformNET.DgnEC;
using Bentley.ECObjects.Schema;
using Bentley.ECObjects.Instance;
using Bentley.GenerativeComponents.ElementBasedNodes;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GenerativeComponents.GCScript.GCTypes;
using Bentley.GenerativeComponents.GeneralPurpose.Collections;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.ScriptEditor.Controls.ExpressionEditor;
using Bentley.EC.Persistence.Query;
using System.Windows.Controls;
using System.Linq;
using Bentley.GenerativeComponents.ElementBasedNodes.Specific;
using Bentley.GeometryNET;
using Bentley.GenerativeComponents.MicroStation;

namespace GCCommunity
{
    [GCNamespace("User")]
    [GCNodeTypePaletteCategory("Custom")]
    public class Test : ElementBasedNode
    {

        [GCDefaultTechnique]
        public NodeUpdateResult Default
        (
            NodeUpdateContext updateContext,
            [GCOut] ref LineNode line
        )
        {
            try
            {
                line = CreateConstituentNode<LineNode>(this, "line", true);
                line.ByDPoint3d(new Bentley.GeometryNET.DPoint3d(0, 0, 0), new Bentley.GeometryNET.DPoint3d(10000, 10000, 0));
                line.LatestUpdateResult = NodeUpdateResult.Success;
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
            return NodeUpdateResult.Success;
        }


        [GCTechnique]
        public NodeUpdateResult SetLineElement
        (
            NodeUpdateContext updateContext,
            [GCOut] ref LineNode line
        )
        {
            try
            {
                line = CreateConstituentNode<LineNode>(this, "line", true);
                LineElement lineEle = new LineElement(Session.Instance.GetActiveDgnModel(), null, new Bentley.GeometryNET.DSegment3d(new Bentley.GeometryNET.DPoint3d(0, 0, 0), new Bentley.GeometryNET.DPoint3d(10000, 10000, 0)));
                line.SetElement(lineEle);
                line.LatestUpdateResult = NodeUpdateResult.Success;
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
            return NodeUpdateResult.Success;
        }

        [GCTechnique]
        [GCSummary("Creates a line between a given start and end point, or a list of lines between lists of start points and/or end points.")]
        [GCParameter("StartPoint", "Start point of the line.")]
        [GCParameter("EndPoint", "End point of the line.")]
        public NodeUpdateResult ByPoints
        (
            NodeUpdateContext updateContext,
            [GCDgnModelProvider, GCReplicatable] IPointNode StartPoint,
            [GCReplicatable] IPointNode EndPoint,
            [GCOut] ref double Length
        )
        {
            DPoint3d startPt = StartPoint.GetDPoint3d();
            DPoint3d endPt = EndPoint.GetDPoint3d();

            DPoint3d uorStartPt = NativeDgnTools.FromMUToUOR(startPt);
            DPoint3d uorEndPt = NativeDgnTools.FromMUToUOR(endPt);

            LineElement lineElement = new LineElement(GCDgnModel().DgnModel(), TemplateElement(),
                                                      new DSegment3d(uorStartPt, uorEndPt));

            SetElement(lineElement);
            
            Length = startPt.Distance(endPt);

            return NodeUpdateResult.Success;
        }
    }
}