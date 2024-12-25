using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WMSModManager;

namespace WMSModManager.UI
{
    public class ScrollBar : MonoBehaviour
    {
        public float scrollSz = 660.0f;
        public float scrollRealSz = 700.0f;
        public float scrollRes = 1000.0f;

        GameObject modPanel;

        public void CustomStart(GameObject _modPanel)
        {
            modPanel = _modPanel;
            float res = scrollSz / scrollRes;
            if (res > 1.0f)
                res = 1.0f;
            RectTransform rectTransform = transform.GetChild(0).gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, res * scrollRealSz);
            rectTransform.SetLocalPositionAndRotation(new Vector3(rectTransform.localPosition.x, (scrollRealSz - res * scrollRealSz) / 2, rectTransform.localPosition.z), rectTransform.localRotation);
            
            Scroller scroller = GetComponentInChildren<Scroller>();
            scroller.end = rectTransform.localPosition.y;
            scroller.start = -rectTransform.localPosition.y;
        }

        public void setProgress(float progress)
        {
            ModElement[] elements = modPanel.transform.GetComponentsInChildren<ModElement>();
            foreach (ModElement element in elements)
            {
                if (scrollSz < scrollRes)
                    element.setProgress(progress, scrollRes - scrollSz);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
