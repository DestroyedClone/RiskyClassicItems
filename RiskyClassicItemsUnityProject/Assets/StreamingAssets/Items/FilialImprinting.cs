using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.Items.BaseItemBodyBehavior;
using System.Collections.Generic;
using static RiskyClassicItems.Items.FilialImprinting;
using RoR2.Items;

namespace RiskyClassicItems.Items
{
    public class FilialImprinting : ItemBase<FilialImprinting>
    {
        public override string ItemName => "FilialImprinting";

        public override string ItemLangTokenName => ItemName.ToUpper();

        public float cooldown = 20f;
        public float damageCoefficient = 1f;
        public float movespeedCoefficient = 1f;
        public float regenBoost = 1;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            cooldown,
            (damageCoefficient * 100),
            (movespeedCoefficient * 100),
            regenBoost,
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadPickupModel("FilialImprinting");

        public override Sprite ItemIcon => LoadItemIcon("FilialImprinting");

        public static GameObject strangeCreatureFollower;
        public static GameObject strangeCreatureFollowerChild;
        public static GameObject buffPickupObject;

        public override void CreateAssets(ConfigFile config)
        {
            base.CreateAssets(config);

            //var strangeCreatureMesh = Assets.mainAssetBundle.LoadAsset<Mesh>("Assets/Models/mdlStrangeCreature.fbx");

            strangeCreatureFollower = PrefabAPI.InstantiateClone(Modules.Assets.LoadAddressable<GameObject>("RoR2/Base/PassiveHealing/HealingFollower.prefab"), "RCI_StrangeCreatureFollower");
            //UnityEngine.Object.Destroy(strangeCreatureFollower.transform.Find("Indicator").gameObject);
            strangeCreatureFollower.transform.Find("Indicator").gameObject.SetActive(false);
            var healingFollowerController = strangeCreatureFollower.GetComponent<HealingFollowerController>();
            healingFollowerController.rotationAngularVelocity = 0f;
            healingFollowerController.healingInterval = Mathf.Infinity;
            //strangeCreatureFollower.transform.Find("Offset/Effect").GetComponent<MeshFilter>().mesh = strangeCreatureMesh;
            var strangeCreatureFollowerDisplay = Assets.LoadObject("Assets/Prefabs/FollowerStrangeCreature.prefab");
            strangeCreatureFollowerDisplay.transform.localScale = Vector3.one * 0.25f;

            var followerdisplay1 = Object.Instantiate(strangeCreatureFollowerDisplay, strangeCreatureFollower.transform.Find("Offset/Effect"));
            followerdisplay1.transform.localPosition = Vector3.zero;


            var displaycomp = strangeCreatureFollower.AddComponent<StrangeCreatureController>();
            displaycomp.healingFollowerController = healingFollowerController;

            buffPickupObject = PrefabAPI.InstantiateClone(Assets.LoadAddressable<GameObject>("RoR2/Base/BonusGoldPackOnKill/BonusMoneyPack.prefab"), "RCI_BuffPickup");
            var trigger = buffPickupObject.transform.Find("PackTrigger").gameObject;
            var moneycomp = trigger.GetComponent<MoneyPickup>();
            var buffcomp = trigger.gameObject.AddComponent<BuffPickup>();
            buffcomp.baseObject = moneycomp.baseObject;
            buffcomp.teamFilter = moneycomp.teamFilter;

            strangeCreatureFollowerChild = PrefabAPI.InstantiateClone(new GameObject(), "RCI_StrangeCreatureFollowerChild", false);
            strangeCreatureFollowerChild.AddComponent<NetworkIdentity>();
            PrefabAPI.RegisterNetworkPrefab(strangeCreatureFollowerChild);
            var followerdisplay2 = Object.Instantiate(strangeCreatureFollowerDisplay, strangeCreatureFollowerChild.transform);
            followerdisplay2.transform.localPosition = Vector3.zero;
            followerdisplay2.transform.localScale = Vector3.one * 0.25f;
            followerdisplay1.transform.localPosition = new Vector3(0, -0.7f, 0);
            //strangeCreatureFollowerChild.AddComponent<MeshFilter>().mesh = strangeCreatureMesh;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public class RCI_FilialImprintingBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => FilialImprinting.Instance.ItemDef;
            public BuffDef damageBuff => RoR2Content.Buffs.FullCrit;
            public BuffDef regenBuff => RoR2Content.Buffs.CrocoRegen;
            public BuffDef movespeedBuff => RoR2Content.Buffs.CloakSpeed;

            public float stopwatch = 0;
            public float cooldown => FilialImprinting.Instance.cooldown;

            public HealingFollowerController strangeCreatureFollower;
            public StrangeCreatureController strangeCreatureController;


            public void Start()
            {
                if (!strangeCreatureFollower && NetworkServer.active)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(FilialImprinting.strangeCreatureFollower, base.transform.position, Quaternion.identity);
                    strangeCreatureFollower = gameObject.GetComponent<HealingFollowerController>();
                    strangeCreatureFollower.NetworkownerBodyObject = base.gameObject;
                    strangeCreatureController = gameObject.GetComponent<StrangeCreatureController>();
                    strangeCreatureController.filialImprintingItemBehavior = this;
                    NetworkServer.Spawn(gameObject);
                }
            }

