using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ClassicItemsReturns.Items.Common
{
    public class RazorPenny : ItemBase<RazorPenny>
    {
        public static ConfigEntry<bool> disableInBazaar;
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

        public override void CreateConfig(ConfigFile config)
        {
            disableInBazaar = config.Bind(ConfigCategory, "Disable in Bazaar", true, "Disable money gain from this item in the Bazaar.");
        }


        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            SneedHooks.ProcessHitEnemy.OnHitAttackerActions += GoldOnCrit;
        }

        private void GoldOnCrit(DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody)
        {
            if (!damageInfo.crit || !attackerBody.master || !attackerBody.master.inventory) return;
            int itemCount = attackerBody.master.inventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;
            if (disableInBazaar.Value && SceneCatalog.GetSceneDefForCurrentScene() == ClassicItemsReturnsPlugin.bazaarScene) return;

            attackerBody.master.GiveMoney((uint)(goldOnHit * itemCount));
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                int itemCount = sender.inventory.GetItemCount(ItemDef);
                args.critAdd += itemCount * critChance;
            }
        }
    }
}
