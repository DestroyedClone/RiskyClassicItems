using BepInEx.Configuration;
using ClassicItemsReturns.Items.NoTier;
using R2API;
using RoR2;
using RoR2.Items;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items.Uncommon
{
    internal class MortarTube : ItemBase<MortarTube>
    {
        public float damageCoeff = 6f;
        public float procCoeff = 0f;
        public float procChance = 10f;
        public float blastRadius = 8f;

        public GameObject mortarProjectilePrefab;
        public GameObject mortarImpactEffectPrefab;

        public override string ItemName => "Mortar Tube";
        public override string ItemLangTokenName => "MORTARTUBE";
        public override object[] ItemFullDescriptionParams => new object[]
        {
            procChance,
            damageCoeff * 100f
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("ArmsRace");

        public override Sprite ItemIcon => LoadItemSprite("ArmsRace");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override void CreateAssets(ConfigFile config)
        {
            mortarProjectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab").WaitForCompletion().InstantiateClone("CIR_MortarProjectile", true);
            mortarProjectilePrefab.AddComponent<MortarRotation>();

            ProjectileController projectileController = mortarProjectilePrefab.GetComponent<ProjectileController>();
            ProjectileDamage projectileDamage = mortarProjectilePrefab.GetComponent<ProjectileDamage>();
            ProjectileImpactExplosion projectileImpact = mortarProjectilePrefab.GetComponent<ProjectileImpactExplosion>();

            projectileImpact.blastRadius = blastRadius;

            mortarImpactEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFX.prefab").WaitForCompletion().InstantiateClone("CIR_MortarImpactVFX", true);
            if (!mortarImpactEffectPrefab.GetComponent<NetworkIdentity>()) mortarImpactEffectPrefab.AddComponent<NetworkIdentity>();
            mortarImpactEffectPrefab.GetComponent<EffectComponent>().soundName = ""; // insert sound here

            projectileController.ghostPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Firework/FireworkGhost.prefab").WaitForCompletion();

            projectileImpact.lifetimeExpiredSound = Modules.Assets.CreateNetworkSoundEventDef(""); // firing sound here
            projectileImpact.offsetForLifetimeExpiredSound = 0.1f;
            projectileImpact.destroyOnEnemy = false;
            projectileImpact.destroyOnWorld = false;
            projectileImpact.timerAfterImpact = true;
            projectileImpact.falloffModel = BlastAttack.FalloffModel.Linear;
            projectileImpact.lifetime = 30f;
            projectileImpact.lifetimeAfterImpact = 0f;
            projectileImpact.lifetimeRandomOffset = 0f;
            projectileImpact.blastDamageCoefficient = 1f;
            projectileImpact.blastProcCoefficient = procCoeff;
            projectileImpact.fireChildren = false;
            projectileImpact.childrenCount = 0;
            projectileImpact.childrenProjectilePrefab = null;
            projectileImpact.childrenDamageCoefficient = 0f;
            projectileImpact.impactEffect = mortarImpactEffectPrefab;

            projectileController.startSound = "";
            projectileController.procCoefficient = procCoeff;

            projectileDamage.crit = false;
            projectileDamage.damage = 0f;
            projectileDamage.damageColorIndex = DamageColorIndex.Default;
            projectileDamage.damageType = DamageType.Generic;
            projectileDamage.force = 0f;

            Object.Destroy(mortarProjectilePrefab.GetComponent<ApplyTorqueOnStart>());
            //Object.Destroy(mortarProjectilePrefab.GetComponent<ProjectileSimple>());

            ContentAddition.AddEffect(mortarImpactEffectPrefab);
            ContentAddition.AddProjectile(mortarProjectilePrefab);
        }

        public override void Hooks()
        {
            SharedHooks.OnHitEnemy.OnHitEnemyAttackerInventoryAndVictimBodyActions += MortarTubeOnHit;
        }

        private void MortarTubeOnHit(GlobalEventManager globalEventManager, DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody, Inventory attackerInventory)
        {
            if (damageInfo.damage <= 0f || !attackerBody) return;

            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            float chance = procChance;
            if (!Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master)) return;

            Ray aimRay = attackerBody.inputBank.GetAimRay();
            Ray ray = new Ray(aimRay.origin, Vector3.up);
            Vector3 targetPoint = damageInfo.position;
            RaycastHit raycastHit;

            if (Physics.Raycast(aimRay, out raycastHit, 1000f, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
            {
                targetPoint = raycastHit.point;
            }

            float magnitude = 40f;

            Vector3 vector = targetPoint - ray.origin;
            Vector2 a2 = new Vector2(vector.x, vector.z);
            float magnitude2 = a2.magnitude;
            Vector2 vector2 = a2 / magnitude2;
            if (magnitude2 < EntityStates.MiniMushroom.SporeGrenade.minimumDistance)
            {
                magnitude2 = EntityStates.MiniMushroom.SporeGrenade.minimumDistance;
            }
            if (magnitude2 > EntityStates.MiniMushroom.SporeGrenade.maximumDistance)
            {
                magnitude2 = EntityStates.MiniMushroom.SporeGrenade.maximumDistance;
            }
            float y = Trajectory.CalculateInitialYSpeed(EntityStates.MiniMushroom.SporeGrenade.timeToTarget, vector.y);
            float num = magnitude2 / EntityStates.MiniMushroom.SporeGrenade.timeToTarget;
            Vector3 direction = new Vector3(vector2.x * num, y, vector2.y * num);
            magnitude = direction.magnitude;
            ray.direction = direction;

            Quaternion rotation = Util.QuaternionSafeLookRotation(ray.direction + UnityEngine.Random.insideUnitSphere * 0.05f);

            ProjectileManager.instance.FireProjectileServer(new FireProjectileInfo
            {
                crit = Util.CheckRoll(attackerBody.crit),
                damage = (damageInfo.damage * damageCoeff) * itemCount,
                damageColorIndex = DamageColorIndex.Default,
                force = 800f,
                owner = attackerBody.gameObject,
                position = ray.origin,
                procChainMask = default(ProcChainMask),
                projectilePrefab = mortarProjectilePrefab,
                rotation = rotation,
                speedOverride = magnitude,
                _speedOverride = magnitude,
                useFuseOverride = false,
                useSpeedOverride = true
            });
        }
    }

    internal class MortarRotation : MonoBehaviour
    {
        private Rigidbody rb;

        private void Awake()
        {
            this.rb = this.GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            this.transform.rotation = Util.QuaternionSafeLookRotation(this.rb.velocity.normalized);
        }
    }
}