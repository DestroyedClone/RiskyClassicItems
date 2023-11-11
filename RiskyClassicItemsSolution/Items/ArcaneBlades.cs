using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ClassicItemsReturns.Items
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
            ItemTag.Utility
        };

        public float speedIncrease = 0.2f;
        private static bool teleporterActivated = false;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            speedIncrease * 100f
        };

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            RoR2.Stage.onStageStartGlobal += Stage_onStageStartGlobal;

            //Do in fixedupdate to be extra sure in case of lag or whatever.
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;
            On.RoR2.TeleporterInteraction.ChargedState.OnEnter += ChargedState_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Active.OnEnter += Active_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Travelling.OnEnter += Travelling_OnEnter;
        }

        private void ChargedState_OnEnter(On.RoR2.TeleporterInteraction.ChargedState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            orig(self);
            ArcaneBlades.teleporterActivated = true;
        }

        private void Active_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Active.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Active self)
        {
            orig(self);
            ArcaneBlades.teleporterActivated = true;
        }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            orig(self);
            ArcaneBlades.teleporterActivated = true;
        }

        private void Travelling_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Travelling.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Travelling self)
        {
            orig(self);
            ArcaneBlades.teleporterActivated = false;
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            ArcaneBlades.teleporterActivated = false;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (ArcaneBlades.teleporterActivated && sender.inventory)
            {
                int itemCount = sender.inventory.GetItemCount(this.ItemDef);
                args.moveSpeedMultAdd += itemCount * speedIncrease;
            }
        }
    }
}
