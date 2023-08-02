using System.Collections.Generic;
using UnityEngine;
using static AtOSkinExtender.Plugin;

namespace AtOSkinExtender
{
    /// <summary>
    /// Helpers for making and cataloguing new skins.
    /// </summary>
    public static class Assets
    {
        /// <summary>
        /// The sku for the DLC Wolf Wars
        /// </summary>
        public const string sku_WolfWars = "2325780";
        /// <summary>
        /// The sku for the DLC Halloween
        /// </summary>
        public const string sku_Halloween = "2168960";
        /// <summary>
        /// Returns the first skin found with .BaseSkin set to true. If there's no BaseSkins, then it will return the first skin found. Otherwise, returns null.
        /// </summary>
        /// <param name="id">SubclassData id</param>
        /// <returns></returns>
        public static SkinData GetBaseSkinOrDefaultForSubclass(string id)
        {
            id = id.ToLower();
            SkinData fallbackSkinData = null;
            foreach (KeyValuePair<string, SkinData> keyValuePair in Globals.Instance._SkinDataSource)
            {
                if (keyValuePair.Value.SkinSubclass.Id.ToLower() == id)
                {
                    if (keyValuePair.Value.BaseSkin)
                    {
                        return keyValuePair.Value;
                    }
                    else if (fallbackSkinData == null)
                    {
                        fallbackSkinData = keyValuePair.Value;
                    }
                }
            }
            return fallbackSkinData;
        }

        #region ContentManagement
        /// <summary>
        /// List of skin data packs. 
        /// </summary>
        public static Dictionary<string, List<SkinData>> skinDataPacks = new Dictionary<string, List<SkinData>>();

        /// <summary>
        /// Creates an associated SkinData list for the assigned identifier, or gets the existed associated SkinData list.
        /// </summary>
        /// <param name="identifier">The identifier for the SkinData list</param>
        /// <returns></returns>
        public static List<SkinData> GetOrCreateSkinDataListForIdentifier(string identifier)
        {
            if (!skinDataPacks.TryGetValue(identifier, out List<SkinData> skinDataList))
            {
                skinDataList = new List<SkinData>();
                skinDataPacks.Add(identifier, skinDataList);
            }
            return skinDataList;
        }

        /// <summary>
        /// Adds an IEnumerable of SkinData to the collection
        /// </summary>
        /// <param name="skinDataCollection">Collection of SkinData's to add to the pack</param>
        /// <param name="identifier">SkinData List identifier</param>
        public static void AddSkinDataToPack(IEnumerable<SkinData> skinDataCollection, string identifier)
        {
            foreach (var skinData in skinDataCollection)
            {
                AddSkinDataToPack(skinData, identifier);
            }
        }

        /// <summary>
        /// Adds a SkinData to the collection
        /// </summary>
        /// <param name="skinData">SkinData to add to the pack</param>
        /// <param name="identifier">SkinData List identifier</param>
        public static void AddSkinDataToPack(SkinData skinData, string identifier)
        {
            var skinDataList = GetOrCreateSkinDataListForIdentifier(identifier);
            if (skinDataList.Contains(skinData))
            {
                _logger.LogWarning($"{nameof(AddSkinDataToPack)}:: Aborting adding \"{skinData.skinId}\" to identifier {identifier} since pack already contains it.");
                return;
            }
            else
            {
                skinDataList.Add(skinData);
            }
        }

        /// <summary>
        /// Returns the associated identifier for a SkinData
        /// </summary>
        /// <param name="skinData">SkinData that you want the identifier of.</param>
        /// <returns></returns>
        public static string GetIdentiferForSkin(SkinData skinData)
        {
            return skinIdIdentifierReference[skinData.SkinId.ToLower()];
        }

        #endregion ContentManagement

