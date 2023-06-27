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
using Bentley.GenerativeComponents.GCScript.GCTypes;
using Bentley.GenerativeComponents.GeneralPurpose.Collections;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.ScriptEditor.Controls.ExpressionEditor;
using System.Linq;
using GCCommunity.Extensions;

namespace GCCommunity
{
    [GCNamespace("User")]
    [GCNodeTypePaletteCategory("Custom")]
    [GCNodeTypeIcon("Resources/data-edit-outline.png")]
    [GCSummary("Read, attach and update item properties on elements")]
    public class ItemTypes : GeometricNode
    {
        private static readonly DgnFile _activeDgnFile = Session.Instance.GetActiveDgnFile();
        private static ItemType _selectedItemType { get; set; }


        #region Supporting Methods
        static ExpressionEditorCustomConfiguration GetEECCForItems()
        {
            return new ExpressionEditorCustomConfiguration(getItemLibs);

            IEnumerable<ScriptChoice> getItemLibs(ExpressionEditor parentExpressionEditor)
            {
                ItemTypes node = (ItemTypes)parentExpressionEditor.MeaningOfThis;
                ScriptChoiceList result = node.GetItemLibList();
                return result;
            }
        }

        ScriptChoiceList GetItemLibList()
        {
            IList<ItemTypeLibrary> itemLibs = ItemTypeLibrary.PopulateListFromFile(_activeDgnFile);

            ScriptChoiceList libList = new ScriptChoiceList();
            for (int i = 0; i < itemLibs.Count; i++)
            {
                ScriptChoice itemList = new ScriptChoice(itemLibs[i].DisplayLabel);
                foreach (ItemType itemType in itemLibs[i].ItemTypes)
                {
                    itemList.AddSubChoice(itemType.ItemNamesConcat().ToQuotedScriptText());
                }
                libList.Add(itemList);
            }

            return libList;
        }
        #endregion




