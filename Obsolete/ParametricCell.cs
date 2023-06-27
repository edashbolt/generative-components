using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.Features;
using Bentley.GenerativeComponents.GCScript;
using Bentley.GenerativeComponents.GCScript.NameScopes;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.MicroStation;
using Bentley.GenerativeComponents.View;
//using Bentley.Interop.MicroStationDGN;

using IElement = Bentley.Interop.MicroStationDGN.Element;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using Bentley.GeometryNET;
using Bentley.DgnPlatformNET.DgnEC;
using Bentley.ECObjects.Schema;
using Bentley.ECObjects.Instance;
using Bentley.GenerativeComponents.Features.Specific;

namespace GCCommunity
{
    [GCNamespace("User")]
    [NodeTypePaletteCategory("Custom")]
    [NodeTypeIcon("Resources/ParametricCell.png")]
    [Summary("Place and modify Parametric Cells.")] 

    public class ParametricCell : Feature
    {
        [DefaultTechnique]
        [Summary("Creates a Parametric Cell.")]
        [Parameter("PlacementPoint", "Cell origin placement point.")]
        [Parameter("Direction", "Cell rotation.")]
        [Parameter("CellLibraryPath", "Cell Library path.")]
        [Parameter("CellDefinitionName", "Cell definition name.")]
        [Parameter("CellVariation", "Cell variation.")]
        [Parameter("CellVariableNames", "Cell variables.")]
        [Parameter("CellVariableValues", "Cell variable values.")]

        public NodeUpdateResult PlaceParametricCell
        (
            NodeUpdateContext updateContext,
            [Replicatable, DgnModelProvider] IPoint PlacementPoint,
            [Replicatable] IPlane PlacementPlane,
            [In] string CellLibraryPath,
            [In] string CellDefinitionName,
            [In] string CellVariation,
            [Replicatable(-1, true)] string[] CellVariableNames,
            [Replicatable(-1, true)] string[] CellVariableValues
        )
        {
            this.ClearAndErase(updateContext); // Remove old feature
            
            if (this.ReplicationIndex == 0 && CellLibraryPath == null || CellDefinitionName == null)
            {
                return new NodeUpdateResult.IncompleteInputs(CellLibraryPath, CellDefinitionName);
            }
            else
            {
                //NEED TO IMPLEMENT A SHARED CELL UPDATE IF UpdateSharedCell == true; THEN ONCE UPDATED SET THE PARAMETER TO FALSE SO THAT THE NEXT PLACED CELL DOESN'T UPDATE THE SHARED CELL AGAIN
                //Check if cell library is attached and if not attach it
                if (this.ReplicationIndex == 0 && MSApp.IsCellLibraryAttached == false || MSApp.AttachedCellLibrary.FullName != CellLibraryPath)
                {
                    MSApp.AttachCellLibrary(CellLibraryPath);
                }
                
                DgnFile activeDgnFile = Session.Instance.GetActiveDgnFile();
                DgnModel activeModel = Session.Instance.GetActiveDgnModel();
                DgnModel cellModel = null;
                ParametricCellElement pCell = null;

                ParametricCellDefinitionElement cellDef = ParametricCellDefinitionElement.FindByName(CellDefinitionName, activeDgnFile);

                if (cellDef == null) //cell not in active file, load from attached cell library
                {
                    var opts = CellLibraryOptions.Include3d | CellLibraryOptions.IncludeAllLibraries | CellLibraryOptions.IncludeParametric;
                    var libs = new CellLibraryCollection(opts);

                    foreach (var lib in libs)
                    {
                        if (CellDefinitionName.Equals(lib.Name))
                        {
                            StatusInt status;
                            cellModel = lib.File.LoadRootModelById(out status, lib.File.FindModelIdByName(lib.Name), true, false, true);
                            break;
                        }
                    }

                    if (null == cellModel) //Cell definition (model) doesn't exist in the cell model file
                    {
                        LsBuilder lsBuilder = new LsBuilder();
                        Ls ls = lsBuilder.AppendLineLiteral("Error loading cell definition. Check cell definition name and library are correct.").ToLs();
                        return new NodeUpdateResult.TechniqueFailureMessage(ls);
                    }
                    else
                    {
                        var hdlr = DgnComponentDefinitionHandler.GetForModel(cellModel);
                        var status = hdlr.DefinitionModelHandler.CreateCellDefinition(activeDgnFile);

                        if (ParameterStatus.Success == status)
                            cellDef = ParametricCellDefinitionElement.FindByName(CellDefinitionName, activeDgnFile);
                        else
                        {
                            LsBuilder lsBuilder = new LsBuilder();
                            Ls ls = lsBuilder.AppendLineLiteral("Error creating cell definition in active file.").ToLs();
                            return new NodeUpdateResult.TechniqueFailureMessage(ls);
                        }
                    }
                }
                

                try
                {
                    pCell = ParametricCellElement.Create(cellDef, CellVariation, activeModel);

                    //Cell origin point - adjusted for U.O.R.
                    double uor = MSApp.ActiveModelReference.UORsPerMasterUnit;
                    DPoint3d cellOrigin = DPoint3d.Multiply(uor, PlacementPoint.GetDPoint3d());
                    pCell.Origin = cellOrigin;

                    DTransform3d dTransform3D = PlacementPlane.GetDTransform3d();
                    DPlane3d dPlane3D = PlacementPlane.GetDPlane3d();

                    DMatrix3d dMatrix3D = new DMatrix3d(dTransform3D);

                    pCell.Rotation = dMatrix3D;

                    //pCell.IsInvisible = true; //We don't want multiple elements visible
                    pCell.AddToModel(); //Add to the model so we can assign variables and retrieve the element

                    //Assign custom variables if they exist
                    if (CellVariableNames != null && CellVariableValues != null)
                    {
                        IECClass ecClass = pCell.Parameters.ClassDefinition;
                        IDgnECInstance eci = DgnECManager.FindECInstanceOnElement(pCell, ecClass);

                        for (int i = 0; i < CellVariableNames.Length; ++i)
                        {                            
                            IEnumerator<IECPropertyValue> props = eci.GetEnumerator(false, true);
                            
                            while (props.MoveNext())
                            {
                                if (props.Current.Property.DisplayLabel == CellVariableNames[i].ToString())
                                {
                                    props.Current.StringValue = CellVariableValues[i].ToString();
                                }
                            }
                        }
                        eci.WriteChanges();
                    }
                    
                    IElement ele = MSApp.ActiveModelReference.GetLastValidGraphicalElement();

                    SetElement(ele); //Commit as a GC feature
                    pCell.Dispose(); //Clean up the memory of pCell, only 'ele' will be retained
                }
                catch (Exception ex)
                {
                    return new NodeUpdateResult.TechniqueException(ex);
                }

                return NodeUpdateResult.Success;
            }
        }

