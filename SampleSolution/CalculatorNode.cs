// This file, in conjunction with the files CalculatorNodeViewContent.xml and
// CalculatorNodeViewContent.xaml.cs, defines a new GC node type named Calculator.
// It emulates a simple, four-function handheld calculator; conceptually, it's an
// extremely simplified version of GC's own Value node type.
//
// GC supports two distinct architectures for node types, differentiated by which of the
// two classes, ElementBasedNode or UtilityNode, the node type inherits from.
//
// Element-based node types are designed for creating and/or managing elements;
// typically, graphical elements in the geometry views. The class SimpleLineNode, defined
// elsewhere in this project, is an example.
//
// Utility node types are designed for non-element-related operations with GC's
// graph, having custom appearances and/or behaviors. This class, CalculatorNode, is an
// example: It provides a calculator simulation whose results can be passed along to other
// nodes in the GC model.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.GCScript;
using Bentley.GenerativeComponents.GCScript.GCTypes;
using Bentley.GenerativeComponents.GCScript.NameScopes;
using Bentley.GenerativeComponents.GCScript.ReflectedNativeTypeSupport;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.UtilityNodes;
using Bentley.GenerativeComponents.View;

namespace SampleAddIn
{
    [GCNamespace("User")]                                // The GCNamespace attribute lets us specify
                                                         // where this Calculator node type will appear
                                                         // within GCScript's namespace tree (that
                                                         // is, the namespaces that are perceived by
                                                         // the GC user). This namespace has no relation
                                                         // to our C# namespace, which is (in this case)
                                                         // SampleAddIn.

    [GCNodeTypePaletteCategory("Sample Add-In")]         // The GCNodeTypePaletteCategory attribute
                                                         // lets us specify where this Calculator node
                                                         // type will appear within GC's Node Types
                                                         // panel. So, it will appear within a group
                                                         // named "Sample Add-In".

    [GCNodeTypeIcon("Resources/CalculatorNode.png")]     // The GCNodeTypeIcon attribute lets us specify
                                                         // the graphical image (icon) that will appear
                                                         // on the Calculator node type's button
                                                         // within GC's Node Types panel.

    [GCSummary("An emulation of a simple calculator.")]  // The GCSummary attribute lets us provide
                                                         // a brief description of this node's
                                                         // intended purposed. The text will be
                                                         // displayed when the user hovers over our
                                                         // node type in GC's Node Types panel.

