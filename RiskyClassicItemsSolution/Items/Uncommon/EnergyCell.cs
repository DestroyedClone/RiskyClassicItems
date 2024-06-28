using ClassicItemsReturns.Utils;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RoR2.Items.BaseItemBodyBehavior;
using UnityEngine.Networking;
using RoR2.Items;
using ClassicItemsReturns.Modules;

namespace ClassicItemsReturns.Items.Uncommon
{
    public class EnergyCell : ItemBase<EnergyCell>
    {
        public override string ItemName => "Energy Cell";

        public override string ItemLangTokenName => "ENERGYCELL";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Cell");

        public override Sprite ItemIcon => LoadItemSprite("Cell");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.LowHealth
        };
        public const float atkSpdIncrease = 0.1f;
        public const float atkSpdIncreaseStack = 0.1f;
        public const float atkSpdIncreaseLowHealth = 0.3f;
        public const float atkSpdIncreaseLowHealthStack = 0.3f;

        public const float atkSpdIncreaseLowHealthThreshold = 0.25f;
        public const float atkSpdIncreaseLowHealthThresholdInverse = 1f - atkSpdIncreaseLowHealthThreshold;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            atkSpdIncrease * 100,
            atkSpdIncreaseStack * 100,
            (atkSpdIncrease + atkSpdIncreaseLowHealth) * 100,
            (atkSpdIncreaseStack + atkSpdIncreaseLowHealthStack) * 100
        };

        public override bool unfinished => true;

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var itemCount = GetCount(sender);
            if (itemCount == 0) return;

            float atkSpdPassive = ItemHelpers.StackingLinear(itemCount, atkSpdIncrease, atkSpdIncreaseStack);

            float maxAtkSpd = ItemHelpers.StackingLinear(itemCount, atkSpdIncreaseLowHealth, atkSpdIncreaseLowHealthStack);

            float boostPercent = (sender.healthComponent.combinedHealthFraction - atkSpdIncreaseLowHealthThreshold) / atkSpdIncreaseLowHealthThresholdInverse;
            float atkSpdConditional = Mathf.Lerp(0f, maxAtkSpd, 1f - boostPercent);

            args.attackSpeedMultAdd += atkSpdPassive + Mathf.FloorToInt(atkSpdConditional * 100f) / 100f;
        }

        public class EnergyCellBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => Instance.ItemDef;

            private int lastAtkSpdToInt = 0;
            private Inventory inventory;

            private void OnEnable()
            {
                inventory = body.inventory;
            }

            private void FixedUpdate()
            {
                if (!body.inventory) return;

                float healthPercent = (body.healthComponent.combinedHealthFraction - atkSpdIncreaseLowHealthThreshold) / atkSpdIncreaseLowHealthThresholdInverse;
                float totalBoost = Mathf.Lerp(0f,
                    ItemHelpers.StackingLinear(stack, atkSpdIncreaseLowHealth, atkSpdIncreaseLowHealthStack),
                    1f - healthPercent);

                //Only mark stats dirty if boost changes in a 1% increment, to prevent it from being spammed every frame
                int atkSpdToInt = Mathf.FloorToInt(totalBoost * 100);
                if (atkSpdToInt == lastAtkSpdToInt) return;

                lastAtkSpdToInt = atkSpdToInt;
                body.statsDirty = true;
            }
        }
    }
}
