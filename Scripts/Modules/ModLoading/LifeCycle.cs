using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace WMSModManager
{
    public class LifeCycle
    {
        private Regex modidChecker = new Regex(@"^[a-z0-9._]+$");

        public Dictionary<string, ModInfo> mods;
        bool existOrCreateDir(string path) {
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

        ModInfo GatherMod(string modFolder) {
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

                return modInfo;

            } else {
                WMSModManagerMain.LOGGER.LogWarning(modFolder + " mod.json not found");
                return null;
            }
        }

        void LoadMods(string GameFolder) {
            if (existOrCreateDir(Path.Combine(GameFolder, "Mods"))) {
                List<Task> tasks = new List<Task>();

                foreach (var modsFolder in Directory.GetDirectories(Path.Combine(GameFolder, "Mods")))
                {
                    tasks.Add(Task<ModInfo>.Factory.StartNew(() => GatherMod(Path.Combine(GameFolder, "Mods", modsFolder))));
                }

                Task.WaitAll(tasks.ToArray());


            } else {
                WMSModManagerMain.LOGGER.LogFatal("Failed to read or create Mods folder!");
            }
        }
    }
}
