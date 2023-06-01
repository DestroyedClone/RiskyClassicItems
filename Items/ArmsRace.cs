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
        public int missileCount = 1;
        public int missileCountPerStack = 1;
        public float damageCoeff = 2;
        public override string ItemName => "Arms Race";
        public override string ItemLangTokenName => "ARMSRACE";

        public override string[] ItemFullDescriptionParams => new string[]
        {
            cooldown.ToString(),
            missileCount.ToString(),
            missileCountPerStack.ToString(),
            (damageCoeff*100f).ToString()
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Assets.NullModel;

        public override Sprite ItemIcon => Assets.NullSprite;

        public static ItemDef ArmsRaceDroneItemDef => ArmsRaceDroneItem.Instance.ItemDef;

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
                            body.gameObject.AddComponent<ArmsRaceSyncComponent>().ownerMaster = summonMasterInstance;
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


        //https://github.com/WhitePhant0m/RoR2-Mods/blob/master/SyncedTurrets
        public class ArmsRaceSyncComponent : MonoBehaviour
        {
            public int droneMissileCount = ArmsRace.Instance.missileCount;
            public CharacterMaster ownerMaster;

            public void Start()
            {
                if (ownerMaster && ownerMaster.inventory)
                    ownerMaster.inventory.onInventoryChanged += OnOwnerInventoryChanged;
            }

            private void OnOwnerInventoryChanged()
            {
                var itemCount = ArmsRace.Instance.GetCount(ownerMaster);
                if (itemCount > 0)
                {
                    droneMissileCount = Instance.missileCount + Instance.missileCountPerStack * (itemCount - 1);
                }
            }

            public void OnDestroy()
            {
                if (ownerMaster && ownerMaster.inventory)
                    ownerMaster.inventory.onInventoryChanged -= OnOwnerInventoryChanged;
            }
        }
    }
}
