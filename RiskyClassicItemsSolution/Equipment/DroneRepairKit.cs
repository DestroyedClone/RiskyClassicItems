using BepInEx.Configuration;
using ClassicItemsReturns.Items.Uncommon;
using ClassicItemsReturns.Modules;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ClassicItemsReturns.Equipment
{
    public class DroneRepairKit : EquipmentBase<DroneRepairKit>
    {
        public override string EquipmentName => "Drone Repair Kit";

        public override string EquipmentLangTokenName => "DRONEREPAIRKIT";
        public const float buffDuration = 8f;
        public const float buffAttackSpeed = 0.5f;
        public const float buffCDReduction = 0.5f;
        public const float buffArmor = 50f;

        public static NetworkSoundEventDef activationSound;
        public static GameObject repairDroneMasterPrefab;
        public static GameObject repairDroneBodyPrefab;

        public override bool EnigmaCompatible { get; } = true;
        public override bool CanBeRandomlyTriggered { get; } = true;

        public override object[] EquipmentFullDescriptionParams => new object[]
        {
        };

        public override GameObject EquipmentModel => LoadItemModel("RepairKit");

        public override Sprite EquipmentIcon => LoadItemSprite("RepairKit");

        public override void CreateAssets(ConfigFile config)
        {
            base.CreateAssets(config);

            activationSound = Modules.Assets.CreateNetworkSoundEventDef("Play_ClassicItemsReturns_RepairKit");

            CreateUniqueDrone();
        }

        private void CreateUniqueDrone()
        {
            repairDroneBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Body.prefab").WaitForCompletion().InstantiateClone("CLASSICITEMSRETURNS_BODY_RepairDroneBody", true);
            CharacterBody body = repairDroneBodyPrefab.GetComponent<CharacterBody>();
            body.baseNameToken = "CLASSICITEMSRETURNS_BODY_REPAIRDRONEBODY_NAME";
            PluginContentPack.bodyPrefabs.Add(repairDroneBodyPrefab);

            repairDroneMasterPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Master.prefab").WaitForCompletion().InstantiateClone("CLASSICITEMSRETURNS_MASTER_RepairDroneMaster", true);
            CharacterMaster master = repairDroneMasterPrefab.GetComponent<CharacterMaster>();
            master.bodyPrefab = repairDroneBodyPrefab;
            RoR2.SetDontDestroyOnLoad ddol = master.GetComponent<RoR2.SetDontDestroyOnLoad>();
            ddol.enabled = false;
            if (ddol) UnityEngine.Object.Destroy(ddol);
            PluginContentPack.masterPrefabs.Add(repairDroneMasterPrefab);
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            bool hasBuff =  sender.HasBuff(Buffs.DroneRepairBuff);
            if (hasBuff)
            {
                args.attackSpeedMultAdd += buffAttackSpeed;
                args.cooldownReductionAdd += buffCDReduction;
                args.armorAdd += buffArmor;
            }
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
                    localPos = new Vector3(-0.222F, 0F, 2.51855F),
                    localAngles = new Vector3(0.39625F, 59.72602F, 274.5737F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });

            return dict;
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.master) return false;

            DroneRepairKitSummonTracker summonTracker = slot.characterBody.master.GetComponent<DroneRepairKitSummonTracker>();
            if (!summonTracker)
            {
                summonTracker = slot.characterBody.master.gameObject.AddComponent<DroneRepairKitSummonTracker>();
            }

            TeamIndex activatorTeam = TeamIndex.None;
            if (slot.characterBody.teamComponent) activatorTeam = slot.characterBody.teamComponent.teamIndex;

            if (!summonTracker.summonMasterInstance)
            {
                if (slot.characterBody.isPlayerControlled)
                {
                    float y = Quaternion.LookRotation(slot.GetAimRay().direction).eulerAngles.y;
                    Quaternion rotation = Quaternion.Euler(0f, y, 0f);
                    CharacterMaster characterMaster = new MasterSummon
                    {
                        masterPrefab = DroneRepairKit.repairDroneMasterPrefab,
                        position = slot.transform.position + rotation * Vector3.forward * 3f + Vector3.up * 3f,
                        rotation = rotation,
                        summonerBodyObject = slot.gameObject,
                        ignoreTeamMemberLimit = true,
                        useAmbientLevel = true
                    }.Perform();
                    if (characterMaster)
                    {
                        summonTracker.summonMasterInstance = characterMaster.gameObject;

                        if (characterMaster.teamIndex == TeamIndex.Player && characterMaster.inventory)
                        {
                            if (Items.NoTier.DroneRepairKitDroneItem.Instance.ItemDef)
                            {
                                characterMaster.inventory.GiveItem(Items.NoTier.DroneRepairKitDroneItem.Instance.ItemDef);
                            }

                            if (ModSupport.ModCompatRiskyMod.loaded)
                            {
                                Modules.ModSupport.ModCompatRiskyMod.GiveAllyItem(characterMaster.inventory, true);
                                Modules.ModSupport.ModCompatRiskyMod.GiveAllyRegenItem(characterMaster.inventory, 40);
                            }
                        }
                    }
                }
            }

            foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList)
            {
                if (!master) continue;
                CharacterBody body = master.GetBody();
                if (!body
                    || body.isPlayerControlled
                    || !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical)
                    || !body.teamComponent
                    || body.teamComponent.teamIndex != activatorTeam) continue;
                if (body.healthComponent) body.healthComponent.HealFraction(1f, default);
                body.AddTimedBuff(Buffs.DroneRepairBuff, buffDuration);
            }

            RoR2.Audio.EntitySoundManager.EmitSoundServer(activationSound.index, slot.characterBody.gameObject);

            return true;
        }

        protected override void CreateCraftableDef()
        {
            bool armsRace = ArmsRace.Instance != null && ArmsRace.Instance.ItemDef;
            bool mortar = MortarTube.Instance != null && MortarTube.Instance.ItemDef;
            if (ArmsRace.Instance != null && ArmsRace.Instance.ItemDef)
            {
                CraftableDef cdDroneRepairKit = ScriptableObject.CreateInstance<CraftableDef>();
                cdDroneRepairKit.pickup = EquipmentDef;
                cdDroneRepairKit.recipes = new Recipe[]
                {
                    new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = ArmsRace.Instance.ItemDef
                            },
                            new RecipeIngredient()
                            {
                                pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/EquipmentMagazine/EquipmentMagazine.asset").WaitForCompletion()
                            }
                        }
                    }
                };
                (cdDroneRepairKit as ScriptableObject).name = "cdDroneRepairKit";
                PluginContentPack.craftableDefs.Add(cdDroneRepairKit);
            }

            if (mortar || armsRace)
            {
                ItemDef sdp = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/DroneWeapons/DroneWeapons.asset").WaitForCompletion();
                CraftableDef drkToSDP = ScriptableObject.CreateInstance<CraftableDef>();
                drkToSDP.pickup = sdp;
                drkToSDP.recipes = new Recipe[0];

                if (mortar)
                {
                    drkToSDP.recipes = drkToSDP.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = MortarTube.Instance.ItemDef
                            },
                            new RecipeIngredient()
                            {
                                pickup = EquipmentDef
                            }
                        }
                    }).ToArray();
                }

                if (armsRace)
                {
                    drkToSDP.recipes = drkToSDP.recipes.Append(new Recipe()
                    {
                        amountToDrop = 1,
                        ingredients = new RecipeIngredient[]
                        {
                            new RecipeIngredient()
                            {
                                pickup = ArmsRace.Instance.ItemDef
                            },
                            new RecipeIngredient()
                            {
                                pickup = EquipmentDef
                            }
                        }
                    }).ToArray();
                }

                (drkToSDP as ScriptableObject).name = "cdDRKToSDP";
                PluginContentPack.craftableDefs.Add(drkToSDP);
            }
        }
    }

    public class DroneRepairKitSummonTracker : MonoBehaviour
    {
        public GameObject summonMasterInstance = null;
    }
}