    public class CalculatorNode: UtilityNode  // Every utility node-type class derives from the class
                                              // UtilityNode, or from another class that, itself,
                                              // derives from UtilityNode.
    {
        // You can see that this class is named "CalculatorNode" rather than just "Calculator",
        // which is the name that will be presented to the user. By convention, all classes
        // that define GC node types are named with the suffix "Node".

        // The following static method, which must be named AddAdditionalMembersToGCType and have
        // this signature, is where we define this node type's techniques, as well as the inputs
        // and outputs to each technique.

        static void AddAdditionalMembersToGCType(IGCEnvironment environment, GCType gcType, NativeNamespaceTranslator namespaceTranslator)
        {
            // This Calculator node type has only one technique, which we've named "Default".
            // It takes two inputs, DecimalPlaces and NumberColor, and produces one output,
            // Result. (This is slightly unusual; most techniques of most node types have only
            // inputs, not outputs.)
            //
            // To define each of a technique's inputs and outputs, we call the method,
            // technique.AddParameter. Each of those calls takes the following [C#] arguments:
            //
            //    1. The current environment, which we simply pass along.
            //
            //    2. The name of the input or output property, as it will be seen by the GC user.
            //
            //    3. The data type of the property (in C# terms).
            //
            //    4. The GCScript expression that will become the initial value of that property
            //       whenever the GC user creates a new instance of this node type. This is relevant
            //       only to inputs, although this argument must still be present when we define
            //       outputs (in which case, we simply pass "default(string)").
            //
            //    5. The description of this property, which will seen by the GC user in various
            //       tooltips. The type of this argument is "Ls", which means "localized string".
            //       Since we won't be translating this project into multiple (human) languages,
            //       we can simply use the form Ls.Literal("string").
            //
            //    6. If the property is an output property, it takes a sixth argument,
            //       NodePortRole.TechniqueOutputOnly.

            UtilityNodeTechnique technique1 = gcType.AddDefaultNodeTechnique("Default", DefaultTechnique);

            technique1.AddParameter(environment, nameof(DecimalPlaces), typeof(int), "2",
                                    Ls.Literal("The number of digits to display after the decimal point."));

            technique1.AddParameter(environment, nameof(NumberColor), typeof(Color), "Colors.Yellow",
                                    Ls.Literal("The color of the displayed number."));

            technique1.AddParameter(environment, nameof(Result), typeof(double), default(string),
                                    Ls.Literal("The result of the calculation."),
                                    NodePortRole.TechniqueOutputOnly);
        }

        // The following static method implements this node type's one-and-only technique, "Default".
        // Every technique method implementation must have this signature. (The method's name can
        // be anything.)

        static NodeUpdateResult DefaultTechnique(UtilityNode node, NodeUpdateContext updateContext)
        {
            CalculatorNode calculator = (CalculatorNode) node;  // It's safe to assume that the
                                                                // given node is of this class type,
                                                                // Calculator.

            // Following is an example of an input validity test: Before proceeding, we check
            // whether the input value DecimalPlaces, which is an integer, is in the range 0
            // through 9 (inclusive). (9 is an arbitrary upper limit.)
            //
            // If it's outside of that range, we return a result that indicates the name of the
            // offending property. The GC user will see an error badge on this Calculator node
            // instance, along with a message indicating which input is problematic.

            if (!Range<int>.IsInRange(calculator.DecimalPlaces, 0, 9))
                return new NodeUpdateResult.TechniqueInvalidArguments(nameof(DecimalPlaces));

            // Following is where we define the main functionality of this node technique. It may
            // comprise just a single statement (as it does here), or, more commonly, a block of
            // code that processes the node instance.

            node.NotifyAllPropertiesChanged();  // In the case of this Calculator node type, all
                                                // of the calculator logic is implemented in the
                                                // WPF layer, so this C# method simply forwards
                                                // the new values of DecimalPlaces and NumberColor
                                                // to that layer, using WPF data bindings.

            // Finally, if all's well, we return the indication of success. Other options include,
            // for example, returning a value like this:
            // return new NodeUpdateResult.TechniqueFailureMessage(Ls.Literal("Reason that the technique failed.")).

            return NodeUpdateResult.Success;
        }

        // Beginning of instance members.

        // The following two members -- State and GetInitialState -- are simply "must be defined"
        // methods that every utility node class must have. You can simply copy and
        // paste these two definitions into your own utility classes.

        internal new NodeState State => (NodeState) base.State;

        protected override UtilityNode.NodeState GetInitialState(NodeTechniqueDetermination initialActiveTechniqueDetermination)
                                            => new NodeState(this, initialActiveTechniqueDetermination);

        // The following property associates a WPF framework element with this node type. (Typically,
        // as is the case here, the framework element is a kind of WPF UserControl.) Subsequently,
        // whenever this node type is instantiated, an instance of the framework element will be
        // instantiated, as well, and appear on the face of the node within GC's graph.

        public override Type TypeOfCustomViewContent =>  typeof(CalculatorNodeViewContent);

        // The following three "convenience" properties give us easy access, at the C# level, to
        // the values of this node's technique inputs and outputs. They also allow us to use
        // C#'s 'nameof' operator in various places within this class.
        //
        // (Furthermore, as in the case of this Calculator node type, these properties can serve
        // as binding sources for this node type's WPF layer.)

        public int DecimalPlaces
        {
            get => State.DecimalPlacesProperty.GetNativeValue<int>();
            set => State.DecimalPlacesProperty.SetNativeValueAndInputExpression(value);
        }

        public Color NumberColor
        {
            get => State.NumberColorProperty.GetNativeValue<Color>();
            set => State.NumberColorProperty.SetNativeValueAndInputExpression(value);
        }

        public double Result
        {
            get => State.ResultProperty.GetNativeValue<double>();
            set => State.ResultProperty.SetNativeValueAndInputExpression(value);
        }

        // The following method (specific to this CalculatorNode class) will be called from
        // the WPF layer, whenever the calculator has a new result because the user clicked
        // the "=" (equals) key.

        internal void SetResultAndUpdate(double result)
        {
            Result = result;

            this.UpdateNodeTree(false);  // This is how a node can force an update of its own
                                         // successor/downstream nodes within the GC model.
                                         //
                                         // (Normally, a node doesn't do this; it lets GC's
                                         // infrastructure handle all node updating. However, this
                                         // Calculator node is a special case; we want the updating
                                         // to occur as soon as the user has clicked the "="
                                         // (equals) key on the WPF side.
        }

        // Following is the embedded NodeState class that every utility class must have.
        //
        // This embedded class can be copied-and-pasted from this class to your own node type class,
        // then you can replace the list of properties with the inputs and outputs that are specific
        // to your own class.

        public new class NodeState: UtilityNode.NodeState
        {
            // There must be one NodeProperty field for each unique input and output of your node
            // type, regardless of which technique(s) each input and output belongs to. The order
            // of these fields is irrelevant, but we suggest listing them alphabetically.

            internal readonly UtilityNodeProperty DecimalPlacesProperty;
            internal readonly UtilityNodeProperty NumberColorProperty;
            internal readonly UtilityNodeProperty ResultProperty;

            internal protected NodeState(CalculatorNode parentNode, NodeTechniqueDetermination initialActiveTechniqueDetermination):
                                         base(parentNode, initialActiveTechniqueDetermination)
            {
                // IMPORTANT: This constructor calls 'AddProperty' to get each property field,
                // whereas the following constructor calls 'GetProperty'.

                DecimalPlacesProperty = AddProperty(nameof(DecimalPlaces));
                NumberColorProperty   = AddProperty(nameof(NumberColor));
                ResultProperty        = AddProperty(nameof(Result));
            }

            protected NodeState(NodeState source): base(source)
            {
                // IMPORTANT: This constructor calls 'GetProperty' to get each property field,
                // whereas the preceding constructor calls 'AddProperty'.

                DecimalPlacesProperty = GetProperty(nameof(DecimalPlaces));
                NumberColorProperty   = GetProperty(nameof(NumberColor));
                ResultProperty        = GetProperty(nameof(Result));
            }

            protected new CalculatorNode UtilityNode => (CalculatorNode) base.UtilityNode();

            public override UtilityNode.NodeState Clone() => new NodeState(this);

            // The following method, TryGetDefaultOutputProperty, isn't always relevant;
            // it depends on how you want your node type to behave. It comes into play
            // if the user attaches a wire to this node's "this entire node" output port
            // (in the top right corner of the node image within the graph), or references
            // a node of this type within an expression.
            //
            // This method establishes that, in those circumstances, the value of this
            // overall node treated as the value of just the indicated property (in this
            // case, Result).
            //
            // So, for example, if the user wires from this Calculator node to an input port
            // on another node, that other node will receive the value of this node's Result,
            // rather than receiving an instance of this node as a whole.

            public override bool TryGetDefaultOutputProperty(out INodeProperty property)
            {
                property = ResultProperty;
                return true;
            }
        }
    }
}
