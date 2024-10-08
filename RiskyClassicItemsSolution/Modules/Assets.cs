﻿using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ClassicItemsReturns.Modules
{
    internal class Assets
    {
        internal const string unlockableDefPrefix = "ClassicItemsReturns.";
        internal const string unlockableDefItemPrefix = "ClassicItemsReturns.";
        internal const string unlockableDefEquipmentPrefix = "ClassicItemsReturns.";
        internal const string prefabPrefix = "ClassicItemsReturns_";

        internal static GameObject NullModel = LoadAddressable<GameObject>("RoR2/Base/Core/NullModel.prefab");
        internal static Sprite NullSprite = LoadAddressable<Sprite>("RoR2/Base/Core/texNullIcon.png");

        internal static ItemTierDef itemLunarTierDef = LoadAddressable<ItemTierDef>("RoR2/Base/Common/LunarTierDef.asset");
        internal static ItemTierDef itemBossTierDef = LoadAddressable<ItemTierDef>("RoR2/Base/Common/BossTierDef.asset");
        internal static ItemTierDef itemTier1Def = LoadAddressable<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset");
        internal static ItemTierDef itemTier2Def = LoadAddressable<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset");
        internal static ItemTierDef itemTier3Def = LoadAddressable<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset");
        internal static ItemTierDef itemVoidBossTierDef = LoadAddressable<ItemTierDef>("RoR2/DLC1/Common/VoidBossDef.asset");
        internal static ItemTierDef itemVoidTier1Def = LoadAddressable<ItemTierDef>("RoR2/DLC1/Common/VoidTier1Def.asset");
        internal static ItemTierDef itemVoidTier2Def = LoadAddressable<ItemTierDef>("RoR2/DLC1/Common/VoidTier2Def.asset");
        internal static ItemTierDef itemVoidTier3Def = LoadAddressable<ItemTierDef>("RoR2/DLC1/Common/VoidTier3Def.asset");

        public static ItemTierDef ResolveTierDef(ItemTier itemTier)
        {
            switch (itemTier)
            {
                case ItemTier.AssignedAtRuntime:
                    return null;

                case ItemTier.Boss:
                    return itemBossTierDef;

                case ItemTier.Lunar:
                    return itemLunarTierDef;

                case ItemTier.NoTier:
                    return null;

                case ItemTier.Tier1:
                    return itemTier1Def;

                case ItemTier.Tier2:
                    return itemTier2Def;

                case ItemTier.Tier3:
                    return itemTier3Def;

                case ItemTier.VoidBoss:
                    return itemVoidBossTierDef;

                case ItemTier.VoidTier1:
                    return itemVoidTier1Def;

                case ItemTier.VoidTier2:
                    return itemVoidTier2Def;

                case ItemTier.VoidTier3:
                    return itemVoidTier3Def;
            }
            return null;
        }

        public static AssetBundle mainAssetBundle;
        public const string bundleName = "classicitemsreturnsbundle";
        public const string assetBundleFolder = "AssetBundles";

        public static string AssetBundlePath
        {
            get
            {
                //This returns the path to your assetbundle assuming said bundle is on the same folder as your DLL. If you have your bundle in a folder, you can uncomment the statement below this one.
                // return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.PInfo.Location), bundleName);
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ClassicItemsReturnsPlugin.PInfo.Location), assetBundleFolder, bundleName);
            }
        }

        //WHY DOES THIS EXIST???
        public static T LoadAddressable<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }

        public static UnlockableDef CreateUnlockableDef(string name, Sprite icon = null)
        {
            UnlockableDef unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockableDef.achievementIcon = icon;
            unlockableDef.cachedName = unlockableDefPrefix + name;
            return unlockableDef;
        }

        public static void Init()
        {
            PopulateAssets();
        }

        public static void PopulateAssets()
        {
            mainAssetBundle = AssetBundle.LoadFromFile(AssetBundlePath);

            //Async causes this to not work properly
            ShaderSwapper.ShaderSwapper.UpgradeStubbedShaders(mainAssetBundle);
        }

        public static Sprite LoadSprite(string path)
        {
            try
            {
                return mainAssetBundle.LoadAsset<Sprite>(path);
            }
            catch
            {
                ClassicItemsReturnsPlugin.ModLogger.LogError($"Modules.Assets.LoadSprite failed to load path \"{path}\", defaulting to Modules.Assets.NullSprite.");
                return Modules.Assets.NullSprite;
            }
        }

        public static GameObject LoadObject(string path)
        {
            try
            {
                return mainAssetBundle.LoadAsset<GameObject>(path);
            }
            catch
            {
                ClassicItemsReturnsPlugin.ModLogger.LogError($"Modules.Assets.LoadObject failed to load path \"{path}\", defaulting to Modules.Assets.NullModel.");
                return Modules.Assets.NullModel;
            }
        }

        internal static NetworkSoundEventDef CreateNetworkSoundEventDef(string eventName)
        {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;

            R2API.ContentAddition.AddNetworkSoundEventDef(networkSoundEventDef);

            return networkSoundEventDef;
        }
    }
}