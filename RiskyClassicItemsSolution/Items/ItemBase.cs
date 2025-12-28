using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static ClassicItemsReturns.Utils.Components.MaterialControllerComponents;
using ClassicItemsReturns.Utils;

namespace ClassicItemsReturns.Items
{
    // The directly below is entirely from TILER2 API (by ThinkInvis) specifically the Item module. Utilized to implement instancing for classes.
    // TILER2 API can be found at the following places:
    // https://github.com/ThinkInvis/RoR2-TILER2
    // https://thunderstore.io/package/ThinkInvis/TILER2/

    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        //This, which you will see on all the -base classes, will allow both you and other modders to enter through any class with this to access internal fields/properties/etc as if they were a member inheriting this -Base too from this class.
        public static T Instance { get; private set; }

        public ItemBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class ItemBase
    {
        ///<summary>
        ///Name of the item.
        ///</summary>
        public abstract string ItemName { get; }

        ///<summary>
        ///Language Token Name responsible for the internals.
        ///</summary>
        public abstract string ItemLangTokenName { get; }

        ///<summary>
        ///The auto-generated token for the pickup.
        ///</summary>
        public string ItemPickupToken
        {
            get
            {
                return LanguageOverrides.LanguageTokenPrefix + "ITEM_" + ItemLangTokenName + "_PICKUP";
            }
        }

        ///<summary>
        ///Parameters for formatting the pickup language token. Parameter amount passed <b>must equal</b> the amount of parameters in the token.
        ///</summary>
        public virtual object[] ItemPickupDescParams { get; }

        ///<summary>
        ///The auto-generated token for the description.
        ///</summary>
        public string ItemDescriptionToken
        {
            get
            {
                return LanguageOverrides.LanguageTokenPrefix + "ITEM_" + ItemLangTokenName + "_DESCRIPTION";
            }
        }

        ///<summary>
        ///Parameters for formatting the description language token. Parameter amount passed <b>must equal</b> the amount of parameters in the token.
        ///</summary>
        public virtual object[] ItemFullDescriptionParams { get; }

        ///<summary>
        ///The auto-generated token for the lore.
        ///</summary>
        public virtual string ItemLoreToken
        {
            get
            {
                return LanguageOverrides.LanguageTokenPrefix + "ITEM_" + ItemLangTokenName + "_LORE";
            }
        }

        ///<summary>
        ///Parameters for formatting the lore language token. Parameter amount passed <b>must equal</b> the amount of parameters in the token.
        ///</summary>
        public virtual object[] ItemLoreParams { get; }

        /// <summary>
        /// Whether to have a token override the Item's description in the Logbook.
        /// <para>ItemDescriptionLogbookToken, ItemLogbookDescriptionParams</para>
        /// </summary>
        public virtual bool ItemDescriptionLogbookOverride { get; } = false;

        ///<summary>
        ///The auto-generated token for the logbook description override. Requires ItemDescriptionLogbookOverride enabled.
        ///</summary>
        public virtual string ItemDescriptionLogbookToken
        {
            get
            {
                return LanguageOverrides.LanguageTokenPrefix + "ITEM_" + ItemLangTokenName + "_LOGBOOK_DESCRIPTION";
            }
        }

        ///<summary>
        ///Parameters for formatting the lore language token. Parameter amount passed <b>must equal</b> the amount of parameters in the token. Requires ItemDescriptionLogbookOverride enabled.
        ///</summary>
        public virtual string[] ItemLogbookDescriptionParams { get; }

        ///<summary>
        ///The item's tier.
        ///</summary>
        public abstract ItemTier Tier { get; }

        ///<summary>
        ///The item's ItemTags.
        ///</summary>
        public virtual ItemTag[] ItemTags { get; set; } = new ItemTag[] { };

        ///<summary>
        ///The item's pickup model.
        ///</summary>
        public abstract GameObject ItemModel { get; }

        ///<summary>
        ///The item's icon sprite.
        ///</summary>
        public abstract Sprite ItemIcon { get; }

        ///<summary>
        ///The item's ItemDef.
        ///</summary>

        public ItemDef ItemDef;
        ///<summary>
        ///<b>Survivors of the Void</b>
        ///<br>The ItemDef that this is a corruption of.</br>
        ///<para>Example: DeprecateMeVoid's ContagiousOwnerItemDef = DeprecateMe.instance.ItemDef</para>
        ///</summary>

        public virtual ItemDef ContagiousOwnerItemDef { get; } = null;

        /// <summary>
        /// Whether or not this item can be stolen, dropped by an Umbra, etc.
        /// </summary>
        public virtual bool CanRemove { get; } = true;

        /// <summary>
        /// Force sets the ItemTags to include AIBlacklist.
        /// </summary>
        public virtual bool AIBlacklisted { get; set; } = false;

        /// <summary>
        /// The internal name of its parent equipment, so that when its Parent is disabled, so too will it as a child.
        /// <para>Ex: AppleConsumed has its ParentEquipmentName as "Apple". AppleConsumed loses its ability to be disabled and requires Apple to be disabled.</para>
        /// </summary>
        public virtual string ParentEquipmentName { get; } = null;

        /// <summary>
        /// The internal name of its parent item, so that when its Parent is disabled, so too will it as a child.
        /// <para>Ex: UseOnLowHealthItemConsumed has its ParentItemName as "UseOnLowHealthItem". UseOnLowHealthItemConsumed loses its ability to be disabled and requires UseOnLowHealthItem to be disabled.</para>
        /// </summary>
        public virtual string ParentItemName { get; } = null;

        /// <summary>
        /// Whether to hide this item in the inventory display, such as for internal items or tally items that need to persist across stages (MonsoonPlayerHelper).
        /// </summary>
        public virtual bool Hidden { get; } = false;
        /// <summary>
        /// If true, then won't get enabled unless the config setting for unfinished is also enabled.
        /// </summary>
        public virtual bool Unfinished { get; } = false;

        /// <summary>
        /// Autogenerated category name for the config.
        /// </summary>
        public string ConfigCategory
        {
            get
            {
                return "Item: " + ItemName;
            }
        }

        /// <summary>
        /// This method structures your code execution of this class. An example implementation inside of it would be:
        /// <para>CreateConfig(config);</para>
        /// <para>CreateLang();</para>
        /// <para>CreateItem();</para>
        /// <para>Hooks();</para>
        /// <para>This ensures that these execute in this order, one after another, and is useful for having things available to be used in later methods.</para>
        /// <para>P.S. CreateItemDisplayRules(); does not have to be called in this, as it already gets called in CreateItem();</para>
        /// </summary>
        /// <param name="config">The config file that will be passed into this from the main class.</param>
        public virtual void Init(ConfigFile config)
        {
            CreateLang();
            CreateConfig(config);
            CreateAssets(config);
            CreateItem();
            if (ItemDef) ClassicItemsReturnsPlugin.onFinishScanning += CreateCraftableDef;
            Hooks();
        }

        public virtual void CreateAssets(ConfigFile config)
        { }

        public virtual void CreateConfig(ConfigFile config)
        { }

        protected virtual void CreateCraftableDef()
        { }

        /// <summary>
        /// Responsible for handling token deferring.
        /// </summary>
        protected virtual void CreateLang() //create lang (addtokens for nwo) -> modify lang (this will be kept later)
        {
            bool formatPickup = ItemPickupDescParams?.Length > 0;
            bool formatDescription = ItemFullDescriptionParams?.Length > 0; //https://stackoverflow.com/a/41596301
            bool formatLogbook = ItemLogbookDescriptionParams?.Length > 0;
            bool formatLore = ItemLoreParams?.Length > 0;
            if (!formatDescription && !formatPickup && !formatLogbook && !formatLore)
            {
                //Main._logger.LogMessage("Nothing to format.");
                return;
            }

            if (formatPickup)
            {
                LanguageOverrides.DeferToken(ItemPickupToken, ItemPickupDescParams);
            }

            if (formatDescription)
            {
                LanguageOverrides.DeferToken(ItemDescriptionToken, ItemFullDescriptionParams);
            }

            if (formatLogbook)
            {
                LanguageOverrides.DeferToken(ItemDescriptionLogbookToken, ItemLogbookDescriptionParams);
            }

            if (formatLore)
            {
                LanguageOverrides.DeferToken(ItemLoreToken, ItemLoreParams);
            }

            if (ItemDescriptionLogbookOverride || formatLogbook)
            {
                LanguageOverrides.logbookTokenOverrideDict.Add(ItemDescriptionToken, ItemDescriptionLogbookToken);
            }
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateItem()
        {
            var prefix = LanguageOverrides.LanguageTokenPrefixItem;
            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = prefix + ItemLangTokenName;
            ItemDef.nameToken = prefix + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = ItemPickupToken;
            ItemDef.descriptionToken = ItemDescriptionToken;
            ItemDef.loreToken = ItemLoreToken;
            ItemDef.pickupModelPrefab = ItemModel;
            ItemDef.pickupIconSprite = ItemIcon;
            ItemDef.hidden = Hidden;
            ItemDef.canRemove = CanRemove;
            ItemDef.deprecatedTier = Tier;

            if (Tier == ItemTier.VoidTier1
            || Tier == ItemTier.VoidTier2
            || Tier == ItemTier.VoidTier3
            || Tier == ItemTier.VoidBoss)
            {
                ItemDef.requiredExpansion = Modules.DLCSupport.DLC1.GetSOTVExpansionDef();
                if (ContagiousOwnerItemDef)
                {
                    DLCSupport.DLC1.voidConversions.Add(ContagiousOwnerItemDef, ItemDef);
                }
            }

            if (ItemTags.Length > 0) { ItemDef.tags = ItemTags; }
            if (AIBlacklisted && ItemDef.DoesNotContainTag(ItemTag.AIBlacklist))
            {
                HG.ArrayUtils.ArrayAppend(ref ItemDef.tags, ItemTag.AIBlacklist);
            }
            else if (!AIBlacklisted && ItemDef.ContainsTag(ItemTag.AIBlacklist))
            {
                ItemDef.tags = ItemDef.tags.Where(tag => tag != ItemTag.AIBlacklist).ToArray();
            }    

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));
        }

