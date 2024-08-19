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
            Object.Destroy(mortarProjectilePrefab.GetComponent<ProjectileSimple>());

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

            ProjectileManager.instance.FireProjectileServer(new FireProjectileInfo
            {
                crit = Util.CheckRoll(attackerBody.crit),
                damage = (damageInfo.damage * damageCoeff) * itemCount,
                damageColorIndex = DamageColorIndex.Default,
                force = 800f,
                owner = attackerBody.gameObject,
                position = attackerBody.aimOrigin,
                procChainMask = default(ProcChainMask),
                projectilePrefab = mortarProjectilePrefab,
                rotation = Util.QuaternionSafeLookRotation(attackerBody.characterDirection.moveVector),
                speedOverride = 40f,
                _speedOverride = 40f,
                useFuseOverride = false,
                useSpeedOverride = true
            });
        }
    }

    internal class MortarRotation : MonoBehaviour
    {
        public float forwardForce = 8f;
        public float upwardForce = 24f;

        private Rigidbody rb;

        private void Awake()
        {
            this.rb = this.GetComponent<Rigidbody>();
            if (this.rb)
            {
                this.rb.velocity = this.transform.forward * this.forwardForce + new Vector3(0f, this.upwardForce, 0f);
            }
        }

        private void FixedUpdate()
        {
            this.transform.rotation = Util.QuaternionSafeLookRotation(this.rb.velocity.normalized);
        }
    }
}