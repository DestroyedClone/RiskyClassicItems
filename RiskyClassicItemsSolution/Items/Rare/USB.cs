using BepInEx.Configuration;
using ClassicItemsReturns.Modules;
using IL.RoR2.EntityLogic;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;

//Entire code needs a refactor, too many static variables used to keep track of cannon state, with inconsistent usages of them.
//VFX being hardcoded isn't good either.
namespace ClassicItemsReturns.Items.Rare
{
    public class USB : ItemBase<USB>
    {
        public override string ItemName => "Classified Access Codes";

        public override string ItemLangTokenName => "USB";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("USB");

        public override Sprite ItemIcon => LoadItemSprite("USB");

        public static GameObject atlasCannonNetworkPrefab;
        public static GameObject teleporterVisualNetworkPrefab;
        public static GameObject atlasCannonInteractablePrefab;
        public static InteractableSpawnCard atlasCannonSpawnCard;
        public static NetworkSoundEventDef atlasCannonFireSound;

        //By default, the cannon attempts to trigger on any Scripted Combat Encounter if the stage has no teleporter.
        public static HashSet<string> atlasCannonScriptedCombatEncounterSceneBlacklist = new HashSet<string>
        {
            "goldshores",
            "limbo"
        };

        //Forces random placement of the interactable even if no Teleporter Interaction exists
        public static HashSet<string> atlasCannonInteractableSceneForceRandomPlacement = new HashSet<string>
        {
            "goldshores",
            "moon",
            "moon2"
        };

        public static Dictionary<string, Vector3> atlasCannonInteractableManualPlacementDict = new Dictionary<string, Vector3>
        {
            {"voidraid" , new Vector3(0f, 217f, -443f)},
            { "meridian" , new Vector3(78f, 35f, -94f) }
        };

        public static bool cannonSpawned = false;
        public static bool cannonActivated = false;
        private static bool addedTeleporterVisual = false;
        
        //This is used for special stages that don't have a Teleporter interaction, to prevent multi-fires.
        //Teleporter sets this, but ignores this when firing.
        //Also used to control whether the interactable should show its beam or not.
        public static bool firedCannon = false;

        private static GameObject teleporterVisualNetworkInstance;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.AIBlacklist,
            ItemTag.Damage
        };

        public override void Hooks()
        {
            RoR2.Stage.onStageStartGlobal += Stage_onStageStartGlobal;

            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;

            On.RoR2.TeleporterInteraction.Start += TeleporterInteraction_Start;

            On.EntityStates.Missions.Goldshores.GoldshoresBossfight.SetBossImmunity += GoldshoresBossfight_SetBossImmunity;
            On.RoR2.ScriptedCombatEncounter.Spawn += ScriptedCombatEncounter_Spawn;
            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }

        public static float CalcDamagePercent(int itemCount)
        {
            itemCount = Mathf.Max(itemCount, 1);
            return 1f - (0.6f * Mathf.Pow(0.8f, itemCount - 1));
        }

