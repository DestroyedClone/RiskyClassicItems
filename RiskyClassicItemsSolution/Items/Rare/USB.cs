using BepInEx.Configuration;
using ClassicItemsReturns.Modules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        public static GameObject explosionEffectPrefab;

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

            explosionEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BFG/BeamSphereExplosion.prefab")
                .WaitForCompletion().InstantiateClone("ClassicItemsReturns_AtlasCannonExplosionEffect", false);
            explosionEffectPrefab.GetComponent<EffectComponent>().soundName = "";
            ContentAddition.AddEffect(explosionEffectPrefab);

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
                if (body && body.healthComponent)
                {
                    GameObject cannonObject = UnityEngine.Object.Instantiate(atlasCannonNetworkPrefab, body.transform);
                    AtlasCannonController controller = cannonObject.GetComponent<AtlasCannonController>();
                    if (controller)
                    {
                        controller.targetHealthComponent = body.healthComponent;
                    }
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
        public float lifetimeAfterFiring = 1f;
        public HealthComponent targetHealthComponent;
        public float laserFireWidth = 15f;

        [SyncVar]
        private bool _hasFired = false;
        private bool hasFiredLocal = false;

        private LineRenderer laser;
        private SpriteRenderer crosshairRenderer, rotatorRenderer;
        private Transform rotatorTransform;
        private uint soundId;

        private float stopwatch;
        private float rotationStopwatch;
        private float laserFadeStopwatch;

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

                Transform laserTransform = cl.FindChild("Laser");
                if(laserTransform)
                {
                    laser = laserTransform.GetComponent<LineRenderer>();
                }
            }
            stopwatch = 0f;
            rotationStopwatch = 0f;
            laserFadeStopwatch = 0f;
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
                laser.widthMultiplier = laserFireWidth;
                stopwatch = 0f;
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
            else if (hasFiredLocal)
            {
                //Wait for hasFiredLocal since some VFX rely on that
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

            if (targetHealthComponent)
            {
                int itemCount = Mathf.Max(1, Util.GetItemCountForTeam(TeamIndex.Player, USB.Instance.ItemDef.itemIndex, false, true));
                float damage = targetHealthComponent.fullCombinedHealth * USB.CalcDamagePercent(itemCount);
                Vector3 damagePosition = targetHealthComponent.body ? targetHealthComponent.body.corePosition : base.transform.position;
                targetHealthComponent.TakeDamage(new DamageInfo()
                {
                    damage = damage,
                    damageType = DamageType.BypassArmor | DamageType.BypassBlock,
                    attacker = null,
                    canRejectForce = true,
                    force = Vector3.zero,
                    crit = false,
                    damageColorIndex = DamageColorIndex.Item,
                    dotIndex = DotController.DotIndex.None,
                    inflictor = base.gameObject,
                    position = damagePosition,
                    procChainMask = default,
                    procCoefficient = 0f
                });

                if (USB.explosionEffectPrefab)
                {
                    EffectManager.SimpleEffect(USB.explosionEffectPrefab, damagePosition, UnityEngine.Random.rotation, true);
                }
            }
        }

        //Controls how fast the center thing rotates
        private void Update()
        {
            
            if (!hasFiredLocal)
            {
                if (laser)
                {
                    laser.SetPositions(new Vector3[]
                    {
                    base.transform.position + Vector3.up * 1000f,
                    base.transform.position + Vector3.down * 1000f,
                    });
                }

                if (rotatorTransform)
                {
                    rotationStopwatch += Time.deltaTime;
                    if (rotationStopwatch >= 2f) rotationStopwatch -= 2f;
                    rotatorTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 360f * rotationStopwatch));
                }
            }
            else
            {
                laserFadeStopwatch += Time.deltaTime;
                float laserFadePercent = 1f - (laserFadeStopwatch / lifetimeAfterFiring);
                laser.widthMultiplier = laserFireWidth * laserFadePercent;
            }
        }

        private void OnDestroy()
        {
            if (!hasFiredLocal) AkSoundEngine.StopPlayingID(soundId);
        }
    }
}
