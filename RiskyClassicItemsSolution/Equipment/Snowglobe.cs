using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Equipment
{
    public class Snowglobe : EquipmentBase<Snowglobe>
    {
        public override string EquipmentName => "Snowglobe";

        public override string EquipmentLangTokenName => "SNOWGLOBE";

        public override GameObject EquipmentModel => LoadModel();

        public override Sprite EquipmentIcon => LoadSprite();

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
            return false;
        }
    }
}