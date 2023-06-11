using R2API;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RoR2.MasterSpawnSlotController;

namespace RiskyClassicItems.Modules
{
    internal class Orbs
    {
        public static void Initialize()
        {
            OrbAPI.AddOrb(typeof(LostDollOrb));
        }

        public class LostDollOrb : DevilOrb
        {
            public CharacterBody attackerCharacterBody;

            public override void Begin()
            {
                base.duration = base.distanceToTarget / 30f;
                attackerCharacterBody.healthComponent.TakeDamage(new DamageInfo()
                {
                    attacker = attacker,
                    crit = false,
                    damage = attackerCharacterBody.healthComponent.combinedHealth * Equipment.LostDoll.selfHurtCoefficient,
                    damageColorIndex = DamageColorIndex.Item,
                    damageType = DamageType.NonLethal,
                    inflictor = attacker,
                    position = attackerCharacterBody.corePosition,
                    procCoefficient = 0,
                    procChainMask = default
                });

                EffectData effectData = new EffectData
                {
                    scale = this.scale,
                    origin = this.origin,
                    genericFloat = duration
                };
                effectData.SetHurtBoxReference(this.target);
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/HauntOrbEffect"), effectData, true);
            }

            public override void OnArrival()
            {
                if (this.target)
                {
                    HealthComponent healthComponent = this.target.healthComponent;
                    if (healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = this.damageValue;
                        damageInfo.attacker = this.attacker;
                        damageInfo.inflictor = null;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = this.isCrit;
                        damageInfo.procChainMask = this.procChainMask;
                        damageInfo.procCoefficient = this.procCoefficient;
                        damageInfo.position = this.target.transform.position;
                        damageInfo.damageColorIndex = this.damageColorIndex;
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);

                        Util.PlaySound("Play_item_proc_deathMark", target.gameObject);
                    }
                }
            }
        }
    }
}
