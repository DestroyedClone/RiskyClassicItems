using R2API;
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

            using (Stream manifestResourceStream = new FileStream(SoundBankDirectory + "\\ClassicItemsReturnsSoundbank.bnk", FileMode.Open))
            {
                byte[] array = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }
        }
    }
}
