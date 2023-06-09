using System;
using System.Collections.Generic;
using System.Text;
using R2API;
using RoR2;

namespace RiskyClassicItems.Modules
{
    internal class Dots
    {
        internal static DotController.DotIndex ThalliumDotIndex;
        internal static DotController.DotDef ThalliumDotDef;
        internal static DotAPI.CustomDotBehaviour ThalliumDotBehavior;
        internal static DotAPI.CustomDotVisual ThalliumDotVisual;
        internal static void Initialize()
        {
            Init_Thallium();
        }

        private static void Init_Thallium()
        {
            var thallium = Items.Thallium.Instance;

            ThalliumDotBehavior = new DotAPI.CustomDotBehaviour((dotController, dotStack) =>
            {
                CharacterBody attackerBody = dotStack.attackerObject.GetComponent<CharacterBody>();
                if (attackerBody && dotController.victimBody)
                {
                    dotStack.damage = thallium.enemyAttackDamageCoef * dotController.victimBody.damage;
                }
            });

            ThalliumDotIndex = DotAPI.RegisterDotDef(0.05f, 0f, DamageColorIndex.Poison, Buffs.ThalliumBuff, ThalliumDotBehavior);
        }
    }
}
