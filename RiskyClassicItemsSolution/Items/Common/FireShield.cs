﻿using ClassicItemsReturns.Utils;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ClassicItemsReturns.Items.Common
{
    public class FireShield : ItemBase<FireShield>
    {
        public override string ItemName => "Fire Shield";

        public override string ItemLangTokenName => "FIRESHIELD";

        public override ItemTier Tier => ItemTier.Tier1;
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.AIBlacklist
        };

        public override GameObject ItemModel => LoadItemModel("FireShield");

        public override Sprite ItemIcon => LoadItemSprite("FireShield");
        public override object[] ItemFullDescriptionParams => new object[]
        {
            blastRadius,
            explosionDamageCoefficient * 100f,
            igniteDamageCoefficient * 100f
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public static float explosionDamageCoefficient = 2f;
        public static float igniteDamageCoefficient = 2f;
        public static float blastRadius = 20f;

        public override void Hooks()
        {
            base.Hooks();
            SharedHooks.TakeDamage.OnDamageTakenInventoryActions += ProcFireShield;
        }

        private void ProcFireShield(DamageInfo damageInfo, HealthComponent self, CharacterBody victimBody, Inventory inventory, bool lostShield, bool lostOutOfDanger)
        {
            int itemCount = inventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            TeamIndex victimTeam = victimBody.teamComponent ? victimBody.teamComponent.teamIndex : TeamIndex.None;
            if (victimTeam == TeamIndex.None) return;

            float percentDamageTaken = damageInfo.damage / self.fullCombinedHealth;
            bool shouldActivate = percentDamageTaken >= 0.049f || lostShield || lostOutOfDanger; //A tiny bit less than 5% just to be safe.
            if (!shouldActivate) return;    

            EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
            {
                origin = victimBody.corePosition,
                scale = blastRadius
            }, true);

            float igniteDamage = victimBody.damage * igniteDamageCoefficient * itemCount;
            List<HealthComponent> igniteList = MiscUtils.FindEnemiesInSphere(blastRadius, victimBody.corePosition, victimTeam);
            foreach (HealthComponent hc in igniteList)
            {
                InflictDotInfo inflictDotInfo = new InflictDotInfo
                {
                    victimObject = hc.gameObject,
                    attackerObject = victimBody.gameObject,
                    totalDamage = new float?(igniteDamage),
                    dotIndex = DotController.DotIndex.Burn,
                    damageMultiplier = 0.75f + 0.25f * itemCount
                };
                StrengthenBurnUtils.CheckDotForUpgrade(inventory, ref inflictDotInfo);
                DotController.InflictDot(ref inflictDotInfo);
            }

            new BlastAttack
            {
                radius = blastRadius,
                baseDamage = victimBody.damage * explosionDamageCoefficient,
                procCoefficient = 1f,
                crit = Util.CheckRoll(victimBody.crit, victimBody.master),
                damageColorIndex = DamageColorIndex.Item,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                falloffModel = BlastAttack.FalloffModel.None,
                attacker = victimBody.gameObject,
                teamIndex = victimTeam,
                position = victimBody.corePosition,
                baseForce = 2000f,
                damageType = DamageType.Generic
            }.Fire();
        }
    }
}
