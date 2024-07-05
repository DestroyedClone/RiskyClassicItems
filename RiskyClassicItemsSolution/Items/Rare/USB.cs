using BepInEx.Configuration;
using ClassicItemsReturns.Modules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items.Rare
{
    public class USB : ItemBase<USB>
    {
        public override string ItemName => "Classified Access Codes";

        public override string ItemLangTokenName => "USB";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("USB");

        public override Sprite ItemIcon => LoadItemSprite("USB");

        public override bool unfinished => true;

        public static GameObject atlasCannonNetworkPrefab;

        private static bool cannonActivated = false;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.AIBlacklist,
            ItemTag.Damage
        };

        public static float CalcDamagePercent(int itemCount)
        {
            return 1f - (0.6f * Mathf.Pow(0.8f, itemCount - 1));
        }

        public override void CreateAssets(ConfigFile config)
        {
            atlasCannonNetworkPrefab = Assets.LoadObject("AtlasCannonTarget");
            atlasCannonNetworkPrefab.AddComponent<RoR2.Billboard>();
            atlasCannonNetworkPrefab.AddComponent<NetworkIdentity>();
            var controller = atlasCannonNetworkPrefab.AddComponent<AtlasCannonController>();
            atlasCannonNetworkPrefab.AddComponent<DestroyOnTimer>().duration = controller.delayBeforeFiring + controller.lifetimeAfterFiring + 2f;
            ContentAddition.AddNetworkedObject(atlasCannonNetworkPrefab);
        }

        public override void Hooks()
        {
            RoR2.Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            On.RoR2.BossGroup.OnMemberAddedServer += BossGroup_OnMemberAddedServer;
        }

        //TODO: NARROW DOWN ACTIVATION CONDITIONS ON THIS
        private void BossGroup_OnMemberAddedServer(On.RoR2.BossGroup.orig_OnMemberAddedServer orig, BossGroup self, CharacterMaster memberMaster)
        {
            orig(self, memberMaster);

            if (true)   //cannonActivated
            {
                CharacterBody body = memberMaster.GetBody();
                if (body)
                {
                    GameObject cannonObject = UnityEngine.Object.Instantiate(atlasCannonNetworkPrefab, body.transform);
                    NetworkServer.Spawn(cannonObject);
                }
            }
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            cannonActivated = false;
        }
    }

    public class AtlasCannonController : NetworkBehaviour
    {
        public float delayBeforeFiring = 5f;
        public float lifetimeAfterFiring = 3f;

        [SyncVar]
        private bool _hasFired = false;
        private bool hasFiredLocal = false;

        private SpriteRenderer crosshairRenderer, rotatorRenderer;
        private Transform rotatorTransform;
        private uint soundId;

        private float stopwatch;
        private float rotationStopwatch;

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
            }
            stopwatch = 0f;
            soundId = Util.PlaySound("Play_captain_utility_variant_laser_loop", base.gameObject);
        }

        private void FixedUpdate()
        {
            //Hide targeting indicator after laser has been fired
            if (_hasFired && !hasFiredLocal)
            {
                hasFiredLocal = true;
                AkSoundEngine.StopPlayingID(soundId);
                Util.PlaySound("Play_captain_utility_variant_impact", base.gameObject);
                if (crosshairRenderer) crosshairRenderer.enabled = false;
                if (rotatorRenderer) rotatorRenderer.enabled = false;
            }

            if (NetworkServer.active) FixedUpdateServer();
        }

        private void FixedUpdateServer()
        {
            stopwatch += Time.fixedDeltaTime;
            if (!_hasFired)
            {
                if (stopwatch >= delayBeforeFiring)
                {
                    FireCannonServer();
                }
            }
            else
            {
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
            //TODO: fire cannon
        }

        //Controls how fast the center thing rotates
        private void Update()
        {
            if (hasFiredLocal || !rotatorTransform) return;
            rotationStopwatch += Time.deltaTime;
            if (rotationStopwatch >= 1f) rotationStopwatch -= 1f;
            rotatorTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 360f * rotationStopwatch));
        }

        private void OnDestroy()
        {
            if (!hasFiredLocal) AkSoundEngine.StopPlayingID(soundId);
        }
    }
}
