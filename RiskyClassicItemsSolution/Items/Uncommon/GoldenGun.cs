using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using ClassicItemsReturns.Items.Common;
using System.Linq;

namespace ClassicItemsReturns.Items.Uncommon
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/GoldenGun.cs
    //moff personally oversaw this one so a 1:1 is fine
    public class GoldenGun : ItemBase<GoldenGun>
    {
        public override string ItemName => "Golden Gun";

        public override string ItemLangTokenName => "GOLDENGUN";

        public static uint buffCount = 40;
        public static uint buffCountPerStack = 20;
        public static uint goldCap = 300;
        public static uint goldCapPerStack = 150;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            buffCount,
            buffCountPerStack,
            goldCap,
            goldCapPerStack
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("GoldGun");

        public override Sprite ItemIcon => LoadItemSprite("GoldGun");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.AIBlacklist,
            ItemTag.CanBeTemporary
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
        protected override void CreateCraftableDef()
        {
            bool penniesEnabled = RazorPenny.Instance != null && RazorPenny.Instance.ItemDef;
            bool savingsEnabled = LifeSavings.Instance != null && LifeSavings.Instance.ItemDef;

            if (!penniesEnabled && !savingsEnabled) return;

            CraftableDef craftable = ScriptableObject.CreateInstance<CraftableDef>();
            craftable.pickup = ItemDef;
            craftable.recipes = new Recipe[0];

            if (penniesEnabled)
            {
                Recipe penny = new Recipe()
                {
                    amountToDrop = 1,
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Missile/Missile.asset").WaitForCompletion()
                        },
                        new RecipeIngredient()
                        {
                            pickup = RazorPenny.Instance.ItemDef
                        }
                    }
                };
                craftable.recipes = craftable.recipes.Append(penny).ToArray();
            }
            if (savingsEnabled)
            {
                Recipe savings = new Recipe()
                {
                    amountToDrop = 1,
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Missile/Missile.asset").WaitForCompletion()
                        },
                        new RecipeIngredient()
                        {
                            pickup = RazorPenny.Instance.ItemDef
                        }
                    }
                };
                craftable.recipes = craftable.recipes.Append(savings).ToArray();
            }
            PluginContentPack.craftableDefs.Add(craftable);
        }



        public class GoldenGunBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => Instance.ItemDef;

            public void OnDestroy()
            {
                SetBuffCount(0);
            }

            public void SetBuffCount(int count)
            {
                if (!body) return;
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
                if (NetworkServer.active && Run.instance && body.master)
                {
                    int singleStackCost = Stage.instance ? Run.instance.GetDifficultyScaledCost((int)goldCap, Stage.instance.entryDifficultyCoefficient) : Run.instance.GetDifficultyScaledCost((int)goldCap);

                    int maxCost = (int)Utils.ItemHelpers.StackingLinear(stack, singleStackCost, goldCapPerStack);
                    int maxBuffs = (int)Utils.ItemHelpers.StackingLinear(stack, buffCount, buffCountPerStack);

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