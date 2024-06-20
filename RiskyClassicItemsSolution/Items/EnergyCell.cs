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

namespace ClassicItemsReturns.Items
{
    public class EnergyCell : ItemBase<EnergyCell>
    {
        public override string ItemName => "Energy Cell";

        public override string ItemLangTokenName => "ENERGYCELL";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("EnergyCell");

        public override Sprite ItemIcon => LoadItemSprite("EnergyCell");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage
        };

        public const float atkSpdIncrease = 0.1f;
        public const float atkSpdIncreaseStack = 0.01f;
        public const float atkSpdIncreaseLowHealth = 0.3f;
        public const float atkSpdIncreaseLowHealthStack = 0.03f;
        public const float atkSpdIncreaseLowHealthThreshold = 0.25f;
        public const float atkSpdIncreaseLowHealthThresholdInverse = 0.75f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            atkSpdIncrease * 100,
            atkSpdIncreaseStack * 100,
            atkSpdIncreaseLowHealth * 100,
            atkSpdIncreaseLowHealthStack * 100,
            atkSpdIncreaseLowHealthThreshold * 100
        };
        public override bool Unfinished => true;

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var itemCount = GetCount(sender);
            if (itemCount == 0) return;

            float calcAtkSpdPassive = Utils.ItemHelpers.StackingLinear(itemCount, atkSpdIncrease, atkSpdIncreaseStack);

            float calcAtkSpdCap = Utils.ItemHelpers.StackingLinear(itemCount, atkSpdIncreaseLowHealth, atkSpdIncreaseLowHealthStack);

            float boostPercent = (sender.healthComponent.combinedHealthFraction - atkSpdIncreaseLowHealthThreshold) / atkSpdIncreaseLowHealthThresholdInverse;
            float calcAtkSpdConditional = itemCount * Mathf.Lerp(0f, calcAtkSpdCap, 1f - boostPercent);

            args.attackSpeedMultAdd += calcAtkSpdPassive;
            args.attackSpeedMultAdd += calcAtkSpdConditional;
        }
        public class EnergyCellBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => EnergyCell.Instance.ItemDef;
            public float lastAttackSpeedBoost = 0;

            public float attackSpeedMax = 1f;

            public void OnEnable()
            {
                body.onInventoryChanged += Body_onInventoryChanged;
                Body_onInventoryChanged();
            }

            private void Body_onInventoryChanged()
            {
                attackSpeedMax = Utils.ItemHelpers.StackingLinear(stack, atkSpdIncreaseLowHealth, atkSpdIncreaseLowHealthStack);
            }

            private void FixedUpdate() => MarkStatsDirty();

            public void MarkStatsDirty()
            {
                float boostPercent = (body.healthComponent.combinedHealthFraction - atkSpdIncreaseLowHealthThreshold) / atkSpdIncreaseLowHealthThresholdInverse;
                float totalBoost = stack * Mathf.Lerp(0f, attackSpeedMax, 1f - boostPercent);
                if (totalBoost == lastAttackSpeedBoost)
                    return;
                lastAttackSpeedBoost = totalBoost;
                body.statsDirty = true;
            }

            public void OnDisable()
            {
                body.onInventoryChanged -= Body_onInventoryChanged;
            }
        }
    }
}
