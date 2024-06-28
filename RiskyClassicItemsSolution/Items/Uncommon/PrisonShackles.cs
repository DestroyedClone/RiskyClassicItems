using ClassicItemsReturns.Modules;
using R2API;
using RoR2;
using UnityEngine;

namespace ClassicItemsReturns.Items.Uncommon
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/PrisonShackles.cs
    public class PrisonShackles : ItemBase<PrisonShackles>
    {
        public override string ItemName => "Prison Shackles";

        public override string ItemLangTokenName => "PRISONSHACKLES";

        public static float attackSpeedSlowMultiplier = 0.4f;

        public static int duration = 2;

        public static int durationStack = 2;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            attackSpeedSlowMultiplier*100,
            duration,
            durationStack,
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Shackles");

        public override Sprite ItemIcon => LoadItemSprite("Shackles");

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
            SharedHooks.OnHitEnemy.OnHitEnemyAttackerInventoryAndVictimBodyActions += ApplyShackles;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(Buffs.ShacklesBuff))
            {
                args.attackSpeedReductionMultAdd += attackSpeedSlowMultiplier;
            }
        }

        private void ApplyShackles(GlobalEventManager globalEventManager, DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody, Inventory attackerInventory)
        {
            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            int itemStack = itemCount - 1;
            float totalDuration = (duration + itemStack * durationStack) * damageInfo.procCoefficient;
            if (totalDuration <= 0f) return;

            victimBody.AddTimedBuff(Buffs.ShacklesBuff, totalDuration);
        }
    }
}