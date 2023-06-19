using BepInEx.Configuration;
using HG;
using R2API;
using RiskyClassicItems.Modules;
using RiskyClassicItems.Utils;
using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;

namespace RiskyClassicItems.Items
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

        public override GameObject ItemModel => LoadPickupModel("WeakenOnContact");

        public override Sprite ItemIcon => LoadItemIcon("WeakenOnContact");

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
            protected float timer = 0.5f;

            //[Min(1E-45f)]
            public float tickRate = 1f;

            public float sizeCorrectionMultiplier = 1.3f;

            public void OnEnable()
            {
                this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
                this.sphereSearch.radius = body.radius * sizeCorrectionMultiplier;
                this.sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            }

            public void FixedUpdate()
            {
                age -= Time.fixedDeltaTime;
                if (age < 0) //floats are effectively impossible to resolve to 0
                {
                    age = timer;
                    List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                    SearchForTargets(list);
                    if (list.Count == 0)
                        return;
                    var duration = Utils.ItemHelpers.StackingLinear(stack, Instance.duration, Instance.durationPerStack);
                    foreach (var hurtBox in list)
                    {
                        if (!hurtBox)
                            continue;
                        hurtBox.healthComponent.body.AddTimedBuff(Buffs.WeakenOnContactBuff, duration);
                    }
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
        }
    }
}