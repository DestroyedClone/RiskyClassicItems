using BepInEx.Configuration;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using R2API;
using RiskyMod.Items;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items.Uncommon
{
    public class RoyalMedallion : ItemBase<RoyalMedallion>
    {
        public override string ItemName => "Royal Medallion";

        public override string ItemLangTokenName => "ROYALMEDALLION";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Medallion");
        public override Sprite ItemIcon => LoadItemSprite("Medallion");

        public static int maxBuffStacks = 10;
        public float procChance = 10f;
        public float buffDuration = 10f;
        public float buffDurationStack = 6f;

        public static GameObject buffObjectPrefab;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.Utility,
            ItemTag.Healing,
            ItemTag.AIBlacklist
        };

        //Based on SS2U Stirring Soul code
        public override void CreateAssets(ConfigFile config)
        {
            RoyalMedallionPickup.procSound = Modules.Assets.CreateNetworkSoundEventDef("Play_ClassicItemsReturns_Medallion");

            //This auto adds it to ContentPack
            buffObjectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Tooth/HealPack.prefab").WaitForCompletion().InstantiateClone("CIR_RoyalMedallinPickup", true);
            buffObjectPrefab.transform.localScale = Vector3.one * 2.5f;
            buffObjectPrefab.GetComponent<Rigidbody>().drag = 15f; //default is 2
            buffObjectPrefab.GetComponent<DestroyOnTimer>().duration = 8f;
            buffObjectPrefab.GetComponent<BeginRapidlyActivatingAndDeactivating>().delayBeforeBeginningBlinking = 7f;

            GravitatePickup gp = buffObjectPrefab.GetComponent<GravitatePickup>();
            if (gp) gp.gravitateAtFullHealth = true;

            //attach pickup to game object
            var pickupObj = buffObjectPrefab.transform.Find("PickupTrigger").gameObject;
            UnityEngine.Object.Destroy(pickupObj.GetComponent<HealthPickup>());
            var pickupComp = pickupObj.AddComponent<RoyalMedallionPickup>();

            //change visuals
            var effect = buffObjectPrefab.transform.Find("HealthOrbEffect").gameObject;
            var coreObject = effect.transform.Find("VFX").Find("Core").gameObject;
            var coreMat = coreObject.GetComponent<ParticleSystem>().main;
            Color color = new Color32(220, 100, 200, 255);
            coreMat.startColor = color;
            var pulseGlow = effect.transform.Find("VFX").Find("PulseGlow").gameObject;

            var pulseGlowSystem = pulseGlow.GetComponent<ParticleSystem>();
            var pulseMain = pulseGlowSystem.main;
            pulseMain.startColor = color;
            pulseMain.simulationSpeed = 0f; //Prevents them from turning green

            UnityEngine.Object.Destroy(effect.transform.Find("TrailParent").gameObject);

            var akEvents = buffObjectPrefab.GetComponentsInChildren<AkEvent>();
            foreach (var akEvent in akEvents)
            {
                UnityEngine.Object.Destroy(akEvent);
            }
            UnityEngine.Object.Destroy(buffObjectPrefab.GetComponentInChildren<AkGameObj>());
        }

        public override void Hooks()
        {
            SharedHooks.OnHitEnemy.OnHitEnemyAttackerInventoryAndVictimBodyActions += ProcItem;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        //Based on SS2U Stirring Soul code.
        private void ProcItem(GlobalEventManager globalEventManager, DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody, Inventory attackerInventory)
        {
            if (!victimBody.isChampion && !victimBody.isBoss) return;
            if (!RoyalMedallionPickup.CanSpawnPickup()) return;

            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (!(itemCount > 0 && Util.CheckRoll(procChance, attackerBody.master))) return;

            GameObject buffObject = UnityEngine.Object.Instantiate(buffObjectPrefab, damageInfo.position, UnityEngine.Random.rotation);

            TeamFilter teamFilter = buffObject.GetComponent<TeamFilter>();
            teamFilter.teamIndex = attackerBody.teamComponent ? attackerBody.teamComponent.teamIndex : TeamIndex.None;

            var pickup = buffObject.GetComponentInChildren<RoyalMedallionPickup>();
            pickup.team = teamFilter;
            pickup.buffDuration = ItemHelpers.StackingLinear(itemCount, buffDuration, buffDurationStack);
            pickup.baseObject = buffObject;
            
            NetworkServer.Spawn(buffObject);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(Modules.Buffs.RoyalMedallionBuff);
            if (buffCount > 0)
            {
                args.attackSpeedMultAdd += buffCount * 0.06f;
                args.damageMultAdd += buffCount * 0.03f;
                args.moveSpeedMultAdd += 0.1f;

                float levelFactor = 1f + 0.2f * (sender.level - 1f);
                args.baseRegenAdd += buffCount * 1f * levelFactor;
            }
        }

        public static void AddRoyalMedallionBuffWithDroneHandling(CharacterBody attackerBody, float duration)
        {
            AddRoyalMedallionBuff(attackerBody, duration);
            if (attackerBody.isPlayerControlled) return;

            //If an NPC steals a buff, try to give it to a nearby teammate.
            TeamIndex team = attackerBody.teamComponent ? attackerBody.teamComponent.teamIndex : TeamIndex.None;
            if (team == TeamIndex.None) return;

            var teammates = CharacterBody.readOnlyInstancesList.Where(cb =>
            (cb.teamComponent && cb.teamComponent.teamIndex == team)).ToList();
            if (teammates.Count <= 0) return;

            teammates.Sort((cb1, cb2) => (
                Mathf.RoundToInt(
                    (cb1.corePosition - attackerBody.corePosition).sqrMagnitude - (cb2.corePosition - attackerBody.corePosition).sqrMagnitude)
                )
            );
            var targetBody = teammates.FirstOrDefault();

            //Only actually give the buff if the closest player is <20m away
            //400 = 20m x 20m
            if (targetBody && (targetBody.corePosition - attackerBody.corePosition).sqrMagnitude <= 400) AddRoyalMedallionBuff(targetBody, duration);
        }

        //Adds to a single body
        public static void AddRoyalMedallionBuff(CharacterBody attackerBody, float duration)
        {
            int currentBuffs = attackerBody.GetBuffCount(Modules.Buffs.RoyalMedallionBuff);
            if (currentBuffs > 0) attackerBody.ClearTimedBuffs(Modules.Buffs.RoyalMedallionBuff);
            int newBuffs = Mathf.Min(currentBuffs + 1, maxBuffStacks);
            for (int i = 0; i < newBuffs; i++)
            {
                attackerBody.AddTimedBuff(Modules.Buffs.RoyalMedallionBuff, duration);
            }
        }
    }

    //Based on SS2U Stirring Soul code.
    public class RoyalMedallionPickup : MonoBehaviour
    {
        public static NetworkSoundEventDef procSound;
        public static int maxInstances = 40;    //Limit orbs to prevent crashing.
        private static int instanceCount = 0;

        public float buffDuration;
        public TeamFilter team;
        private bool gaveBuff = false;
        public GameObject baseObject;   //Needed because this is lower in the hierarchy

        private void OnTriggerStay(Collider other)
        {
            if (!NetworkServer.active || this.gaveBuff || !this.team || !other) return;

            if (TeamComponent.GetObjectTeam(other.gameObject) != this.team.teamIndex) return;

            CharacterBody body = other.GetComponent<CharacterBody>();

            gaveBuff = true;
            if (procSound) EffectManager.SimpleSoundEffect(procSound.index, base.transform.position, true);
            RoyalMedallion.AddRoyalMedallionBuffWithDroneHandling(body, buffDuration);
            UnityEngine.Object.Destroy(this.baseObject);
        }

        public static bool CanSpawnPickup()
        {
            return instanceCount < maxInstances;
        }

        private void Awake()
        {
            RoyalMedallionPickup.instanceCount++;
        }

        private void OnDestroy()
        {
            RoyalMedallionPickup.instanceCount--;
        }
    }
}
