using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RiskyClassicItems.Equipment
{
    internal class CreateGhostTargeting : EquipmentBase<CreateGhostTargeting>
    {
        public override string EquipmentName => "Soul Jar";

        public override string EquipmentLangTokenName => "CREATEGHOSTTARGETING";
        public const int maxGhosts = 1;
        public const int ghostDurationSecondsPlayer = 30;
        public const int ghostDurationSecondsEnemy = 25;
        public const int ghostDamageCoefficientTimesTen = 10;
        DeployableSlot DS_GhostAlly => Deployables.DS_GhostAlly;

        public override string[] EquipmentFullDescriptionParams => new string[] { 
            (ghostDamageCoefficientTimesTen * 10 * 100).ToString(),
            ghostDurationSecondsPlayer.ToString() };

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
            var deployableCount = slot.characterBody.master.GetDeployableCount(DS_GhostAlly);
            if (deployableCount == 0)
            {
                var ghostBody = SpawnMaskGhost(hurtBox.healthComponent.body, slot.characterBody);
                if (ghostBody)
                {
                    Deployable deployable = ghostBody.gameObject.AddComponent<Deployable>();
                    deployable.onUndeploy = deployable.onUndeploy = (deployable.onUndeploy ?? new UnityEvent());
                    deployable.onUndeploy.AddListener(new UnityAction(ghostBody.master.TrueKill));
                    slot.characterBody.master.AddDeployable(deployable, DS_GhostAlly);
                }
                slot.InvalidateCurrentTarget();
            }
            return true;
        }

        public static CharacterBody SpawnMaskGhost(CharacterBody targetBody, CharacterBody ownerBody)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'HappiestMask SpawnMaskGhost' called on client");
                return null;
            }
            if (!targetBody)
            {
                return null;
            }
            GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(targetBody);
            if (!bodyPrefab)
            {
                return null;
            }
            CharacterMaster characterMaster = MasterCatalog.allAiMasters.FirstOrDefault((CharacterMaster master) => master.bodyPrefab == bodyPrefab);
            if (!characterMaster)
            {
                return null;
            }
            MasterSummon masterSummon = new MasterSummon();
            masterSummon.masterPrefab = characterMaster.gameObject;
            masterSummon.ignoreTeamMemberLimit = true;
            masterSummon.position = targetBody.footPosition;
            CharacterDirection component = targetBody.GetComponent<CharacterDirection>();
            masterSummon.rotation = (component ? Quaternion.Euler(0f, component.yaw, 0f) : targetBody.transform.rotation);
            masterSummon.summonerBodyObject = (ownerBody ? ownerBody.gameObject : null);
            masterSummon.inventoryToCopy = targetBody.inventory;
            masterSummon.useAmbientLevel = null;
            CharacterMaster characterMaster2 = masterSummon.Perform();
            if (!characterMaster2)
            {
                return null;
            }
            else
            {
                Inventory inventory = characterMaster2.inventory;
                if (inventory)
                {
                    if (inventory.GetItemCount(RoR2Content.Items.Ghost) <= 0) inventory.GiveItem(RoR2Content.Items.Ghost);
                    if (inventory.GetItemCount(RoR2Content.Items.UseAmbientLevel) <= 0) inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);

                    if (ownerBody && ownerBody.teamComponent && ownerBody.teamComponent.teamIndex == TeamIndex.Player)
                    {
                        inventory.GiveItem(RoR2Content.Items.BoostDamage.itemIndex, ghostDamageCoefficientTimesTen);
                        inventory.GiveItem(RoR2Content.Items.HealthDecay.itemIndex, ghostDurationSecondsPlayer);
                        Modules.ModSupport.ModCompatRiskyMod.GiveAllyItem(inventory);
                    }
                    else //Handle enemy-spawned ghosts
                    {
                        inventory.GiveItem(RoR2Content.Items.HealthDecay.itemIndex, ghostDurationSecondsEnemy);
                        MasterSuicideOnTimer mst = characterMaster2.gameObject.AddComponent<MasterSuicideOnTimer>();
                        mst.lifeTimer = ghostDurationSecondsEnemy;
                    }
                }
            }
            CharacterBody body = characterMaster2.GetBody();
            if (body)
            {
                foreach (EntityStateMachine entityStateMachine in body.GetComponents<EntityStateMachine>())
                {
                    entityStateMachine.initialStateType = entityStateMachine.mainStateType;
                }
            }
            return body;
        }
    }
}