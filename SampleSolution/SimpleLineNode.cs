// This file contains the definition of a new GC node type named SimpleLine. It is an extremely
// simplified version of GC's own Line node type.
//
// GC supports two distinct architectures for node types, differentiated by which of the
// two classes, ElementBasedNode or UtilityNode, the node type inherits from.
//
// Element-based node types are designed for creating and/or managing elements;
// typically, graphical elements in the geometry views. This class, SimpleLineNode, is an
// example: Its only technique, ByPoints, generates a new graphical line element.
//
// Utility node types are designed for non-element-related operations with GC's
// graph, having custom appearances and/or behaviors. The class CalculatorNode, defined
// elsewhere in this project, is an example.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.ElementBasedNodes;
using Bentley.GenerativeComponents.GCScript;
using Bentley.GenerativeComponents.GCScript.GCTypes;
using Bentley.GenerativeComponents.GCScript.NameScopes;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.MicroStation;
using Bentley.GenerativeComponents.View;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GeometryNET;
//using Bentley.MstnPlatformNET;

namespace SampleAddIn
{
    [GCNamespace("User")]                                     // The GCNamespace attribute lets us
                                                              // specify where this SimpleLine node
                                                              // type will appear within GCScript's
                                                              // namespace tree (that is, the namespaces
                                                              // that are perceived by the GC user).
                                                              // This namespace has no relation to our
                                                              // C# namespace, which is (in this case)
                                                              // SampleAddIn.

    [GCNodeTypePaletteCategory("Sample Add-In")]              // The GCNodeTypePaletteCategory attribute
                                                              // lets us specify where this SimpleLine
                                                              // node type will appear within GC's Node
                                                              // Types panel. So, it will appear within
                                                              // a group named "Sample Add-In".

    [GCNodeTypeIcon("Resources/SimpleLineNode.png")]          // The GCNodeTypeIcon attribute lets us
                                                              // specify the graphical image (icon)
                                                              // that will appear on the SimpleLine node
                                                              // type's button within GC's Node Types
                                                              // panel.

    [GCSummary("A line that connects two points in space.")]  // The GCSummary attribute lets us provide
                                                              // a brief description of this node's
                                                              // intended purposed. The text will be
                                                              // displayed when the user hovers over our
                                                              // node type in GC's Node Types panel.