        public virtual void Hooks()
        { }

        //Based on ThinkInvis' methods
        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCountEffective(ItemDef);
        }

        public bool TryGetCount(CharacterBody body, out int count)
        {
            count = 0;
            if (!body || !body.inventory) { return false; }
            count = body.inventory.GetItemCountEffective(ItemDef);
            return count > 0;
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCountEffective(ItemDef);
        }

        public bool TryGetCount(CharacterMaster master, out int count)
        {
            count = 0;
            if (!master || !master.inventory) { return false; }
            count = master.inventory.GetItemCountEffective(ItemDef);
            return count > 0;
        }

        public int GetCountSpecific(CharacterBody body, ItemDef itemDef)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCountEffective(itemDef);
        }

        public bool TryGetCountSpecific(CharacterBody body, ItemDef itemDef, out int count)
        {
            count = 0;
            if (!body || !body.inventory) { return false; }
            count = body.inventory.GetItemCountEffective(itemDef);
            return true;
        }

        /// <summary>
        /// Loads an item Sprite from the AssetBundle.
        /// </summary>
        /// <param name="itemNameToken">langNameToken for the item. Defaults to current Item's langNameToken.</param>
        /// <returns></returns>
        /*public Sprite LoadItemIcon(string itemNameToken = "")
        {
            var token = itemNameToken == "" ? ItemLangTokenName : itemNameToken;
            return Modules.Assets.LoadSprite($"tex{token}Icon");
        }

        /// <summary>
        /// Loads a GameObject from the AssetBundle.
        /// </summary>
        /// <param name="itemNameToken">langNameToken for the item. Defaults to current Item's langNameToken.</param>
        /// <returns></returns>
        public GameObject LoadPickupModel(string itemNameToken = "")
        {
            var token = itemNameToken == "" ? ItemLangTokenName : itemNameToken;
            return Modules.Assets.LoadObject($"Pickup{token}.prefab");
        }*/

        public Sprite LoadSprite(string spriteName)
        {
            return Modules.Assets.LoadSprite(spriteName);
        }

        public GameObject LoadModel(string modelName)
        {
            return Modules.Assets.LoadObject(modelName);
        }

        public Sprite LoadItemSprite(string spriteName)
        {
            return Utils.ItemHelpers.LoadItemSprite(spriteName);
        }

        public GameObject LoadItemModel(string modelName)
        {
            GameObject mdl3d = Modules.Assets.LoadObject("mdl3d" + modelName);
            if (mdl3d)
            {
                ItemHelpers.SetupMaterials(mdl3d);

                if (!mdl3d.GetComponent<ItemDisplay>())
                {
                    ItemDisplay itemDisplay = mdl3d.AddComponent<ItemDisplay>();
                    var renderers = mdl3d.GetComponentsInChildren<MeshRenderer>();
                    List<CharacterModel.RendererInfo> rendererInfos = new List<CharacterModel.RendererInfo>();
                    foreach (Renderer r in renderers)
                    {
                        var rendererInfo = new CharacterModel.RendererInfo();
                        rendererInfo.renderer = r;
                        rendererInfo.defaultMaterial = r.material;
                        rendererInfo.defaultShadowCastingMode = r.shadowCastingMode;
                        rendererInfo.ignoreOverlays = false;
                        rendererInfo.hideOnDeath = false;
                        rendererInfos.Add(rendererInfo);
                    }
                    itemDisplay.rendererInfos = rendererInfos.ToArray();
                }
            }

            GameObject mdlRet = Modules.Assets.LoadObject("mdl" + modelName);
            GameObject mdlClassic = Modules.Assets.LoadObject("mdlClassic" + modelName);
            Queue<GameObject> modelQueue = new Queue<GameObject>();

            if (ClassicItemsReturnsPlugin.use3dModels)
            {
                if (mdl3d) return mdl3d;
                if (mdlRet) return mdlRet;
                if (mdlClassic) return mdlClassic;
            }
            else
            {
                if (!ClassicItemsReturnsPlugin.useClassicSprites)
                {
                    if (mdlRet) return mdlRet;
                    if (mdlClassic) return mdlClassic;
                }
                else
                {
                    if (mdlClassic) return mdlClassic;
                    if (mdlRet) return mdlRet;
                }

                if (mdl3d) return mdl3d;
            }

            Debug.LogError("ClassicItemsReturns: Could not find ANY model for " + modelName);
            return null;
        }
    }
}