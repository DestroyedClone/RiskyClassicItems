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
        public override bool AIBlacklisted => true;

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadPickupModel("Thallium");

        public override Sprite ItemIcon => LoadItemIcon("Thallium");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage
        };

        public static GameObject thalliumTickEffect;

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override void CreateAssets(ConfigFile config)
        {
            var asset = Assets.LoadAddressable<GameObject>("RoR2/Base/DeathProjectile/DeathProjectileTickEffect.prefab");
            /*
            thalliumTickEffect = PrefabAPI.InstantiateClone(asset.transform.Find("DarkWisps01Ring_Ps").gameObject, "ThalliumTickEffect");
            thalliumTickEffect.AddComponent<DestroyOnTimer>().duration = 1;
            var nps = thalliumTickEffect.AddComponent<NormalizeParticleScale>();
            nps.normalizeWithSkinnedMeshRendererInstead = true;*/
            //[Error: R2API.Prefab] ThalliumProcEffect(UnityEngine.GameObject) don't have a NetworkIdentity Component but was marked for network registration.
            thalliumTickEffect = PrefabAPI.InstantiateClone(asset, "ThalliumProcEffect", false);
            thalliumTickEffect.transform.localScale = Vector3.one * 0.5f;
            //thalliumTickEffect.transform.Find("DarkWisps01Ring_Ps").GetComponent<ParticleSystem>().playbackSpeed = 4f;
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
            if (!victimGameObject || !victimGameObject.TryGetComponent(out CharacterBody _) || !damageInfo.attacker || !damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody))
            {
                return;
            }
            if (!TryGetCount(attackerBody, out int itemCount) || !Util.CheckRoll(chance, attackerBody.master))
            {
                return;
            }
            var duration = Utils.ItemHelpers.StackingLinear(itemCount, Thallium.Instance.duration, durationPerStack);
            var inflictDotInfo = new InflictDotInfo()
            {
                attackerObject = damageInfo.attacker,
                dotIndex = Dots.Thallium.CIR_ThalliumDotIndex,
                maxStacksFromAttacker = new uint?(1U),
                victimObject = victimGameObject,
                duration = duration,
            };

            //DotController.InflictDot(victimGameObject, damageInfo.attacker, Dots.Thallium.ThalliumDotIndex, duration);
            DotController.InflictDot(ref inflictDotInfo);
        }
    }
}