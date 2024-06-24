using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using EntityStates.AffixVoid;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Equipment
{
    public class DroneRepairKit : EquipmentBase<DroneRepairKit>
    {
        public override string EquipmentName => "Drone Repair Kit";

        public override string EquipmentLangTokenName => "DRONEREPAIRKIT";
        public const float buffDuration = 8f;
        public const float buffAttackSpeed = 0.5f;
        public const float buffCDReduction = 0.5f;
        public const float buffArmorFlat = 50;

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
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var buffCount = sender.GetBuffCount(DroneRepairBuff);
            if (buffCount > 0)
            {
                args.attackSpeedMultAdd += buffAttackSpeed;
                args.cooldownReductionAdd += buffCDReduction;
                args.armorAdd += buffArmorFlat;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            int activationCount = 0;
            foreach (var mechAlly in CharacterBody.readOnlyInstancesList)
            {
                if (mechAlly.isPlayerControlled)
                    continue;
                if (mechAlly.teamComponent.teamIndex != slot.teamComponent.teamIndex)
                    continue;
                if (!mechAlly.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
                    continue;
                mechAlly.healthComponent.HealFraction(1f, default);
                mechAlly.AddTimedBuff(DroneRepairBuff, buffDuration);
                activationCount++;
            }

            Util.PlaySound("Play_item_proc_healingPotion", slot.gameObject);
            slot.subcooldownTimer = 0.5f;
            return activationCount > 0;
        }
    }
}