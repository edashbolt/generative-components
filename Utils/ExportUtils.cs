using Bentley.DgnPlatformNET;
using Bentley.ECObjects.Schema;
using Bentley.ECObjects;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.DgnPlatformGC;
using Bentley.GenerativeComponents.ElementBasedNodes;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.MicroStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCCommunity.Utils
{
    internal class ExportUtils
    {
        internal static GCElement SaveGCElementToDgnModel(GCElement element, GCDgnModel destinationDgnModel, bool isExternalDesignFile)
        {
            GCElement result = default(GCElement);
            if (element.IsSound() && destinationDgnModel.IsSound())
            {
                if (isExternalDesignFile)
                {
                    try
                    {
                        result = destinationDgnModel.CopyGCElement(element);
                        return result;
                    }
                    catch (AccessViolationException)
                    {
                        return result;
                    }
                }
                destinationDgnModel.AddGCElement(ref element);
                element.Draw(DgnDrawMode.Normal);
                result = element;
            }
            return result;
        }

        internal static IECClass InitFixedPropertyClassDefinition(GCDgnModel gcDgnModel)
        {
            IECClass val = (IECClass)new ECClass("GCInfoCategory");
            ECProperty val2 = new ECProperty("Name", (IECType)(object)ECObjects.StringType);
            val2.IsReadOnly = true;
            val.Add((IECProperty)(object)val2);
            IECSchema ecSchema = NativeDgnTools.CreateSchema("BentleyGC");
            DgnModel model = gcDgnModel.DgnModel();
            if (NativeDgnTools.ImportECSchemaFromDgnModel(model, ref ecSchema))
            {
                val = ((IEnumerable<IECClass>)ecSchema).First((IECClass p) => ((IECNamed)p).Name == "GCInfoCategory");
            }
            else
            {
                ecSchema.AddClass(val);
                NativeDgnTools.ReplaceECSchemaInDgnModel(model, ecSchema);
            }
            return val;
        }
        /*
        internal static IECClass InitVariantPropertyClassDefinition(GCModel gcModel, GCDgnModel gcDgnModel)
        {
            GCSpace space = gcModel.GCSpace();
            DgnModel model = gcDgnModel.DgnModel();
            string text = GCType().FullName() + "_NotationProperty";
            string ecClassName = text.Replace('.', '_');
            string schemaName = "BentleyGCVariant";
            IECSchema ecSchema = NativeDgnTools.CreateSchema(schemaName);
            IECClass val = null;
            if (NativeDgnTools.ImportECSchemaFromDgnModel(model, ref ecSchema))
            {
                val = ((IEnumerable<IECClass>)ecSchema).FirstOrDefault((IECClass p) => ((IECNamed)p).Name == ecClassName);
            }
            if (val == null)
            {
                val = (IECClass)new ECClass(ecClassName);
                foreach (ShadowNotationProperty item in _shadow.ShadowNotationProperties(space))
                {
                    IECType eCTypeFromGCType = GetECTypeFromGCType(gcModel, item);
                    val.Add((IECProperty)new ECProperty(item.Name, eCTypeFromGCType));
                }
                ecSchema.AddClass(val);
                NativeDgnTools.ReplaceECSchemaInDgnModel(model, ecSchema);
            }
            else
            {
                bool flag = true;
                foreach (ShadowNotationProperty item2 in CorrespondingTopLevelElementBasedNode()._shadow.ShadowNotationProperties(space))
                {
                    if (val.FindLocalProperty(item2.Name) == null)
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag)
                {
                    val = (IECClass)new ECClass(ecClassName);
                    foreach (ShadowNotationProperty item3 in _shadow.ShadowNotationProperties(space))
                    {
                        IECType eCTypeFromGCType2 = GetECTypeFromGCType(gcModel, item3);
                        val.Add((IECProperty)new ECProperty(item3.Name, eCTypeFromGCType2));
                    }
                    ecSchema = NativeDgnTools.CreateSchema(schemaName);
                    ecSchema.AddClass(val);
                    NativeDgnTools.ReplaceECSchemaInDgnModel(model, ecSchema);
                }
            }
            return val;
        }
        */

        internal static IEnumerable<GeometricNode> GetValidNodesToExport(GeometricNode[] nodesToExport)
        {
            GeometricNode[] array = (nodesToExport ?? new Node[0]).OfType<GeometricNode>().ToArray();
            List<GeometricNode> list = new List<GeometricNode>();
            foreach (GeometricNode node in array)
            {
                list.AddRange(GetValidNodes(node));
            }
            return list;
        }

        private static IEnumerable<GeometricNode> GetValidNodes(GeometricNode node)
        {
            if (node.IsSound())
            {
                GeometricNode[] subNodes = node.SubNodes().OfType<GeometricNode>().ToArray();
                if (subNodes.Length != 0)
                {
                    //Get immediate elements
                    if (node.GCElementsForExport().Any())
                    {
                        yield return node;
                    }

                    //Get subnode elements
                    foreach (GeometricNode subNode in subNodes)
                    {
                        foreach (GeometricNode item in GetValidNodes(subNode))
                        {
                            yield return item;
                        }
                    }
                }
                else if (node.GCElementsForExportIncludingSubNodes().Any())
                {
                    yield return node;
                }
            }
        }
    }
}
