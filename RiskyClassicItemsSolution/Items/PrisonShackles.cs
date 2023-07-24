using R2API;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Items
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/PrisonShackles.cs
    public class PrisonShackles : ItemBase<PrisonShackles>
    {
        public override string ItemName => "PrisonShackles";

        public override string ItemLangTokenName => "PRISONSHACKLES";

        public static float attackSpeedSlowMultiplier = 0.3f;

        public static int duration = 2;

        public static int durationStack = 2;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            (attackSpeedSlowMultiplier*100),
            duration,
            durationStack,
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadPickupModel("PrisonShackles");

        public override Sprite ItemIcon => LoadItemIcon("PrisonShackles");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility,
            ItemTag.AIBlacklist
        };

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
            if (sender.HasBuff(Modules.Buffs.ShacklesBuff))
            {
                args.attackSpeedReductionMultAdd += attackSpeedSlowMultiplier;
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            if (!obj.attackerBody || !obj.victimBody || !TryGetCount(obj.attackerBody, out var count))
                return;
            obj.victimBody.AddTimedBuff(Modules.Buffs.ShacklesBuff, Utils.ItemHelpers.StackingLinear(count, duration, durationStack));
        }
    }
}