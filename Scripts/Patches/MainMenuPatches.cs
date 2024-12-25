using MonoMod.RuntimeDetour;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WMSModManager.UI;

namespace WMSModManager.Patches
{
    public static class MainMenuPatches
    {
        private static Hook playP;
        public static void OpenModTab(this MainMenuContr mainMenuContr)
        {
            UIManager.CloseDropdown();

            GameObject[] allPanels = mainMenuContr.AllPanels;

            for (int i = 0; i < allPanels.Length; i++)
            {
                allPanels[i].SetActive(value: false);
            }

            WMSModManagerMain.ModP.SetActive(value: true);
        }

        public delegate void OpenTab(MainMenuContr self);

        public static void OpenTabMod(OpenTab orig, MainMenuContr self)
        {
            UIManager.CloseDropdown();
            orig(self);
        }

        public static void ApplyPatches()
        {
            BindingFlags origFlags = BindingFlags.Public | BindingFlags.Instance;
            BindingFlags modFlags = BindingFlags.Public | BindingFlags.Static;
            playP = new Hook(typeof(MainMenuContr).GetMethod("OpenPlayP", origFlags), 
                typeof(MainMenuPatches).GetMethod("OpenTabMod", modFlags));
            Hook settingsP = new Hook(typeof(MainMenuContr).GetMethod("OpenSettingsP", origFlags),
                typeof(MainMenuPatches).GetMethod("OpenTabMod", modFlags));
            Hook statsP = new Hook(typeof(MainMenuContr).GetMethod("OpenStatsP", origFlags),
                typeof(MainMenuPatches).GetMethod("OpenTabMod", modFlags));
            Hook friendsP = new Hook(typeof(MainMenuContr).GetMethod("OpenFriedsP", origFlags),
                typeof(MainMenuPatches).GetMethod("OpenTabMod", modFlags));
            Hook boostyP = new Hook(typeof(MainMenuContr).GetMethod("OpenBoosty", origFlags),
                typeof(MainMenuPatches).GetMethod("OpenTabMod", modFlags));
        }
    }
}
