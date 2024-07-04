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
        public const float classicJumpBonus = 0.1f;
        public const float classicJumpBonusStack = 0.1f;

        public const float jumpBonus = 0.2f;
        //public const float jumpBonusStackMult = 0.8f;
        public override string ItemName => "Rusty Jetpack";

        public override string ItemLangTokenName => "RUSTYJETPACK";

        public static GameObject jumpEffectPrefab;
        public static ConfigEntry<bool> useRework;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            jumpBonus
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Jetpack");

        public override Sprite ItemIcon => LoadItemSprite("Jetpack");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility,
            ItemTag.AIBlacklist
        };

        public override bool unfinished => true;

        public override void CreateConfig(ConfigFile config)
        {
            useRework = config.Bind(ConfigCategory, "Use Rework", true, "Reworks the item into providing a double jump.");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void CreateAssets(ConfigFile config)
        {
            base.CreateAssets(config);
            jumpEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab")
                .WaitForCompletion()
                .InstantiateClone("CIR_RustyJetpackEffect", false);

            jumpEffectPrefab.transform.localScale = 3f * Vector3.one;

            EffectComponent ec = jumpEffectPrefab.GetComponent<EffectComponent>();
            ec.soundName = "Play_ClassicItemsReturns_Jetpack";

            ContentAddition.AddEffect(jumpEffectPrefab);
        }

        public override void Hooks()
        {
            if (useRework.Value)
            {
                On.RoR2.CharacterBody.RecalculateStats += AddJumpCount;
                On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += ReworkJumpVelocity;
                IL.EntityStates.GenericCharacterMain.ProcessJump += ReplaceJumpEffect;
            }
            else
            {
                On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += ClassicJumpVelocity;
            }
        }

        private void ClassicJumpVelocity(On.EntityStates.GenericCharacterMain.orig_ApplyJumpVelocity orig, CharacterMotor characterMotor, CharacterBody characterBody, float horizontalBonus, float verticalBonus, bool vault)
        {
            if (characterBody && characterBody.inventory)
            {
                int itemCount = characterBody.inventory.GetItemCount(ItemDef);
                if (itemCount > 0) verticalBonus += ItemHelpers.StackingLinear(itemCount, classicJumpBonus, classicJumpBonusStack);
            }

            orig(characterMotor, characterBody, horizontalBonus, verticalBonus, vault);
        }

        private void ReworkJumpVelocity(On.EntityStates.GenericCharacterMain.orig_ApplyJumpVelocity orig, CharacterMotor characterMotor, CharacterBody characterBody, float horizontalBonus, float verticalBonus, bool vault)
        {
            if (characterBody)
            {
                bool isLastJump = characterMotor && characterMotor.jumpCount == characterBody.maxJumpCount - 1;
                int itemCount = 0;
                if (characterBody.inventory) itemCount = characterBody.inventory.GetItemCount(ItemDef);
                if (isLastJump && itemCount > 0)
                {
                    float heightBoost = 0f;
                    int stack = itemCount - 1;

                    heightBoost += jumpBonus * stack;

                    /*for (int i = 0; i < stack; i++)
                    {
                        heightBoost += jumpBonus * Mathf.Pow(jumpBonusStackMult, i);
                    }*/
                    verticalBonus += heightBoost;
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
                    bool isLastJump = self.characterMotor && self.characterMotor.jumpCount == self.characterBody.maxJumpCount - 1;
                    bool hasItem = self.characterBody.inventory && self.characterBody.inventory.GetItemCount(ItemDef) > 0;
                    if (isLastJump && hasItem)
                    {
                        return jumpEffectPrefab;
                    }
                }
                return origPrefab;
            });
        }

        private void AddJumpCount(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (GetCount(self) > 0)
                self.maxJumpCount++;
        }
    }
}