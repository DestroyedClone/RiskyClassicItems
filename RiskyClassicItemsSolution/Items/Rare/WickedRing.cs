using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using RoR2;
using UnityEngine;
using System;
using ClassicItemsReturns.Items.Uncommon;
using UnityEngine.AddressableAssets;
using ClassicItemsReturns.Equipment;
using ClassicItemsReturns.Items.Common;
using System.Linq;

namespace ClassicItemsReturns.Items.Rare
{
    public class WickedRing : ItemBase<WickedRing>
    {
        public ConfigEntry<bool> useAlternateVersion;
        public override string ItemName => "Wicked Ring";

        public override string ItemLangTokenName => "WICKEDRING";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("SkullRing");

        public override Sprite ItemIcon => LoadItemSprite("SkullRing");

        public float cdr = 1f;
        public float cdrStack = 0.5f;
        public float bonusCrit = 5f;
        public override object[] ItemFullDescriptionParams => new object[]
        {
            bonusCrit,
            cdr,
            cdrStack
        };


        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage, ItemTag.Utility,
            ItemTag.CanBeTemporary
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            base.Init(config);
            if (useAlternateVersion.Value && ItemDef.DoesNotContainTag(ItemTag.OnKillEffect)) HG.ArrayUtils.ArrayAppend(ref ItemDef.tags, ItemTag.OnKillEffect);
        }

        public override void CreateConfig(ConfigFile config)
        {
            useAlternateVersion = config.Bind(ConfigCategory, "Use Rework", false, "Reworks the item into giving CDR on kill. If false, gives CDR on crit like in Risk of Rain 1.");
        }

        protected override void CreateLang()
        {
            base.CreateLang();
            LanguageOverrides.DeferToken("CLASSICITEMSRETURNS_ITEM_WICKEDRING_DESCRIPTION_ALT", cdr, cdrStack);
        }

        public override void Hooks()
        {
            base.Hooks();
            if (useAlternateVersion.Value)
            {
                ItemDef.pickupToken += "_ALT";
                ItemDef.descriptionToken += "_ALT";
                SharedHooks.OnCharacterDeath.OnCharacterDeathAttackerInventoryActions += CooldownOnKill;
            }
            else
            {
                RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
                SneedHooks.ProcessHitEnemy.OnHitAttackerActions += CooldownOnCrit;
            }
        }

        private void CooldownOnCrit(DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody)
        {
            if (!damageInfo.crit || damageInfo.procCoefficient <= 0f || !attackerBody.inventory || !attackerBody.skillLocator) return;
            int itemCount = attackerBody.inventory.GetItemCountEffective(ItemDef);
            if (itemCount <= 0) return;

            int itemStack = itemCount - 1;
            float totalReduction = cdr + itemStack * cdrStack;
            attackerBody.skillLocator.DeductCooldownFromAllSkillsServer(totalReduction * damageInfo.procCoefficient);
        }

        private void CooldownOnKill(GlobalEventManager globalEventManager, DamageReport damageReport, CharacterBody attackerBody, Inventory attackerInventory)
        {
            int itemCount = attackerInventory.GetItemCountEffective(ItemDef);
            if (itemCount <= 0 || !attackerBody.skillLocator) return;

            int itemStack = itemCount - 1;
            float totalReduction = cdr + itemStack * cdrStack;
            attackerBody.skillLocator.DeductCooldownFromAllSkillsServer(totalReduction);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory && sender.inventory.GetItemCountEffective(ItemDef) > 0) args.critAdd += bonusCrit;
        }

        private void CooldownOnCrit(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, CharacterBody attackerBody, Inventory attackerInventory)
        {
            if (!damageInfo.crit || damageInfo.procCoefficient <= 0f || !attackerBody.skillLocator) return;
            int itemCount = attackerInventory.GetItemCountEffective(ItemDef);
            if (itemCount <= 0) return;

            int itemStack = itemCount - 1;
            float totalReduction = cdr + itemStack * cdrStack;
            attackerBody.skillLocator.DeductCooldownFromAllSkillsServer(totalReduction * damageInfo.procCoefficient);
        }

        protected override void CreateCraftableDef()
        {
            if (!ItemDef) return;
            ItemDef talisman = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Talisman/Talisman.asset").WaitForCompletion();

            bool amethyst = ResetSkillCooldown.Instance != null && ResetSkillCooldown.Instance.EquipmentDef;
            bool snakeEyes = SnakeEyes.Instance != null && SnakeEyes.Instance.ItemDef;

            if (amethyst || snakeEyes)
            {
                CraftableDef craftable = ScriptableObject.CreateInstance<CraftableDef>();
                craftable.pickup = ItemDef;
                craftable.recipes = new Recipe[0];
                if (amethyst)
                {
                    craftable.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = talisman
                            },
                            new RecipeIngredient()
                            {
                                pickup = ResetSkillCooldown.Instance.EquipmentDef
                            }
                        }
                    }).ToArray();
                }
                if (snakeEyes)
                {
                    craftable.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = talisman
                            },
                            new RecipeIngredient()
                            {
                                pickup = SnakeEyes.Instance.ItemDef
                            }
                        }
                    }).ToArray();
                }
                (craftable as ScriptableObject).name = "cdWickedRing";
                PluginContentPack.craftableDefs.Add(craftable);
            }

            CraftableDef toTalisman = ScriptableObject.CreateInstance<CraftableDef>();
            toTalisman.pickup = talisman;
            toTalisman.recipes = new Recipe[]
            {
                new Recipe()
                {
                    amountToDrop = 1,
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = ItemDef
                        },
                        new RecipeIngredient()
                        {
                            pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/EquipmentMagazine/EquipmentMagazine.asset").WaitForCompletion()
                        }
                    }
                },
                new Recipe()
                {
                    amountToDrop = 1,
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = ItemDef
                        },
                        new RecipeIngredient()
                        {
                            pickup = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/DeathProjectile/DeathProjectile.asset").WaitForCompletion()
                        }
                    }
                }
            };
            (toTalisman as ScriptableObject).name = "cdWickedRingToTalisman";
            PluginContentPack.craftableDefs.Add(toTalisman);
        }
    }
}
