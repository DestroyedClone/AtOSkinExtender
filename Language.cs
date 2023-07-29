using System;
using System.Collections.Generic;
using System.Text;

namespace AtOSkinExtender
{
    internal static class Language
    {
        public static Dictionary<string, string> tokens_en = new Dictionary<string, string>()
        {
            { "SkinExtender_Page", "Page" },
        };
        public static Dictionary<string, string> tokens_es = new Dictionary<string, string>()
        {
            { "SkinExtender_Page", "Página" },
        };
        public static Dictionary<string, string> tokens_ko = new Dictionary<string, string>()
        {
            { "SkinExtender_Page", "페이지" },
        };
        public static Dictionary<string, string> tokens_zhCN = new Dictionary<string, string>()
        {
            { "SkinExtender_Page", "页" },
        };

        public static void Init()
        {
            On.Texts.LoadTranslationText += Texts_LoadTranslationText;
        }

        //ToDo: Integrate this into the game's actual stuff
        //Texts.GetText as a starting point
        private static void Texts_LoadTranslationText(On.Texts.orig_LoadTranslationText orig, Texts self, string type)
        {
            orig(self, type);

            Dictionary<string, string> chosenTokens = null;
            switch (self.lang)
            {
                case "en":
                    chosenTokens = tokens_en;
                    break;
                case "es:":
                    chosenTokens = tokens_es;
                    break;
                case "ko":
                    chosenTokens = tokens_ko;
                    break;
                case "zh-CN":
                    chosenTokens = tokens_zhCN;
                    break;
            }
            foreach (var tokenPair in chosenTokens)
            {
                self.TextStrings[self.lang][tokenPair.Key] = tokenPair.Value;
            }
        }
    }
}
