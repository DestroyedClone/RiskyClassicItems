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

        float procChancePercentage = 35f;
        float procChanceStackPercentage = 5f;
        float procDuration = 2f;
        float movementSpeedCoef = -0.3f;
        float stunChancePercentage = 50f;
        float stunDuration = 1.5f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            procChancePercentage,
            procChanceStackPercentage,
            (movementSpeedCoef * 100),
            procDuration,
            stunChancePercentage,
            stunDuration,
        };


        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadPickupModel("Permafrost");

        public override Sprite ItemIcon => LoadItemIcon("Permafrost");
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.Utility
        };

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
                if (Util.CheckRoll(ItemHelpers.StackingLinear(itemCount, procChancePercentage, procChanceStackPercentage) * 100, damageReport.attackerMaster))
                    damageReport.victimBody.AddTimedBuff(Modules.Buffs.PermafrostChilledBuff, procDuration);

                if (damageReport.attackerBody.HasBuff(Modules.Buffs.PermafrostChilledBuff)
                    && Util.CheckRoll(stunChancePercentage, damageReport.attackerMaster)
                    && damageReport.victimBody.TryGetComponent(out SetStateOnHurt setStateOnHurt))
                {
                    setStateOnHurt.SetFrozen(stunDuration);
                }
            }
        }
    }
}