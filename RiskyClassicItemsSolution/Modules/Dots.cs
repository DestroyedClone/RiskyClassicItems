using System;
using System.Collections.Generic;
using System.Text;
using R2API;
using RiskyClassicItems.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace RiskyClassicItems.Modules
{
    internal class Dots
    {
        internal static void Initialize()
        {
            Thallium.Initialize();
            On.RoR2.DotController.Awake += DotController_Awake;
        }

        private static void DotController_Awake(On.RoR2.DotController.orig_Awake orig, DotController self)
        {
            orig(self);
            if (!self.GetComponent<DotVisualTracker>())
                self.gameObject.AddComponent<DotVisualTracker>();
        }

        public class DotVisualTracker : MonoBehaviour
        {
            public BurnEffectController thalliumEffect;
        }

        public static class Thallium
        {
            internal static DotController.DotIndex ThalliumDotIndex;
            internal static DotController.DotDef ThalliumDotDef;
            internal static DotAPI.CustomDotBehaviour ThalliumDotBehavior;
            internal static DotAPI.CustomDotVisual ThalliumDotVisual;
            internal static BurnEffectController.EffectParams ThalliumEffectParams;

            public static void Initialize()
            {
                var thallium = Items.Thallium.Instance;

                ThalliumEffectParams = new BurnEffectController.EffectParams()
                {
                    //overlayMaterial = Assets.LoadAddressable<Material>("RoR2/DLC1/VoidCamp/matVoidCampSphereSubtle.mat"),
                    //fireEffectPrefab = Items.Thallium.thalliumTickEffect
                };

                ThalliumDotDef = new DotController.DotDef()
                {
                    associatedBuff = Buffs.ThalliumBuff,
                    damageCoefficient = 0.333f,
                    damageColorIndex = DamageColorIndex.Poison,
                    interval = thallium.dotInterval,
                    resetTimerOnAdd = false,
                };

                ThalliumDotBehavior = new DotAPI.CustomDotBehaviour((dotController, dotStack) =>
                {
                    float damageToDeal = 0;
                    float victimDamage = dotController.victimBody.damage;
                    float attackerDamage = 1f;
                    CharacterBody attackerBody = null;
                    if (dotStack.attackerObject && dotStack.attackerObject.TryGetComponent(out attackerBody))
                        attackerDamage = attackerBody.damage;

                    //Main.ModDebugLog($"DoTBehav: V.dmg: {victimDamage} vs A.dmg: {attackerDamage}");

                    damageToDeal = Mathf.Max(attackerDamage, victimDamage);
                    damageToDeal *= thallium.enemyAttackDamageCoef * thallium.dotInterval / thallium.duration;

                    //use replacement or original damage, whichever is higher
                    //it will always be higher, while the damageCoefficient is 0.333
                    //we override it anyways
                    //damageToDeal = Mathf.Max(damageToDeal, dotStack.damage);

                    var itemCount = 1;
                    if (attackerBody)
                        itemCount = thallium.GetCount(attackerBody);
                    var duration = Utils.ItemHelpers.StackingLinear(itemCount, thallium.duration, thallium.durationPerStack);
                    bool stackActive = false;

                    foreach (var dotStack1 in dotController.dotStackList)
                    {
                        if (dotStack1.dotIndex == ThalliumDotIndex)
                        {
                            var oldDamage = dotStack1.damage;
                            //Main.ModDebugLog($"Existing: Damage Comparison oldvsnew: {oldDamage} vs {damageToDeal}");
                            damageToDeal = Mathf.Max(oldDamage, damageToDeal);
                            //Main.ModDebugLog($"Existing: Timer Comparison oldvsnew: {dotStack1.timer} vs {duration}");

                            dotStack1.damage = damageToDeal;
                            dotStack1.timer = Mathf.Max(dotStack1.timer, duration);
                            stackActive = true;
                            break;
                        }
                    }
                    if (!stackActive)
                    {
                        //Main.ModDebugLog($"Current: Damage Comparison oldvsnew:  {dotStack.damage} -> {damageToDeal}");
                        dotStack.damage = damageToDeal;
                        //Main.ModDebugLog($"Current: Timer Comparison oldvsnew: {dotStack.timer} -> {duration}");
                        dotStack.timer = duration;
                    } else
                    {
                        //Workaround on R2API because
                        //The early return in DotController makes it so that it skips the following
                        //
                        //this.dotStackList.Add(dotStack);
                        //this.OnDotStackAddedServer(dotStack);
                        //but because this is contained it can't do that...

                        dotStack.timer = 0;
                        dotStack.damage = 0;
                    }
                });

                ThalliumDotVisual = new DotAPI.CustomDotVisual((target) =>
                {
                    var modelLocator = target.victimObject.GetComponent<ModelLocator>();
                    var dotVisualTracker = target.GetComponent<DotVisualTracker>();
                    if (!modelLocator || !dotVisualTracker)
                        return;
                    if (target.HasDotActive(ThalliumDotIndex))
                    {
                        if (!dotVisualTracker.thalliumEffect)
                        {
                            if (modelLocator.modelTransform)
                            {
                                dotVisualTracker.thalliumEffect = target.gameObject.AddComponent<BurnEffectController>();
                                //dotVisualTracker.thalliumEffect.effectType = ThalliumEffectParams;
                                dotVisualTracker.thalliumEffect.target = modelLocator.modelTransform.gameObject;
                            }
                        }
                    }
                    else if (dotVisualTracker.thalliumEffect)
                    {
                        UnityEngine.Object.Destroy(dotVisualTracker.thalliumEffect);
                        dotVisualTracker.thalliumEffect = null;
                    }
                });
                ThalliumDotIndex = DotAPI.RegisterDotDef(ThalliumDotDef, ThalliumDotBehavior);//, ThalliumDotVisual);
                                                                                              //ThalliumDotIndex = DotAPI.RegisterDotDef(0.05f, 0f, DamageColorIndex.Poison, Buffs.ThalliumBuff, ThalliumDotBehavior);
                                                                                              //ThalliumDotIndex = DotAPI.RegisterDotDef(thallium.dotInterval, 1f, DamageColorIndex.Poison, Buffs.ThalliumBuff);

                On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            }

            private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
            {
                orig(self, damageInfo);
                if (NetworkServer.active && damageInfo.dotIndex == ThalliumDotIndex)
                {
                    EffectManager.SimpleEffect(Items.Thallium.thalliumTickEffect, damageInfo.position, Quaternion.identity, true);
                }
            }
        }
    }
}
