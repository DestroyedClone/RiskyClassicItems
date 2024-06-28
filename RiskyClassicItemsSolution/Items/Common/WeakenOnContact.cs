using BepInEx.Configuration;
using HG;
using R2API;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicItemsReturns.Items.Common
{
    public class WeakenOnContact : ItemBase<WeakenOnContact>
    {
        public override string ItemName => "The Toxin";

        public override string ItemLangTokenName => "WEAKENONCONTACT";
        public float armorReduction = 30;
        public float duration = 3;
        public float durationPerStack = 3;

        public override object[] ItemFullDescriptionParams => new object[]
        {
            armorReduction,
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
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (ItemHelpers.TryGetBuffCount(sender, Buffs.WeakenOnContactBuff, out var _))
            {
                args.armorAdd -= Instance.armorReduction;
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
                //If I set it to just basespeed*sprintingspeedmultiplier*2, then the base movement, will just
                //base = 7, which already makes the check frequency hit halfway, so gotta offset it
            }

            public void FixedUpdate()
            {
                sphereSearch.radius = Mathf.Max(4f, body.radius * sizeCorrectionMultiplier);
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