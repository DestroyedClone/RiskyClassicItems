using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Utils;
using RoR2;
using RoR2.Items;
using UnityEngine;
using static RiskyClassicItems.Items.ArmsRace;
using static RoR2.Items.BaseItemBodyBehavior;

namespace RiskyClassicItems.Items
{
    public class Permafrost : ItemBase<Permafrost>
    {
        public override string ItemName => "Permafrost";

        public override string ItemLangTokenName => "PERMAFROST";

        float procChance = 0.35f;
        float procChanceStack = 0.05f;
        float procDuration = 2f;
        float movementSpeedCoef = -0.3f;
        float stunChance = 0.5f;
        float stunDuration = 1.5f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            (procChance * 100),
            (procChanceStack * 100),
            (movementSpeedCoef * 100),
            procDuration,
            (stunChance * 100),
            stunDuration,
        };


        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadPickupModel("Permafrost");

        public override Sprite ItemIcon => LoadItemIcon("Permafrost");

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
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (ItemHelpers.TryGetBuffCount(sender, Modules.Buffs.PermafrostChilledBuff, out int buffCount))
            {
                args.moveSpeedReductionMultAdd += movementSpeedCoef;
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport.attackerBody && damageReport.victimBody && TryGetCount(damageReport.attackerBody, out int itemCount))
            {
                if (Util.CheckRoll(ItemHelpers.StackingLinear(itemCount, procChance, procChanceStack), damageReport.attackerMaster))
                    damageReport.attackerBody.AddTimedBuff(Modules.Buffs.PermafrostChilledBuff, procDuration);

                if (damageReport.attackerBody.HasBuff(Modules.Buffs.PermafrostChilledBuff)
                    && Util.CheckRoll(stunChance, damageReport.attackerMaster)
                    && damageReport.victimBody.TryGetComponent(out SetStateOnHurt setStateOnHurt))
                {
                    setStateOnHurt.SetStun(stunDuration);
                }
            }
        }
    }
}