using R2API;
using RoR2;
using RiskyClassicItems.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RoR2.Items.BaseItemBodyBehavior;
using RoR2.Items;
using UnityEngine.Diagnostics;
using RoR2.CharacterAI;
using BepInEx.Configuration;

namespace RiskyClassicItems.Items
{
    internal class ArmsRace : ItemBase<ArmsRace>
    {
        public float cooldown = 10;
        public int missileCount = 4;
        public int missileCountPerStack = 4;
        public float damageCoeff = 2;
        public override string ItemName => "Arms Race";
        public override string ItemLangTokenName => "ARMSRACE";

        public override object[] ItemFullDescriptionParams => new object[]
        {
            cooldown,
            missileCount,
            missileCountPerStack,
            (damageCoeff*100f)
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadPickupModel("ArmsRace");

        public override Sprite ItemIcon => LoadItemIcon("ArmsRace");

        public static ItemDef ArmsRaceDroneItemDef => ArmsRaceDroneItem.Instance.ItemDef;

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.BrotherBlacklist,
            ItemTag.Damage
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }


        public class ArmsRaceBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => ArmsRace.Instance.ItemDef;
            
            private void OnEnable()
            {
                MasterSummon.onServerMasterSummonGlobal += MasterSummon_onServerMasterSummonGlobal;
            }

            private void MasterSummon_onServerMasterSummonGlobal(MasterSummon.MasterSummonReport summonReport)
            {
                if (body && body.master && body.master == summonReport.leaderMasterInstance)
                {
                    CharacterMaster summonMasterInstance = summonReport.summonMasterInstance;
                    if (summonMasterInstance)
                    {
                        CharacterBody body = summonMasterInstance.GetBody();
                        if (body)
                        {
                            UpdateMinionInventory(summonMasterInstance.inventory, body.bodyFlags, stack);
                        }
                    }
                }
            }
            private void UpdateMinionInventory(Inventory inventory, CharacterBody.BodyFlags bodyFlags, int newStack)
            {
                if (newStack <= 0 || !bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
                {
                    inventory.ResetItem(ArmsRaceDroneItemDef);
                    return;
                }

                int itemCount = inventory.GetItemCount(ArmsRaceDroneItemDef);
                if (itemCount < newStack)
                {
                    inventory.GiveItem(ArmsRaceDroneItemDef, newStack - itemCount);
                }
                else if (itemCount > newStack)
                {
                    inventory.RemoveItem(ArmsRaceDroneItemDef, itemCount - newStack);
                }
            }

            private void OnDisable()
            {
                MasterSummon.onServerMasterSummonGlobal -= MasterSummon_onServerMasterSummonGlobal;
            }
        }
    }
}
