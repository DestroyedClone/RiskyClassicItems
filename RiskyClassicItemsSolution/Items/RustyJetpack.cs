using R2API;
using ClassicItemsReturns.Utils;
using RoR2;
using UnityEngine;
using static RoR2.Items.BaseItemBodyBehavior;
using RoR2.Items;
using EntityStates;

namespace ClassicItemsReturns.Items
{
    public class RustyJetpack : ItemBase<RustyJetpack>
    {
        public const float bonus = 0.5f;
        public const float bonusStack = 0.05f;
        public override string ItemName => "Rusty Jetpack";

        public override string ItemLangTokenName => "RUSTYJETPACK";

        public override object[] ItemFullDescriptionParams => new object[]
        {
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Bear");

        public override Sprite ItemIcon => LoadItemSprite("Bear");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility
        };

        public override bool Unfinished => true;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            //RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            //On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
            On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += GenericCharacterMain_ApplyJumpVelocity;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (GetCount(self) > 0)
                self.maxJumpCount++;
        }

        private void GenericCharacterMain_ApplyJumpVelocity(On.EntityStates.GenericCharacterMain.orig_ApplyJumpVelocity orig, CharacterMotor characterMotor, CharacterBody characterBody, float horizontalBonus, float verticalBonus, bool vault)
        {
            var itemCount = GetCount(characterBody);
            if (itemCount > 0 && characterMotor.jumpCount == characterBody.maxJumpCount - 1)
            {
                verticalBonus = bonus;

                if (itemCount > 1) //otherwise first stack will be scaled
                    verticalBonus += Util.ConvertAmplificationPercentageIntoReductionPercentage(Utils.ItemHelpers.StackingLinear(itemCount, bonus, bonusStack));
            }
            orig(characterMotor, characterBody, horizontalBonus, verticalBonus, vault);
        }

        //called in authority
        private void GenericCharacterMain_ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, GenericCharacterMain self)
        {
            orig(self);
            if (!self.hasCharacterMotor) return;

            if (!self.jumpInputReceived || !self.characterBody || self.characterMotor.jumpCount >= self.characterBody.maxJumpCount)
            {
                return;
            }
            if (GetCount(self.characterBody) > 0 && self.characterMotor.jumpCount > 0 && self.characterMotor.jumpCount == self.characterBody.maxJumpCount - 1)
            {
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CharacterLandImpact"), new EffectData
                {
                    origin = self.characterBody.footPosition,
                    scale = self.characterBody.radius
                }, true);
            }
        }
    }
}