        //NewTechnique
        [Technique]
        [Summary("Drops a Parametric Cell to an orphan cell.")]
        [Parameter("ParametricCell", "The Parametric Cell instance/s from which to extract geometry.")]

        public NodeUpdateResult DropParametricCellToOrphanCell
        (
            NodeUpdateContext updateContext,
            [Replicatable, DgnModelProvider] object ParametricCell,
            [Out, Replicatable] ref string ElementPath
        )
        {
            
            try
            {
                Feature feat = ParametricCell as Feature;
                long id = feat.Element.ID;

                GCModel gcmodel = feat.GCModel();
                DgnModel model = Session.Instance.GetActiveDgnModel();
                Element element = model.FindElementById(new ElementId(ref id));

                if (element is ParametricCellElement instance)
                {
                    MessageCenter.Instance.StatusMessage = $"Parametric cell: {instance.CellDefinition.CellName}";

                    List<Element> extracted = new List<Element>();
                    CellHeaderElement group = (CellHeaderElement)null;
                    int n = 0;

                    try
                    {
                        ParametricCellProcessor processor = new ParametricCellProcessor(extracted, model);
                        ElementGraphicsOutput.Process(instance, processor);
                        
                        group = CellHeaderElement.CreateOrphanCellElement(model, n.ToString(), extracted);
                        group.AddToModel();
                        IElement fetch = MSApp.ActiveModelReference.GetLastValidGraphicalElement();

                        this.SetElement(fetch);
                        
                        Bentley.Interop.MicroStationDGN.Element ele = this.GetElement();
                        string elementPath;
                        DgnTools.TryGetPersistentPath(ele, out elementPath);
                        ElementPath = elementPath.ToString();
                    }
                    finally
                    {
                        if (null != extracted)
                        {
                            foreach (Element el in extracted)
                            {
                                el.Dispose();
                                group.Dispose();
                            }
                        }
                    }
                }
                else
                {
                    LsBuilder lsBuilder = new LsBuilder();
                    Ls ls = lsBuilder.AppendLineLiteral("Input element is not of the correct type.").ToLs();
                    return new NodeUpdateResult.TechniqueFailureMessage(ls);
                }
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }

            return NodeUpdateResult.Success;
        }
        /*
        //NewTechnique
        [Technique]
        [Summary("Drops a Parametric Cell to geometry primitives.")]
        [Parameter("ParametricCell", "The Parametric Cell instance/s from which to extract geometry.")]

        public NodeUpdateResult DropParametricCell
        (
            NodeUpdateContext updateContext,
            [Replicatable, DgnModelProvider] object ParametricCell2,
            [Replicatable, Out] ref Bentley.GenerativeComponents.Features.Specific.Polygon Polygons,
            [Replicatable, Out] ref string ElementPath
        )
        {
            try
            {
                Feature feat = ParametricCell2 as Feature;
                long id = feat.Element.ID;

                GCModel gcmodel = this.GCModel();
                DgnModel model = Session.Instance.GetActiveDgnModel();
                Element element = model.FindElementById(new ElementId(ref id));

                //ParametricCellProcessor processor = new ParametricCellProcessor();

                if (element is ParametricCellElement instance)
                {
                    MessageCenter.Instance.StatusMessage = $"Parametric cell: {instance.CellDefinition.CellName}";

                    List<Element> extracted = new List<Element>();
                    List<IElement> newElements = new List<IElement>();
                    int n = 0;

                    try
                    {
                        ParametricCellProcessor processor = new ParametricCellProcessor(extracted, model);
                        ElementGraphicsOutput.Process(instance, processor);

                        foreach (Element el in extracted)
                        {
                            el.AddToModel();
                            IElement ele = MSApp.ActiveModelReference.GetLastValidGraphicalElement();
                            
                            if (ele != null)
                            {
                                ElementPromotionOptionList options = new ElementPromotionOptionList();
                                if (Bentley.GenerativeComponents.Features.Specific.Polygon.CanPromote(gcmodel, ele, options))
                                {
                                    newElements.Add(ele);
                                    
                                }
                            }


                            if (ele == null) //TO REMOVE THIS FROM TESTING.........................................................................SET TO != IF USING THIS FOR TESTING
                            {
                                Feature feature = (Feature)null;
                                ElementPromotionOptionList options = new ElementPromotionOptionList();
                                if (Circle.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Circle>();
                                else if (Arc.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Arc>();
                                else if (Ellipse.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Ellipse>();
                                else if (EllipticalArc.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<EllipticalArc>();
                                else if (Bentley.GenerativeComponents.Features.Specific.Point.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Bentley.GenerativeComponents.Features.Specific.Point>();
                                else if (Line.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Line>();
                                else if (Cone.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Cone>();
                                else if (Bentley.GenerativeComponents.Features.Specific.Mesh.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Bentley.GenerativeComponents.Features.Specific.Mesh>();
                                else if (BSplineCurve.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<BSplineCurve>();
                                else if (BSplineSurface.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<BSplineSurface>();
                                else if (Bentley.GenerativeComponents.Features.Specific.Polygon.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Bentley.GenerativeComponents.Features.Specific.Polygon>();
                                else if (PolyLine.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<PolyLine>();
                                else if (Solid.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Solid>();
                                else if (Text.CanPromote(gcmodel, ele, options))
                                    feature = (Feature)this.CreateFeatureThatWillBecomeAConstituentOfThis<Text>();
                                if (feature != null)
                                {
                                    feature.SetElement(ele);
                                    feature.AddDefaultConstructionBasedOnElements();
                                    this.AddReplicatedChildFeature(feature, "ChildElements", n);
                                    ++n;
                                }
                            }

                            //this.SetReplicatedChildFeature();

                            //newElements.Add(ele);

                            //IElement ele = MSApp.ActiveModelReference.GetElementByID(el.ElementId);
                            //Feature child;
                            //child.ReplicatedChildFeatures();
                            //Feature child = CreateTemporaryFeature<Feature>(gcmodel);
                            //SetReplicatedChildFeature(n, child);
                            //++n;
                            //ele.tryget
                            //GeometryFeature gf = GeometryFeature.SelectFeatureTypeAndCreateConstituentFeatureBasedOnElement(ele);

                            //Feature nfeat = Feature.CreateAnonymousConstituentOfSameTypeAs(this);
                            //nfeat.;
                            //this.AddReplicatedChildFeatures()
                        }

                        IEnumerable<IElement> ie = newElements;
                        //this.IndexibleSubFeatures();
                        //this.SetElements(ie);

                        Feature poly = CreateAnonymousConstituentOfSameTypeAs(Polygons);
                        poly.SetElements(ie);
                        Polygons = (Bentley.GenerativeComponents.Features.Specific.Polygon)poly;
                        //Polygons.SetElements(ie);
                        //SetElement(null);
                    }
                    finally
                    {
                        if (null != extracted)
                        {
                            foreach (Element el in extracted)
                            {
                                el.Dispose();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }

            return NodeUpdateResult.Success;
        }
        */
    } // class


    internal class ParametricCellProcessor : Bentley.DgnPlatformNET.ElementGraphicsProcessor
    {
        private readonly IList<Element> elements;
        private readonly DgnModel dgnModel;
        private DTransform3d trans;

        private ParametricCellProcessor()
        {
        }

        public ParametricCellProcessor(IList<Element> elements, DgnModel dgnModel)
        {
            this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
            this.dgnModel = dgnModel ?? throw new ArgumentNullException(nameof(dgnModel));
        }

        public override void AnnounceTransform(DTransform3d trans)
        {
            this.trans = trans;
        }

        public override bool ProcessAsBody(bool isCurved) => false;

        public override bool ProcessAsFacets(bool isPolyface) => false;

        public override BentleyStatus ProcessCurveVector(CurveVector curves, bool isFilled)
        {
            Element element = DraftingElementSchema.ToElement(this.dgnModel, curves, null);
            element.ApplyTransform(new TransformInfo(this.trans));
            this.elements.Add(element);
            return BentleyStatus.Success;
        }
    public override BentleyStatus ProcessCurvePrimitive(CurvePrimitive curve, bool isClosed, bool isFilled) => BentleyStatus.Error;
    }

} // namespace