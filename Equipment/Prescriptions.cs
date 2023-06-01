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

        public override string[] EquipmentFullDescriptionParams => new string[]
        {
            buffDuration.ToString(),
            (buffMovementSpeed * 100).ToString(),
            (buffAttackSpeed * 100).ToString(),
        };

        public override GameObject EquipmentModel => LoadModel();

        public override Sprite EquipmentIcon => LoadSprite();

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

        protected override void CreateConfig(ConfigFile config)
        {
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