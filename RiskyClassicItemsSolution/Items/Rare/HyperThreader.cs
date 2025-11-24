using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;
using RoR2.Orbs;
using System;
using UnityEngine.AddressableAssets;
using ClassicItemsReturns.Equipment;
using static RoR2.MasterSpawnSlotController;
using ClassicItemsReturns.Items.Uncommon;
using System.Linq;

namespace ClassicItemsReturns.Items.Rare
{
    public class HyperThreader : ItemBase<HyperThreader>
    {
        float chance = 100;
        float damageCoeff = 0.8f;
        float bounceRange = 30f;
        int bounceCount = 2;
        int bounceCountPerStack = 1;
        public static GameObject orbEffect;
        public static NetworkSoundEventDef procSound;

        public override string ItemName => "Hyper-Threader";

        public override string ItemLangTokenName => "HYPERTHREADER";

        public override object[] ItemFullDescriptionParams => new object[]
        {
            chance,
            damageCoeff * 100,
            bounceCount,
            bounceCountPerStack,
        };

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("HyperThreader");

        public override Sprite ItemIcon => LoadItemSprite("HyperThreader");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.AIBlacklist,
            ItemTag.CanBeTemporary,
            ItemTag.Technology
        };

        public override void CreateAssets(ConfigFile config)
        {
            orbEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/ChainGunOrbEffect.prefab").WaitForCompletion().InstantiateClone("CIR_HyperThreaderOrbEffect", false);
            var tr = orbEffect.transform.Find("TrailParent/Trail").GetComponent<TrailRenderer>();
            tr.startColor = Color.red;
            tr.endColor = Color.red;

            PluginContentPack.effectDefs.Add(new EffectDef(orbEffect));

            procSound = Modules.Assets.CreateNetworkSoundEventDef("Play_ClassicItemsReturns_Reflect");

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            SneedHooks.ProcessHitEnemy.OnHitAttackerActions += OnHit;
        }

        private void OnHit(DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody)
        {
            if (!attackerBody.inventory) return;
            int itemCount = attackerBody.inventory.GetItemCountEffective(ItemDef);
            if (itemCount <= 0) return;

            if (!Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master)) return;

            int calcBounceCount = Utils.ItemHelpers.StackingLinear(itemCount, bounceCount, bounceCountPerStack);
            ChainGunOrb chainGunOrb = new ChainGunOrb(orbEffect);
            chainGunOrb.damageValue = attackerBody.damage * damageCoeff;
            chainGunOrb.isCrit = damageInfo.crit;
            chainGunOrb.teamIndex = attackerBody.teamComponent.teamIndex;
            chainGunOrb.attacker = attackerBody.gameObject;
            chainGunOrb.procCoefficient = 0f;
            chainGunOrb.procChainMask = damageInfo.procChainMask;
            chainGunOrb.origin = attackerBody.corePosition;
            chainGunOrb.speed = 150f;
            chainGunOrb.bouncesRemaining = calcBounceCount;
            chainGunOrb.bounceRange = bounceRange;
            chainGunOrb.damageCoefficientPerBounce = 1f;
            chainGunOrb.targetsToFindPerBounce = 1;
            chainGunOrb.canBounceOnSameTarget = false;
            chainGunOrb.damageColorIndex = DamageColorIndex.Item;
            chainGunOrb.damageType = new DamageTypeCombo()
            {
                damageTypeExtended = DamageTypeExtended.Electrical
            };

            chainGunOrb.target = victimBody.mainHurtBox;
            OrbManager.instance.AddOrb(chainGunOrb);

            RoR2.Audio.EntitySoundManager.EmitSoundServer(procSound.index, damageInfo.attacker);
        }

        protected override void CreateCraftableDef()
        {
            bool golden = GoldenGun.Instance != null && GoldenGun.Instance.ItemDef;
            bool energy = EnergyCell.Instance != null && EnergyCell.Instance.ItemDef;
            if (golden || energy)
            {
                CraftableDef craftable = ScriptableObject.CreateInstance<CraftableDef>();
                craftable.pickup = ItemDef;
                craftable.recipes = new Recipe[0];
                if (golden)
                {
                    craftable.recipes = craftable.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/ShockNearby/ShockNearby.asset").WaitForCompletion()
                            },
                            new RecipeIngredient()
                            {
                                pickup = GoldenGun.Instance.ItemDef
                            }
                        }
                    }).ToArray();

                    craftable.recipes = craftable.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/Lightning/Lightning.asset").WaitForCompletion()
                            },
                            new RecipeIngredient()
                            {
                                pickup = GoldenGun.Instance.ItemDef
                            }
                        }
                    }).ToArray();
                }

                if (energy)
                {
                    craftable.recipes = craftable.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                            {
                            new RecipeIngredient()
                            {
                                pickup = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/GoldGat/GoldGat.asset").WaitForCompletion()
                            },
                            new RecipeIngredient()
                            {
                                pickup = EnergyCell.Instance.ItemDef
                            }
                            }
                    }).ToArray();
                }

                if (energy && golden)
                {
                    craftable.recipes = craftable.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = GoldenGun.Instance.ItemDef
                            },
                            new RecipeIngredient()
                            {
                                pickup = EnergyCell.Instance.ItemDef
                            }
                        }
                    }).ToArray();
                }

                (craftable as ScriptableObject).name = "cdHyperThreader";
                
                PluginContentPack.craftableDefs.Add(craftable);
            }
        }
    }
}