        [GCDefaultTechnique]
        [GCSummary("Select an ItemType from the active design file to read property information")]
        [GCParameter("ItemType", "The ItemType to read from (must be imported in the active design file)")]
        public NodeUpdateResult GetItemTypeInfo
        (
            NodeUpdateContext updateContext,
            [GCExpressionEditorCustomConfiguration(nameof(GetEECCForItems))] string ItemType,
            [GCOut, GCInitiallyPinned] ref string ItemTypeName,
            [GCOut, GCInitiallyPinned] ref string[] ItemTypeProperties
        )
        {
            try
            {
                if (string.IsNullOrEmpty(ItemType))
                {
                    ItemTypeProperties.Clear();
                }
                else
                {
                    //Get item type
                    _selectedItemType = ItemType.GetItemTypeByConcatString();

                    //Add item properties to output
                    List<string> propList = new List<string>();
                    IEnumerator<CustomProperty> properties = _selectedItemType.GetEnumerator();
                    while (properties.MoveNext())
                    {
                        CustomProperty prop = properties.Current;
                        propList.Add(prop.DisplayLabel);
                    }
                    if (propList.Any())
                        ItemTypeProperties = propList.ToArray();
                    else
                        ItemTypeProperties = null;

                    //Pass selected item type to output
                    ItemTypeName = ItemType;
                }
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
            return NodeUpdateResult.Success;
        }

        [GCTechnique]
        [GCSummary("Read the Item Type properties attached to elements")]
        [GCParameter("ElementsToRead", "Input elements to read custom item type data from")]
        [GCParameter("ItemTypeName", "The concatenated library and item type name")]
        public NodeUpdateResult ReadItems
        (
            NodeUpdateContext updateContext,
            [GCDgnModelProvider, GCReplicatable] GeometricNode ElementsToRead,
            [GCIn] string ItemTypeName,
            [GCOut, GCReplicatable, GCInitiallyPinned] ref IGCObject[] ItemProperties
        )
        {
            if (string.IsNullOrEmpty(ItemTypeName))
                return new NodeUpdateResult.IncompleteInputs(ItemTypeName);

            Boxer boxer = GCEnvironment().Boxer;
            
            try
            {
                //Update ItemType selection if required
                if (this.ReplicationIndex < 1)
                {
                    //Clear previous values
                    this.ClearReplicatedChildNodes();

                    //Update selected item if not matching input
                    if (_selectedItemType == null || _selectedItemType.ItemNamesConcat() != ItemTypeName)
                        _selectedItemType = ItemTypeName.GetItemTypeByConcatString();
                }                

                //Get platformNET element
                long id = ElementsToRead.GCElement().ElementId();
                Element element = ElementsToRead.DgnModel().FindElementById(new ElementId(ref id));

                //Get custom item properties
                CustomItemHost customItemHost = new CustomItemHost(element, false);
                IDgnECInstance ecInstance = customItemHost.GetCustomItem(_selectedItemType.Library.DisplayLabel, _selectedItemType.DisplayLabel);
                if (ecInstance == null)
                    throw new Exception($"Item not found on element");

                List<IGCObject> values = new List<IGCObject>();
                IEnumerator<IECPropertyValue> ie = ecInstance.GetEnumerator(true, true);
                while (ie.MoveNext())
                {
                    if (ie.Current.IsNull)
                        continue;

                    //values.Add(ie.Current.NativeValue);
                    if (boxer.TryBox(out IGCObject result, ie.Current.NativeValue))
                        values.Add(result);
                    else
                        throw new Exception($"Error boxing value '{ie.Current.NativeValue}' for item property '{ie.Current.AccessString}'");
                }

                ItemProperties = values.ToArray();
                //this.AddOrSetOutputOnlyAdHocProperty("props", values, isInitiallyPinned: true, description: Ls.Literal("The item type property values"));
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }
            return NodeUpdateResult.Success;
        }


        [GCTechnique]
        [GCSummary("Attach an ItemTypeLibrary and/or write new values to Items on the input elements")]
        [GCParameter("ElementsToWriteTo", "Input elements to write the custom item type data to")]
        [GCParameter("ItemTypeName", "The concatenated library and item type name")]
        public NodeUpdateResult WriteItems
        (
            NodeUpdateContext updateContext,
            [GCDgnModelProvider, GCReplicatable] GeometricNode ElementsToWriteTo,
            [GCIn] string ItemTypeName,
            [GCIn] IGCObject[] Properties,
            [GCIn] IGCObject[] Values
        )
        {
            try
            {
                //Update ItemType selection if required
                if (this.ReplicationIndex < 1)
                {
                    //Update selected item if not matching input
                    if (_selectedItemType == null || _selectedItemType.ItemNamesConcat() != ItemTypeName)
                        _selectedItemType = ItemTypeName.GetItemTypeByConcatString();
                }

                if (ItemTypeName.ItemNamesSplitLib(out string libName, out string itemName))
                {
                    DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

                    long id = ElementsToWriteTo.GCElement().ElementId();
                    Element element = dgnModel.FindElementById(new ElementId(ref id)).Clone();
                    this.SetElement(element);
                    
                    CustomItemHost customItemHost = new CustomItemHost(element, false);

                    IDgnECInstance ecInstance = customItemHost.GetCustomItem(libName, itemName);
                    if (ecInstance == null)
                    {
                        ecInstance = customItemHost.ApplyCustomItem(_selectedItemType);
                    }
                    for (int i = 0; i < Properties.Length; i++)
                    {
                        string propName = Properties[i].Unbox<string>();
                        object propValue = Values[i].Unbox<object>();
                        //bool getName = Bentley.ECObjects.Schema.ECNameValidation.EncodeToValidName(ref propName);
                        ecInstance.SetAsString(propName, propValue.ToString());
                    }
                    
                    ecInstance.WriteChanges();
                }
                else
                    return new NodeUpdateResult.TechniqueInvalidArguments(ItemTypeName);
            }
            catch (Exception ex)
            {
                return new NodeUpdateResult.TechniqueException(ex);
            }

            return NodeUpdateResult.Success;
        }
    }
}