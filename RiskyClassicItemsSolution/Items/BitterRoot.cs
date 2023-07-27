using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using RoR2;
using UnityEngine;

namespace ClassicItemsReturns.Items
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/BitterRoot.cs
    public class BitterRoot : ItemBase<BitterRoot>
    {
        public ConfigEntry<bool> useAlternateVersion;
        public override string ItemName => "Bitter Root";

        public override string ItemLangTokenName => "BITTERROOT";

        private float maxHealthMultiplier = 0.08f;
        private float maxHealthMultiplierStack = 0.08f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            (maxHealthMultiplier*100),
            (maxHealthMultiplierStack*100),
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadItemModel("Root");

        public override Sprite ItemIcon => LoadItemSprite("Root");
        public float alt_regenIncrease = 3;
        public float alt_duration = 3;
        public float alt_durationStack = 3;

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Healing
        };

        public override void Init(ConfigFile config)
        {
            base.Init(config);
            if (useAlternateVersion.Value && ItemDef.DoesNotContainTag(ItemTag.OnKillEffect)) HG.ArrayUtils.ArrayAppend(ref ItemDef.tags, ItemTag.OnKillEffect);
        }

        protected override void CreateLang()
        {
            base.CreateLang();
            //LanguageOverrides.DeferToken("CLASSICITEMSRETURNS_ITEM_BITTERROOT_NAME_ALT", "Bitter Root");
            //LanguageOverrides.DeferToken("CLASSICITEMSRETURNS_ITEM_BITTERROOT_PICKUP_ALT", "Regenerate health on kill.");
            LanguageOverrides.DeferToken("CLASSICITEMSRETURNS_ITEM_BITTERROOT_DESCRIPTION_ALT", alt_regenIncrease, alt_duration, alt_durationStack);
            //LanguageOverrides.DeferToken("CLASSICITEMSRETURNS_ITEM_BITTERROOT_LORE_ALT", "");
        }

        public override void CreateConfig(ConfigFile config)
        {
            useAlternateVersion = config.Bind(ConfigCategory, "Use Rework", true, "Reworks the item into providing health regen on kill since Bison Steak already exists. If false, gives +HP like in Risk of Rain 1.");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            if (useAlternateVersion.Value)
            {
                GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
                RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients_Alt;
                ItemDef.pickupToken += "_ALT";
                ItemDef.descriptionToken += "_ALT";
                return;
            }
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (TryGetCount(sender, out var count))
            {
                args.healthMultAdd += ItemHelpers.StackingLinear(count, maxHealthMultiplier, maxHealthMultiplierStack);
            }
        }

        private void GetStatCoefficients_Alt(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (Utils.ItemHelpers.TryGetBuffCount(sender, Buffs.BitterRootBuff, out int buffCount))
            {
                float levelFactor = 1f + 0.2f * (sender.level - 1f);
                args.baseRegenAdd += alt_regenIncrease * buffCount * levelFactor;
            }
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            if (obj != null && obj.attackerBody && obj.victim)
            {
                var itemCount = GetCount(obj.attackerMaster);
                if (itemCount > 0)
                {
                    obj.attackerBody.AddTimedBuff(Buffs.BitterRootBuff, ItemHelpers.StackingLinear(itemCount, alt_duration, alt_durationStack));
                }
            }
        }
    }
}