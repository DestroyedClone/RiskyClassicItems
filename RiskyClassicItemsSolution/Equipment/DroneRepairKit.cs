using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using EntityStates.AffixVoid;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Equipment
{
    public class DroneRepairKit : EquipmentBase<DroneRepairKit>
    {
        public override string EquipmentName => "Drone Repair Kit";

        public override string EquipmentLangTokenName => "DRONEREPAIRKIT";
        public const float buffDuration = 8f;
        public const float buffAttackSpeed = 0.5f;
        public const float buffCDReduction = 0.5f;
        public const float buffArmorFlat = 50;

        public override bool EnigmaCompatible { get; } = true;
        public override bool CanBeRandomlyTriggered { get; } = true;

        public override object[] EquipmentFullDescriptionParams => new object[]
        {
        };

        public override GameObject EquipmentModel => LoadItemModel("DroneRepairKit");

        public override Sprite EquipmentIcon => LoadItemSprite("DroneRepairKit");

        public static BuffDef DroneRepairBuff => Buffs.DroneRepairBuff;
        public override bool Unfinished => true;

        public static GameObject droneBody;
        public static GameObject droneMaster;
        public static CharacterSpawnCard droneSpawnCard;

        public override void CreateAssets(ConfigFile config)
        {
            base.CreateAssets(config);
            //DRK = DroneRepairKit
            droneBody = PrefabAPI.InstantiateClone(UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/BackupDroneBody.prefab").WaitForCompletion(), "CIR_DRKDroneBody");
            var body = droneBody.GetComponent<CharacterBody>();
            body.baseNameToken = "CLASSICITEMSRETURNS_DRKDRONE_BODY_NAME";
            body.subtitleNameToken = "CLASSICITEMSRETURNS_DRKDRONE_BODY_SUBTITLE";

            droneMaster = PrefabAPI.InstantiateClone(UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/DroneBackupMaster.prefab").WaitForCompletion(), "CIR_DRKDroneMaster");
            droneMaster.GetComponent<CharacterMaster>().bodyPrefab = droneBody;
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var buffCount = sender.GetBuffCount(DroneRepairBuff);
            if (buffCount > 0)
            {
                args.attackSpeedMultAdd += buffAttackSpeed;
                args.cooldownReductionAdd += buffCDReduction;
                args.armorAdd += buffArmorFlat;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            int sliceCount = 1;
            float num = 25f;
            if (NetworkServer.active) //todo: remove slice code
            {
                float y = Quaternion.LookRotation(slot.GetAimRay().direction).eulerAngles.y;
                float d = 3f;
                foreach (float num2 in new DegreeSlices(sliceCount, 0.5f))
                {
                    Quaternion rotation = Quaternion.Euler(-30f, y + num2, 0f);
                    Quaternion rotation2 = Quaternion.Euler(0f, y + num2 + 180f, 0f);
                    Vector3 position = slot.transform.position + rotation * (Vector3.forward * d);
                    CharacterMaster characterMaster = slot.SummonMaster(droneMaster, position, rotation2);
                    if (characterMaster)
                    {
                        characterMaster.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = num + UnityEngine.Random.Range(0f, 3f);
                    }
                }
            }
            int activationCount = 0;
            foreach (var mechAlly in CharacterBody.readOnlyInstancesList)
            {
                if (mechAlly.isPlayerControlled)
                    continue;
                if (mechAlly.teamComponent.teamIndex != slot.teamComponent.teamIndex)
                    continue;
                if (!mechAlly.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
                    continue;
                mechAlly.healthComponent.HealFraction(1f, default);
                mechAlly.AddTimedBuff(DroneRepairBuff, buffDuration);
                activationCount++;
            }

            Util.PlaySound("Play_item_proc_healingPotion", slot.gameObject);
            slot.subcooldownTimer = 0.5f;
            return activationCount > 0;
        }
    }
}