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
    //This is used to turn a normal Gunner Drone into a Repair Drone.
    //Opting for this so that it's compatible with mods that modify Gunner Drones (ex. RiskyMod)
    internal class DroneRepairKitDroneItem : ItemBase<DroneRepairKitDroneItem>
    {
        public override string ItemName => "DRONEREPAIRKITDRONEITEM";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => LoadItemModel("RepairKitDroneDisplay");

        public override Sprite ItemIcon => Assets.NullSprite;

        public override string ItemLangTokenName => "DRONEREPAIRKITDRONEITEM";

        public override void Hooks()
        {
            On.RoR2.CharacterBody.GetDisplayName += ReplaceName;

            //Hacky
            RoR2.CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.EntityStates.Drone.DeathState.OnImpactServer += DeathState_OnImpactServer;
        }

        //This never gets unset but that's fine since that's not going to happen in normal circumstances.
        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody self)
        {
            if (NetworkServer.active && self.inventory.GetItemCount(ItemDef) > 0)
            {
                if (!self.GetComponent<DontAllowRepair>()) self.gameObject.AddComponent<DontAllowRepair>();
            }
        }


        private void DeathState_OnImpactServer(On.EntityStates.Drone.DeathState.orig_OnImpactServer orig, EntityStates.Drone.DeathState self, Vector3 contactPoint)
        {
            if (self.GetComponent<DontAllowRepair>())
            {
                return;
            }
            orig(self, contactPoint);
        }

        private string ReplaceName(On.RoR2.CharacterBody.orig_GetDisplayName orig, CharacterBody self)
        {
            if (self.inventory && self.inventory.GetItemCount(ItemDef) > 0)
            {
                return Language.GetString("CLASSICITEMSRETURNS_BODY_REPAIRDRONEBODY_NAME");
            }
            return orig(self);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var dict = new ItemDisplayRuleDict();

            //Don't set up displays if 3d model isn't available
            GameObject display = ItemModel;
            if (!display.name.Contains("mdl3d")) return dict;

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

    //Body marker used to skip drone repairs
    internal class DontAllowRepair : MonoBehaviour
    {

    }
}