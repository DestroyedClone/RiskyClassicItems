using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Utils;
using RoR2;
using UnityEngine;
using ClassicItemsReturns.Modules;
using UnityEngine.AddressableAssets;

namespace ClassicItemsReturns.Items.Common
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/MysteriousVial.cs
    public class MysteriousVial : ItemBase<MysteriousVial>
    {
        public override string ItemName => "Mysterious Vial";

        public override string ItemLangTokenName => "MYSTERIOUSVIAL";

        private float regen = 1f;
        private float regenPerStack = 1f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            regen,
            regenPerStack
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadItemModel("Vial");

        public override Sprite ItemIcon => LoadItemSprite("Vial");

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Healing,
            ItemTag.CanBeTemporary
        };

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (TryGetCount(sender, out int count))
            {
                float levelFactor = 1f + 0.2f * (sender.level - 1f);
                args.baseRegenAdd += ItemHelpers.StackingLinear(count, regen, regenPerStack) * levelFactor;
            }
        }

        protected override void CreateCraftableDef()
        {
            CraftableDef toInfusion = ScriptableObject.CreateInstance<CraftableDef>();
            toInfusion.pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Infusion/Infusion.asset").WaitForCompletion();
            toInfusion.recipes = new Recipe[]
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
                            pickup = ItemDef
                        }
                    }
                }
            };
            (toInfusion as ScriptableObject).name = "cdMysteriousVialToInfusion";
            PluginContentPack.craftableDefs.Add(toInfusion);
        }
    }
}