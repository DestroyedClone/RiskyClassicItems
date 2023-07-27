using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Items;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RiskyClassicItems.Items.ArmsRace;
using static RoR2.Items.BaseItemBodyBehavior;

namespace RiskyClassicItems.Items
{
    public class LifeSavings : ItemBase<LifeSavings>
    {
        int gold = 1;
        int goldStack = 1;
        float intervalSeconds = 3;
        public override string ItemName => "LifeSavings";

        public override string ItemLangTokenName => ItemName.ToUpper();

        public override object[] ItemFullDescriptionParams => new object[]
        {
            gold,
            goldStack,
            intervalSeconds
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadItemModel("Pig");

        public override Sprite ItemIcon => LoadItemSprite("Pig");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility,
            ItemTag.AIBlacklist
        };

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public static ConfigEntry<string> cfgBannedSceneNames;
        public static string[] bannedSceneDefNames = new string[] { };
        public override void CreateConfig(ConfigFile config)
        {
            cfgBannedSceneNames = config.Bind(ConfigCategory, "Banned Scenes", "bazaar", "Input the names of the scenes you don't want the item to perform on. Entries are separated by commas.");

            bannedSceneDefNames = cfgBannedSceneNames.Value.Split(',');
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
        }



        public class LifeSavingsBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => Instance.ItemDef;
            CharacterMaster master;
            float stopwatch = Instance.intervalSeconds;

            private void OnEnable()
            {
                master = body.master;
                if (bannedSceneDefNames.Contains(SceneManager.GetActiveScene().name))
                    enabled = false;
            }

            private void FixedUpdate()
            {
                stopwatch -= Time.fixedDeltaTime;
                if (stopwatch <= 0)
                {
                    stopwatch = Instance.intervalSeconds;
                    var stackEffect = (Utils.ItemHelpers.StackingLinear(this.stack, Instance.gold, Instance.goldStack));
                    var multiplier = Run.instance ? Run.instance.GetDifficultyScaledCost(stackEffect, Run.instance.difficultyCoefficient) : 1;
                    master.GiveMoney((uint)(stackEffect * multiplier));
                }
            }
        }

    }
}