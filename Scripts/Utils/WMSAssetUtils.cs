using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using WMSModManager.Patches;

namespace WMSModManager
{
    public class WMSAssetUtils
    {

        private static Dictionary<AssetInfo, Object> cache = new Dictionary<AssetInfo, Object>();
        private static Dictionary<string, LoadedAssetBundle> loadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
        public static Dictionary<string, AssetBundleInfo> registeredBundles = new Dictionary<string, AssetBundleInfo>();

        private class AssetInfo {
            public string assetBundle;
            public string assetName;
            public AssetInfo(string assetBundle, string assetName) {
                this.assetBundle = assetBundle;
                this.assetName = assetName;
            }
        }

        private class LoadedAssetBundle {
            public AssetBundle bundle;
            public AssetBundleManifest manifest;

            public LoadedAssetBundle(string assetBundleName, string assetBundlePath) {
                bundle = AssetBundle.LoadFromFile(assetBundlePath);
                manifest = bundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
                if(manifest != null) {
                    if(manifest.GetAllDependencies(assetBundleName).Length > 0) { 
                        WMSModManagerMain.LOGGER.LogWarning($"Asset bundle {assetBundleName} requires {manifest.GetAllDependencies(assetBundleName).ToString()}");
                    }
                }
                loadedAssetBundles.Add(assetBundleName, this);
            }
        }

        public class AssetBundleInfo {
            public string assetBundleName;
            public string assetBundlePath;

            public AssetBundleInfo(string assetBundleName, string assetBundlePath) {
                this.assetBundlePath = assetBundlePath;
                this.assetBundleName = assetBundleName;
            }
            public void Load() {
                new LoadedAssetBundle(assetBundleName, assetBundlePath);
            }
        }

        public static void RegisterAssetBundle(string assetBundlePath) {
            registeredBundles[Path.GetFileName(assetBundlePath)] = new AssetBundleInfo(Path.GetFileName(assetBundlePath), assetBundlePath);
        }

        public static GameObject getWMSPrefab(string name)
        {
            AssetInfo assetInfo = new AssetInfo(WMSModManagerMain.modBundle.name, $"assets/wmsmod/assets/prefabs/{name}.prefab");
            if (cache.ContainsKey(assetInfo)) {
                return (GameObject)cache[assetInfo];
            }
            string path = $"assets/wmsmod/assets/prefabs/{name}.prefab";
            GameObject prefab = WMSModManagerMain.modBundle.LoadAsset<GameObject>(path);
            FixShaders.FixShadersOnObject(prefab);
            cache[assetInfo] = prefab;
            return prefab;
        }
        public static GameObject getPrefab(string bundle, string path) {
            if (!loadedAssetBundles.ContainsKey(bundle)) {
                if(registeredBundles.ContainsKey(bundle)) {
                    registeredBundles[bundle].Load();
                } else {
                    WMSModManagerMain.LOGGER.LogError($"Sorry, bundle {bundle} is not registered!");
                }
            }
            GameObject prefab = loadedAssetBundles[bundle].bundle.LoadAsset<GameObject>(path);
            FixShaders.FixShadersOnObject(prefab);
            return prefab;
        }

        private static void UnloadBundle(AssetBundle bundle) {
            AsyncOperation operation = bundle.UnloadAsync(true);
            while (!operation.isDone);
            loadedAssetBundles.Remove(bundle.name);
        }

        private static void UnloadBundle(string bundle) {
            if (loadedAssetBundles.ContainsKey(bundle)) {
                AsyncOperation operation = loadedAssetBundles[bundle].bundle.UnloadAsync(true);
                while (!operation.isDone) ;
                loadedAssetBundles.Remove(bundle);
            }
        }

        public static void UnloadAll() {
            cache.Clear();
            List<Task> tasks = new List<Task>();
            foreach (var loadedBundle in loadedAssetBundles) {
                tasks.Add(Task.Factory.StartNew(() => UnloadBundle(loadedBundle.Value.bundle)));
            }
            Task.WaitAll(tasks.ToArray());
            loadedAssetBundles.Clear();
        }
    }
}
