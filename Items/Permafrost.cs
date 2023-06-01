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
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport.attackerBody && damageReport.victimBody && TryGetCount(damageReport.attackerBody, out int itemCount))
            {
                if (Util.CheckRoll(ItemHelpers.StackingLinear(itemCount, procChance, procChanceStack), damageReport.attackerMaster))
                    damageReport.attackerBody.AddTimedBuff(Modules.Buffs.PermafrostChilledBuff, procDuration);
                if (damageReport.attackerBody.HasBuff(Modules.Buffs.PermafrostChilledBuff)
                    && Util.CheckRoll(stunChance, damageReport.attackerMaster)
                    && damageReport.attackerBody.TryGetComponent(out SetStateOnHurt setStateOnHurt))
                {
                    setStateOnHurt.SetStun(stunDuration);
                }
                    

            }
        }

        public class PermafrostBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => Instance.ItemDef;

            private void OnEnable()
            {
                
            }


            private void OnDisable()
            {
            }
        }

    }
}