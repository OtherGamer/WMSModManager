using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WMSModManager.Patches;

namespace WMSModManager.UI
{
    public class UIManager
    {
        private static bool dropdownShowed;
        private static GameObject dropdown;
        private static GameObject Canvas;

        public static GameObject ModInfoP;
        public static GameObject ModP;

        public static void Start() {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        public static void ShowDropdown(ModElement element, PointerEventData dropdownMod, string name)
        {
            if (dropdown == null)
            {
                dropdown = Object.Instantiate(WMSAssetUtils.getWMSPrefab("DropDown"), Canvas.transform);
                dropdownShowed = true;
            }
            dropdownShowed = true;
            dropdown.SetActive(true);
            dropdown.transform.SetPositionAndRotation(new Vector3(
                    dropdownMod.position.x,
                    dropdownMod.position.y,
                    dropdown.transform.position.z
                ), Quaternion.identity);
            dropdown.transform.GetChild(0).GetComponent<UIButton>().leftClick = () =>
            {
                OpenModInfoPanel(name);
                CloseDropdown();
            };
            dropdown.transform.GetChild(1).GetComponent<UIButton>().leftClick = () =>
            {
                CloseDropdown();
            };
            dropdown.transform.GetChild(2).GetComponent<UIButton>().leftClick = () =>
            {
                WMSModManagerMain.ToggleMod(name);
                element.UpdateStatus();
                CloseDropdown();
            };
        }

        public static void CloseDropdown()
        {
            if (dropdown == null)
            {
                dropdown = Object.Instantiate(WMSAssetUtils.getWMSPrefab("DropDown"), Canvas.transform);
                dropdownShowed = true;
            }
            if (dropdownShowed)
            {
                dropdown.SetActive(false);
                dropdownShowed = false;
            }
        }

        public static void OpenModTab()
        {
            MainMenuContr menuContr = GameObject.Find("Canvas").GetComponent<MainMenuContr>();

            menuContr.OpenModTab();
        }

        public static void OpenModInfoPanel(string modID)
        {
            CloseDropdown();

            MainMenuContr menuContr = GameObject.Find("Canvas").GetComponent<MainMenuContr>();

            if (LifeCycle.mods.ContainsKey(modID))
            {
                ModInfoPanel panel = ModInfoP.GetComponent<ModInfoPanel>();
                panel.ModNameText.text = LifeCycle.mods[modID].Name;
                panel.ModAuthorText.text = LifeCycle.mods[modID].Author;
                panel.ModVersionText.text = LifeCycle.mods[modID].Version;
                panel.ModDescriptionText.text = LifeCycle.mods[modID].Description;
            }

            for(int i = 0; i<menuContr.AllPanels.Length; ++i)
            {
                menuContr.AllPanels[i].SetActive(false);
            }

            ModInfoP.SetActive(true);
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {

            if (scene != null)
            {
                Canvas = GameObject.Find("Canvas");

                SceneContr sceneContr = Canvas.GetComponent<SceneContr>();

                if (scene.name == "MainMenu")
                {
                    GameObject MainMenuP = GameObject.Find("MainMenuP");

                    Canvas.SetActive(false);

                    if (Canvas.GetComponent<CanvasScaler>() != null)
                    {
                        Canvas.GetComponent<CanvasScaler>().enabled = false;
                    }

                    MainMenuContr mainMenuContr = Canvas.GetComponent<MainMenuContr>();

                    GameObject simpleBTN = WMSAssetUtils.getWMSPrefab("uibutton");
                    GameObject modPAsset = WMSAssetUtils.getWMSPrefab("modpold");
                    GameObject modElementAsset = WMSAssetUtils.getWMSPrefab("modelement");
                    GameObject modInfoP = WMSAssetUtils.getWMSPrefab("modinfop");

                    modPAsset.SetActive(false);
                    modInfoP.SetActive(false);
                    simpleBTN.SetActive(false);

                    GameObject modBtn = Object.Instantiate(simpleBTN, MainMenuP.transform);
                    ModP = Object.Instantiate(modPAsset, Canvas.transform);
                    ModInfoP = Object.Instantiate(modInfoP, Canvas.transform);

                    modBtn.name = "WMSModBTN";
                    modBtn.GetComponent<UIButton>().buttonText.text = "MODS";
                    modBtn.GetComponent<UIButton>().leftClick = OpenModTab;
                    modBtn.transform.SetLocalPositionAndRotation(new Vector3(0.0f, -90.0f, 0.0f), Quaternion.identity);

                    modBtn.SetActive(true);

                    float lastYpos = 36000.0f;
                    float minY = 0.0f;
                    float maxY = 0.0f;

                    foreach (var modInfo in LifeCycle.mods)
                    {
                        GameObject modElement = Object.Instantiate(modElementAsset, ModP.transform.GetChild(1).transform);
                        RectTransform rect = modElement.GetComponent<RectTransform>();
                        if (lastYpos == 36000.0f)
                        {
                            lastYpos = modElement.transform.position.y;
                            minY = lastYpos - (rect.sizeDelta.y / 2) * rect.lossyScale.y;
                            maxY = lastYpos + (rect.sizeDelta.y / 2) * rect.lossyScale.y;
                        }
                        else
                        {
                            lastYpos -= (rect.sizeDelta.y + 10.0f) * rect.lossyScale.y;
                            minY = lastYpos - (rect.sizeDelta.y / 2) * rect.lossyScale.y;
                            modElement.transform.SetPositionAndRotation(
                                new Vector3(
                                    modElement.transform.position.x,
                                    lastYpos,
                                    modElement.transform.position.z
                                ),
                                Quaternion.identity);
                        }

                        ModElement element = modElement.GetComponent<ModElement>();

                        element.ModName.text = modInfo.Value.Name;
                        element.ModAuthor.text = modInfo.Value.Author;
                        element.ModVersion.text = modInfo.Value.Version;
                        element.modID = modInfo.Value.ModID;

                        element.UpdateStatus();

                        element.fixTransform();
                    }

                    //Logger.LogInfo($"ScrollBar size min {minY}, max {maxY}");

                    GameObject[] allPanelsTMP = mainMenuContr.AllPanels;
                    GameObject[] newAllPanels = new GameObject[allPanelsTMP.Length + 2];
                    for (int i = 0; i < allPanelsTMP.Length; i++)
                    {
                        newAllPanels[i] = allPanelsTMP[i];
                    }
                    newAllPanels[newAllPanels.Length - 2] = ModP;
                    newAllPanels[newAllPanels.Length - 1] = ModInfoP;

                    mainMenuContr.AllPanels = newAllPanels;

                    UI.ScrollBar scrollBar = ModP.transform.GetChild(1).GetChild(0).gameObject.GetComponent<UI.ScrollBar>();

                    scrollBar.scrollRes = maxY - minY;
                    scrollBar.CustomStart(ModP.transform.GetChild(1).gameObject);

                    scrollBar.setProgress(0.0f);

                    if (Canvas.GetComponent<CanvasScaler>() != null)
                    {
                        CanvasScaler scaler = Canvas.GetComponent<CanvasScaler>();
                        scaler.referenceResolution = new Vector2(1920f, 1080f);
                        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.enabled = true;
                    }

                    Canvas.SetActive(true);

                }
                if (scene.name.StartsWith("MT"))
                {
                    GameObject UICam = GameObject.Find("UI_Cam");
                    GameObject Credits = UICam.transform.GetChild(4).gameObject;

                    GameObject modCreditsAsset = WMSAssetUtils.getWMSPrefab("modcredits");

                    GameObject modCredits = Object.Instantiate(modCreditsAsset, UICam.transform);
                    modCredits.transform.SetPositionAndRotation(new Vector3(150.0f, 9.0f, -85.25f), Credits.transform.rotation);

                }
            }
        }
    }
}
