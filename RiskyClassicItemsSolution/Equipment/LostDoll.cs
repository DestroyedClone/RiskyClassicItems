using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RiskyClassicItems.Items;

namespace ClassicItemsReturns.Equipment
{
    internal class LostDoll : EquipmentBase<LostDoll>
    {
        public override string EquipmentName => "Lost Doll";

        public override bool EnigmaCompatible { get; } = false;
        public override bool CanBeRandomlyTriggered { get; } = false;

        public bool useAltLore = true;

        public override string EquipmentLangTokenName => "LOSTDOLL";

        public const float selfHurtCoefficient = 0.25f;
        public const float healthCoefficient = 4f;
        public const float durationDelay = 0.5f;

        public override object[] EquipmentFullDescriptionParams => new object[]
        {
            (selfHurtCoefficient*100),
            (healthCoefficient*100),
        };

        public override GameObject EquipmentModel => LoadItemModel("Doll");

        public override Sprite EquipmentIcon => LoadItemSprite("Doll");

        public override TargetFinderType EquipmentTargetFinderType => TargetFinderType.Enemies;

        public override bool IsLunar => true;
        public static GameObject dollActivationEffect;
        public static NetworkSoundEventDef dollActivationSound;
        public override float Cooldown => 25;

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
            useAltLore = config.Bind(ConfigCategory, "Alt Lore", true, "Uses an unused Lunar lore entry instead of the original RoR1 one.").Value;
            RoR2Application.onLoad += SetLore;
        }

        private void SetLore()
        {
            if (useAltLore)
            {
                EquipmentDef.loreToken += "_ALT";
            }
        }

        public void CreateAssets()
        {
            dollActivationEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/DeathProjectile/DeathProjectileTickEffect.prefab").WaitForCompletion();
            /*dollActivationEffect = PrefabAPI.InstantiateClone(Modules.Assets.LoadAddressable<GameObject>("RoR2/Base/DeathProjectile/PickupDeathProjectile.prefab"), Modules.Assets.prefabPrefix + "LostDollEffect");
            var comp = dollActivationEffect.gameObject.AddComponent<ClassicItemsReturns_LostDollVisualEffect>();
            var spike = UnityEngine.Object.Instantiate<GameObject>(Modules.Assets.LoadAddressable<GameObject>("RoR2/Base/moon/AbyssSpike.prefab"));
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

            //comp.spikeMat = Modules.Assets.LoadAddressable<Material>("RoR2/Base/AltarSkeleton/matAltarSkeleton.mat");
            comp.blackMat = Modules.Assets.LoadAddressable<Material>("RoR2/Base/Common/matDebugBlack.mat");
            comp.dollMat = Modules.Assets.LoadAddressable<Material>("RoR2/Base/AltarSkeleton/matAltarSkeleton.mat");
            comp.dollMR = dollActivationEffect.transform.Find("Mesh").GetComponent<MeshRenderer>();
            //comp.gameObject.AddComponent<NetworkIdentity>();  //Is this actually needed?

            //idk how to, people usually make one from asset or clone an existing one
            //var efcomp = spike.AddComponent<EffectComponent>();
            //ContentAddition.AddEffect(dollActivationEffect);
            UnityEngine.Object.Destroy(spike);*/

            dollActivationSound = Modules.Assets.CreateNetworkSoundEventDef("Play_ClassicItemsReturns_Doll");
        }

        /// <summary>
        /// An example targeting indicator implementation. We clone the woodsprite's indicator, but we edit it to our liking. We'll use this in our activate equipment.
        /// We shouldn't need to network this as this only shows for the player with the Equipment.
        /// </summary>
        private void CreateTargetingIndicator()
        {
            //TargetingIndicatorPrefabBase = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BossHunter/BossHunterIndicator.prefab").WaitForCompletion();
            TargetingIndicatorPrefabBase = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator"), "RCI_LostDollIndicator", false);
            //TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().sprite = Modules.Assets.LoadSprite("texLostDollTargetIndicator.png");
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().color = new Color32(156, 80, 82, 255);
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            TargetingIndicatorPrefabBase.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color32(156, 80, 82, 255);
            //TargetingIndicatorPrefabBase.transform.localScale = Vector3.one * 0.25f;
            //TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().GetComponent<RotateAroundAxis>().enabled = false;
            //TargetingIndicatorPrefabBase = Modules.Assets.targetIndicatorBossHunter;
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
                    localPos = new Vector3(0.00234F, 0.03307F, 1.32997F),
                    localAngles = new Vector3(281.8F, 180F, 180F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                }
            });

            return dict;
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

            RoR2.Audio.EntitySoundManager.EmitSoundServer(LostDoll.dollActivationSound.akId, slot.gameObject);

            slot.subcooldownTimer = 0.05f;
            RoR2.Orbs.OrbManager.instance.AddOrb(new Orbs.CIR_LostDollOrb()
            {
                attacker = slot.gameObject,
                attackerCharacterBody = slot.characterBody,
                damageColorIndex = DamageColorIndex.Item,
                damageValue = slot.characterBody.healthComponent.fullCombinedHealth * healthCoefficient,
                isCrit = Util.CheckRoll(slot.characterBody.crit, slot.characterBody.master),
                procChainMask = default,
                procCoefficient = 1f,
                target = hurtBox,
                //effectType = RoR2.Orbs.DevilOrb.EffectType.Wisp,
                origin = slot.characterBody.corePosition,
                teamIndex = slot.characterBody.teamComponent.teamIndex,
            });

            if (BeatingEmbryo.EmbryoProc(slot, out int p))
            {
                var orb = new Orbs.CIR_LostDollOrb()
                {
                    attacker = slot.gameObject,
                    attackerCharacterBody = slot.characterBody,
                    damageColorIndex = DamageColorIndex.Item,
                    damageValue = slot.characterBody.healthComponent.fullCombinedHealth * healthCoefficient,
                    isCrit = Util.CheckRoll(slot.characterBody.crit, slot.characterBody.master),
                    procChainMask = default,
                    procCoefficient = 1f,
                    target = hurtBox,
                    //effectType = RoR2.Orbs.DevilOrb.EffectType.Wisp,
                    origin = slot.characterBody.corePosition,
                    teamIndex = slot.characterBody.teamComponent.teamIndex,
                };

                for (int i = 0; i < p; i++)
                {
                    orb.damageValue *= BeatingEmbryo.repeatUsageMultiplier;
                    orb.isCrit = Util.CheckRoll(slot.characterBody.crit, slot.characterBody.master);
                    orb.procCoefficient *= BeatingEmbryo.repeatUsageMultiplier;
                    orb.origin = Util.ApplySpread(slot.characterBody.corePosition, -0.1f, 0.1f, 1, 1);
                    RoR2.Orbs.OrbManager.instance.AddOrb(orb);
                }
            }

            var effectData = new EffectData()
            {
                origin = slot.characterBody.corePosition
            };
            EffectManager.SpawnEffect(dollActivationEffect, effectData, true);

            //Disable this for now until an actual fix is found.
            /*var effect = UnityEngine.Object.Instantiate(dollActivationEffect);
            effect.transform.SetParent(slot.transform, false);
            effect.transform.position = slot.characterBody.corePosition + Vector3.up * 3f;
            NetworkServer.Spawn(effect);*/

            slot.InvalidateCurrentTarget();
            return true;
        }
    }
}