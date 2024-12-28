using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx.Logging;
using System.IO;
using System.Collections.Generic;
using System;
using System.Net;
using Newtonsoft.Json;
using TMPro;
using WMSModManager.Patches;
using WMSModManager.UI;

namespace WMSModManager
{
    [BepInPlugin(bepmodid, modname, version)]
    public class WMSModManagerMain : BaseUnityPlugin
    {
        public const string bepmodid = "ru.n00b.wmsmodmanager";
        public const string modid = "wmsmm";
        public const string modname = "WMSModManager";
        public const string version = "1.0.8";

        private static string PluginFolder;
        private static string AssetBundlePath;
        private static string GameFolder;

        private static string ModListRepoFile = "https://raw.githubusercontent.com/OtherGamer/WMSModManager/main/modsUpdates.json";
        private static string ModListRepoFileNew = "https://raw.githubusercontent.com/OtherGamer/WMSModManager/main/downloadInfo.json";

        public static AssetBundle modBundle;

        public static ManualLogSource LOGGER;
        public static Dictionary<string, string> modUpdates = new Dictionary<string, string>();

        public static bool isLoading = true;
        public static int ModsToBeLoaded = 2;
        public static int ModsLoaded = 1;

        private string wmsVersion = null;
        private string latestWmsVersion = null;

        private void CheckWmsVersion()
        {
            if(wmsVersion != null)
            {
                LifeCycle.mods["wms"].Version = wmsVersion;
            }

            if(wmsVersion != null && latestWmsVersion != null)
            {
                if (ModUpdateChecker.CompareVersion(wmsVersion, latestWmsVersion))
                {
                    LifeCycle.mods["wms"].status = Status.Update;
                }
            }
        }

        private void Awake()
        {
            isLoading = true;

            LOGGER = Logger;

            Logger.LogInfo("Starting WMSModManager...");

            SceneManager.sceneLoaded += SceneLoaded;

            PluginFolder = Directory.GetParent(this.Info.Location).FullName;
            GameFolder = Directory.GetParent(this.Info.Location).Parent.Parent.Parent.FullName;
            AssetBundlePath = PluginFolder + "/wmsmodassets";

            //Logger.LogInfo($"Plugin folder is \"{PluginFolder}\"");

            modBundle = AssetBundle.LoadFromFile(AssetBundlePath);

            Logger.LogInfo($"Mod asset bundle loaded!");

            ModInfo wmsGame = new ModInfo();
            wmsGame.ModID = "wms";
            wmsGame.Name = "Worker Murdering Simulator";
            wmsGame.Author = "RomeoManll";
            wmsGame.Version = "1.0";
            wmsGame.Description = "Base game. Go! Kill some workers!";
            wmsGame.Priority = -2000;
            wmsGame.logger = BepInEx.Logging.Logger.CreateLogSource("WMS");
            wmsGame.status = Status.Loaded;

            ModInfo wmsModInfo = new ModInfo();
            wmsModInfo.ModID = modid;
            wmsModInfo.Name = modname;
            wmsModInfo.Author = "n00b";
            wmsModInfo.Version = version;
            wmsModInfo.Description = "Mod manager for your mods. Thanks for using <3";
            wmsModInfo.Priority = -1000;
            wmsModInfo.logger = Logger;
            wmsModInfo.bundles.Add("wmsmodassets");
            wmsModInfo.status = Status.Loaded;

            Logger.LogInfo($"Loading modules");

            MainMenuPatches.ApplyPatches();

            UIManager.Start();

            string content = "{}";

            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(ModListRepoFile);
                StreamReader reader = new StreamReader(stream);
                content = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex.Message);
                Logger.LogWarning("Mod updates info failed to download, skipping");
                content = "{}";
            }
            if (content == null || content == "")
            {
                content = "{}";
            }

            try
            {
                modUpdates = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            } catch
            {
                Logger.LogWarning("Mod updates info failed to load, skipping");
                modUpdates = new Dictionary<string, string>();
            }

            if (modUpdates.ContainsKey(wmsModInfo.ModID))
            {
                if (ModUpdateChecker.CompareVersion(wmsModInfo.Version, modUpdates[wmsModInfo.ModID]))
                {
                    wmsModInfo.status = Status.Update;
                }
            }

            LifeCycle.mods.Add("wms", wmsGame);
            LifeCycle.mods.Add("wmsmm" + "", wmsModInfo);

            Instantiate(WMSAssetUtils.getWMSPrefab("seccanvas"));

            LifeCycle.LoadMods(GameFolder);

            LOGGER.LogInfo("Mods loaded!");

            if (modUpdates.ContainsKey("wms")) {
                latestWmsVersion = modUpdates["wms"];
                CheckWmsVersion();
            }

            ModsLoaded++;
        }

        private void Update()
        {
            if (ModsLoaded >= ModsToBeLoaded && isLoading)
            {
                isLoading = false;
            }
        }

        private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            LOGGER.LogInfo("Scene loaded!");
            if (scene != null)
            {

                if (scene.name == "MainMenu")
                {

                    GameObject gameVersionObject = GameObject.Find("MainMenuP").transform.GetChild(0).gameObject;
                    if (gameVersionObject != null)
                    {
                        TextMeshProUGUI gameVersion = gameVersionObject.GetComponent<TextMeshProUGUI>();
                        string gameVersionText = gameVersion.text;
                        gameVersionText = gameVersionText.Substring(1, gameVersionText.Length - 1);
                        wmsVersion = gameVersionText;
                        CheckWmsVersion();
                    }
                    else
                    {
                        wmsVersion = "1.0";
                        CheckWmsVersion();
                    }
                }
            }
        }

        public static void ToggleMod(string s)
        {
            if(s == modid || s == "wms") { return; }
            if (LifeCycle.mods.ContainsKey(s))
            {
                if (LifeCycle.mods[s].ModPath == "" || LifeCycle.mods[s].ModPath == null) { return; }
                var disabled = (LifeCycle.mods[s].status == Status.Disabled) ^ (LifeCycle.mods[s].restart);
                LifeCycle.mods[s].restart = !LifeCycle.mods[s].restart;
                LifeCycle.mods[s].Disabled = !disabled;
                File.WriteAllText(LifeCycle.mods[s].ModPath + "/mod.json", JsonUtility.ToJson(LifeCycle.mods[s], true));
            }
        } 
    }
}
