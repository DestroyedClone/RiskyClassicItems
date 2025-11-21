using ClassicItemsReturns.Items.Common;
using ClassicItemsReturns.Modules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items.Uncommon
{
    public class SmartShopper : ItemBase<SmartShopper>
    {
        public float moneyMult = 0.25f;
        public float moneyMultStack = 0.25f;

        public override string ItemName => "Smart Shopper";

        public override string ItemLangTokenName => "SMARTSHOPPER";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Purse");

        public override Sprite ItemIcon => LoadItemSprite("Purse");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility, ItemTag.OnKillEffect, ItemTag.AIBlacklist,
            ItemTag.CanBeTemporary
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override object[] ItemFullDescriptionParams => new object[]
        {
            moneyMult * 100f,
            moneyMultStack * 100f
        };

        public override void Hooks()
        {
            base.Hooks();
            On.RoR2.DeathRewards.OnKilledServer += DeathRewards_OnKilledServer;
        }

        protected override void CreateCraftableDef()
        {
            bool penniesEnabled = RazorPenny.Instance != null && RazorPenny.Instance.ItemDef;
            bool savingsEnabled = LifeSavings.Instance != null && LifeSavings.Instance.ItemDef;

            if (penniesEnabled || savingsEnabled)
            {
                ItemDef collector = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC3/Items/SpeedOnPickup/SpeedOnPickup.asset").WaitForCompletion();

                CraftableDef craftable = ScriptableObject.CreateInstance<CraftableDef>();
                craftable.pickup = ItemDef;
                craftable.recipes = new Recipe[0];

                if (penniesEnabled)
                {
                    craftable.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = RazorPenny.Instance.ItemDef
                            },
                            new RecipeIngredient()
                            {
                                pickup = collector
                            }
                        }
                    }).ToArray();
                }

                if (savingsEnabled)
                {
                    craftable.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = LifeSavings.Instance.ItemDef
                            },
                            new RecipeIngredient()
                            {
                                pickup = collector
                            }
                        }
                    }).ToArray();
                }
                (craftable as ScriptableObject).name = "cdSmartShopper";

                PluginContentPack.craftableDefs.Add(craftable);
            }
        }

        private void DeathRewards_OnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport damageReport)
        {
            CharacterBody attackerBody = damageReport.attackerBody;
            if (attackerBody && attackerBody.inventory)
            {
                int itemCount = attackerBody.inventory.GetItemCount(ItemDef);
                if (itemCount > 0)
                {
                    int itemStack = itemCount - 1;
                    float mult = 1f + moneyMult + itemStack * moneyMultStack;

                    self.goldReward = (uint)Mathf.FloorToInt(self.goldReward * mult);
                }
            }

            orig(self, damageReport);
        }
    }
}
