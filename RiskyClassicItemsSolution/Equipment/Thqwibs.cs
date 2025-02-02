﻿using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;

namespace ClassicItemsReturns.Equipment
{
    public class Thqwibs : EquipmentBase<Thqwibs>
    {
        public override string EquipmentName => "Thqwibs";
        public override string EquipmentLangTokenName => "THQWIBS";

        public override GameObject EquipmentModel => LoadItemModel("Squib");

        public override Sprite EquipmentIcon => LoadItemSprite("Squib");

        public int projectileCount = 15;
        public float damageCoefficient = 3.6f;

        public GameObject projectilePrefab;

        public override bool EnigmaCompatible { get; } = true;
        public override bool CanBeRandomlyTriggered { get; } = true;

        public override float Cooldown => 45f;
        public override object[] EquipmentFullDescriptionParams => new object[]
        {
            projectileCount,
            damageCoefficient * 100f
        };

        public override bool Unfinished => false;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var dict = new ItemDisplayRuleDict();

            //Don't set up displays if 3d model isn't available
            GameObject display = EquipmentModel;
            if (!display.name.Contains("mdl3d")) return dict;

            dict.Add("EquipmentDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "GunBarrelBase",
                    localPos = new Vector3(0F, 0F, 2.02558F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });

            return dict;
        }

        public override void Init(ConfigFile config)
        {
            BuildProjectile();
            base.Init(config);
        }

        private void BuildProjectile()
        {
            projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scav/ScavSackProjectile.prefab").WaitForCompletion().InstantiateClone(Modules.Assets.prefabPrefix + "SquibProjectile", true);

            //Something in here is causing the whole game to freeze when loading.
            //UnityEngine.Object.Destroy(projectilePrefab.GetComponent<AssignTeamFilterToTeamComponent>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<ProjectileGrantOnKillOnDestroy>());
            /*UnityEngine.Object.Destroy(projectilePrefab.GetComponent<HealthComponent>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<CharacterBody>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<SkillLocator>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<TeamComponent>());*/

            ProjectileImpactExplosion pie = projectilePrefab.GetComponent<ProjectileImpactExplosion>();
            pie.destroyOnEnemy = true;
            pie.falloffModel = BlastAttack.FalloffModel.None;
            ContentAddition.AddProjectile(projectilePrefab);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody) return false;

            float projectileVelocity = 80f;

            Ray aimRay = slot.GetAimRay();
            for (int i = 0; i < projectileCount; i++)
            {
                Quaternion rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(aimRay.direction, 0f, 12f, 1f, 1f, 0f, 0f));//25
                ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, rotation, slot.characterBody.gameObject, slot.characterBody.damage * damageCoefficient, 0f, Util.CheckRoll(slot.characterBody.crit, slot.characterBody.master), DamageColorIndex.Default, null, projectileVelocity);
            }
            return true;
        }
    }
}
