﻿using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ClassicItemsReturns.Equipment
{
    public class DroneRepairKit : EquipmentBase<DroneRepairKit>
    {
        public override string EquipmentName => "Drone Repair Kit";

        public override string EquipmentLangTokenName => "DRONEREPAIRKIT";
        public const float buffDuration = 8f;
        public const float buffAttackSpeed = 0.5f;
        public const float buffCDReduction = 0.5f;
        public const float buffArmor = 50f;

        public static NetworkSoundEventDef activationSound;

        public override bool EnigmaCompatible { get; } = true;
        public override bool CanBeRandomlyTriggered { get; } = true;

        public override object[] EquipmentFullDescriptionParams => new object[]
        {
        };

        public override GameObject EquipmentModel => LoadItemModel("RepairKit");

        public override Sprite EquipmentIcon => LoadItemSprite("RepairKit");

        public override void CreateAssets(ConfigFile config)
        {
            base.CreateAssets(config);

            activationSound = Assets.CreateNetworkSoundEventDef("Play_ClassicItemsReturns_RepairKit");
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            bool hasBuff =  sender.HasBuff(Buffs.DroneRepairBuff);
            if (hasBuff)
            {
                args.attackSpeedMultAdd += buffAttackSpeed;
                args.cooldownReductionAdd += buffCDReduction;
                args.armorAdd += buffArmor;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var dict = new ItemDisplayRuleDict();

            //Don't set up displays if 3d model isn't available
            GameObject display = EquipmentModel;
            if (!display.name.Contains("mdl3d")) return dict;

            dict.Add("EquipmentDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = EquipmentModel,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "GunBarrelBase",
                    localPos = new Vector3(-0.222F, 0F, 2.51855F),
                    localAngles = new Vector3(0.39625F, 59.72602F, 274.5737F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });

            return dict;
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.master) return false;
            int activationCount = 0;
            MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(slot.characterBody.master.netId);
            if (minionGroup != null)
            {
                foreach (MinionOwnership minion in minionGroup.members)
                {
                    if (!minion) continue;
                    CharacterMaster master = minion.GetComponent<CharacterMaster>();
                    if (master)
                    {
                        CharacterBody body = master.GetBody();
                        if (body)
                        {
                            if (body.healthComponent) body.healthComponent.HealFraction(1f, default);
                            body.AddTimedBuff(Buffs.DroneRepairBuff, buffDuration);
                            activationCount++;
                        }
                    }
                }
            }

            bool activated = activationCount > 0;
            if (activated)
            {
                RoR2.Audio.EntitySoundManager.EmitSoundServer(activationSound.index, slot.characterBody.gameObject);
            }
            //slot.subcooldownTimer = 0.5f;

            //Always return true so that you don't get stuck with it in Enigma
            return true;
        }
    }
}