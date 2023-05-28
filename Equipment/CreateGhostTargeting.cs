using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RoR2;
using UnityEngine;
using UnityEngine.Events;

namespace RiskyClassicItems.Equipment
{
    internal class CreateGhostTargeting : EquipmentBase<CreateGhostTargeting>
    {
        public override string EquipmentName => "Soul Jar";

        public override string EquipmentLangTokenName => "CREATEGHOSTTARGETING";
        public const int maxGhosts = 1;
        public const int ghostDurationSeconds = 30;
        DeployableSlot DS_GhostAlly => Deployables.DS_GhostAlly;

        public override string[] EquipmentFullDescriptionParams => new string[] { ghostDurationSeconds.ToString() };

        public override GameObject EquipmentModel => Assets.NullModel;

        public override Sprite EquipmentIcon => Assets.NullSprite;

        public override TargetFinderType EquipmentTargetFinderType => TargetFinderType.Enemies;

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
            if (!hurtBox)
            {
                return false;
            }
            if (!hurtBox.healthComponent)
            {
                return false;
            }
            var ghostBody = Util.TryToCreateGhost(hurtBox.healthComponent.body, slot.characterBody, ghostDurationSeconds);
            if (ghostBody)
            {
                Deployable deployable = ghostBody.gameObject.AddComponent<Deployable>();
                deployable.onUndeploy = deployable.onUndeploy = (deployable.onUndeploy ?? new UnityEvent());
                deployable.onUndeploy.AddListener(new UnityAction(ghostBody.master.TrueKill));
                ghostBody.master.AddDeployable(deployable, DS_GhostAlly);
            }
            slot.InvalidateCurrentTarget();
            return true;
        }
    }
}