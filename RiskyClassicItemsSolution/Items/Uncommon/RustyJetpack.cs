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
        public const float jumpBonus = 0.1f;
        public const float jumpBonusStack = 0.1f;
        //public const float jumpBonusStackMult = 0.8f;
        public override string ItemName => "Rusty Jetpack";

        public override string ItemLangTokenName => "RUSTYJETPACK";

        public static GameObject jumpEffectPrefab;
        public static ConfigEntry<bool> enableAirhop;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            10f*jumpBonus,
            10f*jumpBonusStack
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

            jumpEffectPrefab.transform.localScale = 3f * Vector3.one;

            EffectComponent ec = jumpEffectPrefab.GetComponent<EffectComponent>();
            ec.soundName = "Play_ClassicItemsReturns_Jetpack";

            ContentAddition.AddEffect(jumpEffectPrefab);
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += AddJumpCount;
            On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += ReworkJumpVelocity;
            IL.EntityStates.GenericCharacterMain.ProcessJump += ReplaceJumpEffect;
        }

        private void ReworkJumpVelocity(On.EntityStates.GenericCharacterMain.orig_ApplyJumpVelocity orig, CharacterMotor characterMotor, CharacterBody characterBody, float horizontalBonus, float verticalBonus, bool vault)
        {
            if (characterBody)
            {
                int itemCount = 0;
                if (characterBody.inventory) itemCount = characterBody.inventory.GetItemCount(ItemDef);
                if (itemCount > 0)
                {
                    bool isLastJump = RustyJetpack.enableAirhop.Value
                        && characterMotor && characterMotor.jumpCount == characterBody.maxJumpCount - 1;
                    if (!isLastJump)
                    {
                        float heightBoost = ItemHelpers.StackingLinear(itemCount, jumpBonus, jumpBonusStack);
                        verticalBonus += heightBoost;
                    }
                    else
                    {
                        //Prevent a negative verticalBonus
                        float reduced = verticalBonus - 0.5f;
                        verticalBonus = reduced >= 0f ? reduced : verticalBonus * 0.5f;
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

        private void AddJumpCount(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (GetCount(self) > 0)
                self.maxJumpCount++;
        }
    }
}