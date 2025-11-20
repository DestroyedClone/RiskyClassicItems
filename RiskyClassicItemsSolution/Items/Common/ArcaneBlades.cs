using ClassicItemsReturns.Utils;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ClassicItemsReturns.Items.Common
{
    public class ArcaneBlades : ItemBase<ArcaneBlades>
    {
        public override string ItemName => "Arcane Blades";

        public override string ItemLangTokenName => "ARCANEBLADES";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadItemModel("ArcaneBlades");

        public override Sprite ItemIcon => LoadItemSprite("ArcaneBlades");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility,
            ItemTag.CanBeTemporary
        };

        public float speedIncrease = 0.25f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            speedIncrease * 100f
        };

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            IsTeleActivatedTracker.OnTPActivatedActions += ApplySpeedBoost;
        }

        private void ApplySpeedBoost()
        {
            foreach (CharacterMaster cm in CharacterMaster.instancesList)
            {
                if (!cm.inventory) continue;

                if (cm.inventory && cm.inventory.GetItemCount(Instance.ItemDef) > 0)
                {
                    CharacterBody cb = cm.GetBody();
                    if (cb) cb.statsDirty = true;
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (IsTeleActivatedTracker.teleporterActivated && sender.inventory)
            {
                int itemCount = sender.inventory.GetItemCount(ItemDef);
                args.moveSpeedMultAdd += itemCount * speedIncrease;
            }
        }
    }
}
