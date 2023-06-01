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
            var thallium = Items.Thallium.Instance;

            ThalliumDotIndex = DotAPI.RegisterDotDef(0.05f, 0f, DamageColorIndex.Poison, Buffs.ThalliumBuff, ThalliumDotBehavior);

            ThalliumDotBehavior = new DotAPI.CustomDotBehaviour((dotController, dotStack) =>
            {
                CharacterBody attackerBody = dotStack.attackerObject.GetComponent<CharacterBody>();
                if (attackerBody && dotController.victimBody)
                {
                    dotStack.damage = thallium.enemyAttackDamageCoef * dotController.victimBody.damage;
                }
            });
        }
    }
}
