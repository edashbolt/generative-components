using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.Features;
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

namespace GCCommunity
{
    [GCNamespace("User")]
    [NodeTypePaletteCategory("Custom")]
    [NodeTypeIcon("Resources/Items.png")]
    [Summary("Read, attach and update item properties on elements.")]

    public class ItemTypes : Feature
    {
        [DefaultTechnique]
        [Summary("Read Items that are attached to the input elements.")]
        [Parameter("ElementsToRead", "Input elements to read from.")]
        [Parameter("ItemTypeName", "The Item Type.")]

        public NodeUpdateResult ReadItems
        (
            NodeUpdateContext updateContext,
            [Replicatable, DgnModelProvider] object ElementsToRead/*,
            [Out, Replicatable] ref string[] ItemTypeName,
            [Out, Replicatable] ref string[][] PropertyNames,
            [Out, Replicatable] ref string[][] PropertyValues*/
        )
        {/*
            try
            {
                DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
                DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

                Feature feat = (Feature)ElementsToRead;
                long eleID = feat.Element.ID;
                Bentley.DgnPlatformNET.Elements.Element ele = dgnModel.FindElementById(new ElementId(ref eleID));
                
                CustomItemHost customItemHost = new CustomItemHost(ele, false);

                IList<IDgnECInstance> ecInstanceList = customItemHost.CustomItems;

                List<string> types = new List<string>();
                List<string[]> props = new List<string[]>();
                List<string[]> values = new List<string[]>();

                foreach (IDgnECInstance ecInstance in ecInstanceList)
                {
                    types.Add(ecInstance.ClassDefinition.Name);

                    List<string> iprops = new List<string>();
                    List<string> ivalues = new List<string>();

                    IEnumerator<IECPropertyValue> ie = ecInstance.GetEnumerator(true, true);
                    while (ie.MoveNext())
                    {
                        iprops.Add(ie.Current.Property.Name);
                        ivalues.Add(ie.Current.StringValue);
                    }

                    props.Add(iprops.ToArray());
                    values.Add(ivalues.ToArray());
                }

                PropertyNames = props.ToArray();
                PropertyValues = values.ToArray();
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
            */
            return NodeUpdateResult.Success;
        }

        [Technique]
        [Summary("Attach an ItemTypeLibrary and/or write new values to Items on the input elements.")]
        [Parameter("Elements", "Input elements to write to.")]
        public NodeUpdateResult WriteItems
        (
            NodeUpdateContext updateContext,
            [DgnModelProvider, Replicatable] object ElementsToWriteTo,
            [Replicatable] string ItemTypeLibraryName,
            [Replicatable] string ItemTypeName,
            [Replicatable(-1, true)] string[] PropertyName,
            [Replicatable(-1, true)] string[] PropertyValue
        )
        {
            try
            {
                DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
                DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

                Feature feat = (Feature)ElementsToWriteTo;
                long eleID = feat.Element.ID;
                Bentley.DgnPlatformNET.Elements.Element ele = dgnModel.FindElementById(new ElementId(ref eleID));

                
                ItemTypeLibrary itl = ItemTypeLibrary.FindByName(ItemTypeLibraryName, dgnFile);
                ItemType itemType = itl.GetItemTypeByName(ItemTypeName);

                CustomItemHost customItemHost = new CustomItemHost(ele, false);

                IDgnECInstance ecInstance = customItemHost.GetCustomItem(ItemTypeLibraryName, ItemTypeName);
                if (ecInstance == null)
                {
                    ecInstance = customItemHost.ApplyCustomItem(itemType);
                }
                for(int i = 0; i < PropertyName.Length; i++)
                {
                    ecInstance.SetString(PropertyName[i].ToString(), PropertyValue[i].ToString());   
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