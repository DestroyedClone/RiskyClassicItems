using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassicItemsReturns.Utils
{
    public class IsTeleActivatedTracker
    {
        public static bool teleporterActivated = false;
        public static List<string> alwaysActiveStages = new List<string>()
        {
            "voidraid",
            "limbo"
        };

        public IsTeleActivatedTracker()
        {
            RoR2.Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;
            On.RoR2.TeleporterInteraction.ChargedState.OnEnter += ChargedState_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Active.OnEnter += Active_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Travelling.OnEnter += Travelling_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter += BrotherEncounterPhaseBaseState_OnEnter;
            On.RoR2.ArenaMissionController.BeginRound += ArenaMissionController_BeginRound;
            On.EntityStates.Missions.Moon.MoonBatteryActive.OnEnter += MoonBatteryActive_OnEnter;
            On.EntityStates.ArtifactShell.StartHurt.OnEnter += StartHurt_OnEnter;
            On.EntityStates.DeepVoidPortalBattery.Charging.OnEnter += Charging_OnEnter;
            On.EntityStates.Missions.Goldshores.GoldshoresBossfight.SpawnBoss += GoldshoresBossfight_SpawnBoss;
            On.EntityStates.MeridianEvent.MeridianEventStart.OnEnter += MeridianEventStart_OnEnter1;
        }

        private void MeridianEventStart_OnEnter1(On.EntityStates.MeridianEvent.MeridianEventStart.orig_OnEnter orig, EntityStates.MeridianEvent.MeridianEventStart self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void GoldshoresBossfight_SpawnBoss(On.EntityStates.Missions.Goldshores.GoldshoresBossfight.orig_SpawnBoss orig, EntityStates.Missions.Goldshores.GoldshoresBossfight self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void StartHurt_OnEnter(On.EntityStates.ArtifactShell.StartHurt.orig_OnEnter orig, EntityStates.ArtifactShell.StartHurt self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void Charging_OnEnter(On.EntityStates.DeepVoidPortalBattery.Charging.orig_OnEnter orig, EntityStates.DeepVoidPortalBattery.Charging self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void MoonBatteryActive_OnEnter(On.EntityStates.Missions.Moon.MoonBatteryActive.orig_OnEnter orig, EntityStates.Missions.Moon.MoonBatteryActive self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void ArenaMissionController_BeginRound(On.RoR2.ArenaMissionController.orig_BeginRound orig, ArenaMissionController self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void BrotherEncounterPhaseBaseState_OnEnter(On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void ChargedState_OnEnter(On.RoR2.TeleporterInteraction.ChargedState.orig_OnEnter orig, TeleporterInteraction.ChargedState self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void Active_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Active.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Active self)
        {
            orig(self);
            IsTeleActivatedTracker.teleporterActivated = true;
        }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, TeleporterInteraction.ChargingState self)
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

            SceneDef currentScene = SceneCatalog.GetSceneDefForCurrentScene();
            if (currentScene && alwaysActiveStages.Contains(currentScene.cachedName)) IsTeleActivatedTracker.teleporterActivated = true;
        }
    }
}