        public override void CreateAssets(ConfigFile config)
        {
            atlasCannonFireSound = Modules.Assets.CreateNetworkSoundEventDef("Play_captain_utility_variant_impact");

            atlasCannonNetworkPrefab = Modules.Assets.LoadObject("AtlasCannonTarget");
            atlasCannonNetworkPrefab.AddComponent<RoR2.Billboard>();
            atlasCannonNetworkPrefab.AddComponent<NetworkIdentity>();
            var controller = atlasCannonNetworkPrefab.AddComponent<AtlasCannonController>();
            atlasCannonNetworkPrefab.AddComponent<DestroyOnTimer>().duration = controller.delayBeforeFiring + controller.lifetimeAfterFiring + 2f;
            PrefabAPI.RegisterNetworkPrefab(atlasCannonNetworkPrefab);
            //ContentAddition.AddNetworkedObject(atlasCannonNetworkPrefab);

            teleporterVisualNetworkPrefab = Modules.Assets.LoadObject("AtlasCannonTeleporterVisual");
            teleporterVisualNetworkPrefab.AddComponent<NetworkIdentity>();
            teleporterVisualNetworkPrefab.AddComponent<AtlasTeleporterBeamController>();
            PrefabAPI.RegisterNetworkPrefab(teleporterVisualNetworkPrefab);
            //ContentAddition.AddNetworkedObject(teleporterVisualNetworkPrefab);

            atlasCannonInteractablePrefab = Modules.Assets.LoadObject("AtlasCannonInteractable");

            //Debug.Log("Interactable: " + LayerIndex.CommonMasks.interactable.value);
            //atlasCannonInteractablePrefab.layer = LayerIndex.CommonMasks.interactable;

            atlasCannonInteractablePrefab.AddComponent<NetworkIdentity>();
            ChildLocator cl = atlasCannonInteractablePrefab.GetComponent<ChildLocator>();
            Transform modelTransform = cl.FindChild("Model");

            Highlight highlight = atlasCannonInteractablePrefab.AddComponent<Highlight>();
            highlight.targetRenderer = modelTransform.GetComponent<MeshRenderer>();
            highlight.strength = 1f;
            highlight.highlightColor = Highlight.HighlightColor.interactive;
            highlight.isOn = false;

            PurchaseInteraction pi = atlasCannonInteractablePrefab.AddComponent<PurchaseInteraction>();
            pi.cost = 0;
            pi.costType = CostTypeIndex.None;
            pi.displayNameToken = "CLASSICITEMSRETURNS_INTERACTABLE_ATLASCANNON_NAME";
            pi.contextToken = "CLASSICITEMSRETURNS_INTERACTABLE_ATLASCANNON_CONTEXT";
            pi.setUnavailableOnTeleporterActivated = true;
            pi.isShrine = true; //affects icon
            pi.isGoldShrine = false;
            pi.ignoreSpherecastForInteractability = false;
            pi.available = true;

            atlasCannonInteractablePrefab.AddComponent<AtlasCannonInteractableController>();

            EntityLocator el = atlasCannonInteractablePrefab.AddComponent<EntityLocator>();
            el.entity = atlasCannonInteractablePrefab;

            PrefabAPI.RegisterNetworkPrefab(atlasCannonInteractablePrefab);
            //ContentAddition.AddNetworkedObject(atlasCannonInteractablePrefab);

            atlasCannonSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            atlasCannonSpawnCard.maxSpawnsPerStage = 1;
            atlasCannonSpawnCard.occupyPosition = true;
            atlasCannonSpawnCard.prefab = atlasCannonInteractablePrefab;
            atlasCannonSpawnCard.slightlyRandomizeOrientation = false;
            atlasCannonSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            atlasCannonSpawnCard.orientToFloor = false; //Laser is always straight up.
            atlasCannonSpawnCard.hullSize = HullClassification.Human;
            atlasCannonSpawnCard.sendOverNetwork = false;
        }

        public static void PlaceAtlasCannonInteractable(Xoroshiro128Plus rng)
        {
            if (!NetworkServer.active || cannonSpawned) return;
            if (Util.GetItemCountForTeam(TeamIndex.Player, USB.Instance.ItemDef.itemIndex, false, true) <= 0) return;

            GameObject result = null;
            DirectorPlacementRule placementRule = null;

            bool shouldRandomPlace = false;
            SceneDef currentScene = SceneCatalog.GetSceneDefForCurrentScene();
            shouldRandomPlace = TeleporterInteraction.instance
                    || (currentScene && atlasCannonInteractableSceneForceRandomPlacement.Contains(currentScene.cachedName));
            if (shouldRandomPlace)
            {
                placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
            }
            else if (currentScene && atlasCannonInteractableManualPlacementDict.ContainsKey(currentScene.cachedName))
            {
                Vector3 position;
                atlasCannonInteractableManualPlacementDict.TryGetValue(currentScene.cachedName, out position);

                placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    position = position
                };
            }

