using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using GCCommunity.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCCommunity.Utils
{
    public class GraphicsProcessor : ElementGraphicsProcessor
    {
        private DgnModel _dgnModel = Session.Instance.GetActiveDgnModel();
        private double _uor = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerStorage;
        private bool _addToModel = true;
        private DTransform3d _transform = DTransform3d.Identity;

        public IList<ElementGraphics> output = new List<ElementGraphics>();

        public GraphicsProcessor(bool addToModel = true)
        {
            AnnounceTransform(DTransform3d.Identity);
            _addToModel = addToModel;
        }

        public override bool ExpandPatterns() => false;
        public override bool ExpandLineStyles() => false;
        public override bool WantClipping() => true;
        public override bool ProcessAsFacets(bool isPolyface) => false;
        public override bool ProcessAsBody(bool isCurved) => true;
        
        public override void AnnounceTransform(DTransform3d elementTransform)
        {
            _transform = DTransform3d.Multiply(elementTransform, (1 / _uor));
        }

        public override BentleyStatus ProcessBody(SolidKernelEntity entity)
        {
            Element ele = DraftingElementSchema.ToElement(_dgnModel, entity, null);
            output.Add(new ElementGraphics(ele, ElementType.Solid));
            if (_addToModel) { ele.AddToModel(); }
            return BentleyStatus.Success;
        }

        public override BentleyStatus ProcessSolidPrimitive(SolidPrimitive primitive)
        {
            Element ele = DraftingElementSchema.ToElement(_dgnModel, primitive, null);
            output.Add(new ElementGraphics(ele, ElementType.Solid));
            if (_addToModel) { ele.AddToModel(); }
            return BentleyStatus.Success;
        }

        public override BentleyStatus ProcessSurface(MSBsplineSurface surface)
        {
            Element ele = new BSplineSurfaceElement(_dgnModel, null, surface);
            output.Add(new ElementGraphics(ele, ElementType.Surface));
            if (_addToModel) { ele.AddToModel(); }            
            return BentleyStatus.Success;
        }

        public override BentleyStatus ProcessFacets(PolyfaceHeader meshData, bool isFilled)
        {
            Element ele = new MeshHeaderElement(_dgnModel, null, meshData);
            output.Add(new ElementGraphics(ele, ElementType.Mesh));
            if (_addToModel) { ele.AddToModel(); }
            return BentleyStatus.Success;
        }

        public override BentleyStatus ProcessCurveVector(CurveVector curvevectors, bool isFilled)
        {
            Element ele = DraftingElementSchema.ToElement(_dgnModel, curvevectors, null);
            output.Add(new ElementGraphics(ele, ElementType.Curve));
            if (_addToModel) { ele.AddToModel(); }
            return BentleyStatus.Success;
        }

        public override BentleyStatus ProcessCurvePrimitive(CurvePrimitive curve, bool isClosed, bool isFilled)
        {
            Element ele = DraftingElementSchema.ToElement(_dgnModel, curve, null);
            output.Add(new ElementGraphics(ele, ElementType.Curve));
            if (_addToModel) { ele.AddToModel(); }
            return BentleyStatus.Success;
        }
    }
}
