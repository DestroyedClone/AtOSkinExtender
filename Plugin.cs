﻿using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using UnityEngine.Events;
using AtOSkinExtender.Modules;
using System.Security;
using System.Security.Permissions;

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

        //something like this...
        //public static Dictionary<string, Dictionary<string, SkinData>> customSkinContent = new Dictionary<string, Dictionary<string, SkinData>>();

        public static Dictionary<string, SkinData> customSkinDataDict = new Dictionary<string, SkinData>();
        public static Dictionary<string, GameObject> cachedSkinGameObjects = new Dictionary<string, GameObject>();

        public static UnityAction onCreateSkins;
        public static UnityAction Pre_onCharPopupDoSkins;
        public static UnityAction Post_onCharPopupDoSkins;

        public void Awake()
        {
            _logger = Logger;

            On.Globals.CreateGameContent += Globals_CreateGameContent;
            On.CharPopup.Start += CharPopup_Start;
            On.HeroSelectionManager.Start += HeroSelectionManager_Start;
        }

        private void HeroSelectionManager_Start(On.HeroSelectionManager.orig_Start orig, HeroSelectionManager self)
        {
            foreach (var subclass in Globals.Instance._SubClassSource)
            {
                if (subclass.Value == null)
                {
                    _logger.LogError($"Couldn't find subclass! What's going on?");
                    continue;
                }
                if (subclass.Key.StartsWith("young"))
                {
                    _logger.LogMessage($"Subclass {subclass.Key} is a youngtype, skipping.");
                    continue;
                }
                var activeSkin = PlayerManager.Instance.GetActiveSkin(subclass.Key);
                SkinData skinData = Globals.Instance.GetSkinData(activeSkin);
                if (skinData != null)
                {
                    continue;
                }
                _logger.LogWarning($"Subclass {subclass.Key} has a null SkinData selected. Resetting to base skin to prevent nullref.");
                var baseSkinData = GetBaseSkinForSubclass(subclass.Key);
                if (baseSkinData == null)
                {
                    _logger.LogError($"Couldn't find BaseSkin SkinData! What's going on?");
                    continue;
                }
                PlayerManager.Instance.SetSkin(subclass.Key, baseSkinData.SkinId);
                HeroSelectionManager.Instance?.SetSkinIntoSubclassData(baseSkinData.SkinSubclass.SubClassName, baseSkinData.SkinId);
                HeroSelectionManager.Instance?.charPopup?.DoSkins();
            }
            orig(self);

        }

        private void CharPopup_Start(On.CharPopup.orig_Start orig, CharPopup self)
        {
            orig(self);
            var comp = self.gameObject.AddComponent<SkinSelector.SkinSelectorComponent>();
            comp.charPopup = self;
        }

        private void Globals_CreateGameContent(On.Globals.orig_CreateGameContent orig, Globals self)
        {
            orig(self);

            onCreateSkins?.Invoke();

            foreach (var skinDataPairs in customSkinDataDict)
            {
                var skinData = skinDataPairs.Value;
                //self._SkinDataSource.Add(skinData.SkinId.ToLower(), UnityEngine.Object.Instantiate<SkinData>(skinData));
                self._SkinDataSource.Add(skinData.SkinId.ToLower(), UnityEngine.Object.Instantiate<SkinData>(skinData));
            }
        }

        public SkinData GetBaseSkinForSubclass(string id)
        {
            id = id.ToLower();
            foreach (KeyValuePair<string, SkinData> keyValuePair in Globals.Instance._SkinDataSource)
            {
                if (keyValuePair.Value.SkinSubclass.Id.ToLower() == id
                    && keyValuePair.Value.BaseSkin)
                {
                    return keyValuePair.Value;
                }
            }
            return null;
        }
    }
}
