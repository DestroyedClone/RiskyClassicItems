using System;
using System.Collections.Generic;
using System.Text;

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
            Action<RoR2.DamageInfo, UnityEngine.GameObject> action = PreOnHitEnemy;
            if (action == null)
            {
                return;
            }
            action(damageInfo, victim);
            orig(self, damageInfo, victim);
            Action<RoR2.DamageInfo, UnityEngine.GameObject> action2 = PostOnHitEnemy;
            if (action2 == null)
            {
                return;
            }
            action2(damageInfo, victim);
        }
    }
}
