using ClassicItemsReturns.Utils;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Items
{
    public class MuConstruct : ItemBase<MuConstruct>
    {
        public override string ItemName => "Mu Construct";

        public override string ItemLangTokenName => "MUCONSTRUCT";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadItemModel("MuConstruct");

        public override Sprite ItemIcon => LoadItemSprite("MuConstruct");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }
        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Healing
        };

        public override object[] ItemFullDescriptionParams => new object[]
        {
             MuConstructBehavior.healAmount * 100f,  MuConstructBehavior.initialCooldown,  MuConstructBehavior.cooldownReduction * 100f
        };

        public override void Hooks()
        {
            base.Hooks();

            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            //No network check because visuals will also be handled by this.
            body.AddItemBehavior<MuConstructBehavior>(body.inventory.GetItemCount(this.ItemDef));
        }
    }

    public class MuConstructBehavior : CharacterBody.ItemBehavior
    {
        public static float healAmount = 0.025f;
        public static float initialCooldown = 5f;
        public static float cooldownReduction = 0.25f;

        private float cooldown = 0f;

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
                cooldown = 5f / (Mathf.Max(1f, 1f + cooldownReduction * (this.stack - 1)));
            }
        }
    }
}
