using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WMSModManager.Patches;

namespace WMSModManager
{
    public class WMSAssetUtils
    {
        public static GameObject getWMSPrefab(string name)
        {
            string path = $"assets/wmsmod/assets/prefabs/{name}.prefab";
            GameObject prefab = WMSModManagerMain.modBundle.LoadAsset<GameObject>(path);
            FixShaders.FixShadersOnObject(prefab);
            return prefab;
        }
    }
}
