using BepInEx.Configuration;
using R2API;
using Rewired.ComponentControls.Effects;
using RiskyClassicItems.Modules;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RiskyClassicItems.Equipment
{
    internal class CreateGhostTargeting : EquipmentBase<CreateGhostTargeting>
    {
        public override string EquipmentName => "Soul Jar";

        public override string EquipmentLangTokenName => "CREATEGHOSTTARGETING";
        public const int ghostDurationSecondsPlayer = 30;
        public const int ghostDurationSecondsEnemy = 25;
        public const int boostDamageItemCount = 10;

        public int ghostsPerCommon = 3;
        public int ghostsPerChampion = 1;
        public int maxGhosts = 9;

        //DeployableSlot DS_GhostAlly => Deployables.DS_GhostAlly;
        public static ConfigEntry<bool> limitSpawns;

        public override object[] EquipmentFullDescriptionParams => new object[] {
            (boostDamageItemCount * 10),
            ghostsPerCommon,
            ghostDurationSecondsPlayer,
             };

        public override GameObject EquipmentModel => LoadItemModel("JarSouls");

        public override Sprite EquipmentIcon => LoadItemSprite("JarSouls");

        public override TargetFinderType EquipmentTargetFinderType => TargetFinderType.Enemies;

        public override float Cooldown => 90;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateTargetingIndicator();
            CreateEquipment();
            Hooks();
        }

        public static GameObject commonTargetIndicator;
        public static GameObject championTargetIndicator;

        /// <summary>
        /// An example targeting indicator implementation. We clone the woodsprite's indicator, but we edit it to our liking. We'll use this in our activate equipment.
        /// We shouldn't need to network this as this only shows for the player with the Equipment.
        /// </summary>
        private void CreateTargetingIndicator()
        {
            commonTargetIndicator = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator"), Assets.prefabPrefix + "SoulJarCommonIndicator", false);
            commonTargetIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
            commonTargetIndicator.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            commonTargetIndicator.GetComponentInChildren<SpriteRenderer>().GetComponent<RotateAroundAxis>().enabled = false;
            commonTargetIndicator.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(0.423f, 1, 0.749f);

            championTargetIndicator = PrefabAPI.InstantiateClone(commonTargetIndicator, Assets.prefabPrefix + "SoulJarChampionIndicator", false);
            commonTargetIndicator.GetComponentInChildren<SpriteRenderer>().sprite = Assets.LoadSprite("texJarOfSoulsTargetIndicator.png");
            championTargetIndicator.GetComponentInChildren<SpriteRenderer>().sprite = Assets.LoadSprite("texJarOfSoulsChampionTargetIndicator.png");
        }

        protected override void ConfigureTargetIndicator(EquipmentSlot equipmentSlot, EquipmentIndex targetingEquipmentIndex, GenericPickupController genericPickupController, ref bool shouldShowOverride)
        {
            //base.ConfigureTargetIndicator(equipmentSlot, targetingEquipmentIndex, genericPickupController);
            EquipmentSlot.UserTargetInfo currentTarget = equipmentSlot.currentTarget;
            if (currentTarget.hurtBox && currentTarget.hurtBox.healthComponent && currentTarget.hurtBox.healthComponent.alive)
            {
                if (currentTarget.hurtBox.healthComponent.body.isChampion)
                    equipmentSlot.targetIndicator.visualizerPrefab = championTargetIndicator;
                else
                    equipmentSlot.targetIndicator.visualizerPrefab = commonTargetIndicator;
                return;
            }
            shouldShowOverride = false;
        }

        protected override void CreateConfig(ConfigFile config)
        {
            base.CreateConfig(config);
            limitSpawns = config.Bind(ConfigCategory, "Limit Spawns", true, $"If true, max ghosts per player will be set to {maxGhosts}.");
            maxGhosts *= (limitSpawns.Value ? 1 : 999999);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public class JarGhostTrackerComponent : MonoBehaviour
        {
            public List<CharacterMaster> ghostList = new List<CharacterMaster>();

            public bool CanSpawnGhost()
            {
                int currentPoints = 0;
                foreach (CharacterMaster cm in ghostList)
                {
                    CharacterBody cb = cm.GetBody();
                    if (cb && cb.isChampion)
                    {
                        currentPoints += Instance.ghostsPerCommon;
                    }
                    else
                    {
                        currentPoints += 1;
                    }
                }

                bool canSpawn = ((currentPoints + Instance.ghostsPerCommon) <= Instance.maxGhosts);
                /*string message = $"Ghost List Count: {ghostList.Count}, Ghosts Per Common: {Instance.ghostsPerCommon}, Max Ghosts: {Instance.maxGhosts}, Can Spawn Ghost: {canSpawn}";
                Debug.Log(message);*/
                return canSpawn;
            }

            public void FixedUpdate()
            {
                for (int i = ghostList.Count - 1; i >= 0; i--)
                {
                    CharacterMaster ghost = ghostList[i];
                    if (ghost && ghost.IsDeadAndOutOfLivesServer())
                    {
                        ghostList.Remove(ghost);
                    }
                }
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            HurtBox hurtBox = slot.currentTarget.hurtBox;
            if (!hurtBox || !hurtBox.healthComponent || !hurtBox.healthComponent.alive)
            {
                return false;
            }
            var component = slot.GetComponent<JarGhostTrackerComponent>();
            if (!component)
            {
                component = slot.gameObject.AddComponent<JarGhostTrackerComponent>();
            }

            int ghostsToSpawn = hurtBox.healthComponent.body.isChampion ? ghostsPerChampion : ghostsPerCommon;
            bool hasSpawnedGhost = false;

            if (component.CanSpawnGhost())
                while (ghostsToSpawn > 0)
                {
                    SpawnMaskGhost(hurtBox.healthComponent.body, slot.characterBody, out CharacterMaster master);
                    if (master)
                    {
                        component.ghostList.Add(master);
                        ghostsToSpawn--;
                        hasSpawnedGhost = true;
                    }
                    else
                    {
                        ghostsToSpawn = 0;
                    }
                }
            if (hasSpawnedGhost)
            {
                CreateGhostOrbEffect(slot.characterBody, hurtBox.healthComponent.body, 0.25f);
            }
            slot.InvalidateCurrentTarget();

            /*var deployableCount = slot.characterBody.master.GetDeployableCount(DS_GhostAlly);
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
            }*/
            return hasSpawnedGhost;
        }

        public static CharacterBody SpawnMaskGhost(CharacterBody targetBody, CharacterBody ownerBody, out CharacterMaster ghostMaster)
        {
            ghostMaster = null;
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
                ghostMaster = characterMaster2;
                Inventory inventory = characterMaster2.inventory;
                if (inventory)
                {
                    if (inventory.GetItemCount(RoR2Content.Items.Ghost) <= 0) inventory.GiveItem(RoR2Content.Items.Ghost);
                    if (inventory.GetItemCount(RoR2Content.Items.UseAmbientLevel) <= 0) inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);

                    MasterSuicideOnTimer mst = characterMaster2.gameObject.AddComponent<MasterSuicideOnTimer>();

                    if (ownerBody && ownerBody.teamComponent && ownerBody.teamComponent.teamIndex == TeamIndex.Player)
                    {
                        inventory.GiveItem(RoR2Content.Items.BoostDamage.itemIndex, boostDamageItemCount);
                        //inventory.GiveItem(RoR2Content.Items.HealthDecay.itemIndex, ghostDurationSecondsPlayer);
                        if (ModSupport.ModCompatRiskyMod.loaded)
                        {
                            Modules.ModSupport.ModCompatRiskyMod.GiveAllyItem(inventory);
                        }
                        mst.lifeTimer = ghostDurationSecondsPlayer;
                    }
                    else //Handle enemy-spawned ghosts
                    {
                        inventory.GiveItem(RoR2Content.Items.HealthDecay.itemIndex, ghostDurationSecondsEnemy);
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
                Util.PlaySound("Play_elite_haunt_ghost_convert", body.gameObject);

                if (body.mainHurtBox)
                {
                    CreateGhostOrbEffect(targetBody, body, 0.5f);
                }
            }
            return body;
        }

        private static void CreateGhostOrbEffect(CharacterBody targetBody, CharacterBody originBody, float duration)
        {
            EffectData effectData = new EffectData
            {
                scale = 0.5f,
                origin = targetBody.corePosition,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(originBody.mainHurtBox);
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/HauntOrbEffect"), effectData, true);
        }
    }
}