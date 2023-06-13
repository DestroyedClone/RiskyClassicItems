using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Items
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/Thallium.cs
    public class Thallium : ItemBase<Thallium>
    {
        public float chance = 5f;
        public float enemyAttackDamageCoef = 5f;
        public float enemyMoveSpeedCoef = 1f;
        public float duration = 3f;
        public float durationPerStack = 1.5f;

        public float dotInterval = 0.5f;


        public override string ItemName => "Thallium";

        public override string ItemLangTokenName => "THALLIUM";

        public override object[] ItemFullDescriptionParams => new object[]
        {
            chance,
            (enemyAttackDamageCoef * 100f),
            (enemyMoveSpeedCoef * 100),
            duration,
            durationPerStack
        };

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadPickupModel("Thallium");

        public override Sprite ItemIcon => LoadItemIcon("Thallium");
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage
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
            Events.PostOnHitEnemy += Events_PostOnHitEnemy;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(Buffs.ThalliumBuff))
            {
                args.moveSpeedReductionMultAdd += enemyMoveSpeedCoef;
            }
        }

        private void Events_PostOnHitEnemy(DamageInfo damageInfo, GameObject victimGameObject)
        {
            if (!victimGameObject || !victimGameObject.TryGetComponent(out CharacterBody victimBody) || !damageInfo.attacker || !damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody))
            {
                return;
            }
            if (!TryGetCount(attackerBody, out int itemCount) || !Util.CheckRoll(chance))
            {
                return;
            }

            float victimDamage = victimBody.damage;
            float attackerDamage = attackerBody.damage;

            float damage;
            if (victimDamage > attackerDamage)
            {
                damage = victimDamage / attackerDamage;
            } else
            {
                damage = 1;
            }
                damage /= attackerDamage;
            damage *= enemyAttackDamageCoef;

            var newDuration = Utils.ItemHelpers.StackingLinear(itemCount, duration, durationPerStack);

            DotController.InflictDot(victimGameObject, damageInfo.attacker, Dots.ThalliumDotIndex, newDuration, damage * dotInterval);
        }
    }
}