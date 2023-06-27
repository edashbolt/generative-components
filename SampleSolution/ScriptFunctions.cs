using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.GCScript;
using Bentley.GenerativeComponents.GCScript.FundamentalValues;
using Bentley.GenerativeComponents.GCScript.GCTypes;
using Bentley.GenerativeComponents.GCScript.NameScopes;

namespace SampleAddIn
{
    static internal class ScriptFunctions  // Note this is a static class.
    {
        static internal void Load()
        {
            // This method is called from within the constructor of the class, Initializer (elsewhere
            // within this project). So, this method will be called automatically whenever the user
            // loads this assembly, SampleAddIn, into GC.

            IGCEnvironment environment = UniversalGCEnvironment.TheOnlyInstance;
            NameCatalog nameCatalog = environment.TopLevelNameCatalog();

            // To add a new function to the GCScript processor, we call the method,
            // nameCatalog.AddNamespaceLevelFunction.
            //
            // 1. The first argument is the name of the new function, as the GC user will see it.
            //    (It's our responsibility to ensure that the function name doesn't conflict with
            //    the name of another top-level object within the GC world, such as a pre-existing
            //    node type.)
            //
            // 2. The second argument is the type (including signature) of the new function,
            //    expressed in GCScript form.
            //
            // 3. The third argument is the name of the C# method that implements the new function.
            //    That method can have any name.

            nameCatalog.AddGlobalFunction("ConvertTemperature",
                                          "double function(double temperature, string mode)",
                                          ConvertTemperatureFunction);
        }

        // Each method that implements a script function may be prefaced by a 'GCSummary' attribute,
        // which provides user documentation for that function. The documentation text will appear
        // in GC's Functions panel. (The presence or absence of a GCSummary attribute has no effect
        // on how the function may be used in GC.)

        [GCSummary("Converts a temperature value between degrees Celsius (C) and degrees Fahrenheit (F). The 'mode' parameter indicates the direction of the conversion: 'c' means F -> C and 'f' means C -> F.")]

        static void ConvertTemperatureFunction(CallFrame callFrame)
        {
            // This is the implementation method for our new script function, ConvertTemperature.
            //
            // Every implementation method must have this signature:
            // static void <method name> (CallFrame callFrame)

            // Start by getting the "native" C# values of the given arguments.

            double temperature = callFrame.UnboxArgument<double>(0);  // Get the first argument
                                                                      // (i.e., the argument at index 0)
                                                                      // as a C# double value.
            string mode        = callFrame.UnboxArgument<string>(1);  // Get the second argument
                                                                      // (i.e., the argument at index 1)
                                                                      // as a C# string value.

            // Following is the main body of the ConvertTemperature function.

            double result;

            if (mode == "C" || mode == "c")
                result = (temperature - 32) / 1.8;  // Standard formula to convert Fahrenheit
                                                    // degrees to Celsius.
            else if (mode == "F" || mode == "f")
                result = temperature * 1.8 + 32;  // Standard formula to convert Celsius degrees
                                                  // to Fahrenheit.
            else
            {
                // To cause a runtime exception to occur (with an appropriate message presented
                // to the user), throw a GCUserException.
                //
                // The argument to GCUserException is the error message. It's of the type Ls,
                // which means "localized string". Since we're not concerned with international-
                // ization in this project, it's sufficient to merely enclose the quoted string
                // within a call to the static method, Ls.Literal.

                throw new GCUserException(Ls.Literal($"Invalid mode \"{mode}\" given to ConvertTemperature."));
            }

            // If this GCScript function returns a value -- which it does, in this case -- we call
            // the method SetFunctionFunction on the given callFrame, as shown below.
            //
            // If our function does NOT return a value -- that is, if our function's return type,
            // as presented to the GC user, is "void" -- then we don't need to do anything special
            // at the end of our function implementation method.

            callFrame.SetFunctionResult(result);
        }
    }
}
