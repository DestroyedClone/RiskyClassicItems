using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

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
            ItemTag.AIBlacklist,
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

            public MinionOwnership minionOwnership;

            private const float display2Chance = 0.1f;

            private int previousStack;

            private Xoroshiro128Plus rng;

            private void OnEnable()
            {
                ulong seed = Run.instance.seed ^ (ulong)((long)Run.instance.stageClearCount);
                this.rng = new Xoroshiro128Plus(seed);
                this.UpdateAllMinions(this.stack);
                MasterSummon.onServerMasterSummonGlobal += MasterSummon_onServerMasterSummonGlobal;

                minionOwnership = body.GetComponent<MinionOwnership>();
            }

            private void OnDisable()
            {
                MasterSummon.onServerMasterSummonGlobal -= MasterSummon_onServerMasterSummonGlobal;
                this.UpdateAllMinions(0);
            }

            private void FixedUpdate()
            {
                if (this.previousStack != this.stack)
                {
                    this.UpdateAllMinions(this.stack);
                }
            }

            private void UpdateAllMinions(int newStack)
            {
                if (this.body)
                {
                    CharacterBody body = this.body;
                    if ((body != null) ? body.master : null)
                    {
                        MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(this.body.master.netId);
                        if (minionGroup != null)
                        {
                            foreach (MinionOwnership minionOwnership in minionGroup.members)
                            {
                                if (minionOwnership)
                                {
                                    CharacterMaster component = minionOwnership.GetComponent<CharacterMaster>();
                                    if (component && component.inventory)
                                    {
                                        CharacterBody body2 = component.GetBody();
                                        if (body2)
                                        {
                                            this.UpdateMinionInventory(component.inventory, body2.bodyFlags, newStack);
                                        }
                                    }
                                }
                            }
                            this.previousStack = newStack;
                        }
                    }
                }
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
                if (inventory && newStack > 0 && (bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
                {
                    int itemCount = inventory.GetItemCount(ArmsRaceDroneItemDef);
                    //Need to use a seperate display cause the display add/removal conflicts with spare drone parts
                    //int itemCount2 = inventory.GetItemCount(DLC1Content.Items.DroneWeaponsDisplay1);
                    //int itemCount3 = inventory.GetItemCount(DLC1Content.Items.DroneWeaponsDisplay2);
                    if (itemCount < this.stack)
                    {
                        inventory.GiveItem(ArmsRaceDroneItemDef, this.stack - itemCount);
                    }
                    else if (itemCount > this.stack)
                    {
                        inventory.RemoveItem(ArmsRaceDroneItemDef, itemCount - this.stack);
                    }
                    /*if (itemCount2 + itemCount3 <= 0)
                    {
                        ItemDef itemDef = DLC1Content.Items.DroneWeaponsDisplay1;
                        if (UnityEngine.Random.value < display2Chance)
                        {
                            itemDef = DLC1Content.Items.DroneWeaponsDisplay2;
                        }
                        inventory.GiveItem(itemDef, 1);
                        return;
                    }*/
                }
                else
                {
                    inventory.ResetItem(DLC1Content.Items.DroneWeaponsBoost);
                    //inventory.ResetItem(DLC1Content.Items.DroneWeaponsDisplay1);
                    //inventory.ResetItem(DLC1Content.Items.DroneWeaponsDisplay2);
                }
            }
        }
    }
}