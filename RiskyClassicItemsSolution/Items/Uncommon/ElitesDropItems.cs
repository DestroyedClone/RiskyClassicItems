using BepInEx.Configuration;
using ClassicItemsReturns.Modules;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items.Uncommon
{
    public class ElitesDropItems : ItemBase<ElitesDropItems>
    {
        public override string ItemName => "56 Leaf Clover";

        public override string ItemLangTokenName => "56LEAFCLOVER";

        public override object[] ItemFullDescriptionParams => new object[]
        {
            cloverPercentageDropChance,
            cloverPercentageDropChancePerStack
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Clover");

        public override Sprite ItemIcon => LoadItemSprite("Clover");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.AIBlacklist,
            ItemTag.BrotherBlacklist,
            ItemTag.OnKillEffect,
            ItemTag.Utility,
            ItemTag.CannotCopy,
            ItemTag.CanBeTemporary,
            ItemTag.FoodRelated
        };

        public const float cloverPercentageDropChance = 4.5f;
        public const float cloverPercentageDropChancePerStack = 1.5f;
        public static ConfigEntry<bool> useClassic;
        public static PickupDropTable SacrificePickupDropTable => RoR2.Artifacts.SacrificeArtifactManager.dropTable;
        private static Xoroshiro128Plus treasureRng;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
            ClassicItemsReturnsPlugin.onFinishScanning += CreateCraftableDef;
            treasureRng = new Xoroshiro128Plus(0UL);
            Artifact.ArtifactOfClover.cloverDef = ItemDef;
        }

        public override void CreateConfig(ConfigFile config)
        {
            useClassic = config.Bind(ConfigCategory, "Use Classic Chances", true, "Roll Clover for all players until an item drops, like in RoR1. Disabling this will make it only roll for the killer.");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private void Run_onRunStartGlobal(Run run)
        {
            if (NetworkServer.active && run.treasureRng != null) treasureRng.ResetSeed(run.treasureRng.nextUlong);
        }

        protected override void CreateCraftableDef()
        {
            CraftableDef cdCloverToClover = ScriptableObject.CreateInstance<CraftableDef>();
            Recipe fromNectar = new Recipe()
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
                        pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC2/Items/BoostAllStats/BoostAllStats.asset").WaitForCompletion()
                    }
                }
            };
            Recipe fromPlant = new Recipe()
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
                        pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Plant/Plant.asset").WaitForCompletion()
                    }
                }
            };
            cdCloverToClover.pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Clover/Clover.asset").WaitForCompletion();
            cdCloverToClover.recipes = new Recipe[]
            {
                fromNectar,
                fromPlant
            };
            (cdCloverToClover as ScriptableObject).name = "cdCloverToClover";
            PluginContentPack.craftableDefs.Add(cdCloverToClover);

            CraftableDef bigCloverToSmallClover = ScriptableObject.CreateInstance<CraftableDef>();
            Recipe downgrade = new Recipe()
            {
                amountToDrop = 2,
                ingredients = new RecipeIngredient[]
                {
                    new RecipeIngredient()
                    {
                        pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Clover/Clover.asset").WaitForCompletion()
                    },
                    new RecipeIngredient()
                    {
                        pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Scrap/ScrapGreen.asset").WaitForCompletion()
                    }
                }
            };
            bigCloverToSmallClover.pickup = ItemDef;
            bigCloverToSmallClover.recipes = new Recipe[]
            {
                downgrade
            };
            (bigCloverToSmallClover as ScriptableObject).name = "bigCloverToSmallClover";
            PluginContentPack.craftableDefs.Add(bigCloverToSmallClover);
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            //Server handles item drops
            if (!NetworkServer.active)
                return;

            var checks = damageReport.victimBody && damageReport.victimBody.isElite
                && damageReport.attacker
                && damageReport.attackerTeamIndex != damageReport.victimTeamIndex
                && !damageReport.victimBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless);
            if (!checks)
                return;

            if (useClassic.Value)
                CloverActivation_UseClassic(damageReport);
            else
                CloverActivation_KillerOnly(damageReport);
        }

        private void CloverActivation_UseClassic(DamageReport damageReport)
        {
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (RollForMaster(player.master))
                {
                    CloverActivation_CreatePickupDroplet(damageReport);
                    return;
                }
            }
        }

        private void CloverActivation_KillerOnly(DamageReport damageReport)
        {
            if (damageReport.attackerMaster && RollForMaster(damageReport.attackerMaster))
                CloverActivation_CreatePickupDroplet(damageReport);
        }

        private bool RollForMaster(CharacterMaster master)
        {
            if (master)
            {
                var itemCount = GetCount(master);
                if (itemCount > 0)
                {
                    CharacterBody body = master.GetBody();
                    if (body && body.healthComponent && body.healthComponent.alive)
                    {
                        var rollChance = Utils.ItemHelpers.StackingLinear(itemCount, cloverPercentageDropChance, cloverPercentageDropChancePerStack);
                        return Util.CheckRoll(rollChance, 0);
                    }
                }
            }
            return false;
        }

        private void CloverActivation_CreatePickupDroplet(DamageReport damageReport)
        {
            var pickup = SacrificePickupDropTable.GeneratePickupPreReplacement(treasureRng);
            if (pickup != null)
            {
                PickupDropletController.CreatePickupDroplet(pickup, damageReport.victimBody.corePosition, Vector3.up * 20f, false, false);
            }
        }
    }
}