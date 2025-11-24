using R2API;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace ClassicItemsReturns.Modules
{
    internal class Orbs
    {
        public static void Initialize()
        {
            OrbAPI.AddOrb(typeof(CIR_LostDollOrb));

            CIR_LostDollOrb.arrivalSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            CIR_LostDollOrb.arrivalSound.eventName = "Play_item_proc_deathMark";
            PluginContentPack.networkSoundEventDefs.Add(CIR_LostDollOrb.arrivalSound);
        }

        public class CIR_LostDollOrb : DevilOrb
        {
            public CharacterBody attackerCharacterBody;
            public static GameObject effectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/HauntOrbEffect");
            public static NetworkSoundEventDef arrivalSound;


            public override void Begin()
            {
                base.duration = base.distanceToTarget / 60f;
                attackerCharacterBody.healthComponent.TakeDamage(new DamageInfo()
                {
                    attacker = null,
                    crit = false,
                    damage = attackerCharacterBody.healthComponent.combinedHealth * Equipment.LostDoll.selfHurtCoefficient,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.NonLethal,
                    inflictor = null,
                    position = attackerCharacterBody.corePosition,
                    procCoefficient = 0,
                    procChainMask = default,
                    force = Vector3.zero
                });

                EffectData effectData = new EffectData
                {
                    scale = this.scale,
                    origin = this.origin,
                    genericFloat = duration
                };
                effectData.SetHurtBoxReference(this.target);
                EffectManager.SpawnEffect(effectPrefab, effectData, true);
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
                        damageInfo.inflictor = this.attacker;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = this.isCrit;
                        damageInfo.procChainMask = this.procChainMask;
                        damageInfo.procCoefficient = this.procCoefficient;
                        damageInfo.position = this.target.transform.position;
                        damageInfo.damageColorIndex = this.damageColorIndex;
                        damageInfo.damageType = (DamageTypeCombo)DamageSource.Equipment;
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);

                        if (arrivalSound) EffectManager.SimpleSoundEffect(arrivalSound.index, this.target.transform.position, true);
                    }
                }
            }
        }
    }
}