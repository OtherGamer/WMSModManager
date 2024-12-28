using UnityEngine;

namespace WMSModManager
{
    class GameObjectForMod : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
