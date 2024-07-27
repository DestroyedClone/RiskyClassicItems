using ClassicItemsReturns.Modules;
using RoR2.Projectile;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Configuration;

namespace ClassicItemsReturns.Items.Rare
{
    public class HitList : ItemBase<HitList>
    {
        public override string ItemName => "The Hit List";

        public override string ItemLangTokenName => "HITLIST";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("HitList");

        public override Sprite ItemIcon => LoadItemSprite("HitList");

        public float damagePerStack = 1f;
        //public int killsPerCycle = 30;

        public static NetworkSoundEventDef markEnemySound;
        public static GameObject markerPrefab;
        public static GameObject checkPrefab;

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.AIBlacklist
        };

        public override object[] ItemFullDescriptionParams => new object[]
        {
            damagePerStack,
            //killsPerCycle
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void CreateAssets(ConfigFile config)
        {
            markEnemySound = Assets.CreateNetworkSoundEventDef("Play_ClassicItemsReturns_HitListMark");

            markerPrefab = Assets.LoadObject("HitListMarker");
            markerPrefab.AddComponent<RoR2.Billboard>();

            checkPrefab = Assets.LoadObject("HitListCheck");
            checkPrefab.AddComponent<RoR2.Billboard>();
            checkPrefab.AddComponent<FadeOverDuration>();
            var effectComponent = checkPrefab.AddComponent<EffectComponent>();
            effectComponent.soundName = "Play_ClassicItemsReturns_HitList";
            var vfxAttributes = checkPrefab.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
            ContentAddition.AddEffect(checkPrefab);
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            Inventory.onInventoryChangedGlobal += onInventoryChangedGlobal_SetupHitListServer;
            CharacterBody.onBodyInventoryChangedGlobal += onBodyInventoryChangedGlobal_RemoveBuffsServer;
            RoR2.Stage.onServerStageBegin += onServerStageBegin_InitMinigame;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            CharacterBody.onBodyStartGlobal += onBodyStartGlobal_InitBuffs;
            On.RoR2.CharacterBody.OnClientBuffsChanged += CharacterBody_OnClientBuffsChanged;
        }

        private void CharacterBody_OnClientBuffsChanged(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody self)
        {
            orig(self);

            var controller = self.GetComponent<BodyHitListMarkerController>();
            if (self.HasBuff(Buffs.HitListEnemyMarker))
            {
                if (!controller)
                {
                    controller = self.gameObject.AddComponent<BodyHitListMarkerController>();
                    controller.body = self;
                }
            }
            else
            {
                if (controller) UnityEngine.Object.Destroy(controller);
            }
        }

        private void onBodyStartGlobal_InitBuffs(CharacterBody body)
        {
            if (!NetworkServer.active) return;

            if (body.master && body.master.inventory && body.master.inventory.GetItemCount(ItemDef) > 0)
            {

                var kc = body.master.gameObject.GetComponent<HitListKillCounterServer>();
                if (!kc) kc = body.master.gameObject.AddComponent<HitListKillCounterServer>();
                kc.SetBuffsServer();
            }
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.victimBody
                && damageReport.victimBody.teamComponent
                && damageReport.victimBody.HasBuff(Buffs.HitListEnemyMarker)
                && HitListMinigameController.instance
                && TeamMask.GetEnemyTeams(HitListMinigameController.instance.teamIndex)
                .HasTeam(damageReport.victimBody.teamComponent.teamIndex))
            {
                IncrementHitListServer();

                if (HitList.checkPrefab)
                {
                    EffectManager.SimpleEffect(HitList.checkPrefab, damageReport.victimBody.corePosition, Quaternion.identity, true);
                }
            }
        }

        private void onBodyInventoryChangedGlobal_RemoveBuffsServer(CharacterBody body)
        {
            if (!NetworkServer.active || !body.HasBuff(Buffs.HitListPlayerBuff)) return;

            bool hasHitList = body.inventory && body.inventory.GetItemCount(HitList.Instance.ItemDef) > 0;
            if (hasHitList) return;

            if (body.master)
            {
                var kc = body.master.GetComponent<HitListKillCounterServer>();
                if (kc) kc.RemoveAllBuffsServer();
            }
        }

