using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Utils;
using RoR2;
using RoR2.Items;
using UnityEngine;
using static RiskyClassicItems.Items.ArmsRace;
using static RoR2.Items.BaseItemBodyBehavior;

namespace RiskyClassicItems.Items
{
    public class Permafrost : ItemBase<Permafrost>
    {
        public override string ItemName => "Permafrost";

        public override string ItemLangTokenName => "PERMAFROST";

        float procChancePercentage = 35f;
        float procChanceStackPercentage = 5f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            procChancePercentage,
            procChanceStackPercentage,
        };


        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadPickupModel("Permafrost");

        public override Sprite ItemIcon => LoadItemIcon("Permafrost");
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility
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
                var rollChance = Utils.ItemHelpers.StackingLinear(itemCount, procChancePercentage, procChanceStackPercentage);
                if (Util.CheckRoll(rollChance, attackerBody.master))
                {
                    damageInfo.damageType |= DamageType.Freeze2s;
                }
            }
            orig(self, damageInfo);
        }
    }
}