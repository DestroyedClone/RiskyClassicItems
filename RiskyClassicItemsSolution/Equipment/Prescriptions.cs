using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Equipment
{
    public class Prescriptions : EquipmentBase<Prescriptions>
    {
        public override string EquipmentName => "Prescriptions";

        public override string EquipmentLangTokenName => "DRUGS";
        public const float buffDuration = 8f;
        public const float buffMovementSpeed = 0.5f;
        public const float buffAttackSpeed = 1f;

        public override object[] EquipmentFullDescriptionParams => new object[]
        {
            buffDuration,
            (buffMovementSpeed * 100),
            (buffAttackSpeed * 100),
        };

        public override GameObject EquipmentModel => LoadPickupModel("Drugs");

        public override Sprite EquipmentIcon => LoadEquipmentIcon("Drugs");

        public static BuffDef DrugsBuff => Buffs.DrugsBuff;

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var buffCount = sender.GetBuffCount(DrugsBuff);
            if (buffCount > 0)
            {
                args.moveSpeedMultAdd += buffMovementSpeed;
                args.attackSpeedMultAdd += buffAttackSpeed;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuff(DrugsBuff, buffDuration);
            return true;
        }
    }
}