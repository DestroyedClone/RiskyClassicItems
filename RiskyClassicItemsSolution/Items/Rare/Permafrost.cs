using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicItemsReturns.Items.Rare
{
    public class Permafrost : ItemBase<Permafrost>
    {
        public override string ItemName => "Permafrost";

        public override string ItemLangTokenName => "PERMAFROST";

        private float procChancePercentage = 3f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            procChancePercentage
        };

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("IceCube");

        public override Sprite ItemIcon => LoadItemSprite("IceCube");

        //This is used to handle boss freezing
        public bool allowFreezeBoss = false;
        public static HashSet<BuffIndex> ModdedFreezeDebuffs = new HashSet<BuffIndex>();

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility,
            ItemTag.AIBlacklist
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void CreateConfig(ConfigFile config)
        {
            allowFreezeBoss = config.Bind(ConfigCategory, "Freeze Bosses", false, "Allow this item to freeze bosses.").Value;
        }

        public override void Hooks()
        {
            RoR2Application.onLoad += GetModdedFreezeDebuffs;
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += SetStateOnHurt_OnTakeDamageServer;
        }

        private void SetStateOnHurt_OnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, DamageReport damageReport)
        {
            orig(self, damageReport);

            if (!self.targetStateMachine || !self.spawnedOverNetwork || damageReport.damageInfo.procCoefficient <= 0f || !damageReport.attackerMaster || !damageReport.attackerMaster.inventory) return;

            DamageInfo damageInfo = damageReport.damageInfo;
            Inventory attackerInventory = damageReport.attackerMaster.inventory;

            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            if (!Util.CheckRoll(procChancePercentage * itemCount, damageReport.attackerMaster)) return;

            float duration = 2f * damageReport.damageInfo.procCoefficient;

            if (self.canBeFrozen && (!damageReport.victimIsChampion || allowFreezeBoss))
            {
                self.SetFrozen(duration);
            }
            else if (damageReport.victimBody)
            {
                foreach (BuffIndex buff in ModdedFreezeDebuffs)
                {
                    damageReport.victimBody.AddTimedBuff(buff, duration);
                }
            }
        }

        private void GetModdedFreezeDebuffs()
        {
            BuffIndex toAdd = BuffCatalog.FindBuffIndex("RiskyMod_FreezeDebuff");
            if (toAdd != BuffIndex.None) ModdedFreezeDebuffs.Add(toAdd);

            toAdd = BuffCatalog.FindBuffIndex("Freeze Debuff");
            if (toAdd != BuffIndex.None) ModdedFreezeDebuffs.Add(toAdd);
        }
    }
}