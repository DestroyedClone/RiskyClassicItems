using R2API;
using RoR2;
using RiskyClassicItems.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RoR2.Items.BaseItemBodyBehavior;
using RoR2.Items;
using UnityEngine.Diagnostics;
using RoR2.CharacterAI;
using BepInEx.Configuration;
using static RiskyClassicItems.Items.ArmsRace;

namespace RiskyClassicItems.Items
{
    internal class ArmsRaceDroneItem : ItemBase<ArmsRaceDroneItem>
    {
        public override string ItemName => "ARMSRACEDRONEITEM";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => Assets.NullModel;

        public override Sprite ItemIcon => Assets.NullSprite;

        public override string ItemLangTokenName => "ARMSRACEDRONEITEM";

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }


        public class ArmsRaceDroneBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => ArmsRaceDroneItem.Instance.ItemDef;
            public float cooldown => ArmsRace.Instance.cooldown;
            public float failedTargetCooldown = 2f;
            public float damage => ArmsRace.Instance.damageCoeff;
            public float timer = 2f;
            public BaseAI baseAI = null;
            public int missileCount = ArmsRace.Instance.missileCount;
            public ArmsRace.ArmsRaceSyncComponent ArmsRaceSyncComponent;
            private void OnEnable()
            {
                if (!(body && body.master && body.master.GetComponent<ArmsRaceSyncComponent>() is ArmsRaceSyncComponent component))
                {
                    enabled = false;
                    return;
                }
                ArmsRaceSyncComponent = component;

                baseAI = body.master.aiComponents[0];
                if (!baseAI)
                {
                    enabled = false;
                    return;
                }

                timer = cooldown;
            }


            private void OnDestroy()
            {
            }

            private void FixedUpdate()
            {
                timer -= Time.fixedDeltaTime;
                if (timer < 0)
                {
                    if (baseAI.currentEnemy != null && baseAI.currentEnemy.gameObject)
                    {
                        timer = 0;
                        FireMissile();
                        return;
                    }
                    timer = failedTargetCooldown;
                }
            }

            private void FireMissile()
            {
                var missileCount = ArmsRaceSyncComponent.droneMissileCount; 
                for (int i = 0; i < missileCount; i++)
                {
                    MissileUtils.FireMissile(body.corePosition, body, default, baseAI.currentEnemy.gameObject, damage, body.RollCrit(), GlobalEventManager.CommonAssets.missilePrefab, DamageColorIndex.Item, true);
                }
            }
        }
    }
}
