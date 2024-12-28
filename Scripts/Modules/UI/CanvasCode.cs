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
            if (fCanvas != null) {
                if (timePassed < 1.0f && fCanvas.activeSelf) {
                    fCanvas.SetActive(false);
                }
                if (timePassed >= 1.0f && !fCanvas.activeSelf) {
                    if (!fCanvas.activeSelf) {
                        fCanvas.SetActive(true);
                    }
                    group.alpha = (2.0f - timePassed);
                }
            } else {
                fCanvas = GameObject.Find("Canvas");
            }
            if(timePassed >= 2.0f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
