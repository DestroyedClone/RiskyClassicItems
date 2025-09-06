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
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatRiskOfOptions.guid))
            {
                ModCompatRiskOfOptions.Init();
            }*/
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatRiskyMod.guid))
            {
                ModCompatRiskyMod.Init();
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatAssistManager.guid))
            {
                ModCompatAssistManager.Init();
            }
            LinearDamage.loaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(LinearDamage.guid);
        }

        internal class LinearDamage
        {
            internal const string guid = "com.RiskyLives.LinearDamage";
            internal static bool loaded = false;
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

            internal static bool IsAtgEnabled()
            {
                if (!loaded) return false;
                return IsAtgEnabledInternal();
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private static bool IsAtgEnabledInternal()
            {
                return RiskyMod.Items.ItemsCore.enabled && RiskyMod.Items.Uncommon.AtG.enabled;
            }
        }

        internal class ModCompatAssistManager
        {
            internal const string guid = "com.Moffein.AssistManager";
            internal static bool loaded = false;

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void Init()
            {
                loaded = true;
            }
        }
    }
}