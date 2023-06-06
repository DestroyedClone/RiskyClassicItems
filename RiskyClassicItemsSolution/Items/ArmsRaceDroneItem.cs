using R2API;
using RoR2;
using RiskyClassicItems.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RoR2.Items.BaseItemBodyBehavior;
using RoR2.Items;
using UnityEngine.Diagnostics;
using RoR2.CharacterAI;
using BepInEx.Configuration;
using static RiskyClassicItems.Items.ArmsRace;
using EntityStates.Drone.DroneWeapon;
using RoR2.Projectile;
using UnityEngine.Networking;
using RiskyClassicItems.Utils;
using System.Linq;
using RoR2.Orbs;

namespace RiskyClassicItems.Items
{
    internal class ArmsRaceDroneItem : ItemBase<ArmsRaceDroneItem>
    {
        public override string ItemName => "ARMSRACEDRONEITEM";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => Assets.NullModel;

        public override Sprite ItemIcon => Assets.NullSprite;

        public override string ItemLangTokenName => "ARMSRACEDRONEITEM";

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        //https://github.com/Moffein/RiskyMod/blob/master/RiskyMod/Allies/DroneBehaviors/AutoMissileBehavior.cs#L10
        public class ArmsRaceDroneBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => ArmsRaceDroneItem.Instance.ItemDef;

            public static float searchInterval = 1f;
            public static float maxActivationDistance = 80f;
            public static int missilesPerBarrage = 4;

            public static float damageCoefficient = 1.1f;
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
            public int missileCount;

            private void GetMissileBarrageCount()
            {
                if (body && body.master && body.master.minionOwnership && body.master.minionOwnership.ownerMaster && ArmsRace.Instance.TryGetCount(body.master.minionOwnership.ownerMaster, out int ownerArmsRaceItemCount))
                {
                    missilesPerBarrage = ItemHelpers.StackingLinear(ownerArmsRaceItemCount, ArmsRace.Instance.missileCount, ArmsRace.Instance.missileCountPerStack);
                    return;
                }
                missilesPerBarrage = ItemHelpers.StackingLinear(stack, ArmsRace.Instance.missileCount, ArmsRace.Instance.missileCountPerStack);
            }

            private void OnEnable()
            {
                GetMissileBarrageCount();
                missilesLoaded = missilesPerBarrage;
                searchStopwatch = 0f;
                cooldownStopwatch = UnityEngine.Random.Range(0.1f, 0.75f);
                fireStopwatch = 0f;
                fireInterval = baseFireInterval;
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

                targetHurtBox = search.GetResults().FirstOrDefault<HurtBox>();

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
                                //fireInterval = baseFireInterval / body.attackSpeed;
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
                    Ray aimRay = body.inputBank ? body.inputBank.GetAimRay() : default;
                    MicroMissileOrb missileOrb = new MicroMissileOrb();
                    missileOrb.origin = aimRay.origin;
                    //missileOrb.damageValue = body.damage * damageCoefficient * (AlliesCore.normalizeDroneDamage ? 1f : 0.857142857f);  // 12/14
                    missileOrb.damageValue = body.damage * ArmsRace.Instance.damageCoeff;
                    missileOrb.isCrit = body.RollCrit();
                    missileOrb.teamIndex = body.teamComponent.teamIndex;
                    missileOrb.attacker = base.gameObject;
                    missileOrb.procChainMask = default;
                    missileOrb.procCoefficient = 1f;
                    missileOrb.damageColorIndex = DamageColorIndex.Default;
                    missileOrb.target = targetHurtBox;
                    missileOrb.speed = 25f; //Same as misisleprojectile. Default is 55f
                    OrbManager.instance.AddOrb(missileOrb);

                    if (EntityStates.Drone.DroneWeapon.FireMissileBarrage.effectPrefab)
                    {
                        EffectManager.SimpleMuzzleFlash(EntityStates.Drone.DroneWeapon.FireMissileBarrage.effectPrefab, base.gameObject, "Muzzle", true);
                    }

                    //Technically animation is missing but no one will notice.
                }

                missilesLoaded--;
                if (missilesLoaded <= 0)
                {
                    firingBarrage = false;
                }
            }
        }
    }
}
