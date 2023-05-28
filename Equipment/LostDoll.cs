using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Equipment
{
    internal class LostDoll : EquipmentBase<LostDoll>
    {
        public override string EquipmentName => "Lost Doll";

        public override string EquipmentLangTokenName => "LOSTDOLL";

        public const float selfHurtCoefficient = 0.25f;
        public const float damageCoefficient = 5f;

        public override string[] EquipmentFullDescriptionParams => new string[]
        {
            (selfHurtCoefficient*100).ToString(),
            (damageCoefficient*100).ToString(),
        };

        public override GameObject EquipmentModel => Assets.NullModel;

        public override Sprite EquipmentIcon => Assets.NullSprite;

        public override TargetFinderType EquipmentTargetFinderType => TargetFinderType.Enemies;

        public override bool IsLunar => true;

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
                slot.characterBody.healthComponent.TakeDamage(new DamageInfo()
                {
                    attacker = slot.gameObject,
                    crit = false,
                    damage = slot.characterBody.healthComponent.combinedHealth * selfHurtCoefficient,
                    damageColorIndex = DamageColorIndex.Item,
                    damageType = DamageType.NonLethal,
                    inflictor = slot.gameObject,
                    position = slot.characterBody.corePosition,
                    procCoefficient = 0,
                    procChainMask = default
                });
                RoR2.Orbs.OrbManager.instance.AddOrb(new RoR2.Orbs.DevilOrb
                {
                    attacker = slot.gameObject,
                    damageColorIndex = DamageColorIndex.Item,
                    damageValue = slot.characterBody.healthComponent.fullCombinedHealth * damageCoefficient,
                    isCrit = Util.CheckRoll(slot.characterBody.crit, slot.characterBody.master),
                    procChainMask = default,
                    procCoefficient = 1f,
                    target = hurtBox,
                    effectType = RoR2.Orbs.DevilOrb.EffectType.Wisp
                });
                slot.InvalidateCurrentTarget();
                return true;
            }
            return false;
        }
    }
}