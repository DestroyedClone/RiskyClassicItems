using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Utils;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Items
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/MysteriousVial.cs
    public class MysteriousVial : ItemBase<MysteriousVial>
    {
        public override string ItemName => "Mysterious Vial";

        public override string ItemLangTokenName => "MYSTERIOUSVIAL";

        float regen = 0.7f;
        float regenPerStack = 0.6f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            regen,
            regenPerStack
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadPickupModel("MysteriousVial");

        public override Sprite ItemIcon => LoadItemIcon("MysteriousVial");

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Healing
        };

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (TryGetCount(sender, out int count))
            {
                args.baseRegenAdd += ItemHelpers.StackingLinear(count, regen, regenPerStack);
            }
        }
    }
}