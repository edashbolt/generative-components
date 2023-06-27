using System;
using System.Collections.Generic;
using System.Text;
using Bentley.GenerativeComponents;

namespace SampleAddIn
{
    public sealed class Initializer: IAssemblyInitializer
    {
        // Whenever the GC user loads a .NET assembly (DLL file) into GC, GC examines all
        // of the classes defined within that assembly, looking for those classes that...
        //
        //   1. Are public, and...
        //
        //   2. Implement the interface, IAssemblyInitializer, and...
        //
        //   3. Have a default constructor (that is, a public constructor that takes no arguments).
        //
        // For each such class it finds, GC instantiates it, automatically. (Then, GC does nothing
        // further with that instance. That class's sole worth is whatever it does in its
        // contructor.)

        public Initializer()
        {
            ScriptFunctions.Load();  // In this case, we're using the initialization mechanism
                                     // to load the script function we defined in the static
                                     // class, ScriptFunctions.
        }
    }
}
