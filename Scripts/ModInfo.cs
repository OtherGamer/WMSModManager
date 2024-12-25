using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WMSModManager
{
    public enum Status
    {
        Loaded,
        Disabled,
        Loading,
        Failed,
        Crashed,
        Update,
        Unknown
    }

    [Serializable]
    public class ModLoadFile
    {
        public string ModID;
        public string Name;
        public string Author;
        public string Version;
        public string Description;
        public int Priority;
        public bool Disabled;
    }

    [Serializable]
    public class ModInfo
    {
        public string ModID = null;
        public string Name = "Unknown name";
        public string Author = "Unknown author";
        public string Version = "1.0.0";
        public string Description = "Empty description";
        public int Priority = 0;
        public bool Disabled = false;
        [NonSerialized]
        public string ModPath;
        [NonSerialized]
        public Status status;
        [NonSerialized]
        public bool restart;
        [NonSerialized]
        public Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
        [NonSerialized]
        public ManualLogSource logger;
    }
}
