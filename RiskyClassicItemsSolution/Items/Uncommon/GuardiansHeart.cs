using BepInEx.Configuration;
using ClassicItemsReturns.Modules;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
            ItemTag.Utility,
            ItemTag.CanBeTemporary,
            ItemTag.Technology,
            ItemTag.FoodRelated
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public float shield = 0.12f;
        public float armor = 100f;
        public ConfigEntry<bool> useFullInvuln;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            shield * 100f,
            shield * 100f,
            armor,
            armor
        };
        protected override void CreateLang()
        {
            base.CreateLang();
            LanguageOverrides.DeferToken("CLASSICITEMSRETURNS_ITEM_GUARDIANSHEART_DESCRIPTION_ALT", ItemFullDescriptionParams[0], ItemFullDescriptionParams[1]);
        }

        public override void CreateConfig(ConfigFile config)
        {
            useFullInvuln = config.Bind(ConfigCategory, "Full Invulnerability", false, "Gain i-frames on shield break.");
        }

        public override void Hooks()
        {
            if (useFullInvuln.Value)
            {
                ItemDef.descriptionToken += "_ALT";
                IL.RoR2.HealthComponent.TakeDamageProcess += FullInvuln;
            }
            else
            {
                IL.RoR2.HealthComponent.TakeDamageProcess += ArmorOnShieldBreak;
            }

                On.RoR2.HealthComponent.ServerFixedUpdate += ManageBuff;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                int itemCount = sender.inventory.GetItemCountEffective(ItemDef);
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
                if (body.inventory.GetItemCountEffective(ItemDef) <= 0) body.RemoveBuff(Buffs.GuardiansHeartReadyBuff);
            }
            else
            {
                if (body.inventory.GetItemCountEffective(ItemDef) > 0 && body.healthComponent && body.healthComponent.shield >= body.healthComponent.fullShield)
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
                if (body.inventory && body.inventory.GetItemCountEffective(ItemDef) > 0 && !body.HasBuff(Buffs.GuardiansHeartReadyBuff))
                {
                    body.AddBuff(Buffs.GuardiansHeartReadyBuff);
                }
            }
        }

        private void FullInvuln(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                 x => x.MatchLdfld<HealthComponent>("shield"),
                 x => x.MatchSub()
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((remainingDamage, self, damageInfo) =>
                {
                    bool bypassShield = (damageInfo.damageType & DamageType.BypassArmor) == DamageType.BypassArmor
                    || (damageInfo.damageType & DamageType.BypassOneShotProtection) == DamageType.BypassOneShotProtection
                    || (damageInfo.damageType & DamageType.BypassBlock) == DamageType.BypassBlock
                    || (damageInfo.damageType & DamageType.VoidDeath) == DamageType.VoidDeath;

                    /* Debug.Log((damageInfo.damageType & DamageType.BypassArmor) == DamageType.BypassArmor);
                     Debug.Log((damageInfo.damageType & DamageType.BypassOneShotProtection) == DamageType.BypassOneShotProtection);
                     Debug.Log((damageInfo.damageType & DamageType.BypassBlock) == DamageType.BypassBlock);
                     Debug.Log((damageInfo.damageType & DamageType.VoidDeath) == DamageType.VoidDeath);*/

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
                UnityEngine.Debug.LogError("ClassicItemsReturns: ShieldGating FullInvuln TakeDamage IL Hook failed");
            }
        }

        private void ArmorOnShieldBreak(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                 x => x.MatchLdfld<HealthComponent>("shield"),
                 x => x.MatchSub()
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((remainingDamage, self, damageInfo) =>
                {
                    bool bypassShield = (damageInfo.damageType & DamageType.BypassArmor) == DamageType.BypassArmor
                    || (damageInfo.damageType & DamageType.BypassOneShotProtection) == DamageType.BypassOneShotProtection
                    || (damageInfo.damageType & DamageType.BypassBlock) == DamageType.BypassBlock
                    || (damageInfo.damageType & DamageType.VoidDeath) == DamageType.VoidDeath;

                    /* Debug.Log((damageInfo.damageType & DamageType.BypassArmor) == DamageType.BypassArmor);
                     Debug.Log((damageInfo.damageType & DamageType.BypassOneShotProtection) == DamageType.BypassOneShotProtection);
                     Debug.Log((damageInfo.damageType & DamageType.BypassBlock) == DamageType.BypassBlock);
                     Debug.Log((damageInfo.damageType & DamageType.VoidDeath) == DamageType.VoidDeath);*/

                    CharacterBody body = self.body;
                    bool canShieldgate = !bypassShield && body.HasBuff(Buffs.GuardiansHeartReadyBuff);

                    if (canShieldgate)
                    {
                        //Get armor values
                        float currentArmor = body.armor + self.adaptiveArmorValue;
                        int itemCount = 1;
                        if (body.inventory) itemCount = Mathf.Max(1, body.inventory.GetItemCountEffective(ItemDef));
                        float desiredArmor = currentArmor + itemCount * armor;

                        //Set effective damage resistance
                        remainingDamage *= (100f + currentArmor) / (100f + desiredArmor);
                    }
                    body.RemoveBuff(Buffs.GuardiansHeartReadyBuff);
                    return remainingDamage;
                });
            }
            else
            {
                UnityEngine.Debug.LogError("ClassicItemsReturns: ShieldGating Armor TakeDamage IL Hook failed");
            }
        }

        protected override void CreateCraftableDef()
        {
            if (EnergyCell.Instance != null && EnergyCell.Instance.ItemDef)
            {
                CraftableDef craftable = ScriptableObject.CreateInstance<CraftableDef>();
                craftable.pickup = ItemDef;
                craftable.recipes = new Recipe[]
                {
                    new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = EnergyCell.Instance.ItemDef
                            },
                            new RecipeIngredient()
                            {
                                pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/FlatHealth/FlatHealth.asset").WaitForCompletion()
                            }
                        }
                    }
                };
                (craftable as ScriptableObject).name = "cdGuardiansHeart";
                PluginContentPack.craftableDefs.Add(craftable);
            }
        }
    }
}
