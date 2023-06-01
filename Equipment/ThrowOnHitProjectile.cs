using BepInEx.Configuration;
using R2API;
using RoR2;
using RiskyClassicItems.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace RiskyClassicItems.Equipment
{
    internal class ThrowOnHitProjectile : EquipmentBase<ThrowOnHitProjectile>
    {
        public override string EquipmentName => "ThrowOnHitProjectile";

        public override string EquipmentLangTokenName => "THROWONHITPROJECTILE";

        const int squidCount = 30;
        const float damageCoefficient = 2f;
        const float procCoefficient = 1f;
        public override string[] EquipmentFullDescriptionParams => new string[]
        {
            squidCount.ToString(),
            (damageCoefficient*100f).ToString()
        };

        public override GameObject EquipmentModel => LoadModel();

        public override Sprite EquipmentIcon => LoadSprite();

        public static GameObject scavProjectile = null;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            Hooks();
            CreateAssets(config);
        }

        public void CreateAssets(ConfigFile config)
        {
            scavProjectile = Assets.LoadAddressable<GameObject>("RoR2/Base/Scav/ScavSackProjectile.prefab").InstantiateClone("CIR_ScavProjectileCopy");
            var onHitComponent = scavProjectile.AddComponent<RCI_ProjectileGrantOnHitOnDestroy>();
            var onKillComponent = scavProjectile.GetComponent<ProjectileGrantOnKillOnDestroy>();
            onHitComponent.projectileController = scavProjectile.GetComponent<ProjectileController>();
            onHitComponent.projectileDamage = scavProjectile.GetComponent<ProjectileDamage>();
            onHitComponent.healthComponent = scavProjectile.GetComponent<HealthComponent>();
            UnityEngine.Object.Destroy(onKillComponent);
        }

        //based off EquipmentSlot.FireMolotov();
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            Ray aimRay = slot.GetAimRay();
            GameObject prefab = scavProjectile;
            ProjectileManager.instance.FireProjectile(prefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), slot.gameObject, slot.characterBody.damage, 0f, Util.CheckRoll(slot.characterBody.crit, slot.characterBody.master), DamageColorIndex.Default, null, -1f);
            return true;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            throw new NotImplementedException();
        }

        [RequireComponent(typeof(ProjectileDamage))]
        [RequireComponent(typeof(ProjectileController))]
        [RequireComponent(typeof(HealthComponent))]
        public class RCI_ProjectileGrantOnHitOnDestroy : MonoBehaviour
        {
            //based on ProjectileGrantOnKillOnDestroy
            private void OnDestroy()
            {
                healthComponent = GetComponent<HealthComponent>();
                projectileController = GetComponent<ProjectileController>();
                projectileDamage = GetComponent<ProjectileDamage>();
                if (NetworkServer.active && projectileController.owner)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = projectileController.owner,
                        crit = projectileDamage.crit,
                        damage = projectileDamage.damage,
                        position = transform.position,
                        procCoefficient = procCoefficient,
                        damageType = projectileDamage.damageType,
                        damageColorIndex = projectileDamage.damageColorIndex
                    };
                    HealthComponent victim = healthComponent;
                    DamageReport damageReport = new DamageReport(damageInfo, victim, damageInfo.damage, healthComponent.combinedHealth);
                    GlobalEventManager.OnHitAll(damageInfo, );
                }
            }

            public ProjectileController projectileController;

            public ProjectileDamage projectileDamage;

            public HealthComponent healthComponent;
        }
    }
}