using R2API;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicItemsReturns.Modules
{
    internal class LanguageOverrides
    {
        public const string LanguageTokenPrefix = "CLASSICITEMSRETURNS_";
        public const string LanguageTokenPrefixArtifact = LanguageTokenPrefix + "ARTIFACT_";
        public const string LanguageTokenPrefixItem = LanguageTokenPrefix + "ITEM_";
        public const string LanguageTokenPrefixEquipment = LanguageTokenPrefix + "EQUIPMENT_";
        public const string LanguageTokenPrefixEliteEquipment = LanguageTokenPrefix + "ELITE_EQUIPMENT_";
        public const string LanguageTokenPrefixElite = LanguageTokenPrefix + "ELITE_";

        public const string LanguageTokenPrefixBuffs = LanguageTokenPrefix + "BUFF_";

        public struct ReplacementToken
        {
            public string assignedToken;
            public string formatToken;
            public object[] args;
        }

        ///<summary>
        ///Helper method to defer language tokens.
        ///</summary>
        public static void DeferToken(string token, params object[] args)
        {
            //Main._logger.LogMessage($"Deferring {token} w/ lang {lang}");
            replacementTokens.Add(new ReplacementToken() { assignedToken = token, formatToken = token, args = args });
        }

        ///<summary>
        ///Helper method to defer language tokens. All passed args must be existing tokens.
        ///</summary>
        public static void DeferLateTokens(string token, params object[] args)
        {
            postReplacementTokens.Add(new ReplacementToken() { assignedToken = token, formatToken = token, args = args });
        }

        public static void DeferUniqueToken(string assignedToken, string formatToken, params object[] args)
        {
            replacementTokens.Add(new ReplacementToken() { assignedToken = assignedToken, formatToken = formatToken, args = args });
        }

        ///<summary>
        ///Helper method to defer language tokens. All passed args must be existing tokens.
        ///</summary>
        public static void DeferLateUniqueTokens(string assignedToken, string formatToken, params object[] args)
        {
            postReplacementTokens.Add(new ReplacementToken() { assignedToken = assignedToken, formatToken = formatToken, args = args });
        }

        public static List<ReplacementToken> replacementTokens = new List<ReplacementToken>();
        public static List<ReplacementToken> postReplacementTokens = new List<ReplacementToken>();
        public static List<Type> configEntries = new List<Type>();

        public static Dictionary<string, string> logbookTokenOverrideDict = new Dictionary<string, string>();

        public static void Initialize()
        {
            RoR2.Language.collectLanguageRootFolders += CollectLanguageRootFolders;
            RoR2.Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
        }

        private static void Language_onCurrentLanguageChanged()
        {
            RoR2.Language currentLanguage = RoR2.Language.currentLanguage;
            // defer tokens
            foreach (var replacementToken in replacementTokens)
            {
                var newString = currentLanguage.GetLocalizedFormattedStringByToken(replacementToken.formatToken, replacementToken.args);
                currentLanguage.SetStringByToken(replacementToken.assignedToken, newString);
            }
            // post replacement tokens
            foreach (var postReplacementToken in postReplacementTokens)
            {
                List<string> resolvedReplacementTokens = new List<string>();
                foreach (var tokenArg in postReplacementToken.args)
                {
                    resolvedReplacementTokens.Add(currentLanguage.GetLocalizedStringByToken(tokenArg.ToString()));
                    var assignedToken = postReplacementToken.formatToken;
                    if (postReplacementToken.assignedToken == null || postReplacementToken.assignedToken != postReplacementToken.formatToken)
                    {
                        assignedToken = postReplacementToken.assignedToken;
                    }

                    var newString = currentLanguage.GetLocalizedFormattedStringByToken(postReplacementToken.formatToken, resolvedReplacementTokens.ToArray());
                    currentLanguage.SetStringByToken(assignedToken, newString);
                }
            }
        }

        private static void CollectLanguageRootFolders(List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ClassicItemsReturnsPlugin.PInfo.Location), "Language"));
        }
    }
}