using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WMSModManager;

namespace WMSModManager.UI
{
    public class Scroller : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        ScrollBar scrollBar;
        
        public float start = 0.0f;
        public float end = 700.0f;

        float beginDragMouse = 0.0f;
        float beginPosition = 0.0f;

        // Start is called before the first frame update
        void Start()
        {
            scrollBar = transform.parent.GetComponent<ScrollBar>();
        }
        public void OnDrag(PointerEventData eventData)
        {
            float yPos = beginPosition + (Input.mousePosition.y - beginDragMouse);

            if(yPos > end)
            {
                yPos = end;
            }

            if(yPos < start)
            {
                yPos = start;
            }

            float posSize = end - start;
            float posCur = end - transform.localPosition.y;

            scrollBar.setProgress(posCur / posSize);

            transform.SetLocalPositionAndRotation(new Vector3(transform.localPosition.x, yPos, transform.localPosition.z), transform.localRotation);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            beginDragMouse = Input.mousePosition.y;
            beginPosition = transform.localPosition.y;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
