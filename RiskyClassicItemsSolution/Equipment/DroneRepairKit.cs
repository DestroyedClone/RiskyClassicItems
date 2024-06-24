using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using EntityStates.AffixVoid;
using UnityEngine.Networking;
using UnityEngine.Timeline;

namespace ClassicItemsReturns.Equipment
{
    public class DroneRepairKit : EquipmentBase<DroneRepairKit>
    {
        public override string EquipmentName => "Drone Repair Kit";

        public override string EquipmentLangTokenName => "DRONEREPAIRKIT";
        public const float buffDuration = 8f;
        public const float buffAttackSpeed = 0.5f;
        public const float buffCDReduction = 0.5f;

        public static NetworkSoundEventDef activationSound;

        public override bool EnigmaCompatible { get; } = true;
        public override bool CanBeRandomlyTriggered { get; } = true;

        public override object[] EquipmentFullDescriptionParams => new object[]
        {
        };

        public override GameObject EquipmentModel => LoadItemModel("RepairKit");

        public override Sprite EquipmentIcon => LoadItemSprite("RepairKit");

        public static BuffDef DroneRepairBuff => Buffs.DroneRepairBuff;
        public override bool Unfinished => true;

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
            bool hasBuff =  sender.HasBuff(DroneRepairBuff);
            if (hasBuff)
            {
                args.attackSpeedMultAdd += buffAttackSpeed;
                args.cooldownReductionAdd += buffCDReduction;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
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
                            body.AddTimedBuff(DroneRepairBuff, buffDuration);
                            body.AddTimedBuff(RoR2Content.Buffs.Immune, buffDuration);
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