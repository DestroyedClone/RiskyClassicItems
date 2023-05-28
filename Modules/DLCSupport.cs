using HarmonyLib;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using static RiskyClassicItems.Main;

namespace RiskyClassicItems.Modules
{
    internal class DLCSupport
    {
        internal static void Initialize()
        {
            DLC1.Initialize();
        }

        internal class DLC1
        {
            public static void Initialize()
            {
                On.RoR2.Items.ContagiousItemManager.Init += SetupContagiousItemManager;
            }

            public static RoR2.ExpansionManagement.ExpansionDef sotvExpansionDef;

            public static RoR2.ExpansionManagement.ExpansionDef GetSOTVExpansionDef()
            {
                if (!sotvExpansionDef)
                    sotvExpansionDef = RoR2.ExpansionManagement.ExpansionCatalog.expansionDefs
                        .FirstOrDefault(def => def.nameToken == "DLC1_NAME");
                return sotvExpansionDef;
            }

            public static Dictionary<ItemDef, ItemDef> voidConversions = new Dictionary<ItemDef, ItemDef>();

            internal static void SetupContagiousItemManager(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
            {
                foreach (var itemPair in voidConversions)
                {
                    ModLogger.LogMessage($"Adding conversion: {itemPair.Key.nameToken} to {itemPair.Value.nameToken}");
                    RoR2.ItemDef.Pair transformation = new RoR2.ItemDef.Pair()
                    {
                        itemDef1 = itemPair.Key,
                        itemDef2 = itemPair.Value
                    };
                    RoR2.ItemCatalog.itemRelationships[RoR2.DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
                }

                orig();
            }
        }
    }
}