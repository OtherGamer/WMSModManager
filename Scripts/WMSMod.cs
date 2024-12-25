using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WMSModManager
{
    public class WMSMod : MonoBehaviour
    {
        public string modName;
        public ManualLogSource Logger;
        public Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();
        public string ModFolder;

        public void ModStartup(string ModName)
        {
            try
            {
                modName = ModName;
                Logger = WMSModManagerMain.LoadedMods[modName].logger;
                LoadedBundles = WMSModManagerMain.LoadedMods[modName].bundles;
                ModFolder = WMSModManagerMain.LoadedMods[modName].ModPath;

                Logger.LogInfo($"Starting {modName}, version {WMSModManagerMain.LoadedMods[modName].Version}");

                OnStartup();

                WMSModManagerMain.LoadedMods[modName].status = Status.Loaded;

                if(WMSModManagerMain.modUpdates.ContainsKey(modName))
                {
                    string internetVersion = WMSModManagerMain.modUpdates[modName];
                    string currentVersion = WMSModManagerMain.LoadedMods[modName].Version;

                    if(ModUpdateChecker.CompareVersion(currentVersion, internetVersion)) {
                        WMSModManagerMain.LoadedMods[modName].status = Status.Update;
                    }
                }
            }
            catch (Exception ex) 
            {
                Logger.LogError($"Mod {modName} crashed on loading...");
                Logger.LogError($"Error: {ex.Message}");
                Logger.LogError($"Error: {ex.StackTrace}");
                WMSModManagerMain.LoadedMods[modName].status = Status.Crashed;
            }
            WMSModManagerMain.ModsLoaded++;
        }

        public virtual void OnStartup()
        {
            
        }
    }
}
