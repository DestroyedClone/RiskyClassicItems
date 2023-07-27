using BepInEx.Configuration;
using HG;
using R2API;
using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils;
using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicItemsReturns.Items
{
    public class WeakenOnContact : ItemBase<WeakenOnContact>
    {
        public override string ItemName => "The Toxin";

        public override string ItemLangTokenName => "WEAKENONCONTACT";
        public float armorReduction = 20;
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
            protected float timer = 1/8;

            //[Min(1E-45f)]
            public float tickRate = 1f;

            public float sizeCorrectionMultiplier = 4f;

            readonly float maxTickDuration = 1f / 12f;
            readonly float minTickDuration = 1f / 8f;
            float lerp_denominator = 2;

            public void OnEnable()
            {
                this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
                this.sphereSearch.radius = body.radius * sizeCorrectionMultiplier;
                this.sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

                lerp_denominator = (body.baseMoveSpeed * body.sprintingSpeedMultiplier * 2f) + body.baseMoveSpeed;
                //If I set it to just basespeed*sprintingspeedmultiplier*2, then the base movement, will just
                //base = 7, which already makes the check frequency hit halfway, so gotta offset it
            }

            public void FixedUpdate()
            {
                this.sphereSearch.radius = Mathf.Max(4f, body.radius * sizeCorrectionMultiplier);
                AdjustFrequencyBasedOnSpeed();

                age -= Time.fixedDeltaTime;
                if (age <= 0f)
                {
                    age = timer;
                    List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                    SearchForTargets(list);
                    if (list.Count == 0)
                        goto ReturnCollection;
                    var duration = Utils.ItemHelpers.StackingLinear(stack, Instance.duration, Instance.durationPerStack);
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
                this.sphereSearch.origin = body.corePosition;
                this.sphereSearch.RefreshCandidates();
                this.sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex));
                this.sphereSearch.OrderCandidatesByDistance();
                this.sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                this.sphereSearch.GetHurtBoxes(dest);
                this.sphereSearch.ClearCandidates();
            }

            public void AdjustFrequencyBasedOnSpeed()
            {
                timer = Mathf.Lerp(minTickDuration, maxTickDuration, body.moveSpeed / lerp_denominator);
            }
        }
    }
}