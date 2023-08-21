using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RoR2.CombatDirector;

namespace ClassicItemsReturns.Equipment.EliteEquipment
{
    public abstract class EliteEquipmentBase<T> : EliteEquipmentBase where T : EliteEquipmentBase<T>
    {
        public static T Instance { get; private set; }

        public EliteEquipmentBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting EliteEquipmentBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class EliteEquipmentBase
    {
        public abstract string EliteEquipmentName { get; }

        /// <summary>
        /// The lang token that will be used in registering most of your strings.
        /// <para>E.g.: AFFIXHYPERCHARGED</para>
        /// </summary>
        public abstract string EliteAffixToken { get; }

        public virtual string EliteEquipmentPickupToken
        {
            get
            {
                return LanguageOverrides.LanguageTokenPrefixEliteEquipment + EliteAffixToken + "_PICKUP";
            }
        }

        /// <summary>
        /// Optional parameters for the Equipment Pickup Token
        /// </summary>
        public virtual string[] EliteEquipmentPickupDescParams { get; }

        public virtual string EquipmentDescriptionToken
        {
            get
            {
                return LanguageOverrides.LanguageTokenPrefixEquipment + EliteAffixToken + "_DESCRIPTION";
            }
        }

        /// <summary>
        /// Optional parameters for the Equipment Description Token
        /// </summary>
        public virtual string[] EliteEquipmentFullDescriptionParams { get; }

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;

        public virtual bool CanDrop { get; } = false;

        public virtual float Cooldown { get; } = 60f;

        public virtual bool EnigmaCompatible { get; } = false;
        public virtual bool CanBeRandomlyTriggered { get; } = false;

        public virtual bool IsBoss { get; } = false;

        public virtual bool IsLunar { get; } = false;

        ///<summary>
        ///The required ExpansionDef for this artifact.
        ///</summary>
        public virtual ExpansionDef ExpansionDef { get; }

        public abstract GameObject EliteEquipmentModel { get; }
        public abstract Sprite EliteEquipmentIcon { get; }

        public EquipmentDef EliteEquipmentDef;

        public BuffDef EliteBuffDef;

        /// <summary>
        /// Implement before calling CreateEliteEquipment.
        /// </summary>
        public abstract Sprite EliteBuffIcon { get; }

        /// <summary>
        /// If not overriden, the elite can spawn in all tiers defined.
        /// </summary>
        public virtual EliteTierDef[] CanAppearInEliteTiers { get; set; } = EliteAPI.GetCombatDirectorEliteTiers();

        /// <summary>
        /// If you want the elite to have an overlay with your custom material.
        /// </summary>
        public virtual Material EliteMaterial { get; set; } = null;

        /// <summary>
        /// How many more/less times the health should the elite have compared to its standard form?
        /// </summary>
        public virtual float HealthMultiplier { get; set; } = 1;

        /// <summary>
        /// How many more/less times the damage should the elite have compared to its standard form?
        /// </summary>
        public virtual float DamageMultiplier { get; set; } = 1;

        public EliteDef EliteDef;

        /// <summary>
        /// This method structures your code execution of this class. An example implementation inside of it would be:
        /// <para>CreateConfig(config);</para>
        /// <para>CreateLang();</para>
        /// <para>CreateBuff();</para>
        /// <para>CreateEquipment();</para>
        /// <para>CreateElite();</para>
        /// <para>Hooks();</para>
        /// <para>This ensures that these execute in this order, one after another, and is useful for having things available to be used in later methods.</para>
        /// <para>P.S. CreateItemDisplayRules(); does not have to be called in this, as it already gets called in CreateEquipment();</para>
        /// </summary>
        /// <param name="config">The config file that will be passed into this from the main class.</param>
        public abstract void Init(ConfigFile config);

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateLang()
        {
            bool formatPickup = EliteEquipmentPickupDescParams?.Length > 0;
            //Main._logger.LogMessage("pickupCheck");
            bool formatDescription = EliteEquipmentFullDescriptionParams?.Length > 0; //https://stackoverflow.com/a/41596301
            //Main._logger.LogMessage("descCheck");
            if (formatDescription && formatPickup)
            {
                //Main._logger.LogMessage("Nothing to format.");
                return;
            }

            if (formatPickup)
            {
                LanguageOverrides.DeferToken(EliteEquipmentPickupToken, EliteEquipmentPickupDescParams);
            }

            if (formatDescription)
            {
                LanguageOverrides.DeferToken(EquipmentDescriptionToken, EliteEquipmentFullDescriptionParams);
            }
        }

        protected void CreateEquipment()
        {
            var prefix = LanguageOverrides.LanguageTokenPrefixEliteEquipment;
            if (EliteBuffDef == null)
            {
                EliteBuffDef = Modules.Buffs.CreateEliteBuffDef(EliteAffixToken, new Color32(255, 255, 255, byte.MaxValue), null, EliteBuffIcon, null);
            }

            EliteEquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            EliteEquipmentDef.name = prefix + EliteAffixToken;
            EliteEquipmentDef.nameToken = prefix + EliteAffixToken + "_NAME";
            EliteEquipmentDef.pickupToken = prefix + EliteAffixToken + "_PICKUP";
            EliteEquipmentDef.descriptionToken = prefix + EliteAffixToken + "_DESCRIPTION";
            EliteEquipmentDef.pickupModelPrefab = EliteEquipmentModel;
            EliteEquipmentDef.pickupIconSprite = EliteEquipmentIcon;
            EliteEquipmentDef.appearsInSinglePlayer = AppearsInSinglePlayer;
            EliteEquipmentDef.appearsInMultiPlayer = AppearsInMultiPlayer;
            EliteEquipmentDef.canDrop = CanDrop;
            EliteEquipmentDef.cooldown = Cooldown;
            EliteEquipmentDef.enigmaCompatible = EnigmaCompatible;
            EliteEquipmentDef.isBoss = IsBoss;
            EliteEquipmentDef.isLunar = IsLunar;
            EliteEquipmentDef.passiveBuffDef = EliteBuffDef;
            EliteEquipmentDef.canBeRandomlyTriggered = false;
            EliteEquipmentDef.dropOnDeathChance = 0.00025f;
            EliteEquipmentDef.requiredExpansion = null;
            EliteEquipmentDef.unlockableDef = null;

            ItemAPI.Add(new CustomEquipment(EliteEquipmentDef, CreateItemDisplayRules()));

            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;

            if (UseTargeting && TargetingIndicatorPrefabBase)
            {
                On.RoR2.EquipmentSlot.Update += UpdateTargeting;
            }

            if (EliteMaterial)
            {
                On.RoR2.CharacterBody.FixedUpdate += OverlayManager;
            }
        }

        private void OverlayManager(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            if (self.modelLocator && self.modelLocator.modelTransform && self.HasBuff(EliteBuffDef))
            {
                var eliteOverlayManagers = self.gameObject.GetComponents<EliteOverlayManager>();
                EliteOverlayManager eliteOverlayManager = null;
                if (!eliteOverlayManagers.Any())
                {
                    eliteOverlayManager = self.gameObject.AddComponent<EliteOverlayManager>();
                }
                else
                {
                    foreach (EliteOverlayManager overlayManager in eliteOverlayManagers)
                    {
                        if (overlayManager.EliteBuffDef == EliteBuffDef)
                        {
                            orig(self);
                            return;
                        }
                    }

                    RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                    overlay.duration = float.PositiveInfinity;
                    overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    overlay.animateShaderAlpha = true;
                    overlay.destroyComponentOnEnd = true;
                    overlay.originalMaterial = EliteMaterial;
                    overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());

                    if (!eliteOverlayManager) { eliteOverlayManager = self.gameObject.AddComponent<EliteOverlayManager>(); }
                    eliteOverlayManager.Overlay = overlay;
                    eliteOverlayManager.Body = self;
                    eliteOverlayManager.EliteBuffDef = EliteBuffDef;
                }
            }
            orig(self);
        }

