using Bentley.DgnPlatformNET.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCCommunity.Objects
{
    public class ElementGraphics
    {
        public Element Element { get; set; }
        public ElementType Type { get; set; }

        public ElementGraphics(Element element, ElementType type)
        {
            Element = element;
            Type = type;
        }
    }

    public enum ElementType
    {
        Solid,
        Surface,
        Mesh,
        Curve
    }
}
