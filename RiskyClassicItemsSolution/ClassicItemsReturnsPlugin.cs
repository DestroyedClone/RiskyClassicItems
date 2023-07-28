using BepInEx;
using ClassicItemsReturns.ReadmeGenerator;
using R2API.Utils;
using ClassicItemsReturns.Artifact;
using ClassicItemsReturns.Equipment;
using ClassicItemsReturns.Equipment.EliteEquipment;
using ClassicItemsReturns.Items;
using ClassicItemsReturns.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//https://github.com/Bubbet/Risk-Of-Rain-Mods/blob/04540d86404e5ab8609742be568ed579e0176ac0/BubbetsItems/BubbetsItemsPlugin.cs
using SearchableAttribute = HG.Reflection.SearchableAttribute;

[assembly: SearchableAttribute.OptIn]

namespace ClassicItemsReturns
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(ModSupport.ModCompatBetterUI.guid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ModSupport.ModCompatRiskOfOptions.guid, BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class ClassicItemsReturnsPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.DestroyedClone.ClassicItemsReturns";
        public const string ModName = "Classic Items Returns";
        public const string ModVer = "0.2.0";

        //For RiskOfOptions
        public const string ModDescription = "Adds items and equipment from Risk of Rain & Returns.";

        public static PluginInfo PInfo { get; private set; }
        public static bool useClassicSprites = true;    //TODO: IMPLEMENT TOGGLE IF REAL MODELS BECOME A THING

        public static List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        public static List<ItemBase> Items = new List<ItemBase>();
        public static List<EquipmentBase> Equipments = new List<EquipmentBase>();
        public static List<EliteEquipmentBase> EliteEquipments = new List<EliteEquipmentBase>();

        //Provides a direct access to this plugin's logger for use in any of your other classes.
        public static BepInEx.Logging.ManualLogSource ModLogger;

        public const bool debug = true;

        public static void ModDebugLog(object data)
        {
            if (debug)
                ModLogger.LogMessage(data);
        }

        private void Awake()
        {
            PInfo = Info;
            ModLogger = Logger;
            Assets.Init();
            DLCSupport.Initialize();
            ModSupport.CheckForModSupport();
            LanguageOverrides.Initialize();
            Deployables.Initialize();
            Buffs.Initialize();
            RiskyVisuals.Init(Config);
            Events.Initialize();
            Orbs.Initialize();
            AddToAssembly();
            Dots.Initialize();
            //ReadmeGeneratorMain.Init();
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
        }
        private void Start()
        {
            SoundBanks.Init();
        }

        private void AddToAssembly()
        {
            //this section automatically scans the project for all equipment
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));
            var loadedEquipmentNames = new List<string>();
            var childEquipmentTypes = new List<EquipmentBase>();

            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                if (equipment.ParentEquipmentName != null)
                {
                    childEquipmentTypes.Add(equipment);
                    continue;
                }

                if (ValidateEquipment(equipment, Equipments))
                {
                    equipment.Init(Config);
                    loadedEquipmentNames.Add(equipment.EquipmentName);
                }
            }

            foreach (var childEquip in childEquipmentTypes)
            {
                if (loadedEquipmentNames.Contains(childEquip.ParentEquipmentName))
                    childEquip.Init(Config);
            }

            //this section automatically scans the project for all elite equipment
            var EliteEquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            foreach (var eliteEquipmentType in EliteEquipmentTypes)
            {
                EliteEquipmentBase eliteEquipment = (EliteEquipmentBase)System.Activator.CreateInstance(eliteEquipmentType);
                if (ValidateEliteEquipment(eliteEquipment, EliteEquipments))
                {
                    eliteEquipment.Init(Config);
                }
            }

            //This section automatically scans the project for all items
            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));
            var loadedItemNames = new List<string>();
            var childItemTypes = new List<ItemBase>();

            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                if (item.ParentEquipmentName != null || item.ParentItemName != null)
                {
                    childItemTypes.Add(item);
                    continue;
                }
                if (ValidateItem(item, Items))
                {
                    item.Init(Config);
                    loadedItemNames.Add(item.ItemName);
                }
            }

            foreach (var childItem in childItemTypes)
            {
                if (loadedItemNames.Contains(childItem.ParentItemName)
                || loadedEquipmentNames.Contains(childItem.ParentEquipmentName))
                {
                    //dependent children dont have rights, no validation.
                    //if (ValidateItem(childItem, Items))
                    {
                        childItem.Init(Config);
                    }
                }
            }

            //Run this at the end because Clover auto-disables if 56 Leaf Clover is disabled.
            //This section automatically scans the project for all artifacts
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);
                }
            }
        }

        /// <summary>
        /// A helper to easily set up and initialize an artifact from your artifact classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="artifact">A new instance of an ArtifactBase class."</param>
        /// <param name="artifactList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;

            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }

        /// <summary>
        /// A helper to easily set up and initialize an item from your item classes if the user has it enabled in their configuration files.
        /// <para>Additionally, it generates a configuration for each item to allow blacklisting it from AI.</para>
        /// </summary>
        /// <param name="item">A new instance of an ItemBase class."</param>
        /// <param name="itemList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = Config.Bind(item.ConfigCategory, "Enable Item?", true, "Should this item appear in runs?").Value;
            bool itemAlreadyHasBlacklist = item.ItemTags.Contains(RoR2.ItemTag.AIBlacklist);
            var aiBlacklist = Config.Bind(item.ConfigCategory, "Blacklist Item from AI Use?", itemAlreadyHasBlacklist, "Should the AI not be able to obtain this item?").Value;
            if (enabled)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
            }
            return enabled;
        }

        /// <summary>
        /// A helper to easily set up and initialize an equipment from your equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="equipment">A new instance of an EquipmentBase class."</param>
        /// <param name="equipmentList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            if (Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }

        /// <summary>
        /// A helper to easily set up and initialize an elite equipment from your elite equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="eliteEquipment">A new instance of an EliteEquipmentBase class.</param>
        /// <param name="eliteEquipmentList">The list you would like to add this to if it passes the config check.</param>
        /// <returns></returns>
        public bool ValidateEliteEquipment(EliteEquipmentBase eliteEquipment, List<EliteEquipmentBase> eliteEquipmentList)
        {
            var enabled = Config.Bind<bool>("Equipment: " + eliteEquipment.EliteEquipmentName, "Enable Elite Equipment?", true, "Should this elite equipment appear in runs? If disabled, the associated elite will not appear in runs either.").Value;

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }
    }
}