        // CharPopup.DoSkins
        // setup the skinorder to properly autoshow
        /// <summary>
        /// Creates and returns a SkinData instance.
        /// </summary>
        /// <param name="subclassId">"ranger"<br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=17158299">Reference: SubclassData (Google Sheets)</see></br></param>
        /// <param name="skinName">"Voodoo Witch"<br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=1568095750">Reference: SkinData (Google Sheets)</see></br></param>
        /// <param name="perkLevel">Set to 0 for no perk level requirement.<br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=1568095750">Reference: SkinData (Google Sheets)</see></br></param>
        /// <param name="portrait"></param>
        /// <param name="portraitLarge"></param>
        /// <param name="silueta">silhouette</param>
        /// <param name="siluetaLarge">silhouette large</param>
        /// <param name="skinGO">The display gameObject for the skin in combat.<br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=1568095750">Reference: SkinData (Google Sheets)</see></br></param>
        /// <param name="Sku">Required DLC<br>Wolfwars(2325780) | Halloween(2168960)</br><br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=1568095750">Reference: SkinData (Google Sheets)</see></br></param>
        /// <returns></returns>
        public static SkinData CreateSkinData(string subclassId, string skinName, int perkLevel,
            Sprite portrait, Sprite portraitLarge, Sprite silueta, Sprite siluetaLarge, GameObject skinGO, string Sku = "")//, bool autoAdd = true)
        {
            return CreateSkinData(subclassId, skinName, subclassId + "Skin" + skinName, perkLevel, portrait, portraitLarge, silueta, siluetaLarge, skinGO, Sku);
        }

        /// <summary>
        /// Creates and returns a SkinData instance.
        /// </summary>
        /// <param name="subclassId">"ranger"<br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=17158299">Reference: SubclassData (Google Sheets)</see></br></param>
        /// <param name="skinName">"Voodoo Witch"<br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=1568095750">Reference: SkinData (Google Sheets)</see></br></param>
        /// <param name="skinId">andrinSkinRegular<br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=1568095750">Reference: SkinData (Google Sheets)</see></br></param>
        /// <param name="perkLevel">Set to 0 for no perk level requirement.<br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=1568095750">Reference: SkinData (Google Sheets)</see></br></param>
        /// <param name="portrait"></param>
        /// <param name="portraitLarge"></param>
        /// <param name="silueta">silhouette</param>
        /// <param name="siluetaLarge">silhouette large</param>
        /// <param name="skinGO">The display gameObject for the skin in combat.<br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=1568095750">Reference: SkinData (Google Sheets)</see></br></param>
        /// <param name="Sku">Required DLC<br>Wolfwars(2325780) | Halloween(2168960)</br><br><see href="https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=1568095750">Reference: SkinData (Google Sheets)</see></br></param>
        /// <returns></returns>
        public static SkinData CreateSkinData(string subclassId, string skinName, string skinId, int perkLevel,
            Sprite portrait, Sprite portraitLarge, Sprite silueta, Sprite siluetaLarge, GameObject skinGO, string Sku = "")//, bool autoAdd = true)
        {
            if (!Globals.Instance._SubClassSource.TryGetValue(subclassId, out SubClassData subClassData))
            {
                _logger.LogError($"{nameof(CreateSkinData)} requested subclassId \"{subclassId}\", but no subClassData matched.");
                return null;
            }

            var skinData = ScriptableObject.CreateInstance<SkinData>();
            //thulsSkinRegular thulsSkinMedium etc
            skinData.name = subclassId + "Skin" + skinName;
            skinData.SkinSubclass = subClassData;
            //BaseSkin for the default skin
            skinData.BaseSkin = false;
            //PerkLevel = required character level. 0 is acceptable
            skinData.PerkLevel = perkLevel;
            //skinId assassin assassinmedium
            skinData.SkinId = skinId;
            //SkinName Assassin Rogue
            skinData.SkinName = skinName;
            //skinOrder = obvious
            //put method here to auto order
            //thulsPortrait thulsPortraitGrande thulssilueta thulssiluetaGrande
            skinData.SpritePortrait = portrait;
            skinData.SpritePortraitGrande = portraitLarge;
            skinData.SpriteSilueta = silueta;
            skinData.SpriteSiluetaGrande = siluetaLarge;

            skinData.SkinGo = skinGO;
            //Should we include skinOrder?
            //gl LOL
            //sku is DLC?
            skinData.Sku = Sku;
            //steamStat no idea
            skinData.SteamStat = "";

            return skinData;
        }
    }
}