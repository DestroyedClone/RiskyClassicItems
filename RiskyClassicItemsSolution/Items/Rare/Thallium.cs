using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;

namespace ClassicItemsReturns.Items.Rare
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/Thallium.cs
    public class Thallium : ItemBase<Thallium>
    {
        public float chance = 10f;
        public float enemyAttackDamageCoef = 3f;
        public float enemyMoveSpeedCoef = 1f;
        public float duration = 3f;
        public float durationPerStack = 1.5f;

        public float dotInterval = 0.5f;

        public override string ItemName => "Thallium";

        public override string ItemLangTokenName => "THALLIUM";

        public override object[] ItemFullDescriptionParams => new object[]
        {
            chance,
            enemyAttackDamageCoef * 100f,
            enemyMoveSpeedCoef * 100,
            duration,
            durationPerStack
        };

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("Thallium");

        public override Sprite ItemIcon => LoadItemSprite("Thallium");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.AIBlacklist
        };

        public static GameObject thalliumTickEffect;

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override void CreateAssets(ConfigFile config)
        {
            var asset = Modules.Assets.LoadAddressable<GameObject>("RoR2/Base/DeathProjectile/DeathProjectileTickEffect.prefab");
            thalliumTickEffect = asset.InstantiateClone("ThalliumProcEffect", false);
            thalliumTickEffect.transform.localScale = Vector3.one * 0.5f;
            var main = thalliumTickEffect.transform.Find("DarkWisps01Ring_Ps").GetComponent<ParticleSystem>().main;
            main.simulationSpeed *= 4f;
            Object.Destroy(thalliumTickEffect.transform.Find("FlarePersitant_Ps").gameObject);
            Object.Destroy(thalliumTickEffect.transform.Find("WispsBurst_Ps").gameObject);
            ContentAddition.AddEffect(thalliumTickEffect);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            SharedHooks.OnHitEnemy.OnHitEnemyAttackerInventoryActions += ApplyThallium;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(Buffs.ThalliumBuff))
            {
                args.moveSpeedReductionMultAdd += enemyMoveSpeedCoef;
            }
        }

        private void ApplyThallium(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, CharacterBody attackerBody, Inventory attackerInventory)
        {
            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            if (!Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master)) return;

            int itemStack = itemCount - 1;
            float totalDuration = Utils.ItemHelpers.StackingLinear(itemCount, Instance.duration, durationPerStack);
            var inflictDotInfo = new InflictDotInfo()
            {
                attackerObject = damageInfo.attacker,
                dotIndex = Dots.Thallium.CIR_ThalliumDotIndex,
                maxStacksFromAttacker = new uint?(1U),
                victimObject = victim,
                duration = duration,
            };
            DotController.InflictDot(ref inflictDotInfo);
        }
    }
}