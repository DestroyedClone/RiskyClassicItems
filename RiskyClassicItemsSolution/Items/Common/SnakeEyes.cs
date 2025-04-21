using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items.Common
{
    public class SnakeEyes : ItemBase<SnakeEyes>
    {
        public override string ItemName => "Snake Eyes";

        public override string ItemLangTokenName => "SNAKEEYES";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadItemModel("Dice");

        public override Sprite ItemIcon => LoadItemSprite("Dice");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.InteractableRelated,
            ItemTag.AIBlacklist
        };

        public static HashSet<string> stageDontResetList = new HashSet<string>
        {
        };
        
        public static HashSet<string> stageForceResetList = new HashSet<string>
        {
            "meridian",
            "moon2",
            "arena",
            "goldshores",
            "voidstage"
        };

        public static HashSet<string> teamwideProcShrineList = new HashSet<string>
        {
            "SHRINE_HALCYONITE_NAME",
            "SHRINE_BOSS_NAME"
        };

        public float critChance = 7.5f;


        public override object[] ItemFullDescriptionParams => new object[]
        {
            critChance
        };

        //Way the buffs are handled is very ugly.
        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            Inventory.onInventoryChangedGlobal += Inventory_onInventoryChangedGlobal;
            //ShrineChanceBehavior.onShrineChancePurchaseGlobal += DiceOnShrineFail;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += DiceOnShrineUse;
            Stage.onStageStartGlobal += ResetCountOnStageStart;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.GeodeController.OnInteractionBegin += GeodeController_OnInteractionBegin;
            On.EntityStates.Interactables.GoldBeacon.Ready.OnEnter += Ready_OnEnter;
            On.EntityStates.Missions.Moon.MoonBatteryActive.OnEnter += MoonBatteryActive_OnEnter;
            On.EntityStates.Missions.Arena.NullWard.Active.OnEnter += Active_OnEnter;
            On.EntityStates.DeepVoidPortalBattery.Charging.OnEnter += Charging_OnEnter;
        }

        public void ProcSnakeEyesForTeam(TeamIndex index)
        {
            foreach (CharacterMaster cm in CharacterMaster.instancesList)
            {
                if (cm.teamIndex != index) continue;
                ProcSnakeEyes(cm);
            }
        }

        public void ProcSnakeEyes(CharacterMaster master)
        {
            if (!master || !master.inventory || master.inventory.GetItemCount(ItemDef) <= 0) return;

            MasterSnakeEyesTracker mset = master.GetComponent<MasterSnakeEyesTracker>();
            if (!mset) mset = master.gameObject.AddComponent<MasterSnakeEyesTracker>();

            CharacterBody body = master.GetBody();
            if (body) mset.Increment(body);
        }

        private void DiceOnShrineUse(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            //Duplicated check, ugly
            bool canBeAfforded = self.CanBeAffordedByInteractor(activator);

            orig(self, activator);

            if (NetworkServer.active && canBeAfforded && (self.isShrine || self.isGoldShrine))
            {
                CharacterBody body = activator.GetComponent<CharacterBody>();
                if (body)
                {
                    //Scuffed but whatever
                    if (teamwideProcShrineList.Contains(self.displayNameToken) && body.teamComponent)
                    {
                        ProcSnakeEyesForTeam(body.teamComponent.teamIndex);
                    }
                    else
                    {
                        ProcSnakeEyes(body.master);
                    }
                }
            }
        }

        //Geodes proc teamwide
        private void GeodeController_OnInteractionBegin(On.RoR2.GeodeController.orig_OnInteractionBegin orig, GeodeController self, Interactor activator)
        {
            orig(self, activator);
            if (NetworkServer.active && activator)
            {
                CharacterBody body = activator.GetComponent<CharacterBody>();
                if (body && body.teamComponent) ProcSnakeEyesForTeam(body.teamComponent.teamIndex);
            }
        }

        private void Active_OnEnter(On.EntityStates.Missions.Arena.NullWard.Active.orig_OnEnter orig, EntityStates.Missions.Arena.NullWard.Active self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                ProcSnakeEyesForTeam(TeamIndex.Player);
            }
        }

        private void MoonBatteryActive_OnEnter(On.EntityStates.Missions.Moon.MoonBatteryActive.orig_OnEnter orig, EntityStates.Missions.Moon.MoonBatteryActive self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                ProcSnakeEyesForTeam(TeamIndex.Player);
            }
        }

        private void Ready_OnEnter(On.EntityStates.Interactables.GoldBeacon.Ready.orig_OnEnter orig, EntityStates.Interactables.GoldBeacon.Ready self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                ProcSnakeEyesForTeam(TeamIndex.Player);
            }
        }

        private void Charging_OnEnter(On.EntityStates.DeepVoidPortalBattery.Charging.orig_OnEnter orig, EntityStates.DeepVoidPortalBattery.Charging self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                ProcSnakeEyesForTeam(TeamIndex.Player);
            }
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (!NetworkServer.active || !body.master) return;
            MasterSnakeEyesTracker mset = body.master.gameObject.GetComponent<MasterSnakeEyesTracker>();
            if (mset)
            {
                mset.RecalculateBuffServer(body);
            }
        }

        private void ResetCountOnStageStart(Stage obj)
        {
            if (!NetworkServer.active) return;

            SceneDef currentScene = SceneCatalog.GetSceneDefForCurrentScene();
            if (currentScene && !stageForceResetList.Contains(currentScene.baseSceneName))
            {
                if (currentScene.isFinalStage || currentScene.blockOrbitalSkills || stageDontResetList.Contains(currentScene.baseSceneName)) return;
            }

            foreach (CharacterMaster characterMaster in CharacterMaster.instancesList)
            {
                MasterSnakeEyesTracker mset = characterMaster.GetComponent<MasterSnakeEyesTracker>();
                if (mset) mset.ResetCount();
            }
        }

        private void Inventory_onInventoryChangedGlobal(Inventory inventory)
        {
            if (!NetworkServer.active || !inventory) return;

            CharacterMaster master = inventory.GetComponent<CharacterMaster>();
            if (!master) return;

            bool hasItem = inventory.GetItemCount(ItemDef) > 0;
            if (hasItem)
            {
                MasterSnakeEyesTracker mset = master.GetComponent<MasterSnakeEyesTracker>();
                if (!mset) master.gameObject.AddComponent<MasterSnakeEyesTracker>();
            }

            CharacterBody body = master.GetBody();
            if (!body) return;
            if (!hasItem) RemoveSnakeEyesBuffs(body);
        }

        public static void RemoveSnakeEyesBuffs(CharacterBody body)
        {
            Utils.MiscUtils.TryRemoveBuff(body, Modules.Buffs.SnakeEyesBuffs.Snake1);
            Utils.MiscUtils.TryRemoveBuff(body, Modules.Buffs.SnakeEyesBuffs.Snake2);
            Utils.MiscUtils.TryRemoveBuff(body, Modules.Buffs.SnakeEyesBuffs.Snake3);
            Utils.MiscUtils.TryRemoveBuff(body, Modules.Buffs.SnakeEyesBuffs.Snake4);
            Utils.MiscUtils.TryRemoveBuff(body, Modules.Buffs.SnakeEyesBuffs.Snake5);
            Utils.MiscUtils.TryRemoveBuff(body, Modules.Buffs.SnakeEyesBuffs.Snake6);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (TryGetCount(sender, out int itemCount))
            {
                float critToAdd = itemCount * critChance;
                int mult = 0;

                //Check individually in case they overlap for some reason.
                if (sender.HasBuff(Modules.Buffs.SnakeEyesBuffs.Snake1)) mult = 1;
                if (sender.HasBuff(Modules.Buffs.SnakeEyesBuffs.Snake2)) mult = 2;
                if (sender.HasBuff(Modules.Buffs.SnakeEyesBuffs.Snake3)) mult = 3;
                if (sender.HasBuff(Modules.Buffs.SnakeEyesBuffs.Snake4)) mult = 4;
                if (sender.HasBuff(Modules.Buffs.SnakeEyesBuffs.Snake5)) mult = 5;
                if (sender.HasBuff(Modules.Buffs.SnakeEyesBuffs.Snake6)) mult = 6;

                args.critAdd += critToAdd * mult;
            }
        }

        //This lets you keep your stacks if you get revived on the same stage.
        public class MasterSnakeEyesTracker : MonoBehaviour
        {
            int count = 0;
            private const int maxStack = 6;

            public void ResetCount()
            {
                count = 0;
            }

            //Very ugly
            public void Increment(CharacterBody body)
            {
                if (count < maxStack) count++;
                if (body) RecalculateBuffServer(body);
            }

            public void RecalculateBuffServer(CharacterBody body)
            {
                if (!NetworkServer.active || !body) return;
                RemoveSnakeEyesBuffs(body);
                switch (count)
                {
                    case 0:
                        break;
                    case 1:
                        body.AddBuff(Modules.Buffs.SnakeEyesBuffs.Snake1);
                        break;
                    case 2:
                        body.AddBuff(Modules.Buffs.SnakeEyesBuffs.Snake2);
                        break;
                    case 3:
                        body.AddBuff(Modules.Buffs.SnakeEyesBuffs.Snake3);
                        break;
                    case 4:
                        body.AddBuff(Modules.Buffs.SnakeEyesBuffs.Snake4);
                        break;
                    case 5:
                        body.AddBuff(Modules.Buffs.SnakeEyesBuffs.Snake5);
                        break;
                    default:
                        body.AddBuff(Modules.Buffs.SnakeEyesBuffs.Snake6);
                        break;
                }
            }
        }
    }
}
