using EntityStates;
using R2API;
using RoR2;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine.AddressableAssets;
using BepInEx.Configuration;
using ClassicItemsReturns.Utils;

namespace ClassicItemsReturns.Items.Uncommon
{
    public class RustyJetpack : ItemBase<RustyJetpack>
    {
        public const float jumpBonus = 0.2f;
        public const float jumpBonusStack = 0.2f;
        //public const float jumpBonusStackMult = 0.8f;
        public override string ItemName => "Rusty Jetpack";

        public override string ItemLangTokenName => "RUSTYJETPACK";

        public static GameObject jumpEffectPrefab;
        public static ConfigEntry<bool> enableAirhop;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            100f*jumpBonus,
            100f*jumpBonusStack
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Jetpack");

        public override Sprite ItemIcon => LoadItemSprite("Jetpack");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility,
            ItemTag.AIBlacklist
        };

        public override void CreateConfig(ConfigFile config)
        {
            enableAirhop = config.Bind(ConfigCategory, "Enable Airhop", true, "This item grants a short airhop.");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void CreateAssets(ConfigFile config)
        {
            base.CreateAssets(config);
            jumpEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2SmokeBombMini.prefab")
                .WaitForCompletion()
                .InstantiateClone("CIR_RustyJetpackEffect", false);

            EffectComponent ec = jumpEffectPrefab.GetComponent<EffectComponent>();
            ec.soundName = "Play_ClassicItemsReturns_Jetpack";

            ContentAddition.AddEffect(jumpEffectPrefab);
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += RecalculateStats_AddJump;
            On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += ApplyJumpVelocity_DoShorthop;
            IL.EntityStates.GenericCharacterMain.ProcessJump += ReplaceJumpEffect;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.inventory) return;
            int itemCount = sender.inventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            args.jumpPowerMultAdd += ItemHelpers.StackingLinear(itemCount, jumpBonus, jumpBonusStack);
        }

        private void ApplyJumpVelocity_DoShorthop(On.EntityStates.GenericCharacterMain.orig_ApplyJumpVelocity orig, CharacterMotor characterMotor, CharacterBody characterBody, float horizontalBonus, float verticalBonus, bool vault)
        {
            if (characterBody)
            {
                int itemCount = 0;
                if (characterBody.inventory) itemCount = characterBody.inventory.GetItemCount(ItemDef);
                if (itemCount > 0)
                {
                    bool isLastJump = RustyJetpack.enableAirhop.Value
                        && characterMotor && characterMotor.jumpCount == characterBody.maxJumpCount - 1;
                    if (isLastJump)
                    {
                        //Based on ApplyJumpVelocity code
                        Vector3 vector = characterMotor.moveDirection;
                        vector.y = 0f;
                        float magnitude = vector.magnitude;
                        if (magnitude > 0f)
                        {
                            vector /= magnitude;
                        }
                        Vector3 velocity = vector * characterBody.moveSpeed * horizontalBonus;
                        velocity.y = 18f;
                        characterMotor.velocity = velocity;
                        characterMotor.Motor.ForceUnground();
                        return;
                    }
                }
            }
            orig(characterMotor, characterBody, horizontalBonus, verticalBonus, vault);
        }

        private void ReplaceJumpEffect(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After, x => x.MatchLdstr("Prefabs/Effects/FeatherEffect"));
            c.Index++;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<GameObject, GenericCharacterMain, GameObject>>((origPrefab, self) =>
            {
                if (self.characterBody)
                {
                    bool isLastJump = RustyJetpack.enableAirhop.Value
                    && self.characterMotor && self.characterMotor.jumpCount == self.characterBody.maxJumpCount - 1;
                    bool hasItem = self.characterBody.inventory && self.characterBody.inventory.GetItemCount(ItemDef) > 0;
                    if (isLastJump && hasItem)
                    {
                        return jumpEffectPrefab;
                    }
                }
                return origPrefab;
            });
        }

        private void RecalculateStats_AddJump(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (GetCount(self) > 0)
                self.maxJumpCount++;
        }
    }
}