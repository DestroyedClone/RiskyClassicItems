using R2API;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using RoR2;
using RoR2.Items;
using RoR2.Orbs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items.NoTier
{
    //Item only acts as an itemdisplay
    internal class DroneRepairKitDroneItem : ItemBase<DroneRepairKitDroneItem>
    {
        public override string ItemName => "DRONEREPAIRKITDRONEITEM";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => LoadItemModel("RepairKitDroneDisplay");

        public override Sprite ItemIcon => Modules.Assets.NullSprite;

        public override string ItemLangTokenName => "DRONEREPAIRKITDRONEITEM";

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var dict = new ItemDisplayRuleDict();

            //Don't set up displays if 3d model isn't available
            GameObject display = ItemModel;
            if (!display.name.Contains("mdl3d")) return dict;

            dict.Add("CLASSICITEMSRETURNS_BODY_RepairDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.35F, 0F),
                    localAngles = new Vector3(0F, 180F, 180F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });

            dict.Add("Drone1Body", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.35F, 0F),
                    localAngles = new Vector3(0F, 180F, 180F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });

            return dict;
        }

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.CannotSteal,
            ItemTag.CannotCopy,
            ItemTag.CannotDuplicate
        };
    }
}