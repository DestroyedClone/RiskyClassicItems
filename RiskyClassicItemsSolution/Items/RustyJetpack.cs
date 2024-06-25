using EntityStates;
using R2API;
using RoR2;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine.AddressableAssets;
using BepInEx.Configuration;

namespace ClassicItemsReturns.Items
{
    public class RustyJetpack : ItemBase<RustyJetpack>
    {
        public const float bonus = 0.2f;
        public const float bonusStackMult = 0.8f;
        public override string ItemName => "Rusty Jetpack";

        public override string ItemLangTokenName => "RUSTYJETPACK";

        public static GameObject jumpEffectPrefab;

        public override object[] ItemFullDescriptionParams => new object[]
        {
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
            On.RoR2.CharacterBody.RecalculateStats += AddJumpCount;
            On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += GenericCharacterMain_ApplyJumpVelocity;
            IL.EntityStates.GenericCharacterMain.ProcessJump += ReplaceJumpEffect;
        }

        private void GenericCharacterMain_ApplyJumpVelocity(On.EntityStates.GenericCharacterMain.orig_ApplyJumpVelocity orig, CharacterMotor characterMotor, CharacterBody characterBody, float horizontalBonus, float verticalBonus, bool vault)
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
                    for (int i = 0; i < stack; i++)
                    {
                        heightBoost += bonus * Mathf.Pow(bonusStackMult, i);
                    }
                    verticalBonus += heightBoost;
                }
            }
            orig(characterMotor, characterBody, horizontalBonus, verticalBonus, vault);
        }

        private void ReplaceJumpEffect(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After, x => x.MatchLdstr("Prefabs/Effects/FeatherEffect"));
            c.Index++;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<GameObject, EntityStates.GenericCharacterMain, GameObject>>((origPrefab, self) =>
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