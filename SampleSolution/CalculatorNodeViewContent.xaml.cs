using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.GeneralPurpose.Wpf;

namespace SampleAddIn
{
    // The following class, CalculatorNodeViewContent, is a WPF user control that defines the
    // appearance and behavior of the Calculator node type within the GC graph.
    //
    // It is (far) beyond the scope of this GC sample project to teach anything about WPF.
    // For that, there are lots of books and online materials.
    //
    // However, following is some information specific to the intercommunication between
    // a GC utility node instance and its corresponding WPF control instance:
    //
    // 1. It is common that the values of one or more technique inputs need to be passed from
    //    the node to the WPF control.
    //
    //    That's the case with this Calculator node type: Suppose the GC user changes the value
    //    of the input, DecimalPlaces, which is a property on the node-type class (Calculator).
    //    We want that changed value to be passed to the WPF control, so that the latter can
    //    adjust the number of decimal places it is showing in its "calculator display".
    //
    //    To accomplish that, we...
    //
    //        a.  Define a dependency property in the WPF control that has the same name
    //            (DecimalPlaces) and type (int) as the node property.
    //
    //        b.  As part of that dependency property's definition, establish an "on changed"
    //            method within the WPF control.
    //
    //        c.  We programmatically establish a WPF binding from the node property to the WPF
    //            dependency property.
    //
    //    Then, within the node class, we can call OnPropertyChanged("DecimalPlaces") or
    //    OnAllPropertiesChanged() to refresh the binding, resulting in our "on changed" method
    //    being called within the WPF control. That "on changed" method can then, in turn,
    //    perform whatever UI adjustments need to be made.
    //
    // 2. While less common, it is often necessary to pass information from the WPF control to the
    //    node, and/or for the former to invoke some functionality on the latter.
    //
    //    That's the case with this Calculator node type: When the GC user calculates a result by
    //    clicking the "=" (equals) key of the virtual calculator, we want that result to be passed
    //    to the node so that the node can (in turn) apply that value to any of its successor/
    //    downstream nodes.
    //
    //    The conventional WPF way to do this would be to use a Command, but we can cut corners
    //    because we know that the WPF control's DataContext is our node instance. So, we can simply
    //    cast the DataContext to the appropriate type (Calculator) and then access that node
    //    instance's properties and methods directly.

    public partial class CalculatorNodeViewContent: UserControl
    {
        // ============================= Types, constants, and static members =============================

        enum Operator
        {
            None, Add, Divide, Subtract, Multiply
        };

        const int MaxDisplayLength = 12;          // Arbitrary upper limit on the number of characters
                                                  // that can be entered into the display, excluding
                                                  // the prefix minus-sign if there is one.

        const double ErrorValue       = 888888;   // What we display to indicate an error condition.

        static public readonly DependencyProperty DecimalPlacesProperty;
        static public readonly DependencyProperty NumberColorProperty;

        static CalculatorNodeViewContent()
        {
            {
                FrameworkPropertyMetadata md = new FrameworkPropertyMetadata(2, (d, e) => ((CalculatorNodeViewContent) d).OnDecimalPlacesChanged(e));
                DecimalPlacesProperty = DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(CalculatorNodeViewContent), md);
            }
            {
                FrameworkPropertyMetadata md = new FrameworkPropertyMetadata(Colors.Yellow, (d, e) => ((CalculatorNodeViewContent) d).OnNumberColorChanged(e));
                NumberColorProperty = DependencyProperty.Register(nameof(NumberColor), typeof(Color), typeof(CalculatorNodeViewContent), md);
            }
        }

        // ============================= Beginning of instance members =============================

        Operator _pendingOperator = Operator.None;  // None indicates there is no pending operation.

        double _pendingOperationFirstOperand;

        bool _nextDigitIsNewEntry = true;

        public CalculatorNodeViewContent()
        {
            InitializeComponent();
            ClearAll();
            this.SetBinding(DecimalPlacesProperty, nameof(CalculatorNode.DecimalPlaces));
            this.SetBinding(NumberColorProperty,   nameof(CalculatorNode.NumberColor));
        }

        public int DecimalPlaces => (int) GetValue(DecimalPlacesProperty);

        public Color NumberColor => (Color) GetValue(NumberColorProperty);

