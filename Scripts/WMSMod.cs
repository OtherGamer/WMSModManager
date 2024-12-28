using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WMSModManager
{
    public class WMSMod : MonoBehaviour
    {
        public string ModID;
        public ManualLogSource Logger;
        public string[] Bundles;
        public string ModFolder;

        public void ModStartup(string ModID)
        {
            try
            {
                this.ModID = ModID;
                Logger = LifeCycle.mods[ModID].logger;
                Bundles = LifeCycle.mods[ModID].bundles.ToArray();
                ModFolder = LifeCycle.mods[ModID].ModPath;

                Logger.LogInfo($"Starting {ModID}, version {LifeCycle.mods[ModID].Version}");

                OnStartup();

                LifeCycle.mods[ModID].status = Status.Loaded;

                if(WMSModManagerMain.modUpdates.ContainsKey(ModID))
                {
                    string internetVersion = WMSModManagerMain.modUpdates[ModID];
                    string currentVersion = LifeCycle.mods[ModID].Version;

                    if(ModUpdateChecker.CompareVersion(currentVersion, internetVersion)) {
                        LifeCycle.mods[ModID].status = Status.Update;
                    }
                }
            }
            catch (Exception ex) 
            {
                Logger.LogError($"Mod {ModID} crashed on loading...");
                Logger.LogError($"Error: {ex.Message}");
                Logger.LogError($"Error: {ex.StackTrace}");
                LifeCycle.mods[ModID].status = Status.Crashed;
            }
            WMSModManagerMain.ModsLoaded++;
        }

        public virtual void OnStartup()
        {
            
        }
    }
}
