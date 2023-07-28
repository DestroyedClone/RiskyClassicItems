using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace ClassicItemsReturns.SharedHooks
{
    public static class OnCharacterDeath
    {
        public delegate void OnCharacterDeathAttackerInventory(GlobalEventManager globalEventManager, DamageReport damageReport, CharacterBody attackerBody, Inventory attackerInventory);
        public static OnCharacterDeathAttackerInventory OnCharacterDeathAttackerInventoryActions;

        public static void Initialize()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        private static void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            orig(self, damageReport);
            if (!NetworkServer.active) return;
            if (damageReport.attackerBody && damageReport.attackerBody.inventory) OnCharacterDeathAttackerInventoryActions?.Invoke(self, damageReport, damageReport.attackerBody, damageReport.attackerBody.inventory);
        }
    }
}
