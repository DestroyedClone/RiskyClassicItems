using R2API;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using RoR2;
using RoR2.Items;
using RoR2.Orbs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using ClassicItemsReturns.Items.Uncommon;

namespace ClassicItemsReturns.Items.NoTier
{
    internal class ArmsRaceDroneItem : ItemBase<ArmsRaceDroneItem>
    {
        public override string ItemName => "ARMSRACEDRONEITEM";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => LoadItemModel("ArmsRaceMissiles");

        public override Sprite ItemIcon => Assets.NullSprite;

        public override string ItemLangTokenName => "ARMSRACEDRONEITEM";

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var dict = new ItemDisplayRuleDict();

            //Don't set up displays if 3d model isn't available
            GameObject display = ItemModel;
            if (!display.name.Contains("mdl3d")) return dict;

            dict.Add("EquipmentDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "GunBarrelBase",
                    localPos = new Vector3(0.6F, 0F, 0F),
                    localAngles = new Vector3(90F, 180F, 0F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "GunBarrelBase",
                    localPos = new Vector3(-0.6F, 0F, 0F),
                    localAngles = new Vector3(90F, 180F, 0F),
                    localScale = new Vector3(-0.25F, 0.25F, 0.25F)
                }
            });

            dict.Add("Drone1Body", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.5F, 0F, 0F),
                    localAngles = new Vector3(0F, 0F, 180F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.5F, 0F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.5F, -0.5F, 0.5F)
                }
            });

            dict.Add("Drone2Body", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.5F, 0F, 0F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.5F, 0F, 0F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(-0.5F, 0.5F, 0.5F)
                }
            });

            dict.Add("BackupDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.5F, 0.35F, 0.2F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.5F, 0.35F, 0.2F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(-0.25F, 0.25F, 0.25F)
                }
            });

            dict.Add("DroneCommanderBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.2F, -0.07F, 0F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.125F, 0.125F, 0.125F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.2F, -0.07F, 0F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(-0.125F, 0.125F, 0.125F)
                }
            });

            dict.Add("EmergencyDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "TopRing",
                    localPos = new Vector3(2.7F, 2.57F, 0F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "TopRing",
                    localPos = new Vector3(-2.7F, 2.57F, 0F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(-0.5F, 0.5F, 0.5F)
                }
            });

            dict.Add("FlameDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.5F, -0.25F, 0F),
                    localAngles = new Vector3(0F, 0F, 180F),
                    localScale = new Vector3(-0.35F, 0.35F, 0.35F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.5F, -0.25F, 0F),
                    localAngles = new Vector3(0F, 0F, 180F),
                    localScale = new Vector3(0.35F, 0.35F, 0.35F)
                }
            });

            dict.Add("MegaDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Base",
                    localPos = new Vector3(2.7F, 0.72F, 0.2F),
                    localAngles = new Vector3(90F, 180F, 0F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Base",
                    localPos = new Vector3(-2.7F, 0.72F, 0.2F),
                    localAngles = new Vector3(90F, 180F, 0F),
                    localScale = new Vector3(-0.4F, 0.4F, 0.4F)
                }
            });

            dict.Add("MissileDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Body",
                    localPos = new Vector3(0.6F, 0.5F, 0F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.35F, 0.35F, 0.35F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Body",
                    localPos = new Vector3(-0.6F, 0.5F, 0F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(-0.35F, 0.35F, 0.35F)
                }
            });

            dict.Add("Turret1Body", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-1.7F, 0F, 0F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(1.7F, 0F, 0F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(-1F, 1F, 1F)
                }
            });

            dict.Add("EngiTurretBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.6F, 0.42F, -0.66F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.6F, 0.42F, -0.66F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(-0.25F, 0.25F, 0.25F)
                }
            });

            dict.Add("EngiBeamTurretBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.6F, 0.42F, -0.66F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.6F, 0.42F, -0.66F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(-0.25F, 0.25F, 0.25F)
                }
            });

            dict.Add("EngiWalkerTurretBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.5F, 0.8F, -1F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                },
                new ItemDisplayRule
                {
                    followerPrefab = display,
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.5F, 0.8F, -1F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(-0.25F, 0.25F, 0.25F)
                }
            });

            return dict;
        }
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.CannotSteal,
            ItemTag.CannotCopy,
            ItemTag.Damage
        };


        //https://github.com/Moffein/RiskyMod/blob/master/RiskyMod/Allies/DroneBehaviors/AutoMissileBehavior.cs#L10
        public class ArmsRaceDroneBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => Instance.ItemDef;

            public static float searchInterval = 1f;
            public static float maxActivationDistance = 80f;
            public static int missilesPerBarrage = 4;

            public static float baseFireInterval = 0.15f;

            public float searchStopwatch;
            public float cooldownStopwatch;
            public float fireStopwatch;
            public int missilesLoaded;
            public bool firingBarrage = false;
            public float fireInterval;
            public HurtBox targetHurtBox;

            public float cooldownInterval => ArmsRace.Instance.cooldown;

            public float failedTargetCooldown = 2f;
            public float timer = 2f;

            private Transform missileMuzzleTransform;

            private void Start()
            {
                ModelLocator component = GetComponent<ModelLocator>();
                if (component)
                {
                    Transform modelTransform = component.modelTransform;
                    if (modelTransform)
                    {
                        CharacterModel component2 = modelTransform.GetComponent<CharacterModel>();
                        if (component2)
                        {
                            List<GameObject> itemDisplayObjects = component2.GetItemDisplayObjects(DLC1Content.Items.DroneWeaponsDisplay1.itemIndex);
                            itemDisplayObjects.AddRange(component2.GetItemDisplayObjects(DLC1Content.Items.DroneWeaponsDisplay2.itemIndex));
                            foreach (GameObject gameObject in itemDisplayObjects)
                            {
                                ChildLocator component3 = gameObject.GetComponent<ChildLocator>();
                                if (component3)
                                {
                                    Transform exists = component3.FindChild("MissileMuzzle");
                                    if (exists)
                                    {
                                        missileMuzzleTransform = exists;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                GetMissileBarrageCount();
                missilesLoaded = missilesPerBarrage;
                searchStopwatch = 0f;
                cooldownStopwatch = Random.Range(0.1f, 0.75f);
                fireStopwatch = 0f;
                fireInterval = baseFireInterval;
            }

            private void GetMissileBarrageCount()
            {
                if (body && body.master && body.master.minionOwnership && body.master.minionOwnership.ownerMaster && ArmsRace.Instance.TryGetCount(body.master.minionOwnership.ownerMaster, out int ownerArmsRaceItemCount))
                {
                    if (ownerArmsRaceItemCount > 0)
                    {
                        missilesPerBarrage = ItemHelpers.StackingLinear(ownerArmsRaceItemCount, ArmsRace.Instance.missileCount, ArmsRace.Instance.missileCountPerStack);
                    }
                    else
                    {
                        missilesPerBarrage = 0;
                    }
                }
                else
                {
                    ItemHelpers.StackingLinear(stack, ArmsRace.Instance.missileCount, ArmsRace.Instance.missileCountPerStack);
                }
            }

            public bool AcquireTarget()
            {
                Ray aimRay = body.inputBank ? body.inputBank.GetAimRay() : default;

                BullseyeSearch search = new BullseyeSearch();

                search.teamMaskFilter = TeamMask.allButNeutral;
                search.teamMaskFilter.RemoveTeam(body.teamComponent.teamIndex);

                search.filterByLoS = true;
                search.searchOrigin = aimRay.origin;
                search.sortMode = BullseyeSearch.SortMode.Angle;
                search.maxDistanceFilter = maxActivationDistance;
                search.maxAngleFilter = 360f;
                search.searchDirection = aimRay.direction;
                search.RefreshCandidates();

                targetHurtBox = search.GetResults().FirstOrDefault();

                return targetHurtBox != null;
            }

            private void FixedUpdate()
            {
                if (NetworkServer.active)// && !characterBody.isPlayerControlled
                {
                    if (!firingBarrage)
                    {
                        GetMissileBarrageCount();
                        //Reloading takes priority
                        if (missilesLoaded < missilesPerBarrage)
                        {
                            cooldownStopwatch += Time.fixedDeltaTime;
                            if (cooldownStopwatch >= cooldownInterval)
                            {
                                cooldownStopwatch = 0f;
                                //missilesLoaded = Mathf.FloorToInt(missilesPerBarrage * Mathf.Max(body.attackSpeed, 1f));
                                missilesLoaded = missilesPerBarrage;
                                fireInterval = baseFireInterval * ArmsRace.Instance.missileCount / missilesPerBarrage;
                            }
                        }
                        else
                        {
                            //Once loaded, search for enemies
                            searchStopwatch += Time.fixedDeltaTime;
                            if (searchStopwatch > searchInterval)
                            {
                                searchStopwatch -= searchInterval;
                                if (body.teamComponent && AcquireTarget())
                                {
                                    firingBarrage = true;
                                    fireStopwatch = 0f;
                                }
                            }
                        }
                    }
                    else //Handle firing
                    {
                        fireStopwatch += Time.fixedDeltaTime;
                        if (fireStopwatch >= fireInterval)
                        {
                            fireStopwatch -= fireInterval;
                            FireMissile();
                        }
                    }
                }
            }

            private void FireMissile()
            {
                if (targetHurtBox != default)
                {
                    MicroMissileOrb missileOrb = new MicroMissileOrb();
                    missileOrb.origin = missileMuzzleTransform ? missileMuzzleTransform.position : body.corePosition;
                    missileOrb.damageValue = body.damage * ArmsRace.Instance.damageCoeff;
                    missileOrb.isCrit = body.RollCrit();
                    missileOrb.teamIndex = TeamComponent.GetObjectTeam(body.gameObject);//body.teamComponent.teamIndex;
                    missileOrb.attacker = gameObject;
                    missileOrb.procChainMask = default;
                    missileOrb.procCoefficient = 1f;
                    missileOrb.damageColorIndex = DamageColorIndex.Default;
                    missileOrb.target = targetHurtBox;
                    missileOrb.speed = 25f; //Same as misisleprojectile. Default is 55f
                    OrbManager.instance.AddOrb(missileOrb);

                    if (EntityStates.Drone.DroneWeapon.FireMissileBarrage.effectPrefab)
                    {
                        EffectManager.SimpleMuzzleFlash(EntityStates.Drone.DroneWeapon.FireMissileBarrage.effectPrefab, gameObject, "Muzzle", true);
                    }

                    //Technically animation is missing but no one will notice.
                }

                missilesLoaded--;
                if (missilesLoaded <= 0 || !targetHurtBox.healthComponent.alive)
                {
                    missilesLoaded = 0;
                    firingBarrage = false;
                }
            }
        }
    }
}