            public void OnDestroy()
            {
                UnityEngine.Object.Destroy(strangeCreatureFollower.gameObject);
                strangeCreatureFollower = null;
            }

            public void FixedUpdate()
            {
                stopwatch -= Time.fixedDeltaTime;
                if (stopwatch <= 0)
                {
                    stopwatch = cooldown;
                    DropRandomBuffs();
                }
            }

            public void DropRandomBuffs()
            {
                if (!NetworkServer.active)
                    return;
                foreach (var child in strangeCreatureController.children)
                {
                    if (!child)
                        continue;
                    DropRandomBuff(child.position);
                }
            }

            public void DropRandomBuff(Vector3 position)
            {
                bool success = Util.CheckRoll(33);

                if (success)
                {
                    BuffDef[] buffs = { damageBuff, regenBuff, movespeedBuff };

                    int randomIndex = UnityEngine.Random.Range(0, buffs.Length);
                    DropBuff(buffs[randomIndex], position);
                }
            }


            public void DropBuff(BuffDef buff, Vector3 position)
            {
                var pickup = UnityEngine.Object.Instantiate(buffPickupObject);
                pickup.transform.position = position;
                BuffPickup buffPickup = pickup.transform.Find("PackTrigger").GetComponent<BuffPickup>();
                buffPickup.buffDef = buff;
                buffPickup.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
                NetworkServer.Spawn(pickup);
            }
        }
        public class StrangeCreatureController : MonoBehaviour
        {
            public HealingFollowerController healingFollowerController;
            public RCI_FilialImprintingBehavior filialImprintingItemBehavior;
            public CharacterMotor ownerMotor;
            public bool isGrounded = true;
            public List<Transform> children = new List<Transform>();

            public float acceleration = 20f;
            public float damping = 0.1f;
            public bool enableSpringMotion;
            private Vector3 velocity;

            public void FixedUpdate()
            {
                //isnt assigned
                //isGrounded = ownerMotor.isGrounded;

                var difference = (filialImprintingItemBehavior.stack - 1) - children.Count;
                if (difference != 0)
                {
                    if (difference > 0)
                    {
                        for (int i = 0; i < difference; i++)
                        {
                            AddChild();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < -difference; i++)
                        {
                            RemoveChild();
                        }
                    }
                }

            }

            public void AddChild()
            {
                var clone = UnityEngine.Object.Instantiate(strangeCreatureFollowerChild);
                children.Add(clone.transform);
            }

            public void RemoveChild()
            {
                int lastIndex = children.Count - 1;
                if (lastIndex >= 0)
                {
                    var child = children[lastIndex];
                    UnityEngine.Object.Destroy(child.gameObject);
                    children.RemoveAt(lastIndex);
                }
            }


            private void Update()
            {
                var childToFollow = transform;
                foreach (var child in children)
                {
                    if (!child)
                        continue;
                    UpdateMotion(child, childToFollow);
                    child.position += velocity * Time.deltaTime;
                    childToFollow = child;
                }
            }

            private Vector3 GetTargetPosition(Transform gameObjectToFollow)
            {
                if (gameObjectToFollow)
                    return gameObjectToFollow.position;
                GameObject hfcGameObject = healingFollowerController.targetBodyObject ?? healingFollowerController.ownerBodyObject;
                if (!hfcGameObject)
                    return healingFollowerController.transform.position;
                return transform.position;
            }
            private Vector3 GetDesiredPosition(Transform gameObjectToFollow)
            {
                return GetTargetPosition(gameObjectToFollow);
            }
            private void UpdateMotion(Transform child, Transform gameObjectToFollow)
            {
                Vector3 desiredPosition = GetDesiredPosition(gameObjectToFollow);
                if (enableSpringMotion)
                {
                    Vector3 lhs = desiredPosition - child.position;
                    if (lhs != Vector3.zero)
                    {
                        Vector3 a = lhs.normalized * acceleration;
                        Vector3 b = velocity * -damping;
                        velocity += (a + b) * Time.deltaTime;
                        return;
                    }
                }
                else
                {
                    child.position = Vector3.SmoothDamp(child.position, desiredPosition, ref velocity, damping);
                }
            }

        }
    }
}