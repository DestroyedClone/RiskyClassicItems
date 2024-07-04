using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;

namespace ClassicItemsReturns.Equipment
{
    public class Prescriptions : EquipmentBase<Prescriptions>
    {
        public override string EquipmentName => "Prescriptions";

        public override string EquipmentLangTokenName => "DRUGS";
        public const float buffDuration = 8f;
        public const float buffDamageMultiplier = 0.25f;
        public const float buffAttackSpeed = 0.5f;

        public override bool EnigmaCompatible { get; } = true;
        public override bool CanBeRandomlyTriggered { get; } = true;

        public override object[] EquipmentFullDescriptionParams => new object[]
        {
            buffDuration,
            (buffDamageMultiplier * 100),
            (buffAttackSpeed * 100),
        };

        public override GameObject EquipmentModel => LoadItemModel("Pills");

        public override Sprite EquipmentIcon => LoadItemSprite("Pills");

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
                args.attackSpeedMultAdd += buffAttackSpeed;
                args.damageMultAdd += buffDamageMultiplier;
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
                    localPos = new Vector3(0.01209F, 0F, 1.69616F),
                    localAngles = new Vector3(0F, 90F, 56F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                }
            });

            return dict;
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuff(DrugsBuff, buffDuration);
            Util.PlaySound("Play_item_proc_healingPotion", slot.gameObject);
            return true;
        }
    }
}