        private void onServerStageBegin_InitMinigame(Stage obj)
        {
            if (HitListMinigameController.instance) UnityEngine.Object.Destroy(HitListMinigameController.instance.gameObject);
            if (Util.GetItemCountForTeam(TeamIndex.Player, ItemDef.itemIndex, false, true) > 0)
            {
                InitializeHitListMinigameServer();
            }
        }

        private void onInventoryChangedGlobal_SetupHitListServer(Inventory inv)
        {
            if (NetworkServer.active && inv.GetItemCount(ItemDef) > 0)
            {
                if (!HitListMinigameController.instance) InitializeHitListMinigameServer();

                var kc = inv.gameObject.GetComponent<HitListKillCounterServer>();
                if (!kc)
                {
                    kc = inv.gameObject.AddComponent<HitListKillCounterServer>();
                }
                kc.SetBuffsServer();
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(Buffs.HitListPlayerBuff);
            if (buffCount > 0)
            {
                args.baseDamageAdd += damagePerStack * buffCount;
                /*float damageBoost = 0f;
                int cycleCount = 1;
                while (buffCount > 0)
                {
                    float damagePerBuff = damagePerStack / (float)cycleCount;
                    if (buffCount > killsPerCycle)
                    {
                        damageBoost += damagePerBuff * killsPerCycle;
                    }
                    else
                    {
                        damageBoost += damagePerBuff * buffCount;
                    }
                    buffCount -= killsPerCycle;
                    cycleCount++;
                }
                args.baseDamageAdd += damageBoost;*/
            }
        }

        private void InitializeHitListMinigameServer()
        {
            if (!NetworkServer.active || HitListMinigameController.instance) return;

            GameObject obj = new GameObject();
            obj.AddComponent<HitListMinigameController>();
            GameObject.Instantiate(obj);
        }

        public static void IncrementHitListServer()
        {
            if (!NetworkServer.active) return;
            if (HitListMinigameController.instance)
            {
                int itemCount = HitListMinigameController.instance.GetTotalStacks();
                if (itemCount <= 0) return;

                var members = TeamComponent.GetTeamMembers(HitListMinigameController.instance.teamIndex).Where(teamComponent =>
                {
                    return teamComponent.body
                    && teamComponent.body.master
                    && teamComponent.body.inventory
                    && teamComponent.body.inventory.GetItemCount(HitList.Instance.ItemDef) > 0;
                });

                foreach (var member in members)
                {
                    var kc = member.body.master.GetComponent<HitListKillCounterServer>();
                    if (!kc) kc = member.body.master.gameObject.AddComponent<HitListKillCounterServer>();
                    kc.Increment();
                }
            }
        }
    }

    public class HitListMinigameController : MonoBehaviour
    {
        //This makes it simpler, but prevents this from being used for multiple teams at once.
        public static HitListMinigameController instance;

        public TeamIndex teamIndex = TeamIndex.Player;
        public static float markDuration = 5f;   //Time enemies are marked for
        public static float markDelay = 5f;  //Delay between marks

        private float timer;
        private Xoroshiro128Plus rng;


        public int GetTotalStacks()
        {
            return Util.GetItemCountForTeam(teamIndex, HitList.Instance.ItemDef.itemIndex, false, true);
        }

