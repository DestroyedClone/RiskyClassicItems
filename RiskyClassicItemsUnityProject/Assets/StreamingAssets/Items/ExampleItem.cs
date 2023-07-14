using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Items
{
    public class ExampleItem : ItemBase<ExampleItem>
    {
        public override string ItemName => "Deprecate Me Item";

        public override string ItemLangTokenName => "DEPRECATE_ME_ITEM";

        public override object[] ItemFullDescriptionParams => new object[]
        {
            (0.5f*100).ToString(),
            100.ToString()
        };

        public override object[] ItemPickupDescParams => new object[]
        {
            112345f.ToString()
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadPickupModel("");

        public override Sprite ItemIcon => LoadItemIcon("");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
        }
    }
}