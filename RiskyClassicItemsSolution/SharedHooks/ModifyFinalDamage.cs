﻿using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ClassicItemsReturns.SharedHooks
{
    public static class ModifyFinalDamage
    {
        public delegate void ModifyFinalDamageDelegate(DamageMult damageMult, DamageInfo damageInfo,
            HealthComponent victim, CharacterBody victimBody,
            CharacterBody attackerBody, Inventory attackerInventory);
        public static ModifyFinalDamageDelegate ModifyFinalDamageActions;

        public static void Initialize()
        {
            IL.RoR2.HealthComponent.TakeDamageProcess += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(
                     x => x.MatchStloc(7)
                    ))
                {
                    c.Emit(OpCodes.Ldarg_0);    //self
                    c.Emit(OpCodes.Ldarg_1);    //damageInfo
                    c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((origDamage, victimHealth, damageInfo) =>
                    {
                        float newDamage = origDamage;
                        CharacterBody victimBody = victimHealth.body;
                        if (victimBody && damageInfo.attacker)
                        {
                            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                            if (attackerBody)
                            {
                                Inventory attackerInventory = attackerBody.inventory;
                                if (attackerInventory)
                                {
                                    DamageMult damageMult = new DamageMult();
                                    if (ModifyFinalDamageActions != null)
                                    {
                                        ModifyFinalDamageActions.Invoke(damageMult, damageInfo, victimHealth, victimBody, attackerBody, attackerInventory);
                                        newDamage *= damageMult.damageMult;
                                    }
                                }
                            }
                        }
                        return newDamage;
                    });
                }
                else
                {
                    UnityEngine.Debug.LogError("ClassicItemsReturns: ModifyFinalDamage IL Hook failed. This will break a lot of things.");
                }
            };
        }

        public class DamageMult
        {
            public float damageMult = 1f;
        }
    }
}
