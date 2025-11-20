using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using ClassicItemsReturns.Utils;

namespace ClassicItemsReturns.Equipment
{
    internal class CreateGhostTargeting : EquipmentBase<CreateGhostTargeting>
    {
        public override string EquipmentName => "Jar of Souls";

        public override string EquipmentLangTokenName => "CREATEGHOSTTARGETING";
        public const int ghostDurationSecondsPlayer = 25;
        public const int ghostDurationSecondsEnemy = 10;
        public const int boostDamageItemCount = 10;
        public const int boostDamageChampionItemCount = 20;
        
        public int ghostsPerCommon = 3;
        public int ghostsPerChampion = 1;
        public int maxGhosts = 6;

        public HashSet<BodyIndex> bodyBlacklist = new HashSet<BodyIndex>();
        public HashSet<BodyIndex> countAsBoss = new HashSet<BodyIndex>();

        public static ConfigEntry<bool> blacklistSS2UNemesis;
        public static ConfigEntry<bool> drainHP;
        public static ConfigEntry<string> bodyBlacklistString;

        public static NetworkSoundEventDef activationSound;

        public override bool EnigmaCompatible { get; } = true;
        public override bool CanBeRandomlyTriggered { get; } = false;

        public override object[] EquipmentFullDescriptionParams => new object[] {
            (boostDamageItemCount * 10),
            ghostDurationSecondsPlayer
        };

        public override GameObject EquipmentModel => LoadItemModel("JarSouls");

        public override Sprite EquipmentIcon => LoadItemSprite("JarSouls");

        public override TargetFinderType EquipmentTargetFinderType => TargetFinderType.Enemies;

        public override float Cooldown => 100;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateTargetingIndicator();
            CreateEquipment();
            Hooks();

            RoR2.RoR2Application.onLoad += GetBodyIndex;

            activationSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            activationSound.eventName = "Play_ClassicItemsReturns_JarSouls";
            PluginContentPack.networkSoundEventDefs.Add(activationSound);
        }

        private void GetBodyIndex()
        {
            string[] bodyNames = bodyBlacklistString.Value.Split(',');
            foreach (string str in bodyNames)
            {
                BodyIndex index = BodyCatalog.FindBodyIndex(str.Trim());
                if (index != BodyIndex.None) bodyBlacklist.Add(index);
            }
        }

        public static GameObject commonTargetIndicator;
        public static GameObject championTargetIndicator;

        /// <summary>
        /// An example targeting indicator implementation. We clone the woodsprite's indicator, but we edit it to our liking. We'll use this in our activate equipment.
        /// We shouldn't need to network this as this only shows for the player with the Equipment.
        /// </summary>
        private void CreateTargetingIndicator()
        {
            /*commonTargetIndicator = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator"), Modules.Assets.prefabPrefix + "SoulJarCommonIndicator", false);
            commonTargetIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
            commonTargetIndicator.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            commonTargetIndicator.GetComponentInChildren<SpriteRenderer>().GetComponent<RotateAroundAxis>().enabled = false;
            commonTargetIndicator.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(0.423f, 1, 0.749f);

            championTargetIndicator = PrefabAPI.InstantiateClone(commonTargetIndicator, Modules.Assets.prefabPrefix + "SoulJarChampionIndicator", false);
            commonTargetIndicator.GetComponentInChildren<SpriteRenderer>().sprite = Modules.Assets.LoadSprite("texJarOfSoulsTargetIndicator.png");
            championTargetIndicator.GetComponentInChildren<SpriteRenderer>().sprite = Modules.Assets.LoadSprite("texJarOfSoulsChampionTargetIndicator.png");*/
            commonTargetIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BossHunter/BossHunterIndicator.prefab").WaitForCompletion();
            championTargetIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BossHunter/BossHunterIndicator.prefab").WaitForCompletion();
        }

        public override void Hooks()
        {
            base.Hooks();
            On.EntityStates.Gup.BaseSplitDeath.OnEnter += FixGhostGupSplit;
        }

        private void FixGhostGupSplit(On.EntityStates.Gup.BaseSplitDeath.orig_OnEnter orig, EntityStates.Gup.BaseSplitDeath self)
        {
            orig(self);
            if (NetworkServer.active && self.GetComponent<DontSplitOnDeath>())
            {
                self.hasDied = true;
                self.DestroyBodyAsapServer();
            }
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

        public override void FilterTargetFinderHurtbox(EquipmentSlot slot, BullseyeSearch targetFinder)
        {
            base.FilterTargetFinderHurtbox(slot, targetFinder);
            if (slot.equipmentIndex == EquipmentDef.equipmentIndex)
            {
                targetFinder.candidatesEnumerable =
                    (from v in targetFinder.candidatesEnumerable
                    where v.hurtBox.healthComponent.body
                    && !v.hurtBox.healthComponent.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless)
                    && v.hurtBox.healthComponent.body.master
                    && !bodyBlacklist.Contains(v.hurtBox.healthComponent.body.bodyIndex)
                    && !HasNemesisItem(v.hurtBox.healthComponent.body.inventory)
                    select v).ToList();
            }
        }

