using RoR2;
using System.Runtime.CompilerServices;

namespace RiskyClassicItems.Modules
{
    internal class ModSupport
    {
        internal static void CheckForModSupport()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatBetterUI.guid))
            {
                ModCompatBetterUI.Init();
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatRiskOfOptions.guid))
            {
                ModCompatRiskOfOptions.Init();
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatRiskyMod.guid))
            {
                ModCompatRiskyMod.Init();
            }
        }

        internal class ModCompatBetterUI
        {
            public const string guid = "com.xoxfaby.BetterUI";
            internal static bool loaded = false;

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void Init()
            {
                loaded = true;
                BetterUICompat_ItemStats();
                BetterUICompat_Buffs();
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void BetterUICompat_Buffs()
            {
                void RegisterBuffInfo(BuffDef buffDef, string baseToken, string[] descTokenParams = null)
                {
                    if (descTokenParams != null && descTokenParams.Length > 0)
                    {
                        LanguageOverrides.DeferToken(LanguageOverrides.LanguageTokenPrefixBuffs + baseToken + "_DESC", descTokenParams);
                    }
                    BetterUI.Buffs.RegisterBuffInfo(buffDef, LanguageOverrides.LanguageTokenPrefixBuffs + baseToken + "_NAME", LanguageOverrides.LanguageTokenPrefixBuffs + baseToken + "_DESC");
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void BetterUICompat_ItemStats()
            {
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

                RiskOfOptions.ModSettingsManager.SetModDescription(Main.ModDescription, Main.ModGuid, Main.ModName);
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
            internal static void GiveAllyItem(Inventory inventory)
            {
                if (!loaded) return;
                inventory.GiveItem(RiskyMod.Allies.AllyItems.AllyMarkerItem);
                inventory.GiveItem(RiskyMod.Allies.AllyItems.AllyScalingItem);
            }
        }
    }
}