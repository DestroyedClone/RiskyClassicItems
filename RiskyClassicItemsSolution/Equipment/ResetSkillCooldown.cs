﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace ClassicItemsReturns.Equipment
{
    internal class ResetSkillCooldown : EquipmentBase<ResetSkillCooldown>
    {
        public override string EquipmentName => "Gigantic Amethyst";
        public override string EquipmentLangTokenName => "RESETSKILLCOOLDOWN";

        public override bool EnigmaCompatible { get; } = true;
        public override bool CanBeRandomlyTriggered { get; } = true;

        public override GameObject EquipmentModel => LoadItemModel("Crystal");

        public override Sprite EquipmentIcon => LoadItemSprite("Crystal");

        public override float Cooldown => 12f;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            Hooks();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (slot && slot.characterBody && slot.characterBody.skillLocator)
            {
                slot.characterBody.skillLocator.ApplyAmmoPack();
                Util.PlaySound("Play_env_hiddenLab_laptop_activate", slot.gameObject);
                return true;
            }
            return false;
        }
    }
}