        void OnDecimalPlacesChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_nextDigitIsNewEntry || DisplayText == "")
            {
                double d = ValueFromDisplayText();
                DisplayText = DisplayTextFromValue(ref d);
            }
        }

        void OnNumberColorChanged(DependencyPropertyChangedEventArgs e)
        {
            Color color = (Color) e.NewValue;
            Brush brush = WpfTools.GetSolidColorBrush(color);
            _displayTextBlock.Foreground = brush;
        }

        string DisplayText
        {
            get
            {
                if (_nextDigitIsNewEntry)
                {
                    _displayTextBlock.Text = DisplayTextThatMeansEmpty();
                    _nextDigitIsNewEntry = false;
                }
                string result = _displayTextBlock.Text;
                if (result == DisplayTextThatMeansEmpty())
                    result = "";
                return result;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = DisplayTextThatMeansEmpty();
                _displayTextBlock.Text = value;
            }
        }

        double ValueFromDisplayText()
        {
            double result = 0.0;
            string text = DisplayText;
            if (text.Length > 0)
                result = double.Parse(text);
            return result;
        }

        string DisplayTextFromValue(ref double value)
        {
            // The result of this method could be simply "value.ToString()" if not for these
            // shenanigans regarding the number of decimal places.

            int decimalPlaces = DecimalPlaces;
            value = Math.Round(value, decimalPlaces);
            StringBuilder builder = new StringBuilder();
            builder.Append(value.ToString());
            int index = builder.IndexOf('.');
            if (index < 0)
            {
                if (decimalPlaces > 0)
                {
                    builder.Append('.');
                    builder.Append('0', decimalPlaces);
                }
            }
            else if (decimalPlaces == 0)
                builder.Length = index;
            else
            {
                int currentDecimalPlaces = builder.Length - index - 1;
                if (currentDecimalPlaces > decimalPlaces)
                    builder.Length = index + decimalPlaces;
                else if (decimalPlaces > currentDecimalPlaces)
                    builder.Append('0', decimalPlaces - currentDecimalPlaces);
            }
            return builder.ToString();
        }

        string DisplayTextThatMeansEmpty()
        {
            double zero = 0.0;
            string result = DisplayTextFromValue(ref zero);
            return result;
        }

        void ProcessDigit(int digitValue)
        {
            string text = DisplayText;
            if (text.Length < MaxDisplayLength)
                DisplayText += digitValue.ToString();
        }

        void InitiatePendingOperation(Operator op)
        {
            if (!FinishPendingOperationIfAny(false))  // Ooo! Chain calculations!
                return;  // An error occured (which the previous call simply swallowed).

            _pendingOperator = op;
            _pendingOperationFirstOperand = ValueFromDisplayText();
            _nextDigitIsNewEntry = true;
        }

        bool FinishPendingOperationIfAny(bool pushResultToNode)
        {
            // Extremely crude error handling: If any error occurs during any of these
            // arithmetic operations, this method performs a simple "Clear All" operation
            // and return false. Otherwise, we return true, meaning all's well (or there's
            // nothing to be done).
            //
            // In any case, when this method returns, the current pending operation (if
            // there was one) is cleared.

            bool success = true;
            if (_pendingOperator != Operator.None)
            {
                double firstOperand = _pendingOperationFirstOperand;
                double secondOperand = ValueFromDisplayText();
                double result = 0.0;
                switch (_pendingOperator)
                {
                    case Operator.Add:      result = firstOperand + secondOperand; break;
                    case Operator.Divide:   result = firstOperand / secondOperand; break;
                    case Operator.Multiply: result = firstOperand * secondOperand; break;
                    case Operator.Subtract: result = firstOperand - secondOperand; break;
                }
                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    result = ErrorValue;
                    success = false;
                }
                ClearPendingOperation();
                SetResultDisplay(ref result);
                if (success)
                {
                    CalculatorNode node = (CalculatorNode) DataContext;
                    node.SetResultAndUpdate(result);
                }
            }
            return success;
        }

        void SetResultDisplay(ref double result)
        {
            DisplayText = DisplayTextFromValue(ref result);
            _nextDigitIsNewEntry = true;
        }

        void ClearPendingOperation()
        {
            _pendingOperator = Operator.None;
            _pendingOperationFirstOperand = 0.0;  // Just to be tidy.
        }

        void ClearDisplay()
        {
            DisplayText = "";
            _nextDigitIsNewEntry = true;
        }

        void ClearAll()
        {
            ClearPendingOperation();
            ClearDisplay();
        }

        void _addButton_Click(object sender, RoutedEventArgs e)
        {
            InitiatePendingOperation(Operator.Add);
        }

        void _backspaceButton_Click(object sender, RoutedEventArgs e)
        {
            string text = DisplayText;
            if (text.Length > 0)
                DisplayText = text.Substring(0, text.Length - 1);
        }

        void _changeSignButton_Click(object sender, RoutedEventArgs e)
        {
            string text = DisplayText;
            if (text.Length > 0)
            {
                // Toggle the appearance of a '-' (minus character) at the beginning of the display.
                if (text[0] == '-')
                    DisplayText = text.Substring(1);
                else
                    DisplayText = '-' + text;
            }
        }

        void _clearAllButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        void _clearEntryButton_Click(object sender, RoutedEventArgs e)
        {
            ClearDisplay();
        }

        void _decimalPointButton_Click(object sender, RoutedEventArgs e)
        {
            // Append a decimal point only if there isn't already one.
            string text = DisplayText;
            if (!text.Contains("."))
                DisplayText += '.';
        }

        void _digit0Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(0);
        void _digit1Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(1);
        void _digit2Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(2);
        void _digit3Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(3);
        void _digit4Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(4);
        void _digit5Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(5);
        void _digit6Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(6);
        void _digit7Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(7);
        void _digit8Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(8);
        void _digit9Button_Click(object sender, RoutedEventArgs e) => ProcessDigit(9);

        void _divideButton_Click(object sender, RoutedEventArgs e)
        {
            InitiatePendingOperation(Operator.Divide);
        }

        void _equalsButton_Click(object sender, RoutedEventArgs e)
        {
            FinishPendingOperationIfAny(true);
        }

        void _multiplyButton_Click(object sender, RoutedEventArgs e)
        {
            InitiatePendingOperation(Operator.Multiply);
        }

        void _subtractButton_Click(object sender, RoutedEventArgs e)
        {
            InitiatePendingOperation(Operator.Subtract);
        }
    }
}
