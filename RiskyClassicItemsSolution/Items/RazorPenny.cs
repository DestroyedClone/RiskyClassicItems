using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ClassicItemsReturns.Items
{
    public class RazorPenny : ItemBase<RazorPenny>
    {
        public override string ItemName => "Razor Penny";

        public override string ItemLangTokenName => "PENNY";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadItemModel("Penny");

        public override Sprite ItemIcon => LoadItemSprite("Penny");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage, ItemTag.Utility
        };

        public float critChance = 5f;
        public int goldOnHit = 1;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            critChance, goldOnHit
        };

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            SharedHooks.OnHitEnemy.OnHitEnemyAttackerInventoryActions += GoldOnCrit;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                int itemCount = sender.inventory.GetItemCount(this.ItemDef);
                args.critAdd += itemCount * critChance;
            }
        }

        private void GoldOnCrit(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, CharacterBody attackerBody, Inventory attackerInventory)
        {
            if (!damageInfo.crit || !attackerBody.master) return;
            int itemCount = attackerInventory.GetItemCount(this.ItemDef);
            if (itemCount <= 0) return;

            attackerBody.master.GiveMoney((uint)(goldOnHit * itemCount));
        }
    }
}
