using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Items
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/Thallium.cs
    public class Thallium : ItemBase<Thallium>
    {
        public float chance = 10f;
        public float chancePerStack = 5f;
        public float enemyAttackDamageCoef = 5f;
        public float enemyMoveSpeedCoef = 1f;
        public float duration = 3f;

        public float dotInterval = 0.5f;


        public override string ItemName => "Thallium";

        public override string ItemLangTokenName => "THALLIUM";

        public override object[] ItemFullDescriptionParams => new object[]
        {
            chance,
            chancePerStack,
            (enemyAttackDamageCoef * 100f),
            (enemyMoveSpeedCoef * 100),
            duration,
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
            //Events.PostOnHitEnemy += Events_PostOnHitEnemy;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victimGameObject)
        {
            orig(self, damageInfo, victimGameObject);
            if (victimGameObject && victimGameObject.TryGetComponent(out CharacterBody victimBody) && damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody))
            {
                if (TryGetCount(attackerBody, out int itemCount) && Util.CheckRoll(Utils.ItemHelpers.StackingLinear(itemCount, chance, chancePerStack)))
                {
                    DotController.InflictDot(victimGameObject, damageInfo.attacker, Dots.ThalliumDotIndex, duration, victimBody.damage * enemyAttackDamageCoef * dotInterval);
                }
            }
        }

        private void Events_PostOnHitEnemy(DamageInfo damageInfo, GameObject victimGameObject)
        {
            if (!damageInfo.attacker || !victimGameObject || victimGameObject.TryGetComponent(out CharacterBody victimBody) || !damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) || !TryGetCount(attackerBody, out var count) || !Util.CheckRoll(Utils.ItemHelpers.StackingLinear(count, chance, chancePerStack)))
                return;
            DotController.InflictDot(victimGameObject, damageInfo.attacker, Modules.Dots.ThalliumDotIndex, duration);
        }
    }
}