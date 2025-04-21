using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ClassicItemsReturns.SharedHooks
{
    public static class TakeDamage
    {
        public delegate void OnDamageTakenInventory(DamageInfo damageInfo, HealthComponent self, CharacterBody victimBody, Inventory inventory, bool lostShield, bool lostOutOfDanger);
        public static OnDamageTakenInventory OnDamageTakenInventoryActions;

        public static void Initialize()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamage;
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            bool hadShield = self.shield > 0f;
            bool wasOutOfDanger = self.body && self.body.outOfDanger;

            orig(self, damageInfo);

            if (!NetworkServer.active) return;

            if (damageInfo.damage > 0f && !damageInfo.rejected)
            {
                if (self.body && self.body.inventory)
                {
                    bool lostShield = hadShield && self.shield <= 0f;
                    bool lostOutOfDanger = wasOutOfDanger && !(self.body && self.body.outOfDanger);

                    OnDamageTakenInventoryActions?.Invoke(damageInfo, self, self.body, self.body.inventory, lostShield, lostOutOfDanger);
                }
            }
        }
    }
}
