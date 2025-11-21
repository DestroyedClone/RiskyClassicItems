using BepInEx.Configuration;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items.Common
{
    public class MuConstruct : ItemBase<MuConstruct>
    {
        public override string ItemName => "Mu Construct";

        public override string ItemLangTokenName => "MUCONSTRUCT";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadItemModel("MuConstruct");

        public override Sprite ItemIcon => LoadItemSprite("MuConstruct");

        public static ConfigEntry<bool> disableFollower;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Healing,
            ItemTag.CanBeTemporary
        };

        public override object[] ItemFullDescriptionParams => new object[]
        {
             MuConstructBehavior.healAmount * 100f,  MuConstructBehavior.initialCooldown,  MuConstructBehavior.cooldownReduction * 100f
        };


        public override void Init(ConfigFile config)
        {
            base.Init(config);
            MuConstructBehavior.followerPrefab = Modules.Assets.LoadObject("MuConstructFollower");
            ItemHelpers.SetupMaterials(MuConstructBehavior.followerPrefab);
        }

        public override void CreateConfig(ConfigFile config)
        {
            disableFollower = config.Bind(ConfigCategory, "Disable Follower", false, "Disable the follower visual.");
        }

        public override void Hooks()
        {
            base.Hooks();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<MuConstructBehavior>(body.inventory.GetItemCount(ItemDef));
        }

        protected override void CreateCraftableDef()
        {
            CraftableDef toBiggerConstruct = ScriptableObject.CreateInstance<CraftableDef>();
            toBiggerConstruct.pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/MinorConstructOnKill/MinorConstructOnKill.asset").WaitForCompletion();
            toBiggerConstruct.recipes = new Recipe[]
            {
                new Recipe()
                {
                    amountToDrop = 1,
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = ItemDef
                        },
                        new RecipeIngredient()
                        {
                            pickup = ItemDef
                        }
                    }
                },
                new Recipe()
                {
                    amountToDrop = 1,
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = ItemDef
                        },
                        new RecipeIngredient()
                        {
                            pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Squid/Squid.asset").WaitForCompletion()
                        }
                    }
                }
            };
            (toBiggerConstruct as ScriptableObject).name = "cdMuToAlpha";
            PluginContentPack.craftableDefs.Add(toBiggerConstruct);
        }
    }

    public class MuConstructBehavior : CharacterBody.ItemBehavior
    {
        public static float healAmount = 0.025f;
        public static float initialCooldown = 5f;
        public static float cooldownReduction = 0.25f;

        private float cooldown = 0f;

        public static float followerRotationTime = 8f;  //time it takes to fully rotate
        private float followerRotationStopwatch = 0f;
        public static GameObject followerPrefab;
        public GameObject followerInstance;
        private float followerHeightOffset = 0f;
        private float followerSideOffset = 0f;

        private void FixedUpdate()
        {
            if (NetworkServer.active) FixedUpdateServer();
        }

        private void FixedUpdateServer()
        {
            if (!IsTeleActivatedTracker.teleporterActivated) return;
            cooldown -= Time.fixedDeltaTime;
            if (cooldown <= 0f && body.healthComponent)
            {
                body.healthComponent.HealFraction(healAmount, default);
                cooldown = 5f / Mathf.Max(1f, 1f + cooldownReduction * (stack - 1));
            }
        }

        #region follower
        public void InitFollower()
        {
            if (followerInstance || !followerPrefab || MuConstruct.disableFollower.Value) return;
            followerInstance = Instantiate(followerPrefab);
            followerInstance.transform.position = body.transform.position;

            Vector3 desiredPosition = GetDesiredPosition();
            followerInstance.transform.position = desiredPosition;

            //Debug.Log("Body Radius: " + body.radius);
            followerHeightOffset = 1.3f;
            followerSideOffset = 1.5f;
        }

        private void OnDisable()
        {
            if (followerInstance) Destroy(followerInstance);
        }

        private void Start()
        {
            InitFollower();
            if (followerInstance) followerInstance.SetActive(IsTeleActivatedTracker.teleporterActivated);
        }

        private void Update()
        {
            if (!followerInstance) return;

            followerInstance.SetActive(IsTeleActivatedTracker.teleporterActivated);

            if (body.modelLocator && body.modelLocator.modelTransform)
            {
                followerInstance.transform.rotation = body.modelLocator.modelTransform.rotation;
            }

            followerRotationStopwatch += Time.deltaTime;
            if (followerRotationStopwatch >= followerRotationTime) followerRotationStopwatch -= followerRotationTime;
            followerInstance.transform.Rotate(Vector3.forward, 360f * followerRotationStopwatch / followerRotationTime, Space.Self);

            Vector3 desiredPosition = GetDesiredPosition();
            followerInstance.transform.position = Vector3.SmoothDamp(followerInstance.transform.position, desiredPosition, ref velocity, 0.05f);

            followerInstance.transform.position += velocity * Time.deltaTime;
        }

        private Vector3 GetDesiredPosition()
        {

            Vector3 basePosition = body.transform.position;
            Vector3 offset = body.inputBank ? body.inputBank.aimDirection : Vector3.forward;
            offset.y = 0;
            offset.Normalize();
            offset = Quaternion.AngleAxis(90f, Vector3.up) * offset * -followerSideOffset;
            offset.y = followerHeightOffset;

            return basePosition + offset;
        }

        private Vector3 velocity = Vector3.zero;
        #endregion
    }
}