        private void Awake()
        {
            timer = markDelay;
            if (NetworkServer.active && !HitListMinigameController.instance && Run.instance)
            {
                HitListMinigameController.instance = this;
                rng = new Xoroshiro128Plus(Run.instance.seed);
                Debug.Log("ClassicItemsReturns: Initialized HitListMinigameController");
            }
            else
            {
                //Debug.Log("ClassicItemsReturns: Failed to initialize HitListMinigameController");
                Destroy(this.gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;
            timer -= Time.fixedDeltaTime;
            if (timer <= 0f)
            {
                timer += markDelay + markDuration;
                MarkEnemiesServer();
            }
        }

        private void OnDestroy()
        {
            if (HitListMinigameController.instance == this)
            {
                HitListMinigameController.instance = null;
                Debug.Log("ClassicItemsReturns: Destroying HitListMinigameController");
            }
        }

        public void ResetTimer()
        {
            timer = HitListMinigameController.markDelay;
        }

        //No need to track enemies marked, just use a timed buff.
        public void MarkEnemiesServer()
        {
            if (!NetworkServer.active) return;

            if (GetTotalStacks() <= 0)
            {
                Destroy(this.gameObject);
                return;
            }

            int toPick = GetTotalStacks();

            var validBodies = CharacterBody.readOnlyInstancesList.Where(body => {
                if (!body) return false;

                bool isEnemy = body.teamComponent && TeamMask.GetEnemyTeams(teamIndex).HasTeam(body.teamComponent.teamIndex);
                if (!isEnemy) return false;

                bool hasMaster = !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && body.master != null;
                if (!hasMaster) return false;

                bool isAlive = body.healthComponent && body.healthComponent.alive;
                bool isNotImmune = !body.HasBuff(RoR2Content.Buffs.Immune);
                bool hasHurtboxes = !body.disablingHurtBoxes;
                bool isNotProjectile = !body.GetComponent<ProjectileController>();

                return isAlive && isNotImmune && hasHurtboxes && isNotProjectile;
            }).ToList();

            toPick = Mathf.Min(validBodies.Count, toPick);

            while (toPick > 0)
            {
                int selectedIndex = rng.RangeInt(0, validBodies.Count);
                CharacterBody body = validBodies[selectedIndex];
                body.AddTimedBuff(Buffs.HitListEnemyMarker, HitListMinigameController.markDuration);
                validBodies.Remove(body);
                toPick--;

                if (HitList.markEnemySound)
                {
                    EffectManager.SimpleSoundEffect(HitList.markEnemySound.index, body.corePosition, true);
                }
            }
        }
    }

    public class HitListKillCounterServer : MonoBehaviour
    {
        CharacterMaster master;
        public int killCount = 0;

        public void Increment()
        {
            killCount++;
            SetBuffsServer();
        }

        public void SetBuffsServer()
        {
            if (!NetworkServer.active || killCount <= 0) return;
            if (!master)
            {
                master = base.GetComponent<CharacterMaster>();
                if (!master) return;
            }

            if (!master.inventory || master.inventory.GetItemCount(HitList.Instance.ItemDef) <= 0) return;

            CharacterBody body = master.GetBody();
            if (!body) return;

            //Killcount will never decrease, don't bother removing.
            int buffCount = body.GetBuffCount(Buffs.HitListPlayerBuff);
            if (buffCount < killCount)
            {
                for (int i = 0; i < killCount - buffCount; i++)
                {
                    body.AddBuff(Buffs.HitListPlayerBuff);
                }
            }
        }

        public void RemoveAllBuffsServer()
        {
            if (!NetworkServer.active) return;
            if (!master)
            {
                master = base.GetComponent<CharacterMaster>();
                if (!master) return;
            }

            CharacterBody body = master.GetBody();
            if (!body) return;

            int buffCount = body.GetBuffCount(Buffs.HitListPlayerBuff);
            for (int i = 0; i < buffCount; i++)
            {
                body.RemoveBuff(Buffs.HitListPlayerBuff);
            }
        }
    }

    public class BodyHitListMarkerController : MonoBehaviour
    {
        private GameObject markerInstance;
        public CharacterBody body;
        private bool destroying = false;

        private void Start()
        {
            if (!markerInstance)
            {
                markerInstance = GameObject.Instantiate(HitList.markerPrefab, base.transform);
            }
        }

        //FixedUpdate allows this to run clientside
        private void FixedUpdate()
        {
            if (destroying) return;

            if (body && body.healthComponent && !body.healthComponent.alive)
            {
                destroying = true;
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            if (markerInstance) Destroy(markerInstance);
        }
    }

    public class FadeOverDuration : MonoBehaviour
    {
        public float initialDelay = 1f;
        public float fadeTime = 1f;

        private float stopwatch;
        private float initialAlpha = 1f;
        private SpriteRenderer renderer;

        private void Awake()
        {
            stopwatch = 0f;
            renderer = base.GetComponentInChildren<SpriteRenderer>();
            if (renderer)
            {
                initialAlpha = renderer.color.a;
            }
            if (!renderer)
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            if (stopwatch >= initialDelay + fadeTime)
            {
                Destroy(this.gameObject);
                return;
            }

            stopwatch += Time.deltaTime;
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, Mathf.Lerp(1f, 0f, (stopwatch - initialAlpha) / fadeTime));
        }
    }
}