        public class EliteOverlayManager : MonoBehaviour
        {
            public RoR2.TemporaryOverlay Overlay;
            public RoR2.CharacterBody Body;
            public BuffDef EliteBuffDef;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(EliteBuffDef))
                {
                    UnityEngine.Object.Destroy(Overlay);
                    UnityEngine.Object.Destroy(this);
                }
            }
        }

        protected void CreateElite()
        {
            EliteDef = ScriptableObject.CreateInstance<EliteDef>();
            EliteDef.name = LanguageOverrides.LanguageTokenPrefixElite + EliteAffixToken;
            EliteDef.modifierToken = LanguageOverrides.LanguageTokenPrefixElite + EliteAffixToken + "_MODIFIER";
            EliteDef.eliteEquipmentDef = EliteEquipmentDef;

            var baseEliteTierDefs = EliteAPI.GetCombatDirectorEliteTiers();
            if (!CanAppearInEliteTiers.All(x => baseEliteTierDefs.Contains(x)))
            {
                var distinctEliteTierDefs = CanAppearInEliteTiers.Except(baseEliteTierDefs);

                foreach (EliteTierDef eliteTierDef in distinctEliteTierDefs)
                {
                    var indexToInsertAt = Array.FindIndex(baseEliteTierDefs, x => x.costMultiplier >= eliteTierDef.costMultiplier);
                    if (indexToInsertAt >= 0)
                    {
                        EliteAPI.AddCustomEliteTier(eliteTierDef, indexToInsertAt);
                    }
                    else
                    {
                        EliteAPI.AddCustomEliteTier(eliteTierDef);
                    }
                    baseEliteTierDefs = EliteAPI.GetCombatDirectorEliteTiers();
                }
            }

            EliteAPI.Add(new CustomElite(EliteDef, CanAppearInEliteTiers));

            EliteBuffDef.eliteDef = EliteDef;
            ContentAddition.AddBuffDef(EliteBuffDef);
        }

        protected bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EliteEquipmentDef)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        /// <summary>
        /// Must be implemented, but you can just return false if you don't want an On Use effect for your elite equipment.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        protected abstract bool ActivateEquipment(EquipmentSlot slot);

        public abstract void Hooks();

        #region Targeting Setup

        //Targeting Support
        public virtual bool UseTargeting { get; } = false;

        public GameObject TargetingIndicatorPrefabBase = null;

        public enum TargetingType
        {
            Enemies,
            Friendlies,
        }

        public virtual TargetingType TargetingTypeEnum { get; } = TargetingType.Enemies;

        //Based on MysticItem's targeting code.
        protected void UpdateTargeting(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
        {
            orig(self);

            if (self.equipmentIndex == EliteEquipmentDef.equipmentIndex)
            {
                var targetingComponent = self.GetComponent<TargetingControllerComponent>();
                if (!targetingComponent)
                {
                    targetingComponent = self.gameObject.AddComponent<TargetingControllerComponent>();
                    targetingComponent.VisualizerPrefab = TargetingIndicatorPrefabBase;
                }

                if (self.stock > 0)
                {
                    switch (TargetingTypeEnum)
                    {
                        case (TargetingType.Enemies):
                            targetingComponent.ConfigureTargetFinderForEnemies(self);
                            break;

                        case (TargetingType.Friendlies):
                            targetingComponent.ConfigureTargetFinderForFriendlies(self);
                            break;
                    }
                }
                else
                {
                    targetingComponent.Invalidate();
                    targetingComponent.Indicator.active = false;
                }
            }
        }

        public class TargetingControllerComponent : MonoBehaviour
        {
            public GameObject TargetObject;
            public GameObject VisualizerPrefab;
            public Indicator Indicator;
            public BullseyeSearch TargetFinder;
            public Action<BullseyeSearch> AdditionalBullseyeFunctionality = (search) => { };

            public void Awake()
            {
                Indicator = new Indicator(gameObject, null);
            }

            public void OnDestroy()
            {
                Invalidate();
            }

            public void Invalidate()
            {
                TargetObject = null;
                Indicator.targetTransform = null;
            }

            public void ConfigureTargetFinderBase(EquipmentSlot self)
            {
                if (TargetFinder == null) TargetFinder = new BullseyeSearch();
                TargetFinder.teamMaskFilter = TeamMask.allButNeutral;
                TargetFinder.teamMaskFilter.RemoveTeam(self.characterBody.teamComponent.teamIndex);
                TargetFinder.sortMode = BullseyeSearch.SortMode.Angle;
                TargetFinder.filterByLoS = true;
                Ray ray = CameraRigController.ModifyAimRayIfApplicable(self.GetAimRay(), self.gameObject, out float _);
                TargetFinder.searchOrigin = ray.origin;
                TargetFinder.searchDirection = ray.direction;
                TargetFinder.maxAngleFilter = 10f;
                TargetFinder.viewer = self.characterBody;
            }

            public void ConfigureTargetFinderForEnemies(EquipmentSlot self)
            {
                ConfigureTargetFinderBase(self);
                TargetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(self.characterBody.teamComponent.teamIndex);
                TargetFinder.RefreshCandidates();
                TargetFinder.FilterOutGameObject(self.gameObject);
                AdditionalBullseyeFunctionality(TargetFinder);
                PlaceTargetingIndicator(TargetFinder.GetResults());
            }

            public void ConfigureTargetFinderForFriendlies(EquipmentSlot self)
            {
                ConfigureTargetFinderBase(self);
                TargetFinder.teamMaskFilter = TeamMask.none;
                TargetFinder.teamMaskFilter.AddTeam(self.characterBody.teamComponent.teamIndex);
                TargetFinder.RefreshCandidates();
                TargetFinder.FilterOutGameObject(self.gameObject);
                AdditionalBullseyeFunctionality(TargetFinder);
                PlaceTargetingIndicator(TargetFinder.GetResults());
            }

            public void PlaceTargetingIndicator(IEnumerable<HurtBox> TargetFinderResults)
            {
                HurtBox hurtbox = TargetFinderResults.Any() ? TargetFinderResults.First() : null;

                if (hurtbox)
                {
                    TargetObject = hurtbox.healthComponent.gameObject;
                    Indicator.visualizerPrefab = VisualizerPrefab;
                    Indicator.targetTransform = hurtbox.transform;
                }
                else
                {
                    Invalidate();
                }
                Indicator.active = hurtbox;
            }
        }

        #endregion Targeting Setup
    }
}