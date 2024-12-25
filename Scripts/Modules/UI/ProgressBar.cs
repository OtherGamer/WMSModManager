using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WMSModManager.UI
{
    public class ProgressBar : MonoBehaviour
    {
        // Start is called before the first frame update

        public float progress = 0.0f;
        public float currentProgress = 0.0f;
        public GameObject progressReducer;
        private RectTransform progresstransform;
        private float progressSize = 0.0f;

        void Start()
        {
            progresstransform = progressReducer.GetComponent<RectTransform>();
            progressSize = GetComponent<RectTransform>().sizeDelta.x - 20.0f;
        }

        // Update is called once per frame
        void Update()
        {
            if(WMSModManagerMain.ModsToBeLoaded != 0)
                progress = (float)WMSModManagerMain.ModsLoaded / WMSModManagerMain.ModsToBeLoaded;
            currentProgress = 0.3f * currentProgress + 0.7f * progress;

            progresstransform.offsetMin = new Vector2(10.0f + progressSize * currentProgress, progresstransform.offsetMin.y);


        }
    }
}
