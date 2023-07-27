using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace ClassicItemsReturns.Modules
{
    internal class RiskyVisuals
    {
        public static ConfigEntry<bool> ExecuteLowHealthElite;
        public static ConfigEntry<bool> Bear_HermitScarf;
        public static ConfigEntry<bool> HealWhileSafe_SproutingEgg;
        public const string cat = "Cosmetics";

        public static void Init(ConfigFile config)
        {
            return;
            ExecuteLowHealthElite = config.Bind(cat, "Guiotinne lopper", true, "");
            Bear_HermitScarf = config.Bind(cat, "Tougher Times Hermit Scarf", true);
            HealWhileSafe_SproutingEgg = config.Bind(cat, "Cautious Slug Sprouting Egg", true, "");

            ItemCatalog.availability.CallWhenAvailable(CallWhenAvail);
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        public static void CallWhenAvail()
        {
            Bear.Init();
            HealWhileSafe.Init();
        }

        private static void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            foreach (var bodyPrefab in BodyCatalog.allBodyPrefabs)
            {
                if (!bodyPrefab || !bodyPrefab.TryGetComponent(out ModelLocator modelLocator)
                    || !modelLocator.modelTransform || !modelLocator.modelTransform.TryGetComponent(out CharacterModel characterModel) || !characterModel.itemDisplayRuleSet)
                {
                    continue;
                }

                if (Bear_HermitScarf.Value)
                {
                    var bearDRG = characterModel.itemDisplayRuleSet.FindDisplayRuleGroup(RoR2Content.Items.Bear);
                    var lunarSunDRG = characterModel.itemDisplayRuleSet.FindDisplayRuleGroup(DLC1Content.Items.LunarSun);
                    if (lunarSunDRG.isEmpty)
                        goto Label_1;

                    ItemDisplayRule itemDisplayRuleToCopy = default;
                    bool displayExists = false;
                    foreach (var displayRule in lunarSunDRG.rules)
                    {
                        if (!displayRule.followerPrefab) continue;
                        if (displayRule.followerPrefab.name.StartsWith("DisplaySunHeadNeck"))
                        {
                            itemDisplayRuleToCopy = displayRule;
                            displayExists = true;
                            break;
                        }
                    }
                    if (!displayExists) goto Label_1;

                    var newRule = new ItemDisplayRule()
                    {
                        childName = itemDisplayRuleToCopy.childName,
                        followerPrefab = Bear.HermitScarfDisplay,
                        limbMask = itemDisplayRuleToCopy.limbMask,
                        localAngles = itemDisplayRuleToCopy.localAngles,
                        localPos = itemDisplayRuleToCopy.localPos,
                        localScale = itemDisplayRuleToCopy.localScale,
                        ruleType = itemDisplayRuleToCopy.ruleType
                    };
                    bearDRG.AddDisplayRule(newRule);
                }
            Label_1:
                if (HealWhileSafe_SproutingEgg.Value)
                {
                    var originalDRG = characterModel.itemDisplayRuleSet.FindDisplayRuleGroup(RoR2Content.Items.HealWhileSafe);
                    var referenceDRG = characterModel.itemDisplayRuleSet.FindDisplayRuleGroup(RoR2Content.Equipment.FireBallDash);
                    if (referenceDRG.isEmpty)
                        goto Label_2;

                    ItemDisplayRule itemDisplayRuleToCopy = default;
                    bool displayExists = false;
                    foreach (var displayRule in referenceDRG.rules)
                    {
                        if (displayRule.followerPrefab.name.StartsWith("DisplayEgg"))
                        {
                            itemDisplayRuleToCopy = displayRule;
                            displayExists = true;
                            break;
                        }
                    }
                    if (!displayExists) goto Label_2;

                    var newRule = new ItemDisplayRule()
                    {
                        childName = itemDisplayRuleToCopy.childName,
                        followerPrefab = HealWhileSafe.displayObject,
                        limbMask = itemDisplayRuleToCopy.limbMask,
                        localAngles = itemDisplayRuleToCopy.localAngles,
                        localPos = itemDisplayRuleToCopy.localPos,
                        localScale = itemDisplayRuleToCopy.localScale,
                        ruleType = itemDisplayRuleToCopy.ruleType
                    };
                    originalDRG.AddDisplayRule(newRule);
                }
            Label_2:
                continue;
            }
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        internal class Bear
        {
            public static ItemDef ItemDef => RoR2Content.Items.Bear;
            public static GameObject HermitScarfDisplay = null;

            public static void Init()
            {
                ReplaceLanguage();
                CreateAssets();
            }

            private static void CreateAssets()
            {
                var lunarSunScarf = Assets.LoadAddressable<GameObject>("RoR2/DLC1/LunarSun/DisplaySunHeadNeck.prefab");
                HermitScarfDisplay = PrefabAPI.InstantiateClone(lunarSunScarf, "DisplayHermitScarf_RCI");
                HermitScarfDisplay.transform.Find("mdlSunHeadNeck/mdlSunHeadNeck").GetComponent<SkinnedMeshRenderer>().SetMaterial(Assets.LoadAddressable<Material>("RoR2/Base/Clover/matClover.mat"));
            }

            public static void ReplaceLanguage()
            {
                ItemDef.nameToken = "CLASSICITEMSRETURNS_ITEM_HERMITSSCARF_NAME";
                ItemDef.descriptionToken = "CLASSICITEMSRETURNS_ITEM_HERMITSSCARF_DESCRIPTION";
            }
        }

        internal class HealWhileSafe
        {
            public static ItemDef ItemDef => RoR2Content.Items.HealWhileSafe;
            public static GameObject displayObject;

            public static void Init()
            {
                var asset = Assets.LoadAddressable<GameObject>("RoR2/Base/FireBallDash/DisplayEgg.prefab");
                displayObject = PrefabAPI.InstantiateClone(asset, "DisplaySproutingEgg_RCI");
                displayObject.transform.Find("mdlEgg").GetComponent<MeshRenderer>().SetMaterial(Assets.LoadAddressable<Material>("RoR2/Base/BeetleGland/matBeetleGland.mat"));
            }
        }
    }
}