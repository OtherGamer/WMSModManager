using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx.Logging;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Runtime;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using TMPro;
using WMSModManager.Patches;

namespace WMSModManager
{
    [BepInPlugin(modid, modname, version)]
    public class WMSModManagerMain : BaseUnityPlugin
    {
        public const string modid = "ru.n00b.wmsmodmanager";
        public const string modname = "WMSModManager";
        public const string version = "1.0.8";

        private static string PluginFolder;
        private static string AssetBundlePath;
        private static string GameFolder;

        private static string ModListRepoFile = "https://raw.githubusercontent.com/OtherGamer/WMSModManager/main/modsUpdates.json";
        private static string ModListRepoFileNew = "https://raw.githubusercontent.com/OtherGamer/WMSModManager/main/downloadInfo.json";

        public static AssetBundle modBundle;

        public static GameObject ModP;

        public static ManualLogSource LOGGER;
        public static Dictionary<string, ModInfo> LoadedMods = new Dictionary<string, ModInfo>();
        public static Dictionary<string, string> modUpdates = new Dictionary<string, string>();
        public static string lastAdded;

        public static bool isLoading = true;
        public static int ModsToBeLoaded = 0;
        public static int ModsLoaded = 1;

        private string wmsVersion = null;
        private string latestWmsVersion = null;

        private void CheckWmsVersion()
        {
            if(wmsVersion != null)
            {
                WMSModManagerMain.LoadedMods["WMS"].Version = wmsVersion;
            }

            if(wmsVersion != null && latestWmsVersion != null)
            {
                if (ModUpdateChecker.CompareVersion(wmsVersion, latestWmsVersion))
                {
                    LoadedMods["WMS"].status = Status.Update;
                }
            }
        }

