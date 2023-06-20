using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static RoR2.Skills.ComboSkillDef;

namespace ClassicItemsReturns.ReadmeGenerator
{
    internal class ReadmeGeneratorMain
    {
        public static void Init()
        {
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private static void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            FormatItemCatalog("CLASSICITEMSRETURNS");
            FormatEquipmentCatalog("CLASSICITEMSRETURNS");
        }


        public static string ReplaceStyleTagsWithBackticks(string input)
        {
            string pattern = @"<style=[^>]+>(.*?)<\/style>";
            string replacement = "`$1`";

            string output = Regex.Replace(input, pattern, replacement);

            return output;
        }

        public static void FormatItemCatalog(string nametokenfilter)
        {
            List<ItemDef> t1 = new List<ItemDef>();
            List < ItemDef > t2 = new List<ItemDef>();
            List < ItemDef > t3 = new List<ItemDef>();
            List<ItemDef> tLunar = new List<ItemDef>();

            foreach (var itemDef in ItemCatalog.itemDefs)
            {
                if (!itemDef) continue;
                if (!itemDef.nameToken.Contains(nametokenfilter)) continue;
                switch (itemDef.tier)
                {
                    case ItemTier.Tier1:
                        t1.Add(itemDef);
                        break;
                    case ItemTier.Tier2:
                        t2.Add(itemDef);
                        break;
                    case ItemTier.Tier3:
                        t3.Add(itemDef);
                        break;
                    case ItemTier.Lunar:
                        tLunar.Add(itemDef);
                        break;
                }
            }
            UnityEngine.Debug.Log(FormatItemSection(t1.ToArray(), "Common"));
            UnityEngine.Debug.Log(FormatItemSection(t2.ToArray(), "Uncommon"));
            UnityEngine.Debug.Log(FormatItemSection(t3.ToArray(), "Legendary"));
            UnityEngine.Debug.Log(FormatItemSection(tLunar.ToArray(), "Lunar"));
        }

        public static string FormatItemRow(ItemDef itemDef, ref StringBuilder stringBuilder)
        {
            if (!itemDef) return null;
            var format = "| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/RiskyClassicItemsUnityProject/Assets/Sprites/Icons/{0}.png) | {1} | {2}";
            return string.Format(format, itemDef.pickupIconSprite.texture.name, Language.GetString(itemDef.nameToken), ReplaceStyleTagsWithBackticks(Language.GetString(itemDef.descriptionToken)));
        }

        public static string FormatItemSection(ItemDef[] itemDefs, string columnName = null)
        {
            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            stringBuilder.AppendLine("\n| Icon | Item | Desc |");
            stringBuilder.AppendLine("|:--:|:--:|--|");
            if (columnName != null) { stringBuilder.AppendLine($"| {columnName} | | |"); }
            foreach (ItemDef itemDef in itemDefs)
            {
                stringBuilder.AppendLine(FormatItemRow(itemDef, ref stringBuilder));
            }
            var result = stringBuilder.ToString();
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            return result;
        }

        public static void FormatEquipmentCatalog(string nametokenfilter)
        {
            List<EquipmentDef> te = new List<EquipmentDef>();
            List<EquipmentDef> tLunar = new List<EquipmentDef>();

            foreach (var equipmentDef in EquipmentCatalog.equipmentDefs)
            {
                if (!equipmentDef) continue;
                if (!equipmentDef.nameToken.Contains(nametokenfilter)) continue;

                if (equipmentDef.isLunar) tLunar.Add(equipmentDef);
                else te.Add(equipmentDef);
            }

            UnityEngine.Debug.Log(FormatEquipmentSection(te.ToArray(), "Equipment"));
            UnityEngine.Debug.Log(FormatEquipmentSection(tLunar.ToArray(), "Lunar Equipment"));
        }

        public static string FormatEquipmentRow(EquipmentDef itemDef, ref StringBuilder stringBuilder)
        {
            if (!itemDef) return null;
            var format = "| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/RiskyClassicItemsUnityProject/Assets/Sprites/Icons/{0}.png) | {1} | {2}";
            return string.Format(format, itemDef.pickupIconSprite.texture.name, Language.GetString(itemDef.nameToken), ReplaceStyleTagsWithBackticks(Language.GetString(itemDef.descriptionToken)));
        }

        public static string FormatEquipmentSection(EquipmentDef[] itemDefs, string columnName = null)
        {
            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            stringBuilder.AppendLine("\n| Icon | Item | Desc |");
            stringBuilder.AppendLine("|:--:|:--:|--|");
            if (columnName != null) { stringBuilder.AppendLine($"| {columnName} | | |"); }
            foreach (EquipmentDef itemDef in itemDefs)
            {
                stringBuilder.AppendLine(FormatEquipmentRow(itemDef, ref stringBuilder));
            }
            var result = stringBuilder.ToString();
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            return result;
        }
    }
}