    public class SimpleLineNode: ElementBasedNode  // Every element-based node-type class derives
                                                   // from the class ElementBasedNode, or from
                                                   // another class type that, itself, derives
                                                   // from ElementBasedNode, such as GeometricNode.
    {
        // You can see that this class is named "SimpleLineNode" rather than just "SimpleLine",
        // which is the name that will be presented to the user. By convention, all classes
        // that define GC node types are named with the suffix "Node".

        // An explicit constructor is neither needed nor wanted. The default constructor
        // is sufficient.

        // protected override void OnInitialized()
        // {
        //     // Here is where we can perform any custom initialization of this class instance.
        //     // (It is NOT necessary to call the base class implementation.)
        // }

        // One of the fundamental differences between the element-based node architecture
        // and the utility node architecture is that, in the former, reflection is used
        // to extract the technique names, the documentation, and the names and types of the inputs
        // and outputs, directly from the compiled C# class. GC uses a number of custom attributes
        // to provide more information to the that reflection process.

        [GCDefaultTechnique]  // Every technique method -- that is, every method that implements
                              // a technique of an element-based node type -- must be marked
                              // with one of the attributes [GCTechnique] or [GCDefaultTechnique].
                              // If a method doesn't have either of those attributes, then it's not
                              // exposed to the GC user; it's just an ordinary C# class method.
                              //
                              // (Only ONE method in the class may have the [GCDefaultTechnique]
                              // attribute.)

        // The following attributes -- GCSummary and GCParameter -- do not affect the functionality
        // of this node within GC. They do, however, enhance the user's experience by providing
        // documentation, which appears in various tooltip / flyover labels in GC's UI.

        [GCSummary("Creates a line between a given start and end point, or a list of lines between lists of start points and/or end points.")]
        [GCParameter("StartPoint", "Start point of the line.")]
        [GCParameter("EndPoint", "End point of the line.")]

        public NodeUpdateResult ByPoints
        (
            NodeUpdateContext                               updateContext,
            [GCDgnModelProvider, GCReplicatable] IPointNode StartPoint,
            [GCReplicatable] IPointNode                     EndPoint,
            [GCOut] ref double                              Length
        )
        {
            // The first parameter of every technique method must be of the type, NodeUpdateContext.
            //
            // The remaining parameters will become the input and output properties that the GC user
            // sees and manipulates.
            //
            // Specific to these particular parameters:
            //
            //   -- The [GCDgnModelProvider] attribute denotes that, if this is a new instance of
            //      SimpleLine, its elements will be added to the DGN model as those of the node
            //      the user inputs to this parameter (StartPoint).
            //
            //   -- The [GCReplicatable] attribute denotes that, at the GC level, the value assigned
            //      to this property may be either a single item or a list. If it's a list, then this
            //      node will become replicated, and (therefore) this method, ByPoints, will be called
            //      multiple times, once for each item in the given list.
            //
            //      (Note that, except in very rare circumstances, you'll never need to worry about
            //      whether the node is in singleton or replicated mode. You simply write your code
            //      to handle the singleton case, and GC will take care of the rest.)
            //
            //   -- The interface type, IPointNode, is implemented by various node types, such as
            //      Point and CoordinateSystem. Essentially, IPointNode represents any node type that
            //      can provide X, Y, and Z coordinates. If, for some reason, you wanted to restrict
            //      this technique to take only actual Point nodes, you would simply change the
            //      parameter type accordingly (from IPointNode to PointNode).
            //
            //   -- Together, the GCOut attribute and the "ref" keyword denote that this parameter
            //      represents an output property, rather than an input property, of this technique.
            //      (This is slightly unusual; most techniques of most node types have only inputs,
            //      not outputs.)
            //
            // We start by checking the validity of the inputs. If necessary, we return a result
            // that indicates which particular inputs are invalid. (Subsequently, the node will be
            // displayed with an error badge and a tooltip showing the names of the invalid inputs.)

            if (StartPoint == EndPoint)
                return new NodeUpdateResult.TechniqueInvalidArguments(nameof(StartPoint), nameof(EndPoint));

            // Okay, the inputs are valid. What follows is the actual functionality of this technique
            // method.

            // Creating a LineElement (Bentley.DgnPlatformNET.Elements.LineElement) will require
            // that we firstly get the DPoint3d coordinates of the nodes that were input by the user.

            DPoint3d startPt = StartPoint.GetDPoint3d();
            DPoint3d endPt   = EndPoint.GetDPoint3d();

            // GC measures things in Master Units, while the element types in
            // Bentley.DgnPlatformNET.Elements work with UORs. So, we need to perform a conversion.

            DPoint3d uorStartPt = NativeDgnTools.FromMUToUOR(startPt);
            DPoint3d uorEndPt   = NativeDgnTools.FromMUToUOR(endPt);

            // Now we can create a LineElement...

            LineElement lineElement = new LineElement(GCDgnModel().DgnModel(), TemplateElement(),
                                                      new DSegment3d(uorStartPt, uorEndPt));

            // ...And associate that LineElement with this instance of SimpleLineNode.

            SetElement(lineElement);

            // Before we can return, we must populate our output parameter. Since, again, GC works
            // in Master Units, our calculation must be based on the Master-Unit representations.

            Length = startPt.Distance(endPt);

            // Finally, if all's well, we return the indication of success. Other options include,
            // for example, returning a value like this:
            // return new NodeUpdateResult.TechniqueFailureMessage(Ls.Literal("Reason that the technique failed.")).

            return NodeUpdateResult.Success;
        }
    } // class
} // namespace

