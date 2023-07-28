using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace ClassicItemsReturns.SharedHooks
{
    public class OnHitEnemy
    {
        public delegate void OnHitEnemyAttackerInventory(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, CharacterBody attackerBody, Inventory attackerInventory);
        public static OnHitEnemyAttackerInventory OnHitEnemyAttackerInventoryActions;

        public static void Initialize()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim)
        {
            orig(globalEventManager, damageInfo, victim);

            if (!NetworkServer.active) return;

            if (damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody && attackerBody.inventory) OnHitEnemyAttackerInventoryActions?.Invoke(globalEventManager, damageInfo, victim, attackerBody, attackerBody.inventory);
            }
        }
    }
}
