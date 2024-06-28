using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassicItemsReturns.Utils
{
    public class IsTeleActivatedTracker
    {
        public static bool teleporterActivated = false;

        public IsTeleActivatedTracker()
        {
            RoR2.Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;
            On.RoR2.TeleporterInteraction.ChargedState.OnEnter += ChargedState_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Active.OnEnter += Active_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Travelling.OnEnter += Travelling_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter += BrotherEncounterPhaseBaseState_OnEnter;
            On.RoR2.VoidRaidEncounterController.Start += VoidRaidEncounterController_Start;
            On.RoR2.ArenaMissionController.BeginRound += ArenaMissionController_BeginRound;
            On.RoR2.VoidStageMissionController.OnBatteryActivated += VoidStageMissionController_OnBatteryActivated;
            On.EntityStates.Missions.Moon.MoonBatteryActive.OnEnter += MoonBatteryActive_OnEnter;
        }

        private void MoonBatteryActive_OnEnter(On.EntityStates.Missions.Moon.MoonBatteryActive.orig_OnEnter orig, EntityStates.Missions.Moon.MoonBatteryActive self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void VoidStageMissionController_OnBatteryActivated(On.RoR2.VoidStageMissionController.orig_OnBatteryActivated orig, VoidStageMissionController self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void ArenaMissionController_BeginRound(On.RoR2.ArenaMissionController.orig_BeginRound orig, ArenaMissionController self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void VoidRaidEncounterController_Start(On.RoR2.VoidRaidEncounterController.orig_Start orig, VoidRaidEncounterController self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void BrotherEncounterPhaseBaseState_OnEnter(On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void ChargedState_OnEnter(On.RoR2.TeleporterInteraction.ChargedState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void Active_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Active.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Active self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void Travelling_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Travelling.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Travelling self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = false;
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            IsTeleActivatedTracker.teleporterActivated = false;
        }
    }
}