        private void Awake()
        {
            isLoading = true;

            LOGGER = Logger;

            Logger.LogInfo("Starting WMSModManager...");

            SceneManager.sceneLoaded += SceneLoaded;

            PluginFolder = System.IO.Directory.GetParent(this.Info.Location).FullName;
            GameFolder = System.IO.Directory.GetParent(this.Info.Location).Parent.Parent.Parent.FullName;
            AssetBundlePath = PluginFolder + "/wmsmodassets";

            //Logger.LogInfo($"Plugin folder is \"{PluginFolder}\"");

            modBundle = AssetBundle.LoadFromFile(AssetBundlePath);

            Logger.LogInfo($"Mod asset bundle loaded!");

            ModInfo wmsGame = new ModInfo();
            wmsGame.Name = "WMS";
            wmsGame.Author = "RomeoManll";
            wmsGame.Version = "1.0";
            wmsGame.Description = "Base game. Go! Kill some workers!";
            wmsGame.Priority = -2000;
            wmsGame.logger = BepInEx.Logging.Logger.CreateLogSource("WMS");
            wmsGame.status = Status.Loaded;

            ModInfo wmsModInfo = new ModInfo();
            wmsModInfo.Name = modname;
            wmsModInfo.Author = "n00b";
            wmsModInfo.Version = version;
            wmsModInfo.Description = "Mod manager for your mods. Thanks for using <3";
            wmsModInfo.Priority = -1000;
            wmsModInfo.logger = Logger;
            wmsModInfo.bundles.Add("default", modBundle);
            wmsModInfo.status = Status.Loaded;

            Logger.LogInfo($"Loading modules");

            MainMenuPatches.ApplyPatches();

            GameObject uiNanager = new GameObject("UIManager");

            uiNanager.AddComponent<GameObjectForMod>();
            uiNanager.AddComponent<UI.UIManager>();

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

            if (modUpdates[wmsModInfo.Name] != null)
            {
                if (ModUpdateChecker.CompareVersion(wmsModInfo.Version, modUpdates[wmsModInfo.Name]))
                {
                    wmsModInfo.status = Status.Update;
                }
            }

            if (modUpdates["WMS"] != null)
            {
                latestWmsVersion = modUpdates["WMS"];
                CheckWmsVersion();
            }

            LoadedMods.Add("WMS", wmsGame);
            LoadedMods.Add("WMSModManager" + "", wmsModInfo);

            try
            {
                if (!Directory.Exists(GameFolder + "/Mods"))
                {
                    Directory.CreateDirectory(GameFolder + "/Mods");
                }

                string[] files = Directory.GetDirectories(GameFolder + "/Mods");

                List<ModInfo> FindedMods = new List<ModInfo>();

                Logger.LogInfo("First mod load iteration -> Collecting info");

                foreach (string file in files)
                {
                    if (File.Exists(file + "/mod.json"))
                    {
                        string modInfoText = File.ReadAllText(file + "/mod.json");
                        ModLoadFile modInfo = JsonUtility.FromJson<ModLoadFile>(modInfoText);

                        if (modInfo.Name == null)
                        {
                            Logger.LogInfo($"Loading mod {Path.GetDirectoryName(file)} failed! No name provided!");
                            continue;
                        }

                        if (modInfo.Name.Length == 0)
                        {
                            Logger.LogInfo($"Loading mod {Path.GetDirectoryName(file)} failed! No name provided!");
                            continue;
                        }

                        if (modInfo.Author == null)
                        {
                            modInfo.Author = "Unknown author";
                        }

                        if (modInfo.Version == null)
                        {
                            modInfo.Version = "Unknown version";
                        }

                        if (modInfo.Description == null)
                        {
                            modInfo.Description = "Unknown description";
                        }

                        Logger.LogInfo($"Mod {modInfo.Name} found!");

                        ModInfo info = new ModInfo();
                        info.Name = modInfo.Name;
                        info.Author = modInfo.Author;
                        info.Version = modInfo.Version;
                        info.Description = modInfo.Description;
                        info.Priority = modInfo.Priority;
                        info.ModPath = file;
                        info.logger = BepInEx.Logging.Logger.CreateLogSource(info.Name);
                        if (modInfo.Disabled)
                        {
                            Logger.LogInfo($"Mod {info.Name} disabled");
                            info.status = Status.Disabled;
                        }
                        else
                        {
                            ModsToBeLoaded++;
                            info.status = Status.Loading;
                        }

                        FindedMods.Add(info);
                    }
                    else
                    {
                        Logger.LogError($"Loading mod {Path.GetDirectoryName(file)} failed: no mod.json found!");
                    }
                }

                //Logger.LogInfo("Sorting mods with provided priority...");

                Comparison<ModInfo> comparison = (x, y) => x.Priority.CompareTo(y.Priority);
                FindedMods.Sort(comparison);

                Logger.LogInfo("Finded mods:");
                foreach (ModInfo mod in FindedMods)
                {
                    Logger.LogInfo(mod.Name);
                }

                Logger.LogInfo("Second mod load iteration -> Loading into game");

                foreach (ModInfo info in FindedMods)
                {
                    try
                    {

                        if (LoadedMods.ContainsKey(info.Name))
                        {
                            ModsLoaded++;
                            Logger.LogError($"Loading mod {info.Name} failed! That mod was already loaded!");
                            continue;
                        }

                        if (info.status == Status.Disabled)
                        {
                            LoadedMods.Add(info.Name, info);
                            continue;
                        }

                        lastAdded = info.Name;

                        LoadedMods.Add(info.Name, info);

                        Logger.LogInfo($"Loading mod {info.Name} with WMS Mod...");

                        if (Directory.Exists(info.ModPath + "/AssetsBundles"))
                        {
                            Logger.LogInfo($"Loading bundles...");

                            try
                            {
                                string[] bundles = Directory.GetFiles(info.ModPath + "/AssetsBundles");

                                foreach (string bundle in bundles)
                                {
                                    if (Path.GetExtension(bundle).ToLower() == ".manifest")
                                    {
                                        Logger.LogInfo($"Manifest for {Path.GetFileName(bundle)} found!");
                                        Logger.LogInfo($"Skipping it and hoping, that everything will be fine (Unity please load this manifest by yourself)");
                                        continue;
                                    }
                                    try
                                    {
                                        LoadedMods[info.Name].bundles.Add(Path.GetFileName(bundle), AssetBundle.LoadFromFile(bundle));
                                    }
                                    catch
                                    {
                                        Logger.LogError($"Loading mod {info.Name} failed! Asset bundle {bundle} loading error!");
                                        Logger.LogError($"Be scared of what can happen next!");
                                    }
                                }
                            }
                            catch
                            {
                                LoadedMods[info.Name].status = Status.Failed;
                                Logger.LogInfo($"Loading mod {info.Name} failed! Asset bundle loading error!");
                            }
                        }

                        if (LoadedMods[info.Name].status == Status.Failed)
                        {
                            ModsLoaded++;
                            continue;
                        }

                        List<Type> modInstances = new List<Type>();

                        if (Directory.Exists(info.ModPath + "/Dlls"))
                        {
                            Logger.LogInfo($"Loading dlls");

                            try
                            {
                                string[] dlls = Directory.GetFiles(info.ModPath + "/Dlls", "*.dll");

                                foreach (string dll in dlls)
                                {
                                    try
                                    {
                                        Assembly assembly = Assembly.LoadFrom(dll);
                                        if (assembly != null)
                                        {
                                            Type[] classes = assembly.GetTypes();
                                            foreach (Type t in classes)
                                            {
                                                if (t.IsSubclassOf(typeof(WMSMod)))
                                                {
                                                    modInstances.Add(t);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LoadedMods[info.Name].status = Status.Failed;
                                            Logger.LogInfo($"Loading mod {info.Name} failed! Assembly not found! (How tf this happened... I readed this folder nanoseconds ago)");
                                        }
                                    }
                                    catch (BadImageFormatException e)
                                    {
                                        LoadedMods[info.Name].status = Status.Failed;
                                        Logger.LogInfo($"Loading mod {info.Name} failed! Invalid Assembly {dll} (Still loading other of them lol, so others assembys will be available in runtime)");
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.LogInfo($"Loading assembly {dll} failed because {e.Message}! It's not bad exception, so load continues");
                                    }
                                }
                            }
                            catch
                            {
                                LoadedMods[info.Name].status = Status.Failed;
                                Logger.LogInfo($"Loading mod {info.Name} failed! Assembly loading error!");
                            }
                        }

                        if (LoadedMods[info.Name].status != Status.Failed)
                        {
                            Logger.LogInfo("Creating instances in Scene...");

                            if (modInstances.Count == 0)
                            {
                                Logger.LogInfo("Mod types not found, pretending it's loaded");
                                LoadedMods[info.Name].status = Status.Loaded;
                                ModsLoaded++;
                            }

                            foreach (Type t in modInstances) {

                                GameObject modObj = new GameObject(info.Name);

                                modObj.SetActive(false);

                                modObj.AddComponent<GameObjectForMod>();
                                WMSMod modClass = (WMSMod) modObj.AddComponent(t);
                                modClass.ModStartup(info.Name);

                                modObj.SetActive(true);
                            }

                            Logger.LogInfo($"Mod {info.Name} loaded!");
                        }
                        else
                        {
                            ModsLoaded++;
                        }
                    }
                    catch
                    {
                        ModsLoaded++;
                        Logger.LogError($"Loading mod {Path.GetDirectoryName(info.ModPath)} failed: internal error happened!");
                    }
                }
            }
            catch
            {
                Logger.LogFatal("Mods loading failed... manager crashed... closing game");
                Application.Quit();
            }

            Logger.LogInfo("Loading mods in process!");

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
            if(s == modname || s == "WMS") { return; }
            if (LoadedMods.ContainsKey(s))
            {
                if (LoadedMods[s].ModPath == "" || LoadedMods[s].ModPath == null) { return; }
                var disabled = (LoadedMods[s].status == Status.Disabled) ^ (LoadedMods[s].restart);
                ModLoadFile file = new ModLoadFile();
                file.Name = LoadedMods[s].Name;
                file.Version = LoadedMods[s].Version;
                file.Author = LoadedMods[s].Author;
                file.Description = LoadedMods[s].Description;
                LoadedMods[s].restart = !LoadedMods[s].restart;
                file.Disabled = !disabled;
                File.WriteAllText(LoadedMods[s].ModPath + "/mod.json", JsonUtility.ToJson(file, true));
            }
        } 
    }
}
