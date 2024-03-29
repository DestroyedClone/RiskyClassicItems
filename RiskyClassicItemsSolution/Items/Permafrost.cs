﻿using R2API;
using RoR2;
using UnityEngine;

namespace ClassicItemsReturns.Items
{
    public class Permafrost : ItemBase<Permafrost>
    {
        public override string ItemName => "Permafrost";

        public override string ItemLangTokenName => "PERMAFROST";

        //util.amplification only accepts one value
        //and figuring out the formula will dent my brain durther
        private float procChancePercentage = 3f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            procChancePercentage
        };

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("IceCube");

        public override Sprite ItemIcon => LoadItemSprite("IceCube");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility,
            ItemTag.AIBlacklist
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && TryGetCount(attackerBody, out int itemCount))
            {
                if (Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(procChancePercentage * itemCount)))
                {
                    damageInfo.damageType |= DamageType.Freeze2s;
                }
            }
            orig(self, damageInfo);
        }
    }
}