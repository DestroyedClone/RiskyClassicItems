using System;
using System.Collections.Generic;
using System.Text;
using R2API;
using RiskyClassicItems.Equipment;
using RoR2;

namespace RiskyClassicItems.Modules
{
    internal class Deployables
    {
        //https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/35a9445e2cacfac2d577590b378a45b4239689bd/Skills/EngiUtilitySpeedispenser.cs#L28
        public static DeployableSlot DS_GhostAlly { get; private set; }
        public static void Initialize()
        {
            DS_GhostAlly = DeployableAPI.RegisterDeployableSlot((master, countMult) => {
                return CreateGhostTargeting.maxGhosts;
            });
        }


    }
}
