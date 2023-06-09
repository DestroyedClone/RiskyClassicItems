using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2.Items.BaseItemBodyBehavior;
using UnityEngine.Networking;
using RoR2.Items;
using RiskyClassicItems.Modules;

namespace RiskyClassicItems.Items
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/GoldenGun.cs
    //moff personally oversaw this one so a 1:1 is fine
    public class GoldenGun : ItemBase<GoldenGun>
    {
        public override string ItemName => "Golden Gun";

        public override string ItemLangTokenName => "GOLDENGUN";

        public static uint goldNeeded = 40;
        public static uint goldNeededPerStack = 40;
        public static uint goldCap = 300;
        public static uint goldCapPerStack = 300;
        public override object[] ItemFullDescriptionParams => new object[]
        {
            (0.5f*100),
            100
        };

        public override object[] ItemPickupDescParams => new object[]
        {
            112345f
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadPickupModel("GoldenGun");

        public override Sprite ItemIcon => LoadItemIcon("GoldenGun");

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.AIBlacklist
        };

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender)
                args.damageMultAdd += 0.01f * sender.GetBuffCount(Buffs.GoldenGunBuff);
        }

        public class GoldenGunBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => Instance.ItemDef;

            public void OnEnable()
            {

            }

            public void OnDestroy()
            {
                SetBuffCount(0);
            }

            public void SetBuffCount(int count)
            {
                var userBuffCount = body.GetBuffCount(Buffs.GoldenGunBuff);
                var difference = userBuffCount - count;

                if (difference > 0)
                {
                    // If the current count is greater than the desired count,
                    // remove the excess buffs until the count matches the desired count.
                    for (int i = 0; i < difference; i++)
                    {
                        body.RemoveBuff(Buffs.GoldenGunBuff);
                    }
                }
                else if (difference < 0)
                {
                    // If the current count is less than the desired count,
                    // add buffs until the count matches the desired count.
                    for (int i = 0; i < -difference; i++)
                    {
                        body.AddBuff(Buffs.GoldenGunBuff);
                    }
                }
            }


            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    int singleStackCost = Stage.instance ? Run.instance.GetDifficultyScaledCost((int)goldCap, Stage.instance.entryDifficultyCoefficient) : Run.instance.GetDifficultyScaledCost((int)goldCap);

                    int maxCost = singleStackCost + ((int)(0.5f * goldCap) * stack - 1);
                    int maxBuffs = (int)goldNeeded + ((int)(0.5f * goldNeeded) * stack - 1);

                    float moneyPercent = (float)body.master.money / maxCost;
                    int targetBuffCount = Mathf.Min(maxBuffs, Mathf.FloorToInt(maxBuffs * moneyPercent));

                    int currentBuffCount = body.GetBuffCount(Buffs.GoldenGunBuff);
                    if (targetBuffCount != currentBuffCount)
                    {
                        SetBuffCount(targetBuffCount);
                    }
                }
            }
        }
    }
}