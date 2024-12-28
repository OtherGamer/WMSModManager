using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace WMSModManager
{
    public class LifeCycle
    {
        private static Regex modidChecker = new Regex(@"^[a-z0-9._]+$");

        public static Dictionary<string, ModInfo> mods = new Dictionary<string, ModInfo>();
        private static bool existOrCreateDir(string path) {
            if (Directory.Exists(path)) {
                return true;
            } else {
                try {
                    Directory.CreateDirectory(path);
                    return true;
                } catch {
                    return false;
                }
            }
        }

        public static ModInfo GatherMod(string modFolder) {
            if (File.Exists(Path.Combine(modFolder, "mod.json"))) {
                string modInfoText = File.ReadAllText(Path.Combine(modFolder, "mod.json"));
                ModInfo modInfo = JsonUtility.FromJson<ModInfo>(modInfoText);

                if(modInfo.ModID == null || modInfo.ModID == "") {
                    WMSModManagerMain.LOGGER.LogError("modid must not be empty");
                    return null;
                }

                if(!modidChecker.IsMatch(modInfo.ModID)) {
                    WMSModManagerMain.LOGGER.LogError("modid may only contains a-z0-9._");
                    return null;
                }

                modInfo.ModPath = modFolder;

                if (!modInfo.Disabled) {
                    modInfo.logger = BepInEx.Logging.Logger.CreateLogSource(modInfo.ModID);
                    modInfo.status = Status.Loading;
                    modInfo.logger.LogMessage($"Collected {modInfo.ModID}");

                    WMSModManagerMain.ModsToBeLoaded++;
                } else {
                    modInfo.status = Status.Disabled;
                }

                return modInfo;

            } else {
                WMSModManagerMain.LOGGER.LogWarning(modFolder + " mod.json not found");
                return null;
            }
        }

        public static void CollectAssetBundles(ref ModInfo modInfo) {
            if (Directory.Exists(Path.Combine(modInfo.ModPath, "AssetsBundles"))) {
                foreach (var assetFile in Directory.GetFiles(Path.Combine(modInfo.ModPath, "AssetsBundles"))) {
                    if (Path.GetExtension(assetFile).ToLower() != ".manifest") {
                        modInfo.logger.LogMessage($"Registering {assetFile} asset bundle for {modInfo.ModID}!");
                        WMSAssetUtils.RegisterAssetBundle(Path.Combine(modInfo.ModPath, "AssetsBundles", assetFile));
                        modInfo.bundles.Add(assetFile);
                    }
                }
            }
        }

        public static void LoadMods(string GameFolder) {
            if (existOrCreateDir(Path.Combine(GameFolder, "Mods"))) {
                List<Task<ModInfo>> gatheredMods = new List<Task<ModInfo>>();

                foreach (var modsFolder in Directory.GetDirectories(Path.Combine(GameFolder, "Mods"))) {
                    gatheredMods.Add(Task<ModInfo>.Factory.StartNew(() => GatherMod(Path.Combine(GameFolder, "Mods", modsFolder))));
                }

                Task.WaitAll(gatheredMods.ToArray());
                List<Task> loadedAssetBundles = new List<Task>();

                foreach (var item in gatheredMods) {
                    if (item.Result != null) {
                        loadedAssetBundles.Add(Task.Factory.StartNew(() => {
                            ModInfo modInfo = item.Result;
                            if(mods.ContainsKey(modInfo.ModID)) {
                                WMSModManagerMain.LOGGER.LogError($"Mod with same ModID {modInfo.ModID} is already loaded!");
                            }
                            if(!modInfo.Disabled) {
                                CollectAssetBundles(ref modInfo);
                            }
                            mods[modInfo.ModID] = modInfo;
                            WMSModManagerMain.ModsLoaded++;
                        }));
                    }
                }

                Task.WaitAll(loadedAssetBundles.ToArray());

            } else {
                WMSModManagerMain.LOGGER.LogFatal("Failed to read or create Mods folder!");
            }
        }
    }
}
