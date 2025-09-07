using ClassicItemsReturns.Modules;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items.Uncommon
{
    public class GuardiansHeart : ItemBase<GuardiansHeart>
    {
        public override string ItemName => "Guardians Heart";

        public override string ItemLangTokenName => "GUARDIANSHEART";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("GuardiansHeart");

        public override Sprite ItemIcon => LoadItemSprite("GuardiansHeart");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public float shield = 0.12f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            shield * 100f,
            shield * 100f
        };

        public override void Hooks()
        {
            IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.ServerFixedUpdate += ManageBuff;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                int itemCount = sender.inventory.GetItemCount(ItemDef);
                if (itemCount > 0)
                {
                    args.baseShieldAdd += sender.baseMaxHealth * shield * itemCount;
                    args.levelShieldAdd += sender.levelMaxHealth * shield * itemCount;
                }
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (!NetworkServer.active || !body.inventory) return;

            if (body.HasBuff(Buffs.GuardiansHeartReadyBuff))
            {
                if (body.inventory.GetItemCount(ItemDef) <= 0) body.RemoveBuff(Buffs.GuardiansHeartReadyBuff);
            }
            else
            {
                if (body.inventory.GetItemCount(ItemDef) > 0 && body.healthComponent && body.healthComponent.shield >= body.healthComponent.fullShield)
                {
                    body.AddBuff(Buffs.GuardiansHeartReadyBuff);
                }
            }
        }

        private void ManageBuff(On.RoR2.HealthComponent.orig_ServerFixedUpdate orig, HealthComponent self, float deltaTime)
        {
            orig(self, deltaTime);
            CharacterBody body = self.body;

            if (self.shield <= 0f)
            {
                if (body.HasBuff(Buffs.GuardiansHeartReadyBuff)) body.RemoveBuff(Buffs.GuardiansHeartReadyBuff);
            }
            else if (self.shield >= self.fullShield)
            {
                if (body.inventory && body.inventory.GetItemCount(ItemDef) > 0 && !body.HasBuff(Buffs.GuardiansHeartReadyBuff))
                {
                    body.AddBuff(Buffs.GuardiansHeartReadyBuff);
                }
            }
        }

        private void HealthComponent_TakeDamage(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(
                 x => x.MatchLdloc(8),
                 x => x.MatchLdarg(0),
                 x => x.MatchLdfld<HealthComponent>("shield"),
                 x => x.MatchSub(),
                 x => x.MatchStloc(8),
                 x => x.MatchLdarg(0)
                ))
            {
                c.Index += 4;
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((remainingDamage, self, damageInfo) =>
                {
                    bool bypassShield = (damageInfo.damageType & DamageType.BypassArmor) == DamageType.BypassArmor
                    || (damageInfo.damageType & DamageType.BypassOneShotProtection) == DamageType.BypassOneShotProtection
                    || (damageInfo.damageType & DamageType.BypassBlock) == DamageType.BypassBlock
                    || (damageInfo.damageType & DamageType.VoidDeath) == DamageType.VoidDeath;

                    CharacterBody body = self.body;
                    bool canShieldgate = !bypassShield && body.HasBuff(Buffs.GuardiansHeartReadyBuff);

                    if (canShieldgate)
                    {
                        body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex, Time.fixedDeltaTime);
                        remainingDamage = 0f;
                    }
                    body.RemoveBuff(Buffs.GuardiansHeartReadyBuff);
                    return remainingDamage;
                });
            }
            else
            {
                UnityEngine.Debug.LogError("ClassicItemsReturns: ShieldGating TakeDamage IL Hook failed");
            }
        }
    }
}
