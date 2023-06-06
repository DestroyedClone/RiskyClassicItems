using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Equipment
{
    internal class ExampleEquipWithTargeting : EquipmentBase<ExampleEquipWithTargeting>
    {
        public override string EquipmentName => "Deprecate Me Equipment Targeting Edition";

        public override string EquipmentLangTokenName => "DEPRECATE_ME_EQUIPMENT_TARGETING";

        public override GameObject EquipmentModel => Assets.NullModel;

        public override Sprite EquipmentIcon => Assets.NullSprite;

        public override TargetFinderType EquipmentTargetFinderType => TargetFinderType.BossesWithRewards;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateTargetingIndicator();
            CreateEquipment();
            Hooks();
        }

        protected override void CreateConfig(ConfigFile config)
        {
        }

        /// <summary>
        /// An example targeting indicator implementation. We clone the woodsprite's indicator, but we edit it to our liking. We'll use this in our activate equipment.
        /// We shouldn't need to network this as this only shows for the player with the Equipment.
        /// </summary>
        private void CreateTargetingIndicator()
        {
            /*
            TargetingIndicatorPrefabBase = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator"), "ExampleIndicator", false);
            //TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().sprite = Assets.LoadSprite("ExampleReticuleIcon.png");
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            TargetingIndicatorPrefabBase.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(0.423f, 1, 0.749f);*/
            //TargetingIndicatorPrefabBase = Assets.targetIndicatorBossHunter;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            HurtBox hurtBox = slot.currentTarget.hurtBox;
            if (hurtBox)
            {
                slot.subcooldownTimer = 0.2f;
                RoR2.Orbs.OrbManager.instance.AddOrb(new RoR2.Orbs.LightningStrikeOrb
                {
                    attacker = slot.gameObject,
                    damageColorIndex = DamageColorIndex.Item,
                    damageValue = slot.characterBody.damage * 10f,
                    isCrit = Util.CheckRoll(slot.characterBody.crit, slot.characterBody.master),
                    procChainMask = default,
                    procCoefficient = 1f,
                    target = hurtBox
                });
                slot.InvalidateCurrentTarget();
                return true;
            }
            return false;
        }
    }
}