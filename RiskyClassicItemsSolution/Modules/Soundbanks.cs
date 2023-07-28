using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassicItemsReturns.Modules
{
    internal static class SoundBanks
    {
        private static bool initialized = false;
        public static string SoundBankDirectory
        {
            get
            {
                return Path.Combine(System.IO.Path.GetDirectoryName(ClassicItemsReturnsPlugin.PInfo.Location), "SoundBanks");
            }
        }

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            AKRESULT akResult = AkSoundEngine.AddBasePath(SoundBankDirectory);

            AkSoundEngine.LoadBank("ClassicItemsReturnsSoundbank.bnk", out _);
        }
    }
}
