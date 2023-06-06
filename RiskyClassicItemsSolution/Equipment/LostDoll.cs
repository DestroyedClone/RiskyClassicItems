using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using IL.RoR2.Projectile;

namespace RiskyClassicItems.Equipment
{
    internal class LostDoll : EquipmentBase<LostDoll>
    {
        public override string EquipmentName => "Lost Doll";

        public override string EquipmentLangTokenName => "LOSTDOLL";

        public const float selfHurtCoefficient = 0.25f;
        public const float damageCoefficient = 5f;
        public const float durationDelay = 0.15f;

        public override object[] EquipmentFullDescriptionParams => new object[]
        {
            (selfHurtCoefficient*100),
            (damageCoefficient*100),
        };

        public override GameObject EquipmentModel => Assets.NullModel;

        public override Sprite EquipmentIcon => Assets.NullSprite;

        public override TargetFinderType EquipmentTargetFinderType => TargetFinderType.Enemies;

        public override bool IsLunar => true;
        public static GameObject dollActivationEffect;
        public override float Cooldown => 1;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateTargetingIndicator();
            CreateAssets();
            CreateEquipment();
            Hooks();
        }

        protected override void CreateConfig(ConfigFile config)
        {
        }

        public void CreateAssets()
        {
            dollActivationEffect = PrefabAPI.InstantiateClone(Assets.LoadAddressable<GameObject>("RoR2/Base/DeathProjectile/PickupDeathProjectile.prefab"), "RCI_LostDollEffect");
            var comp = dollActivationEffect.gameObject.AddComponent<RCI_LostDollVisualEffect>();
            var spike = UnityEngine.Object.Instantiate<GameObject>(Assets.LoadAddressable<GameObject>("RoR2/Base/moon/AbyssSpike.prefab"));
            spike.name = "DollSpike";
            spike.transform.SetParent(dollActivationEffect.transform);
            spike.transform.localScale = new Vector3(25, 25, 125);
            spike.AddComponent<DollSpikeDisplayHelper>().enabled = false;

            int spikeCount = 0;
            GameObject spikeCopy()
            {
                var obj = UnityEngine.Object.Instantiate(spike);
                obj.name = $"Spike{spikeCount++}";
                obj.transform.SetParent(dollActivationEffect.transform);
                return obj;
            }

            comp.AddSpike(spikeCopy(), Util.QuaternionSafeLookRotation(new Vector3(0, 90, 0)), Vector3.up * 3f, Vector3.zero);
            comp.AddSpike(spikeCopy(), Util.QuaternionSafeLookRotation(new Vector3(90, 0, 0)), Vector3.one * 3f, Vector3.zero);
            //add more as needed

            comp.blackMat = Assets.LoadAddressable<Material>("RoR2/Base/Common/matDebugBlack.mat");
            comp.dollMR = dollActivationEffect.transform.Find("Mesh").GetComponent<MeshRenderer>();
            comp.gameObject.AddComponent<NetworkIdentity>();

            //idk how to, people usually make one from asset or clone an existing one
            //var efcomp = spike.AddComponent<EffectComponent>();
            //ContentAddition.AddEffect(dollActivationEffect);
            UnityEngine.Object.Destroy(spike);
        }

        public class DollSpikeDisplayHelper : MonoBehaviour
        {
            string output = "";

            public void FixedUpdate()
            {
                var transform = gameObject.transform;
                output = $"comp.AddSpike(spikeCopy(), new Quaternion({transform.rotation}), new Vector3({transform.localPosition}), Vector3.zero);";

            }
        }

        public class RCI_LostDollVisualEffect : MonoBehaviour
        {
            public float delayStopwatch = 0;

            public bool activatedEffect = false;
            public float effectStopwatch = 0;
            public float effectDuration = 0.4f;

            public bool terminatingEffect = false;
            public float terminationStopwatch = 2f;

            public List<GameObject> spikes = new List<GameObject>();
            public List<Vector3> startPositions = new List<Vector3>();
            public List<Vector3> endPositions = new List<Vector3>();
            public Material blackMat;

            public MeshRenderer dollMR;
            public void Awake()
            {
                delayStopwatch = durationDelay;
                effectStopwatch = 0;
                terminationStopwatch = 0.5f;
            }

            public void AddSpike(GameObject spike, Quaternion initialRotation, Vector3 startPosition, Vector3 endPosition)
            {
                spikes.Add(spike);
                startPositions.Add(startPosition);
                endPositions.Add(endPosition);
                spike.transform.localPosition = startPosition;
                spike.transform.rotation = initialRotation;
            }

            public void Update()
            {
                if (!activatedEffect)
                {
                    delayStopwatch -= Time.deltaTime;
                    if (delayStopwatch <= 0)
                    {
                        //Chat.AddMessage("State: Start");
                        activatedEffect = true;
                    }
                    return;
                }

                if (terminatingEffect)
                {
                    //Chat.AddMessage($"Terminating: Time Remaining: {terminationStopwatch}");
                    terminationStopwatch -= Time.deltaTime;
                    if (terminationStopwatch <= 0)
                    {
                        enabled = false;
                        Destroy(gameObject);
                    }
                }
                else
                {
                    effectStopwatch += Time.deltaTime;
                    //Chat.AddMessage($"Progress: {effectStopwatch / effectDuration}");
                    for (int i = 0; i < spikes.Count; i++)
                    {
                        GameObject spike = spikes[i];
                        spike.transform.localPosition = Vector3.Lerp(startPositions[i], endPositions[i], effectStopwatch / effectDuration);
                    }
                    if (effectStopwatch > effectDuration)
                    {
                        foreach (var spike in spikes)
                        {
                            spike.GetComponent<MeshRenderer>().SetMaterial(blackMat);
                        }
                        dollMR.SetMaterial(blackMat);
                        terminatingEffect = true;
                    }
                }
            }
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
                    effectType = RoR2.Orbs.DevilOrb.EffectType.Wisp,
                    origin = slot.characterBody.corePosition,
                    teamIndex = slot.characterBody.teamComponent.teamIndex,
                    arrivalTime = durationDelay
                });

                /* var effectData = new EffectData()
                 {
                     origin = slot.characterBody.aimOrigin + Vector3.up * 3f
                 };*/
                //EffectManager.SpawnEffect(dollActivationEffect, effectData, true);
                var effect = UnityEngine.Object.Instantiate(dollActivationEffect);
                effect.transform.position = slot.characterBody.corePosition + Vector3.up * 3f;
                effect.transform.SetParent(slot.transform, false);
                NetworkServer.Spawn(effect);
                slot.InvalidateCurrentTarget();
                return true;
            }
            return false;
        }
    }
}