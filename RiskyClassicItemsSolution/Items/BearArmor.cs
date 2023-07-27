using R2API;
using RiskyClassicItems.Utils;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Items
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
            ItemTag.Utility
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
    }
}