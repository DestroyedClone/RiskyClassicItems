using System;

namespace RiskyClassicItems.Modules
{
    internal class Events
    {
        internal static void Initialize()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        public static event Action<RoR2.DamageInfo, UnityEngine.GameObject> PreOnHitEnemy;

        public static event Action<RoR2.DamageInfo, UnityEngine.GameObject> PostOnHitEnemy;

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, UnityEngine.GameObject victim)
        {
            PreOnHitEnemy?.Invoke(damageInfo, victim);
            orig(self, damageInfo, victim);
            PostOnHitEnemy?.Invoke(damageInfo, victim);
        }
    }
}