        protected override void CreateConfig(ConfigFile config)
        {
            int maxUses = config.Bind(ConfigCategory, "Max Uses", 2, "Max amount of active uses (each use spawns 3 normal enemies or 1 boss).").Value;
            drainHP = config.Bind(ConfigCategory, "Drain HP", true, "Ghosts drain HP over their lifetime. Mainly used so that boss ghosts properly trigger their attacks that require certain HP thresholds.");
            blacklistSS2UNemesis = config.Bind(ConfigCategory, "Blacklist SS2U Nemesis Bosses", true, "Jar of Souls cannot target Nemesis Bosses from SS2U.");
            bodyBlacklistString = config.Bind(ConfigCategory, "Body Blacklist", "ShopkeeperBody, VoidInfestorBody, WispSoulBody, UrchinTurretBody, MinorConstructAttachableBody, VoidRaidCrabBody, MiniVoidRaidCrabBodyPhase1, MiniVoidRaidCrabBodyPhase2, MiniVoidRaidCrabBodyPhase3", "Prevents the listed enemies from being targeted. Case-sensitive, use DebugToolKit list_bodies command to see full bodylist.");

            maxGhosts = maxUses * ghostsPerCommon;
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
                    localPos = new Vector3(0F, 0F, 2.22F),
                    localAngles = new Vector3(45F, 90F, 270F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });

            return dict;
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

            int ghostsToSpawn = (hurtBox.healthComponent.body.isChampion|| countAsBoss.Contains(hurtBox.healthComponent.body.bodyIndex)) ? ghostsPerChampion : ghostsPerCommon;
            if ((slot.teamComponent && slot.teamComponent.teamIndex != TeamIndex.Player) && !(slot.characterBody && slot.characterBody.isPlayerControlled)) ghostsToSpawn = 1;

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
                EffectManager.SimpleSoundEffect(activationSound.index, slot.characterBody.corePosition, true);
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
            CharacterDirection component = targetBody.GetComponent<CharacterDirection>();
            masterSummon.rotation = (component ? Quaternion.Euler(0f, component.yaw, 0f) : targetBody.transform.rotation);
            masterSummon.summonerBodyObject = (ownerBody ? ownerBody.gameObject : null);
            masterSummon.inventoryToCopy = targetBody.inventory;
            masterSummon.useAmbientLevel = null;

            //Try to find a safe spawn position
            HullClassification hc = targetBody.isChampion ? HullClassification.BeetleQueen : HullClassification.Golem;
            masterSummon.position = MiscUtils.FindSafeTeleportPosition(targetBody.corePosition, hc, targetBody.isFlying, 30f, DirectorPlacementRule.PlacementMode.Approximate);

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
                        inventory.GiveItem(RoR2Content.Items.BoostDamage.itemIndex, ownerBody.isChampion ? boostDamageChampionItemCount : boostDamageItemCount);
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


                    if (drainHP.Value)
                    {
                        int drainHpCount = inventory.GetItemCount(RoR2Content.Items.HealthDecay);
                        if (drainHpCount > 0) inventory.RemoveItem(RoR2Content.Items.HealthDecay, drainHpCount);
                        inventory.GiveItem(RoR2Content.Items.HealthDecay, Mathf.CeilToInt(mst.lifeTimer));
                    }
                }
            }
            CharacterBody body = characterMaster2.GetBody();
            if (body)
            {
                if (drainHP.Value) body.gameObject.AddComponent<FixHealth>();
                foreach (EntityStateMachine entityStateMachine in body.GetComponents<EntityStateMachine>())
                {
                    entityStateMachine.initialStateType = entityStateMachine.mainStateType;
                }
                Util.PlaySound("Play_elite_haunt_ghost_convert", body.gameObject);

                if (body.mainHurtBox)
                {
                    CreateGhostOrbEffect(targetBody, body, 0.5f);
                }

                if (body.bodyIndex == BodyCatalog.FindBodyIndex("GupBody") || body.bodyIndex == BodyCatalog.FindBodyIndex("GeepBody") || body.bodyIndex == BodyCatalog.FindBodyIndex("GipBody"))
                {
                    body.gameObject.AddComponent<DontSplitOnDeath>();
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

        private bool HasNemesisItem(Inventory inventory)
        {
            if (inventory)
            {
                ItemDef nem = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SS2UNemesisMarkerItem"));
                if (nem && inventory.GetItemCount(nem) > 0) return true;
            }
            return false;
        }
    }

    public class DontSplitOnDeath : MonoBehaviour { }
    public class FixHealth : MonoBehaviour
    {
        private void Start()
        {
            if (NetworkServer.active)
            {
                HealthComponent hc = base.GetComponent<HealthComponent>();
                if (hc) hc.Networkhealth = hc.fullHealth;
            }
            Destroy(this);
        }
    }
}