﻿using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace AtOSkinExtender.Modules
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class SkinSelector
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        //ref: roughly based on TomeManager
        /// <summary>
        /// Component used to control CharPopup's skin menu
        /// </summary>
        public class SkinSelectorComponent : MonoBehaviour
        {/// <summary>
         /// The instance of the SkinSelectorComponent
         /// </summary>
            public static SkinSelectorComponent Instance;
            /// <summary>
            /// The current CharPopup instance, assigned when the component is added.
            /// </summary>
            public CharPopup charPopup;
            private readonly Dictionary<string, int> subClassSkinPages = new Dictionary<string, int>();
            private readonly Dictionary<string, int> subClassSkinMaxPages = new Dictionary<string, int>();

            private bool subscribed = false;

            private int CurrentPage
            {
                get
                {
                    return subClassSkinPages[charPopup.SCD.Id];
                }
                set
                {
                    subClassSkinPages[charPopup.SCD.Id] = value;
                }
            }

            private int MaxPages
            {
                get
                {
                    return subClassSkinMaxPages[charPopup.SCD.Id];
                }
                set
                {
                    subClassSkinMaxPages[charPopup.SCD.Id] = value;
                }
            }

            private string PageDisplay
            {
                get
                {
                    if (MaxPages == 0)
                        return string.Empty;
                    return Texts.Instance.GetText("SkinExtender_Page") +
                        $"\n{CurrentPage + 1} / {MaxPages + 1}";
                }
            }

            private TextMeshPro PageDisplayObject;

            private int MaxSkinsPerPage
            {
                get
                {
                    return charPopup.botonSkinBase.Length;
                }
            }

            private int PreviousSkinIndex
            {
                get
                {
                    return CurrentPage * MaxSkinsPerPage;
                }
            }

            /// <summary>
            /// Unity action.
            /// </summary>
            public void Awake()
            {
                Instance = this;
            }

            /// <summary>
            /// Unity action.
            /// </summary>
            public void Start()
            {
                //Extend the amount of baseskins
                var list = new List<BotonSkin>();
                var copyList = new List<BotonSkin>();

                for (int row = 0; row < 1; row++) //can more than 1 even fit?
                {
                    for (int i = 0; i < charPopup.botonSkinBase.Length; i++)
                    {
                        var item = charPopup.botonSkinBase[i];
                        list.Add(item);
                        var dupe = UnityEngine.Object.Instantiate(item.gameObject);
                        dupe.transform.parent = item.transform.parent;
                        dupe.transform.position = item.transform.position + Vector3.up * -3f;
                        copyList.Add(dupe.GetComponent<BotonSkin>());
                    }
                }
                list.AddRange(copyList);
                for (int i = 0; i < list.Count; i++)
                {
                    BotonSkin item = list[i];
                    item.name = "SkinBoton" + i;
                }

                charPopup.botonSkinBase = list.ToArray();

                //populating dictionary
                //need cache correction immediately
                foreach (var keyValuePair in Globals.Instance._SubClassSource)
                {
                    subClassSkinPages.Add(keyValuePair.Value.Id, 0);
                    //calculating max amount of pages
                    List<SkinData> skinsBySubclass = Globals.Instance.GetSkinsBySubclass(keyValuePair.Value.Id);
                    var calculatedMaxPages = Mathf.CeilToInt(skinsBySubclass.Count / MaxSkinsPerPage);
                    subClassSkinMaxPages.Add(keyValuePair.Value.Id, calculatedMaxPages);
                }

                //Duplicate object so that we have page display.
                var textObject = UnityEngine.Object.Instantiate(charPopup.botonSkinBase[0].transform.Find("Name"));
                textObject.transform.parent = charPopup.botonSkinBase[0].transform.parent;
                textObject.transform.localPosition = new Vector2(5, -4);
                PageDisplayObject = textObject.GetComponent<TextMeshPro>();

                if (!subscribed)
                {
                    subscribed = true;
                    On.CharPopup.DoSkins += CharPopup_DoSkins;
                }
            }

            /// <summary>
            /// Unity action.
            /// </summary>
            public void OnDestroy()
            {
                if (subscribed)
                {
                    subscribed = false;
                    On.CharPopup.DoSkins -= CharPopup_DoSkins;
                }
                Instance = null;
            }

            /// <summary>
            /// Override to patch out of range exceptiona and to add scrolling support.
            /// </summary>
            public static void CharPopup_DoSkins(On.CharPopup.orig_DoSkins orig, CharPopup self)
            {
                //Overriding because of an access error
                //TODO: Replace with an IL-level hook, so we don't have to do this action shit.
                Plugin.Pre_onCharPopupDoSkins?.Invoke();
                //orig(self);
                List<SkinData> skinsBySubclass = Globals.Instance.GetSkinsBySubclass(self.SCD.Id);
                foreach (BotonSkin botonSkin in self.botonSkinBase)
                {
                    botonSkin.gameObject.SetActive(false);
                }

                StringBuilder stringBuilder = new StringBuilder();
                SortedDictionary<string, SkinData> sortedDictionary = new SortedDictionary<string, SkinData>();

                int compensatedSkinsPerPage = Instance.MaxSkinsPerPage;
                if (Instance.PreviousSkinIndex + Instance.MaxSkinsPerPage > skinsBySubclass.Count)
                {
                    compensatedSkinsPerPage = Math.Max(0, skinsBySubclass.Count - Instance.PreviousSkinIndex);
                }
                //Plugin._logger.LogMessage($"Page {Instance.CurrentPage}/{Instance.MaxPages}: Skins {Instance.PreviousSkinIndex} through {Instance.PreviousSkinIndex +compensatedSkinsPerPage} out of {skinsBySubclass.Count} ({compensatedSkinsPerPage} skins shown)");

                for (int j = Instance.PreviousSkinIndex; j < Instance.PreviousSkinIndex + compensatedSkinsPerPage; j++)
                {
                    stringBuilder.Clear();
                    if (skinsBySubclass[j].BaseSkin)
                    {
                        stringBuilder.Append("0");
                    }
                    else
                    {
                        stringBuilder.Append("1");
                    }
                    stringBuilder.Append(skinsBySubclass[j].SkinOrder.ToString("D2"));
                    stringBuilder.Append(skinsBySubclass[j].SkinId.ToLower());
                    sortedDictionary.Add(stringBuilder.ToString(), skinsBySubclass[j]);
                }

                int num = 0;
                foreach (KeyValuePair<string, SkinData> keyValuePair in sortedDictionary)
                {
                    self.botonSkinBase[num].SetSkinData(keyValuePair.Value);
                    self.botonSkinBase[num].gameObject.SetActive(true);
                    num++;
                }
                Instance.UpdatePageDisplay();
                Plugin.Post_onCharPopupDoSkins?.Invoke();
            }


            /// <summary>
            /// Unity action.
            /// </summary>
            public void Update()
            {
                if (!charPopup.opened)
                {
                    return;
                }
                if (Input.GetKeyUp(KeyCode.RightArrow))
                {
                    UpdatePage(1);
                }
                if (Input.GetKeyUp(KeyCode.LeftArrow))
                {
                    UpdatePage(-1);
                }
                if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    UpdatePage(-1);
                }
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    UpdatePage(1);
                }
            }

            private void UpdatePage(int increment)
            {
                //List<SkinData> skinsBySubclass = Globals.Instance.GetSkinsBySubclass(charPopup.SCD.Id);
                //int skinCount = skinsBySubclass.Count;
                var newOffset = PreviousSkinIndex + MaxSkinsPerPage * increment;

                //Main._logger.LogMessage($"Changing Page from {CurrentPage} to {CurrentPage+increment} (Max: {MaxPages})");
                CurrentPage += increment;
                if (increment > 0)
                {
                    if (CurrentPage > MaxPages)
                    {
                        CurrentPage = 0;
                    }
                }
                else
                {
                    if (newOffset < 0)
                    {
                        CurrentPage = MaxPages;
                    }
                }
                UpdatePageDisplay();
                charPopup.DoSkins();
            }

            private void UpdatePageDisplay()
            {
                if (PageDisplayObject)
                {
                    PageDisplayObject.SetText(PageDisplay);
                }
            }
        }
    }
}