using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WMSModManager.UI
{
    public class CanvasCode : MonoBehaviour
    {
        private CanvasGroup group;
        private GameObject fCanvas;
        private float timePassed = 0;
        private bool LoadedCatched = false;
        // Start is called before the first frame update
        void Start()
        {
            fCanvas = GameObject.Find("Canvas");
            group = GetComponent<CanvasGroup>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!WMSModManagerMain.isLoading)
            {
                timePassed += Time.deltaTime;
            }
            else
            {
                fCanvas.SetActive(false);
            }
            if(timePassed >= 2.0f)
            {
                gameObject.SetActive(false);
            }
            if (timePassed >= 1.0f)
            {
                fCanvas.SetActive(true);
                group.alpha = (2.0f - timePassed);
            }
        }
    }
}
