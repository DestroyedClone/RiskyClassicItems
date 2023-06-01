using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using static RiskyClassicItems.Items.ArmsRace;
using static RoR2.Items.BaseItemBodyBehavior;

namespace RiskyClassicItems.Items
{
    public class LifeSavings : ItemBase<LifeSavings>
    {
        int gold;
        int goldStack;
        float intervalSeconds;
        public override string ItemName => "LifeSavings";

        public override string ItemLangTokenName => ItemName.ToUpper();

        public override string[] ItemFullDescriptionParams => new string[]
        {
            gold.ToString(),
            goldStack.ToString(),
            intervalSeconds.ToString()
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadModel();

        public override Sprite ItemIcon => LoadSprite();

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

        public class LifeSavingsBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => Instance.ItemDef;
            CharacterMaster master;
            float stopwatch = Instance.intervalSeconds;

            private void OnEnable()
            {
                master = body.master;
            }

            private void FixedUpdate()
            {
                stopwatch -= Time.fixedDeltaTime;
                if (stopwatch <= 0)
                {
                    stopwatch = Instance.intervalSeconds;
                    var multiplier = Run.instance ? Run.instance.stageClearCount + 1 : 1;
                    master.GiveMoney((uint)(Utils.ItemHelpers.StackingLinear(this.stack, Instance.gold, Instance.goldStack) * multiplier));
                }
            }
        }

    }
}