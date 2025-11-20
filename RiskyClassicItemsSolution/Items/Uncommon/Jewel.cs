using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace ClassicItemsReturns.Items.Uncommon
{
    public class Jewel : ItemBase<Jewel>
    {
        public static ConfigEntry<bool> disableInBazaar;

        public override string ItemName => "Locked Jewel";

        public override string ItemLangTokenName => "JEWEL";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Jewel");

        public override Sprite ItemIcon => LoadItemSprite("Jewel");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Utility,
            ItemTag.InteractableRelated,
            ItemTag.AIBlacklist,
            ItemTag.DevotionBlacklist,
            ItemTag.CanBeTemporary,
            ItemTag.Technology
        };

        public static NetworkSoundEventDef activationSound;

        public int moneyToGrant = 8;
        public float barrierInitial = 0.25f;
        public float barrierStack = 0.15f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            barrierInitial * 100f, barrierStack * 100f, moneyToGrant
        };

        public override void Init(ConfigFile config)
        {
            base.Init(config);
            activationSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            activationSound.eventName = "Play_ClassicItemsReturns_Jewel";
            ContentAddition.AddNetworkSoundEventDef(activationSound);
        }

        public override void CreateConfig(ConfigFile config)
        {
            disableInBazaar = config.Bind(ConfigCategory, "Disable in Bazaar", true, "Disable this item in the Bazaar.");
        }


        public override void Hooks()
        {
            base.Hooks();
            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (disableInBazaar.Value && SceneCatalog.GetSceneDefForCurrentScene() == ClassicItemsReturnsPlugin.bazaarScene) return;
            InteractionProcFilter ipf = interactableObject.GetComponent<InteractionProcFilter>();
            if (ipf && !ipf.shouldAllowOnInteractionBeginProc) return;
            if (interactableObject.GetComponent<GenericPickupController>()) return;
            if (interactableObject.GetComponent<VehicleSeat>()) return;
            if (interactableObject.GetComponent<NetworkUIPromptController>())
            {
                if (!interactableObject.GetComponent<PurchaseInteraction>()) return;
            }

            CharacterBody interactorBody = interactor.GetComponent<CharacterBody>();
            if (!interactorBody || !interactorBody.inventory) return;

            int itemCount = interactorBody.inventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;


            //Copied from Executive Card
            //JewelOrb extends GoldOrb to grant barrier as well
            if (Run.instance && Stage.instance)
            {
                JewelOrb goldOrb = new JewelOrb();
                Orb orb = goldOrb;
                GameObject purchasedObject = interactableObject;
                Vector3? vector;
                if (purchasedObject == null)
                {
                    vector = null;
                }
                else
                {
                    Transform transform = purchasedObject.transform;
                    vector = transform != null ? new Vector3?(transform.position) : null;
                }
                orb.origin = vector ?? interactorBody.corePosition;
                goldOrb.target = interactorBody.mainHurtBox;
                goldOrb.goldAmount = (uint)Run.instance.GetDifficultyScaledCost(moneyToGrant, Stage.instance.entryDifficultyCoefficient);
                goldOrb.barrierPercentGrant = barrierInitial + barrierStack * (itemCount - 1);
                OrbManager.instance.AddOrb(goldOrb);
            }
        }
    }

    public class JewelOrb : GoldOrb
    {
        public float barrierPercentGrant = 0f;
        public override void OnArrival()
        {
            base.OnArrival();
            if (target)
            {
                if (target.healthComponent && barrierPercentGrant > 0f)
                {
                    target.healthComponent.AddBarrier(barrierPercentGrant * target.healthComponent.fullCombinedHealth);
                }
                if (target.transform && Jewel.activationSound)
                {
                    EffectManager.SimpleSoundEffect(Jewel.activationSound.index, target.transform.position, true);
                }
            }
        }
    }
}
