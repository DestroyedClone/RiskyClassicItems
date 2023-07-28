using BepInEx.Configuration;
using R2API;
using Rewired.ComponentControls.Effects;
using ClassicItemsReturns.Modules;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace ClassicItemsReturns.Equipment
{
    internal class LostDoll : EquipmentBase<LostDoll>
    {
        public override string EquipmentName => "Lost Doll";

        public override string EquipmentLangTokenName => "LOSTDOLL";

        public const float selfHurtCoefficient = 0.33f;
        public const float damageCoefficient = 4f;
        public const float durationDelay = 0.5f;

        public override object[] EquipmentFullDescriptionParams => new object[]
        {
            (selfHurtCoefficient*100),
            (damageCoefficient*100),
        };

        public override GameObject EquipmentModel => LoadItemModel("Doll");

        public override Sprite EquipmentIcon => LoadItemSprite("Doll");

        public override TargetFinderType EquipmentTargetFinderType => TargetFinderType.Enemies;

        public override bool IsLunar => true;
        public static GameObject dollActivationEffect;
        public override float Cooldown => 45;

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
            dollActivationEffect = PrefabAPI.InstantiateClone(Assets.LoadAddressable<GameObject>("RoR2/Base/DeathProjectile/PickupDeathProjectile.prefab"), Assets.prefabPrefix + "LostDollEffect");
            var comp = dollActivationEffect.gameObject.AddComponent<ClassicItemsReturns_LostDollVisualEffect>();
            var spike = UnityEngine.Object.Instantiate<GameObject>(Assets.LoadAddressable<GameObject>("RoR2/Base/moon/AbyssSpike.prefab"));
            spike.name = "DollSpike";
            spike.transform.SetParent(dollActivationEffect.transform);
            spike.transform.localScale = new Vector3(25, 25, 125);
            DollSpikeDisplayHelper dollSpikeDisplayHelper = spike.AddComponent<DollSpikeDisplayHelper>();
            dollSpikeDisplayHelper.enabled = false;

            int spikeCount = 0;
            GameObject spikeCopy()
            {
                var obj = UnityEngine.Object.Instantiate(spike);
                obj.name = $"Spike{spikeCount++}";
                obj.transform.SetParent(dollActivationEffect.transform);
                return obj;
            }

            comp.AddSpike(spikeCopy(), new Quaternion(0.7f, 0.0f, 0.0f, 0.7f), new Vector3(0.0f, 3.0f, 0.0f), Vector3.zero);
            comp.AddSpike(spikeCopy(), new Quaternion(0.7f, 0.1f, -0.7f, 0.1f), new Vector3(5.4f, 1.0f, 0.0f), Vector3.zero);
            comp.AddSpike(spikeCopy(), new Quaternion(0.3f, 0.6f, -0.3f, 0.6f), new Vector3(-2.2f, 3.2f, 0.2f), Vector3.zero);
            comp.AddSpike(spikeCopy(), new Quaternion(0.2f, 0.6f, -0.1f, 0.7f), new Vector3(-3.6f, 1.6f, -0.3f), Vector3.zero);
            comp.AddSpike(spikeCopy(), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, -6.0f), Vector3.zero);

            //add more as needed

            //comp.spikeMat = Assets.LoadAddressable<Material>("RoR2/Base/AltarSkeleton/matAltarSkeleton.mat");
            comp.blackMat = Assets.LoadAddressable<Material>("RoR2/Base/Common/matDebugBlack.mat");
            comp.dollMat = Assets.LoadAddressable<Material>("RoR2/Base/AltarSkeleton/matAltarSkeleton.mat");
            comp.dollMR = dollActivationEffect.transform.Find("Mesh").GetComponent<MeshRenderer>();
            //comp.gameObject.AddComponent<NetworkIdentity>();  //Is this actually needed?

            //idk how to, people usually make one from asset or clone an existing one
            //var efcomp = spike.AddComponent<EffectComponent>();
            //ContentAddition.AddEffect(dollActivationEffect);
            UnityEngine.Object.Destroy(spike);
        }

        public class DollSpikeDisplayHelper : MonoBehaviour
        {
            private string output = "";

            public void FixedUpdate()
            {
                var transform = gameObject.transform;
                output = $"comp.AddSpike(spikeCopy(), new Quaternion{transform.rotation}, new Vector3{transform.localPosition}, Vector3.zero);";
            }
        }

        public class ClassicItemsReturns_LostDollVisualEffect : MonoBehaviour
        {
            public float delayStopwatch = 0;

            public bool activatedEffect = false;
            public float effectStopwatch = 0;
            public float effectDuration = 0.05f;

            public bool terminatingEffect = false;
            public float terminationStopwatch = 0.2f;

            public List<GameObject> spikes = new List<GameObject>();
            public List<Vector3> startPositions = new List<Vector3>();
            public List<Vector3> endPositions = new List<Vector3>();
            public Material dollMat;
            public Material blackMat;
            //public Material spikeMat;

            public MeshRenderer dollMR;

            public void Awake()
            {
                delayStopwatch = durationDelay;
                effectStopwatch = 0;
                //terminationStopwatch = 0.4f;

                //Do this to make it darkened from the start.
                if (dollMR && dollMat) dollMR.SetMaterial(dollMat);
                Util.PlaySound("Play_ClassicItemsReturns_Doll", base.gameObject);
            }

            public void AddSpike(GameObject spike, Quaternion initialRotation, Vector3 startPosition, Vector3 endPosition)
            {
                spikes.Add(spike);
                startPositions.Add(startPosition);
                endPositions.Add(endPosition);
                spike.transform.localPosition = startPosition;
                spike.transform.rotation = initialRotation;
                //spike.GetComponent<MeshRenderer>().SetMaterial(spikeMat);
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
                    if (effectStopwatch >= effectDuration)
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
            //TargetingIndicatorPrefabBase = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BossHunter/BossHunterIndicator.prefab").WaitForCompletion();
            TargetingIndicatorPrefabBase = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator"), "RCI_LostDollIndicator", false);
            //TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().sprite = Assets.LoadSprite("texLostDollTargetIndicator.png");
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().color = new Color32(156, 80, 82, 255);
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            TargetingIndicatorPrefabBase.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color32(156, 80, 82, 255);
            //TargetingIndicatorPrefabBase.transform.localScale = Vector3.one * 0.25f;
            //TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().GetComponent<RotateAroundAxis>().enabled = false;
            //TargetingIndicatorPrefabBase = Assets.targetIndicatorBossHunter;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void ConfigureTargetIndicator(EquipmentSlot equipmentSlot, EquipmentIndex targetingEquipmentIndex, GenericPickupController genericPickupController, ref bool shouldShowOverride)
        {
            EquipmentSlot.UserTargetInfo currentTarget = equipmentSlot.currentTarget;
            if (currentTarget.hurtBox && equipmentSlot.currentTarget.hurtBox.healthComponent && equipmentSlot.currentTarget.hurtBox.healthComponent.alive)
            {
                equipmentSlot.targetIndicator.visualizerPrefab = TargetingIndicatorPrefabBase;
                return;
            }
            shouldShowOverride = false;
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            HurtBox hurtBox = slot.currentTarget.hurtBox;
            if (!hurtBox || !hurtBox.healthComponent || !hurtBox.healthComponent.alive)
            {
                return false;
            }

            slot.subcooldownTimer = 0.05f;
            RoR2.Orbs.OrbManager.instance.AddOrb(new Orbs.CIR_LostDollOrb()
            {
                attacker = slot.gameObject,
                attackerCharacterBody = slot.characterBody,
                damageColorIndex = DamageColorIndex.Item,
                damageValue = slot.characterBody.healthComponent.fullCombinedHealth * damageCoefficient,
                isCrit = Util.CheckRoll(slot.characterBody.crit, slot.characterBody.master),
                procChainMask = default,
                procCoefficient = 1f,
                target = hurtBox,
                //effectType = RoR2.Orbs.DevilOrb.EffectType.Wisp,
                origin = slot.characterBody.corePosition,
                teamIndex = slot.characterBody.teamComponent.teamIndex,
            });

            /* var effectData = new EffectData()
             {
                 origin = slot.characterBody.aimOrigin + Vector3.up * 3f
             };*/
            //EffectManager.SpawnEffect(dollActivationEffect, effectData, true);
            var effect = UnityEngine.Object.Instantiate(dollActivationEffect);
            effect.transform.SetParent(slot.transform, false);
            effect.transform.position = slot.characterBody.corePosition + Vector3.up * 3f;

            NetworkServer.Spawn(effect);
            slot.InvalidateCurrentTarget();
            return true;
        }
    }
}