using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace AtOSkinExtender.Modules
{
    public static class SkinSelector
    {
        //ref: roughly based on TomeManager
        public class SkinSelectorComponent : MonoBehaviour
        {
            public static SkinSelectorComponent Instance;
            public CharPopup charPopup;
            public Dictionary<string, int> subClassSkinPages = new Dictionary<string, int>();
            public Dictionary<string, int> subClassSkinMaxPages = new Dictionary<string, int>();

            bool subscribed = false;

            public int CurrentPage
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

            public int MaxPages
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

            public string PageDisplay
            {
                get
                {
                    if (MaxPages == 0)
                        return string.Empty;
                    return Texts.Instance.GetText("SkinExtender_Page") +
                        $"\n{CurrentPage + 1} / {MaxPages + 1}";
                }
            }

            public TextMeshPro PageDisplayObject;

            public int MaxSkinsPerPage
            {
                get
                {
                    return charPopup.botonSkinBase.Length;
                }
            }

            public int PreviousSkinIndex
            {
                get
                {
                    return CurrentPage * MaxSkinsPerPage;
                }
            }

            public void Awake()
            {
                Instance = this;
            }

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
                    item.name = "SkinBoton"+i;
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
            public void OnDestroy()
            {
                if (subscribed)
                {
                    subscribed = false;
                    On.CharPopup.DoSkins -= CharPopup_DoSkins;
                }
                Instance = null;
            }
            private static void CharPopup_DoSkins(On.CharPopup.orig_DoSkins orig, CharPopup self)
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

                for (int j = Instance.PreviousSkinIndex; j < Instance.PreviousSkinIndex +compensatedSkinsPerPage; j++)
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

            public void UpdatePage(int increment)
            {
                //List<SkinData> skinsBySubclass = Globals.Instance.GetSkinsBySubclass(charPopup.SCD.Id);
                //int skinCount = skinsBySubclass.Count;
                var newOffset = PreviousSkinIndex + MaxSkinsPerPage * increment;

                //Main._logger.LogMessage($"Changing Page from {CurrentPage} to {CurrentPage+increment} (Max: {MaxPages})");
                CurrentPage += increment;
                if (increment == +1)
                {
                    if (CurrentPage > MaxPages)
                    {
                        CurrentPage = 0;
                    }
                } else
                {
                    if (newOffset < 0)
                    {
                        CurrentPage = MaxPages;
                    }
                }
                UpdatePageDisplay();
                charPopup.DoSkins();
            }
            public void UpdatePageDisplay()
            {
                if (PageDisplayObject)
                {
                    PageDisplayObject.SetText(PageDisplay);
                }
            }
        }
    }
}
