using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items
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
            ItemTag.Utility
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
            treasureRng = new Xoroshiro128Plus(0UL);
            Artifact.ArtifactOfClover.cloverDef = this.ItemDef;
        }

        public override void CreateConfig(ConfigFile config)
        {
            useClassic = config.Bind(ConfigCategory, "Use Classic Chances", false, "true: Rolls through each player until true." +
                "\nfalse: Only rolls for the attacker that killed.");
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
            treasureRng.ResetSeed(run.treasureRng.nextUlong);
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            //Server handles item drops
            if (!NetworkServer.active)
                return;

            var checks = damageReport.victimBody && damageReport.victimBody.isElite;
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
                    var rollChance = Utils.ItemHelpers.StackingLinear(itemCount, cloverPercentageDropChance, cloverPercentageDropChancePerStack);
                    return Util.CheckRoll(rollChance);
                }
            }
            return false;
        }

        private void CloverActivation_CreatePickupDroplet(DamageReport damageReport)
        {
            PickupIndex pickupIndex = SacrificePickupDropTable.GenerateDrop(treasureRng);
            if (pickupIndex != PickupIndex.none)
            {
                PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
            }
        }
    }
}