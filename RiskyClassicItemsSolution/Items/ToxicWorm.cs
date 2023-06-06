using BepInEx.Configuration;
using HG;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using static RoR2.Items.BaseItemBodyBehavior;

namespace RiskyClassicItems.Items
{
    public class ToxicWorm : ItemBase<ToxicWorm>
    {
        public override string ItemName => "Toxic Worm";

        public override string ItemLangTokenName => "TOXICWORM";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadModel();

        public override Sprite ItemIcon => LoadSprite();

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
        }

        //HealNearbyController
        public class ToxicWormBehaviour : MonoBehaviour
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => ToxicWorm.Instance.ItemDef;

            public float cooldown = 1f;
            public float stopwatch = 0;

            public SphereSearch sphereSearch;

            private void OnEnable()
            {
                sphereSearch = new SphereSearch();
            }

            private void OnDisable()
            {

            }

            private void FixedUpdate()
            {
                stopwatch -= Time.fixedDeltaTime;
                if (stopwatch <= 0)
                {
                    stopwatch = cooldown;
                    ShittyAffectNearby();
                }
            }
            protected void SearchForTargets(List<HurtBox> dest)
            {
                TeamMask enemies = TeamMask.AllExcept(TeamIndex.Player);
                    this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
                this.sphereSearch.origin = this.transform.position;
                this.sphereSearch.radius = 3f;
                this.sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                this.sphereSearch.RefreshCandidates();
                this.sphereSearch.FilterCandidatesByHurtBoxTeam(enemies);
                this.sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                this.sphereSearch.GetHurtBoxes(dest);
                this.sphereSearch.ClearCandidates();
            }

            private void ShittyAffectNearby()
            {
                List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                this.SearchForTargets(list);
                foreach (var box in list)
                {
                    if (box && box.healthComponent && box.healthComponent.alive)
                    {
                        ApplyToxicWormEffectToTarget(box);
                    }
                }
            }

            private void ApplyToxicWormEffectToTarget(HurtBox box)
            {
                //Utils.ItemHelpers.AddBuffAndDot()
            }
        }
    }
}