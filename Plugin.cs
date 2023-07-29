using AtOSkinExtender.Modules;
using BepInEx;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine.Events;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace AtOSkinExtender
{
    [BepInPlugin(modGuid, modName, modVer)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGuid = "com.DestroyedClone.AtOSkinExtender";
        public const string modName = "AtOSkinExtender";
        public const string modVer = "0.1.0";

        public static BepInEx.Logging.ManualLogSource _logger;

        public static bool cfgShowSkinSource;

        /// <summary>
        /// A dictionary containing the "skinId" as the Key, and "mod GUID/Identifier" as the Value.
        /// </summary>
        public static Dictionary<string, string> skinIdIdentifierReference = new Dictionary<string, string>();

        public static UnityAction onCreateSkins;
        public static UnityAction Pre_onCharPopupDoSkins;
        public static UnityAction Post_onCharPopupDoSkins;

        public void Awake()
        {
            _logger = Logger;

            cfgShowSkinSource = Config.Bind("", "Show Skin Source", true, "If true, then the skin name on character select will show the identifier its associated with below it.").Value;

            On.Globals.CreateGameContent += AddSkinsToGame;
            On.CharPopup.Start += StartSkinSelectorComponent;
            On.HeroSelectionManager.Start += ResetSkinsIfInvalid;
            if (cfgShowSkinSource)
                On.BotonSkin.SetSkinData += BotonSkin_SetSkinData;

            Language.Init();
        }

        private void BotonSkin_SetSkinData(On.BotonSkin.orig_SetSkinData orig, BotonSkin self, SkinData _skinData)
        {
            //probably make this an IL hook?
            orig(self, _skinData);
            var identifier = Assets.GetIdentiferForSkin(_skinData);
            self.skinName.text += $"\n(<size=-.5>{identifier}</size>)";
        }

        private void ResetSkinsIfInvalid(On.HeroSelectionManager.orig_Start orig, HeroSelectionManager self)
        {
            foreach (var subclass in Globals.Instance._SubClassSource)
            {
                if (subclass.Value == null)
                {
                    //_logger.LogError($"Couldn't find subclass! What's going on?");
                    continue;
                }
                if (subclass.Key.StartsWith("young"))
                {
                    //_logger.LogMessage($"Subclass {subclass.Key} is a youngtype, skipping.");
                    continue;
                }
                var activeSkin = PlayerManager.Instance.GetActiveSkin(subclass.Key);
                SkinData skinData = Globals.Instance.GetSkinData(activeSkin);
                if (skinData != null)
                {
                    continue;
                }
                _logger.LogWarning($"Subclass {subclass.Key} has a null SkinData selected. Resetting skin.");
                var baseSkinData = Assets.GetBaseSkinOrDefaultForSubclass(subclass.Key);
                if (baseSkinData == null)
                {
                    _logger.LogError($"Couldn't reset skin because no matching SkinData was found!");
                    continue;
                }
                PlayerManager.Instance.SetSkin(subclass.Key, baseSkinData.SkinId);
                HeroSelectionManager.Instance?.SetSkinIntoSubclassData(baseSkinData.SkinSubclass.SubClassName, baseSkinData.SkinId);
                HeroSelectionManager.Instance?.charPopup?.DoSkins();
            }
            orig(self);
        }

        private void StartSkinSelectorComponent(On.CharPopup.orig_Start orig, CharPopup self)
        {
            orig(self);
            var comp = self.gameObject.AddComponent<SkinSelector.SkinSelectorComponent>();
            comp.charPopup = self;
        }

        private void AddSkinsToGame(On.Globals.orig_CreateGameContent orig, Globals self)
        {
            orig(self);

            onCreateSkins?.Invoke();

            //Since vanilla skins have just been added, we can just assign them as so.
            foreach (var skinDataPairs in self._SkinDataSource)
            {
                //The game doesn't localize its name so I won't bother either
                skinIdIdentifierReference.Add(skinDataPairs.Key, "Across the Obelisk");
            }

            //Now we can set the mod skins
            int skinOrder = 5;
            foreach (var identifierSkinDataPacks in Assets.skinDataPacks)
            {
                skinOrder++;
                List<SkinData> skinDataPacks = identifierSkinDataPacks.Value;
                foreach (var skinData in skinDataPacks)
                {
                    skinData.SkinOrder = skinOrder;
                    skinIdIdentifierReference.Add(skinData.SkinId.ToLower(), identifierSkinDataPacks.Key);
                    self._SkinDataSource.Add(skinData.SkinId.ToLower(), UnityEngine.Object.Instantiate<SkinData>(skinData));
                }
            }
        }
    }
}