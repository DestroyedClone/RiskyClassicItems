using R2API;
using ClassicItemsReturns.Utils;
using RoR2;
using UnityEngine;
using ClassicItemsReturns.Modules;
using UnityEngine.AddressableAssets;

namespace ClassicItemsReturns.Items.Uncommon
{
    public class BearArmor : ItemBase<BearArmor>
    {
        public override string ItemName => "Tough Times";

        public override string ItemLangTokenName => "TOUGHTIMES";

        public float armor = 14f;
        public float armorPerStack = 14f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            armor,
            armorPerStack
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Bear");

        public override Sprite ItemIcon => LoadItemSprite("Bear");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility,
            ItemTag.CanBeTemporary
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (TryGetCount(sender, out int count))
            {
                args.armorAdd += ItemHelpers.StackingLinear(count, armor, armorPerStack);
            }
        }

        protected override void CreateCraftableDef()
        {
            ItemDef bear = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Bear/Bear.asset").WaitForCompletion();

            CraftableDef upgrade = ScriptableObject.CreateInstance<CraftableDef>();
            upgrade.pickup = ItemDef;
            upgrade.recipes = new Recipe[]
            {
                new Recipe()
                {
                    amountToDrop = 1,
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = bear
                        },
                        new RecipeIngredient()
                        {
                            pickup = bear
                        }
                    }
                }
            };
            PluginContentPack.craftableDefs.Add(upgrade);

            CraftableDef downgrade = ScriptableObject.CreateInstance<CraftableDef>();
            downgrade.pickup = bear;
            downgrade.recipes = new Recipe[]
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
                            pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Scrap/ScrapWhite.asset").WaitForCompletion()
                        }
                    }
                }
            };
            PluginContentPack.craftableDefs.Add(downgrade);
        }
    }
}