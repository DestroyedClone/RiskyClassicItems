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
        public override string[] EquipmentFullDescriptionParams => new string[] { buffDuration.ToString() };

        public override GameObject EquipmentModel => LoadModel();

        public override Sprite EquipmentIcon => LoadSprite();

        public static BuffDef DrugsBuff => Buffs.DrugsBuff;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            Hooks();
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
            slot.characterBody.AddTimedBuff(RoR2Content.Buffs.WarCryBuff, buffDuration);
            return true;
        }
    }
}