using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.GCScript;
//using Bentley.GenerativeComponents.GCScript.NameScopes;
//using Bentley.GenerativeComponents.GeneralPurpose;
//using Bentley.GenerativeComponents.MicroStation;
using Bentley.GenerativeComponents.View;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
using Bentley.DgnPlatformNET.DgnEC;
using Bentley.ECObjects.Schema;
using Bentley.ECObjects.Instance;
using Bentley.GenerativeComponents.ElementBasedNodes;
using Bentley.DgnPlatformNET.Elements;

namespace GCCommunity
{
    [GCNamespace("User")]
    [GCNodeTypePaletteCategory("Custom")]
    [GCNodeTypeIcon("Resources/Items.png")]
    [GCSummary("Read, attach and update item properties on elements.")]

    public class ItemTypes : ElementBasedNode
    {
        [GCDefaultTechnique]
        [GCSummary("Read Items that are attached to the input elements.")]
        [GCParameter("ElementsToRead", "Input elements to read from.")]
        [GCParameter("ItemTypeName", "The Item Type.")]

        public NodeUpdateResult ReadItems
        (
            NodeUpdateContext updateContext,
            [GCIn, GCReplicatable, GCDgnModelProvider] GeometricNode ElementsToRead,
            [GCOut, GCReplicatable, GCInitiallyPinned] ref string[] Items,
            [GCOut, GCReplicatable, GCInitiallyPinned] ref string[][] ItemProps,
            [GCOut, GCReplicatable, GCInitiallyPinned] ref string[][] ItemValues
        )
        {
            try
            {
                DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
                DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

                long id = ElementsToRead.GCElement().ElementId();
                Element element = dgnModel.FindElementById(new ElementId(ref id));
                //Element element = ElementsToRead.Element();

                CustomItemHost customItemHost = new CustomItemHost(element, false);

                IList<IDgnECInstance> ecInstanceList = customItemHost.CustomItems;

                List<string> typenames = new List<string>();
                List<string[]> props = new List<string[]>();
                List<string[]> values = new List<string[]>();

                foreach (IDgnECInstance ecInstance in ecInstanceList)
                {
                    typenames.Add(ecInstance.ClassDefinition.DisplayLabel);

                    List<string> iprops = new List<string>();
                    List<string> ivalues = new List<string>();

                    IEnumerator<IECPropertyValue> ie = ecInstance.GetEnumerator(true, true);
                    while (ie.MoveNext())
                    {
                        iprops.Add(ie.Current.Property.DisplayLabel);

                        string propValue = ie.Current.StringValue;
                        bool getValue = Bentley.ECObjects.Schema.ECNameValidation.DecodeFromValidName(ref propValue);
                        ivalues.Add(propValue);
                    }

                    props.Add(iprops.ToArray());
                    values.Add(ivalues.ToArray());
                }

                Items = typenames.ToArray();
                ItemProps = props.ToArray();
                ItemValues = values.ToArray();
                //this.AddOrSetOutputOnlyAdHocProperty("ItemTypeNames", typenames.ToArray(), null, true);
                //this.AddOrSetOutputOnlyAdHocProperty("ParameterNames", props.ToArray(), null, true);
                //this.AddOrSetOutputOnlyAdHocProperty("ParameterValues", values.ToArray(), null, true);
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
            return NodeUpdateResult.Success;
        }

        [GCTechnique]
        [GCSummary("Attach an ItemTypeLibrary and/or write new values to Items on the input elements.")]
        [GCParameter("Elements", "Input elements to write to.")]
        public NodeUpdateResult WriteItems
        (
            NodeUpdateContext updateContext,
            [GCDgnModelProvider, GCReplicatable] GeometricNode ElementsToWriteTo,
            [GCReplicatable] string ItemTypeLibraryName,
            [GCReplicatable] string Item,
            [GCReplicatable] string[] Properties,
            [GCReplicatable] object[] Values
        )
        {
            try
            {
                DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
                DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

                long id = ElementsToWriteTo.GCElement().ElementId();
                Element element = dgnModel.FindElementById(new ElementId(ref id));


                ItemTypeLibrary itl = ItemTypeLibrary.FindByName(ItemTypeLibraryName, dgnFile);
                ItemType itemType = itl.GetItemTypeByName(Item);

                CustomItemHost customItemHost = new CustomItemHost(element, false);

                IDgnECInstance ecInstance = customItemHost.GetCustomItem(ItemTypeLibraryName, Item);
                if (ecInstance == null)
                {
                    ecInstance = customItemHost.ApplyCustomItem(itemType);
                }
                for (int i = 0; i < Properties.Length; i++)
                {
                    string propName = Properties[i];
                    bool getName = Bentley.ECObjects.Schema.ECNameValidation.EncodeToValidName(ref propName);
                    ecInstance.SetAsString(propName, Values[i].ToString());
                }
                ecInstance.WriteChanges();
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }

            return NodeUpdateResult.Success;
        }
    }
}