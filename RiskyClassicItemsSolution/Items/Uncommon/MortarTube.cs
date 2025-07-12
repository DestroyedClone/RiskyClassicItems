using BepInEx.Configuration;
using ClassicItemsReturns.Items.NoTier;
using ClassicItemsReturns.Modules;
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
        public float damageCoeff = 3f;
        public float stackDamageCoeff = 3f;
        public float procCoeff = 0f;
        public float procChance = 10f;
        public float blastRadius = 10f;

        public GameObject mortarProjectilePrefab;
        public GameObject mortarProjectileGhostPrefab;
        public GameObject mortarImpactEffectPrefab;

        public override string ItemName => "Mortar Tube";
        public override string ItemLangTokenName => "MORTARTUBE";
        public override object[] ItemFullDescriptionParams => new object[]
        {
            procChance,
            damageCoeff * 100f,
            stackDamageCoeff * 100f
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Mortar");

        public override Sprite ItemIcon => LoadItemSprite("Mortar");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage
        };

        protected override void CreateLang()
        {
            if (ModSupport.ModCompatRiskyMod.IsAtgEnabled())
            {
                stackDamageCoeff = 2.1f;
                ItemFullDescriptionParams[2] = stackDamageCoeff * 100f;
            }
            base.CreateLang();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void CreateAssets(ConfigFile config)
        {
            mortarProjectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab").WaitForCompletion().InstantiateClone("CIR_MortarProjectile", true);
            mortarProjectilePrefab.AddComponent<MortarRotation>();

            ProjectileController projectileController = mortarProjectilePrefab.GetComponent<ProjectileController>();
            ProjectileDamage projectileDamage = mortarProjectilePrefab.GetComponent<ProjectileDamage>();
            ProjectileImpactExplosion projectileImpact = mortarProjectilePrefab.GetComponent<ProjectileImpactExplosion>();

            projectileImpact.blastRadius = blastRadius;

            mortarImpactEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFX.prefab").WaitForCompletion().InstantiateClone("CIR_MortarImpactVFX", false);
            if (!mortarImpactEffectPrefab.GetComponent<NetworkIdentity>()) mortarImpactEffectPrefab.AddComponent<NetworkIdentity>();
            mortarImpactEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_ClassicItemsReturns_MortarImpact"; // insert sound here

            mortarProjectileGhostPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/MicroMissileGhost.prefab").WaitForCompletion().InstantiateClone("CIR_MortarProjectileGhost", false);

            mortarProjectileGhostPrefab.transform.Find("missile VFX").localPosition = new Vector3(0f, 0f, -0.45f);
            mortarProjectileGhostPrefab.transform.Find("missile VFX").localScale = new Vector3(0.5f, 0.3f, 0.5f);

            projectileController.ghostPrefab = mortarProjectileGhostPrefab;

            projectileImpact.lifetimeExpiredSound = Modules.Assets.CreateNetworkSoundEventDef("Play_ClassicItemsReturns_Mortar"); // firing sound here
            projectileImpact.offsetForLifetimeExpiredSound = 0.1f;
            projectileImpact.destroyOnEnemy = false;
            projectileImpact.destroyOnWorld = false;
            projectileImpact.timerAfterImpact = true;
            projectileImpact.falloffModel = BlastAttack.FalloffModel.None;
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

        private static float minDistance = 5f;
        private static float maxDistance = 60f;
        private static float timeToTarget = 1f; //Mushrum is 1.5
        private void MortarTubeOnHit(GlobalEventManager globalEventManager, DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody, Inventory attackerInventory)
        {
            if (damageInfo.damage <= 0f || !attackerBody) return;

            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            float chance = procChance;
            if (!Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master)) return;

            //Manually build aimray since server and client aimray can be desynced.
            Ray aimRay = new Ray()
            {
                origin = attackerBody.aimOrigin,
                direction = (damageInfo.position - attackerBody.aimOrigin).normalized
            };
            Ray ray = new Ray(aimRay.origin, Vector3.up);
            Vector3 targetPoint = damageInfo.position;

            //This causes range to randomly whiff at times
            /*RaycastHit raycastHit;
            if (Physics.Raycast(aimRay, out raycastHit, 1000f, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
            {
                targetPoint = raycastHit.point;
            }*/

            float magnitude = 40f;

            Vector3 vector = targetPoint - ray.origin;
            Vector2 a2 = new Vector2(vector.x, vector.z);
            float magnitude2 = a2.magnitude;
            Vector2 vector2 = a2 / magnitude2;

            if (magnitude2 < minDistance)
            {
                magnitude2 = minDistance;
            }
            if (magnitude2 > maxDistance)
            {
                magnitude2 = maxDistance;
            }
            float y = Trajectory.CalculateInitialYSpeed(timeToTarget, vector.y);
            float num = magnitude2 / timeToTarget;
            Vector3 direction = new Vector3(vector2.x * num, y, vector2.y * num);
            magnitude = direction.magnitude;
            ray.direction = direction;

            Quaternion rotation = Util.QuaternionSafeLookRotation(ray.direction + UnityEngine.Random.insideUnitSphere * 0.05f);

            float coeff = damageCoeff + (itemCount - 1) * stackDamageCoeff;

            ProjectileManager.instance.FireProjectileServer(new FireProjectileInfo
            {
                crit = damageInfo.crit,
                damage = damageInfo.damage * coeff,
                damageColorIndex = DamageColorIndex.Default,
                force = 800f,
                owner = attackerBody.gameObject,
                position = ray.origin,
                procChainMask = damageInfo.procChainMask,
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
            if (NetworkServer.active && this.rb) this.transform.rotation = Util.QuaternionSafeLookRotation(this.rb.velocity.normalized);
        }
    }
}