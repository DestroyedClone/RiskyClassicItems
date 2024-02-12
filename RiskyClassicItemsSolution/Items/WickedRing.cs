using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using RoR2;
using UnityEngine;

namespace ClassicItemsReturns.Items
{
    public class WickedRing : ItemBase<WickedRing>
    {
        public ConfigEntry<bool> useAlternateVersion;
        public override string ItemName => "Wicked Ring";

        public override string ItemLangTokenName => "WICKEDRING";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("SkullRing");

        public override Sprite ItemIcon => LoadItemSprite("SkullRing");

        public float cdr = 1f;
        public float cdrStack = 0.5f;
        public float bonusCrit = 5f;
        public override object[] ItemFullDescriptionParams => new object[]
        {
            bonusCrit,
            cdr,
            cdrStack
        };


        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage, ItemTag.Utility
        };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            base.Init(config);
            if (useAlternateVersion.Value && ItemDef.DoesNotContainTag(ItemTag.OnKillEffect)) HG.ArrayUtils.ArrayAppend(ref ItemDef.tags, ItemTag.OnKillEffect);
        }

        public override void CreateConfig(ConfigFile config)
        {
            useAlternateVersion = config.Bind(ConfigCategory, "Use Rework", false, "Reworks the item into giving CDR on kill. If false, gives CDR on crit like in Risk of Rain 1.");
        }

        protected override void CreateLang()
        {
            base.CreateLang();
            LanguageOverrides.DeferToken("CLASSICITEMSRETURNS_ITEM_WICKEDRING_DESCRIPTION_ALT", cdr, cdrStack);
        }

        public override void Hooks()
        {
            base.Hooks();
            if (useAlternateVersion.Value)
            {
                ItemDef.pickupToken += "_ALT";
                ItemDef.descriptionToken += "_ALT";
                SharedHooks.OnCharacterDeath.OnCharacterDeathAttackerInventoryActions += CooldownOnKill;
            }
            else
            {
                RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
                SharedHooks.OnHitEnemy.OnHitEnemyAttackerInventoryActions += CooldownOnCrit;
            }
        }

        private void CooldownOnKill(GlobalEventManager globalEventManager, DamageReport damageReport, CharacterBody attackerBody, Inventory attackerInventory)
        {
            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (itemCount <= 0 || !attackerBody.skillLocator) return;

            int itemStack = itemCount - 1;
            float totalReduction = cdr + itemStack * cdrStack;
            attackerBody.skillLocator.DeductCooldownFromAllSkillsServer(totalReduction);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory && sender.inventory.GetItemCount(ItemDef) > 0) args.critAdd += bonusCrit;
        }

        private void CooldownOnCrit(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, CharacterBody attackerBody, Inventory attackerInventory)
        {
            if (!damageInfo.crit || damageInfo.procCoefficient <= 0f || !attackerBody.skillLocator) return;
            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            int itemStack = itemCount - 1;
            float totalReduction = cdr + itemStack * cdrStack;
            attackerBody.skillLocator.DeductCooldownFromAllSkillsServer(totalReduction * damageInfo.procCoefficient);
        }
    }
}
