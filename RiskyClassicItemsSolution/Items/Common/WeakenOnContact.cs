using BepInEx.Configuration;
using HG;
using R2API;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;
using ClassicItemsReturns.SharedHooks;

namespace ClassicItemsReturns.Items.Common
{
    public class WeakenOnContact : ItemBase<WeakenOnContact>
    {
        public override string ItemName => "The Toxin";

        public override string ItemLangTokenName => "WEAKENONCONTACT";
        public float damageBonus = 30;
        public float duration = 3;
        public float durationPerStack = 1.5f;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            damageBonus,
            duration,
            durationPerStack
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadItemModel("Toxin");

        public override Sprite ItemIcon => LoadItemSprite("Toxin");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.AIBlacklist
        };


        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            SharedHooks.ModifyFinalDamage.ModifyFinalDamageActions += ModifyDamage;
        }

        private static void ModifyDamage(SharedHooks.ModifyFinalDamage.DamageMult damageMult, DamageInfo damageInfo,
            HealthComponent victim, CharacterBody victimBody,
            CharacterBody attackerBody, Inventory attackerInventory)
        {
            if (victimBody.HasBuff(Buffs.WeakenOnContactBuff))
            {
                damageMult.damageMult += 0.2f;
                if (damageInfo.damageColorIndex == DamageColorIndex.Default) damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
            }
        }

        public class WeakenOnContactBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => Instance.ItemDef;

            public SphereSearch sphereSearch = new SphereSearch();

            public float age = 0;
            protected float timer = 1 / 8;

            //[Min(1E-45f)]
            public float tickRate = 1f;

            public float sizeCorrectionMultiplier = 4f;

            readonly float maxTickDuration = 0.1f;
            readonly float minTickDuration = 0.1f;
            float lerp_denominator = 2;

            public void OnEnable()
            {
                sphereSearch.mask = LayerIndex.entityPrecise.mask;
                sphereSearch.radius = body.radius * sizeCorrectionMultiplier;
                sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

                lerp_denominator = body.baseMoveSpeed * body.sprintingSpeedMultiplier * 2f + body.baseMoveSpeed;
            }

            public void FixedUpdate()
            {
                sphereSearch.radius = Mathf.Max(6f, body.radius * sizeCorrectionMultiplier);
                AdjustFrequencyBasedOnSpeed();

                age -= Time.fixedDeltaTime;
                if (age <= 0f)
                {
                    age = timer;
                    List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                    SearchForTargets(list);
                    if (list.Count == 0)
                        goto ReturnCollection;
                    var duration = ItemHelpers.StackingLinear(stack, Instance.duration, Instance.durationPerStack);
                    foreach (var hurtBox in list)
                    {
                        if (!hurtBox)
                            continue;
                        hurtBox.healthComponent.body.AddTimedBuffAuthority(Buffs.WeakenOnContactBuff.buffIndex, duration);
                    }
                ReturnCollection:
                    CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
                }
            }

            public void SearchForTargets(List<HurtBox> dest)
            {
                sphereSearch.origin = body.corePosition;
                sphereSearch.RefreshCandidates();
                sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex));
                sphereSearch.OrderCandidatesByDistance();
                sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                sphereSearch.GetHurtBoxes(dest);
                sphereSearch.ClearCandidates();
            }

            public void AdjustFrequencyBasedOnSpeed()
            {
                timer = Mathf.Lerp(minTickDuration, maxTickDuration, body.moveSpeed / lerp_denominator);
            }
        }
    }
}