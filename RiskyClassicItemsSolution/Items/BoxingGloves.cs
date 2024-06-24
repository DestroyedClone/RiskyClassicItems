using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;
using System;

namespace ClassicItemsReturns.Items
{
    public class BoxingGloves : ItemBase<BoxingGloves>
    {
        public float chance = 6f;
        public float chanceStack = 6f;
        public float damageMult = 1;

        public override string ItemName => "BoxingGloves";

        public override string ItemLangTokenName => "BOXINGGLOVES";

        public override object[] ItemFullDescriptionParams => new object[]
        {
            chance,
            chanceStack, 
            damageMult * 100
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("BoxingGloves");

        public override Sprite ItemIcon => LoadItemSprite("BoxingGloves");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.AIBlacklist
        };

        public override bool unfinished => true;

        public override void CreateAssets(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            SharedHooks.OnHitEnemy.OnHitEnemyAttackerInventoryAndVictimBodyActions += BoxingGlovesOnHit;
        }

        private void BoxingGlovesOnHit(GlobalEventManager globalEventManager, DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody, Inventory attackerInventory)
        {
            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            var ch = Utils.ItemHelpers.StackingLinear(itemCount, chance, chanceStack);
            if (!Util.CheckRoll(ch * damageInfo.procCoefficient, attackerBody.master)) return;

            DamageInfo bgDamageInfo = new DamageInfo()
            {
                attacker = damageInfo.attacker,
                canRejectForce = damageInfo.canRejectForce,
                crit = damageInfo.crit,
                damage = damageInfo.damage,
                damageColorIndex = damageInfo.damageColorIndex,
                damageType = damageInfo.damageType,
                dotIndex = damageInfo.dotIndex,
                force = damageInfo.force,
                inflictor = damageInfo.inflictor,
                position = damageInfo.position,
                procChainMask = damageInfo.procChainMask,
                procCoefficient = 0,// damageInfo.procCoefficient,
                rejected = damageInfo.rejected,
            };
            victimBody.healthComponent.TakeDamage(bgDamageInfo);
        }
    }
}