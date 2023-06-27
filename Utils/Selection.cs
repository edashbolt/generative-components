using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCCommunity.Utils
{
    public class Selection
    {
        public static List<Element> GetSelectedElements()
        {
            List<Element> elements = new List<Element>();

            if (SelectionSetManager.IsActive())
            {
                uint selectedCount = SelectionSetManager.NumSelected();
                for (uint i = 0; i < selectedCount; i++)
                {
                    Element ele = null;
                    DgnModelRef modelRef = null;
                    if (SelectionSetManager.GetElement(i, ref ele, ref modelRef) == StatusInt.Success)
                    {
                        elements.Add(ele);
                    }
                }
            }

            return elements;
        }
    }
}
