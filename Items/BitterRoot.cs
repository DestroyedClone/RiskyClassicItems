using BepInEx.Configuration;
using R2API;
using RiskyClassicItems.Modules;
using RiskyClassicItems.Utils;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Items
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/BitterRoot.cs
    public class BitterRoot : ItemBase<BitterRoot>
    {
        public ConfigEntry<bool> useAlternateVersion;
        public override string ItemName => "BitterRoot";

        public override string ItemLangTokenName => "BITTERROOT";

        float maxHealthMultiplier = 0.07f;
        float maxHealthMultiplierStack = 0.07f;
        public override string[] ItemFullDescriptionParams => new string[]
        {
            (maxHealthMultiplier*100).ToString(),
            (maxHealthMultiplierStack*100).ToString(),
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadModel();

        public override Sprite ItemIcon => LoadSprite();
        public float alt_regenIncrease = 3;
        public float alt_duration = 3;
        public float alt_durationStack = 3;

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
            useAlternateVersion = config.Bind(ConfigCategory, "Use LiT", false, "If true, uses LiT's implementation.");
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
                RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
                ItemDef.descriptionToken += "_ALT";
                return;
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.HasBuff(Buffs.BitterRootBuff))
            {
                args.baseRegenAdd += alt_regenIncrease;
            }
            if (TryGetCount(sender, out var count))
            {
                args.healthMultAdd += ItemHelpers.StackingLinear(count, maxHealthMultiplier, maxHealthMultiplierStack);
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