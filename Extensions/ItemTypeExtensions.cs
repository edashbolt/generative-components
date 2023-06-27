using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCCommunity.Extensions
{
    public static class ItemTypeExtensions
    {
        //Get the concatenated "LIB :: ITEM" string for easy referencing
        public static string ItemNamesConcat(this ItemType itemType)
        {
            return $"{itemType.Library.DisplayLabel} :: {itemType.DisplayLabel}";
        }

        //Split the concatenated string into the library and item names
        public static bool ItemNamesSplitLib(this string itemNameConcat, out string libName, out string itemName)
        {
            libName = null;
            itemName = null;

            string[] names = itemNameConcat.Split(new string[] { " :: " }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length != 2)
                return false;

            libName = names[0];
            itemName = names[1];

            return true;
        }

        //Fetch the ItemType using concatenated string
        public static ItemType GetItemTypeByConcatString(this string itemNameConcat)
        {
            ItemType item = null;
            if(itemNameConcat.ItemNamesSplitLib(out string libName, out string itemName))
            {
                var lib = ItemTypeLibrary.FindByName(libName, Session.Instance.GetActiveDgnFile());
                item = lib.GetItemTypeByName(itemName);
            }
            return item;
        }
    }
}