            if (placementRule != null)
            {
                result = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(atlasCannonSpawnCard, placementRule, rng));
            }

            if (result)
            {
                Debug.Log("ClassicItemsReturns: Placed Atlas Cannon interactable.");
            }
            else
            {
                Debug.LogError("ClassicItemsReturns: Failed to place Atlas Cannon interactable.");
            }

            if (!cannonSpawned) cannonSpawned = result != null;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active && !cannonSpawned && self.inventory.GetItemCount(ItemDef) > 0)
            {
                if (Run.instance) PlaceAtlasCannonInteractable(new Xoroshiro128Plus(Run.instance.seed));
            }
        }

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);
            cannonSpawned = false;  //TODO: CLEAN UP
            PlaceAtlasCannonInteractable(self.rng);
        }

        private void ScriptedCombatEncounter_Spawn(On.RoR2.ScriptedCombatEncounter.orig_Spawn orig, ScriptedCombatEncounter self, ref ScriptedCombatEncounter.SpawnInfo spawnInfo)
        {
            orig(self, ref spawnInfo);

            SceneDef currentScene = SceneCatalog.GetSceneDefForCurrentScene();
            if (currentScene
                && !TeleporterInteraction.instance
                && !atlasCannonScriptedCombatEncounterSceneBlacklist.Contains(currentScene.cachedName))
            {
                //Jank, firedCannon check is at the end so that the interactable gets disabled if you don't activate it in time.
                if (cannonActivated && !firedCannon)
                {
                    foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList)
                    {
                        TargetCannon(master);
                    }
                }
                firedCannon = true;
            }
        }

        private void GoldshoresBossfight_SetBossImmunity(On.EntityStates.Missions.Goldshores.GoldshoresBossfight.orig_SetBossImmunity orig, EntityStates.Missions.Goldshores.GoldshoresBossfight self, bool newBossImmunity)
        {
            orig(self, newBossImmunity);

            if (NetworkServer.active
                && !newBossImmunity
                && !firedCannon
                && cannonActivated) //Redundant, kept here due to firedCannon check. Probably can be refactored.
            {
                firedCannon = true;
                foreach (CharacterMaster master in self.scriptedCombatEncounter.combatSquad.readOnlyMembersList)
                {
                    TargetCannonIgnoreBossCheck(master);
                }
            }
        }

        private void TeleporterInteraction_Start(On.RoR2.TeleporterInteraction.orig_Start orig, TeleporterInteraction self)
        {
            orig(self);
            if (NetworkServer.active && self.bossGroup && self.bossGroup.combatSquad)
            {
                self.bossGroup.combatSquad.onMemberAddedServer += TargetCannonIgnoreBossCheck;
            }
        }

        private void TargetCannon(CharacterMaster master)
        {
            if (!NetworkServer.active || !cannonActivated) return;
            CharacterBody body = master.GetBody();
            if (body
                && (body.isChampion || body.isBoss)
                && body.healthComponent
                && body.teamComponent
                && TeamMask.GetEnemyTeams(TeamIndex.Player).HasTeam(body.teamComponent.teamIndex))
            {
                GameObject cannonObject = UnityEngine.Object.Instantiate(atlasCannonNetworkPrefab, body.transform);
                AtlasCannonController controller = cannonObject.GetComponent<AtlasCannonController>();
                if (controller)
                {
                    controller.targetHealthComponent = body.healthComponent;
                }
                NetworkServer.Spawn(cannonObject);
            }
        }

        //This triggers before TeleporterInteraction sets the enemy as a boss, which is why this is needed.
        private void TargetCannonIgnoreBossCheck(CharacterMaster master)
        {
            if (!NetworkServer.active || !cannonActivated) return;
            CharacterBody body = master.GetBody();
            if (body
                && body.healthComponent
                && body.teamComponent
                && TeamMask.GetEnemyTeams(TeamIndex.Player).HasTeam(body.teamComponent.teamIndex))
            {
                GameObject cannonObject = UnityEngine.Object.Instantiate(atlasCannonNetworkPrefab, body.transform);
                AtlasCannonController controller = cannonObject.GetComponent<AtlasCannonController>();
                if (controller)
                {
                    controller.targetHealthComponent = body.healthComponent;
                }
                NetworkServer.Spawn(cannonObject);
            }
            firedCannon = true;
        }

        public static void AddTeleporterVisualServer()
        {
            if (!NetworkServer.active) return;

            if (!TeleporterInteraction.instance
                || TeleporterInteraction.instance.currentState.GetType() != typeof(TeleporterInteraction.IdleState)) return;

            if (teleporterVisualNetworkInstance)
            {
                UnityEngine.Object.Destroy(teleporterVisualNetworkInstance);
                teleporterVisualNetworkInstance = null;
            }

            //Component on this will resolve the positioning clientside.
            teleporterVisualNetworkInstance = GameObject.Instantiate(teleporterVisualNetworkPrefab);
            var controller = teleporterVisualNetworkInstance.GetComponent<AtlasTeleporterBeamController>();
            if (controller)
            {
                controller.SetPosServer(TeleporterInteraction.instance.transform.position);
            }
            NetworkServer.Spawn(teleporterVisualNetworkInstance);
    }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            orig(self);
            cannonSpawned = true;   //Prevent cannon from spawning after TP activates
            if (NetworkServer.active && teleporterVisualNetworkInstance)
            {
                UnityEngine.Object.Destroy(teleporterVisualNetworkInstance);
                teleporterVisualNetworkInstance = null;
            }
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            cannonActivated = false;
            addedTeleporterVisual = false;
            firedCannon = false;
        }
    }

    public class AtlasCannonController : NetworkBehaviour
    {
        public float delayBeforeFiring = 5f;
        public float lifetimeAfterFiring = 1f;
        public HealthComponent targetHealthComponent;
        public float laserFireWidth = 15f;

        [SyncVar]
        private bool _hasFired = false;
        private bool hasFiredLocal = false;

        private LineRenderer laser;
        private SpriteRenderer crosshairRenderer, rotatorRenderer;
        private Transform rotatorTransform;

        private float stopwatch;
        private float rotationStopwatch;
        private float laserFadeStopwatch;

        private void Awake()
        {
            ChildLocator cl = base.GetComponent<ChildLocator>();
            if (cl)
            {
                Transform crosshairTransform = cl.FindChild("Crosshair");
                if (crosshairTransform)
                {
                    crosshairRenderer = crosshairTransform.GetComponent<SpriteRenderer>();
                }

                rotatorTransform = cl.FindChild("Rotator");
                if (rotatorTransform)
                {
                    rotatorRenderer = rotatorTransform.GetComponent<SpriteRenderer>();
                }

                Transform laserTransform = cl.FindChild("Laser");
                if(laserTransform)
                {
                    laser = laserTransform.GetComponent<LineRenderer>();
                }
            }
            stopwatch = 0f;
            rotationStopwatch = 0f;
            laserFadeStopwatch = 0f;
        }

        private void FixedUpdate()
        {
            //Hide targeting indicator after laser has been fired
            if (_hasFired && !hasFiredLocal)
            {
                hasFiredLocal = true;
                if (crosshairRenderer) crosshairRenderer.enabled = false;
                if (rotatorRenderer) rotatorRenderer.enabled = false;
                laser.widthMultiplier = laserFireWidth;
                stopwatch = 0f;
            }

            if (NetworkServer.active) FixedUpdateServer();
        }

        private void FixedUpdateServer()
        {
            if (!targetHealthComponent || !targetHealthComponent.alive)
            {
                Destroy(base.gameObject);
                return;
            }

            stopwatch += Time.fixedDeltaTime;
            if (!_hasFired)
            {
                if (stopwatch >= delayBeforeFiring)
                {
                    FireCannonServer();
                }
            }
            else if (hasFiredLocal)
            {
                //Wait for hasFiredLocal since some VFX rely on that
                if (stopwatch >= lifetimeAfterFiring)
                {
                    Destroy(base.gameObject);
                }
            }
        }

        private void FireCannonServer()
        {
            if (!NetworkServer.active) return;
            _hasFired = true;

            if (targetHealthComponent)
            {
                int itemCount = Mathf.Max(1, Util.GetItemCountForTeam(TeamIndex.Player, USB.Instance.ItemDef.itemIndex, false, true));
                float damage = targetHealthComponent.fullCombinedHealth * USB.CalcDamagePercent(itemCount);
                Vector3 damagePosition = targetHealthComponent.body ? targetHealthComponent.body.corePosition : base.transform.position;
                targetHealthComponent.TakeDamage(new DamageInfo()
                {
                    damage = damage,
                    damageType = DamageType.BypassArmor | DamageType.BypassBlock,
                    attacker = null,
                    canRejectForce = true,
                    force = Vector3.zero,
                    crit = false,
                    damageColorIndex = DamageColorIndex.Item,
                    dotIndex = DotController.DotIndex.None,
                    inflictor = base.gameObject,
                    position = damagePosition,
                    procChainMask = default,
                    procCoefficient = 0f
                });
                EffectManager.SimpleSoundEffect(USB.atlasCannonFireSound.index, damagePosition, true);
            }
        }

        //Controls how fast the center thing rotates
        private void Update()
        {
            if (laser)
            {
                laser.SetPositions(new Vector3[]
                {
                    base.transform.position + Vector3.up * 1000f,
                    base.transform.position + Vector3.down * 1000f,
                });
            }

            if (!hasFiredLocal)
            {
                if (rotatorTransform)
                {
                    rotationStopwatch += Time.deltaTime;
                    if (rotationStopwatch >= 2f) rotationStopwatch -= 2f;
                    rotatorTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 360f * rotationStopwatch));
                }
            }
            else
            {
                laserFadeStopwatch += Time.deltaTime;
                float laserFadePercent = 1f - (laserFadeStopwatch / lifetimeAfterFiring);
                laser.widthMultiplier = laserFireWidth * laserFadePercent;
            }
        }
    }

    public class AtlasTeleporterBeamController : NetworkBehaviour
    {
        private LineRenderer lineRenderer;

        [SyncVar]
        private Vector3 _serverPos;

        private Vector3 localPos;

        public void SetPosServer(Vector3 pos)
        {
            if (!NetworkServer.active) return;
            _serverPos = pos;
        }

        private void Awake()
        {
            lineRenderer = base.GetComponent<LineRenderer>();
            if (lineRenderer)
            {
                localPos = base.transform.position;
                lineRenderer.SetPositions(new Vector3[]
                {
                localPos,
                localPos + 1000f * Vector3.up
                });
            }
        }

        private void FixedUpdate()
        {
            if (localPos != _serverPos)
            {
                localPos = _serverPos;
                if (lineRenderer)
                {
                    lineRenderer.SetPositions(new Vector3[]
                    {
                        localPos,
                        localPos + 1000f * Vector3.up
                    });
                }
            }
        }

        private void Update()
        {
            if (!lineRenderer || (expandDuration <= 0f && retractDuration <= 0f)) return;

            expandStopwatch += Time.deltaTime;
            if (!isRetracting)
            {
                lineRenderer.widthMultiplier = Mathf.Lerp(1f, endWidthMult, expandStopwatch / expandDuration);

                if (expandDuration > 0f && expandStopwatch >= expandDuration)
                {
                    expandDuration = 0f;
                    expandStopwatch = 0f;
                    isRetracting = true;
                }
            }
            else
            {
                lineRenderer.widthMultiplier = Mathf.Lerp(endWidthMult, 0f, expandStopwatch / retractDuration);
                if (expandStopwatch >= retractDuration)
                {
                    retractDuration = 0f;
                    expandStopwatch = 0f;
                    isRetracting = false;

                    if (NetworkServer.active)
                    {
                        EffectManager.SimpleSoundEffect(USB.atlasCannonFireSound.index, _serverPos, true);
                        Destroy(base.gameObject);
                    }
                }
            }
        }

        //These are used to make the beam expand/retract.
        private float endWidthMult = 5f;
        private float expandDuration = 0f;
        private float retractDuration = 0f;
        private float expandStopwatch = 0f;
        private bool isRetracting = false;
        private void TriggerBeamAnim()
        {
            expandDuration = 5f;
            retractDuration = 1f;
            isRetracting = false;
            expandStopwatch = 0f;
            endWidthMult = 5f;
        }

        [ClientRpc]
        private void RpcTriggerBeamAnim()
        {
            TriggerBeamAnim();
        }

        public void TriggerBeamAnimServer()
        {
            if (NetworkServer.active) RpcTriggerBeamAnim();
        }
    }

    public class AtlasCannonInteractableController : MonoBehaviour
    {
        private GameObject beamInstance;
        private PurchaseInteraction pi;

        private bool expandedBeam = false;

        private void Awake()
        {
            pi = base.GetComponent<PurchaseInteraction>();
            if (pi)
            {
                pi.onPurchase.AddListener(new UnityAction<Interactor>(AtlasCannonOnPurchase));
            }
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active && pi && USB.firedCannon)
            {
                pi.available = false;
                if (beamInstance && !expandedBeam)
                {
                    expandedBeam = true;
                    var controller = beamInstance.GetComponent<AtlasTeleporterBeamController>();
                    if (controller) controller.TriggerBeamAnimServer();
                }
            }
        }

        private void OnDestroy()
        {
            if (NetworkServer.active && beamInstance) UnityEngine.Object.Destroy(beamInstance);
        }

        private void AtlasCannonOnPurchase(Interactor interactor)
        {
            if (USB.cannonActivated) return;

            USB.cannonActivated = true;
            USB.AddTeleporterVisualServer();

            if (pi)
            {
                pi.lastActivator = interactor;
                pi.available = false;
            }

            Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
            {
                subjectAsCharacterBody = interactor.GetComponent<CharacterBody>(),
                baseToken = "CLASSICITEMSRETURNS_INTERACTABLE_ATLASCANNON_USE_MESSAGE"
            });

            if (!beamInstance && NetworkServer.active)
            {
                beamInstance = GameObject.Instantiate(USB.teleporterVisualNetworkPrefab);
                var controller = beamInstance.GetComponent<AtlasTeleporterBeamController>();
                if (controller)
                {
                    Vector3 beamOffset = base.transform.position + 6f * Vector3.up;
                    ChildLocator cl = base.GetComponent<ChildLocator>();
                    if (cl)
                    {
                        Transform beamOrigin = cl.FindChild("BeamOrigin");
                        beamOffset = beamOrigin.position;
                    }
                    controller.SetPosServer(beamOffset);
                }
                NetworkServer.Spawn(beamInstance);
            }
        }
    }
}
