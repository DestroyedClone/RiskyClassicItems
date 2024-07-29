using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ClassicItemsReturns.Modules
{
    internal class ModSupport
    {
        internal static void CheckForModSupport()
        {
            /*
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatBetterUI.guid))
            {
                ModCompatBetterUI.Init();
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatRiskOfOptions.guid))
            {
                ModCompatRiskOfOptions.Init();
            }*/
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatRiskyMod.guid))
            {
                ModCompatRiskyMod.Init();
            }
        }

        internal class ModCompatRiskOfOptions
        {
            internal const string guid = "com.rune580.riskofoptions";
            internal static bool loaded = false;

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void Init()
            {
                loaded = true;

                RiskOfOptions.ModSettingsManager.SetModDescription(ClassicItemsReturnsPlugin.ModDescription, ClassicItemsReturnsPlugin.ModGuid, ClassicItemsReturnsPlugin.ModName);
            }
        }

        internal class ModCompatRiskyMod
        {
            internal const string guid = "com.RiskyLives.RiskyMod";
            internal static bool loaded = false;

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void Init()
            {
                loaded = true;
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void GiveAllyItem(Inventory inventory, bool expendable = true)
            {
                if (!loaded) return;
                inventory.GiveItem(RiskyMod.Allies.AllyItems.AllyMarkerItem);
                inventory.GiveItem(RiskyMod.Allies.AllyItems.AllyScalingItem);

                if (expendable)
                {
                    inventory.GiveItem(RiskyMod.Allies.AllyItems.AllyAllowVoidDeathItem);
                    inventory.GiveItem(RiskyMod.Allies.AllyItems.AllyAllowOverheatDeathItem);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void GiveAllyRegenItem(Inventory inventory, int count)
            {
                if (!loaded) return;
                inventory.GiveItem(RiskyMod.Allies.AllyItems.AllyRegenItem, count);
            }

            internal static bool IsNormalizeDroneDamage()
            {
                return RiskyMod.Allies.AlliesCore.normalizeDroneDamage;
            }
        }
    }
}