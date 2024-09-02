using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace ClassicItemsReturns.SharedHooks
{
    public class OnHitEnemy
    {
        public delegate void OnHitEnemyAttackerInventory(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, CharacterBody attackerBody, Inventory attackerInventory);
        public static OnHitEnemyAttackerInventory OnHitEnemyAttackerInventoryActions;

        public delegate void OnHitEnemyAttackerInventoryAndVictimBody(GlobalEventManager globalEventManager, DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody, Inventory attackerInventory);
        public static OnHitEnemyAttackerInventoryAndVictimBody OnHitEnemyAttackerInventoryAndVictimBodyActions;

        public static void Initialize()
        {
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim)
        {
            orig(globalEventManager, damageInfo, victim);

            if (!NetworkServer.active || damageInfo.rejected || damageInfo.procCoefficient <= 0f || !damageInfo.attacker) return;

            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            if (attackerBody && attackerBody.inventory)
            {
                OnHitEnemyAttackerInventoryActions?.Invoke(globalEventManager, damageInfo, victim, attackerBody, attackerBody.inventory);

                if (victim)
                {
                    CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                    if (victimBody) OnHitEnemyAttackerInventoryAndVictimBodyActions?.Invoke(globalEventManager, damageInfo, victimBody, attackerBody, attackerBody.inventory);
                }
            }
        }
    }
}
