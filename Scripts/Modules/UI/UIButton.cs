using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WMSModManager;
using UnityEditor;
using UnityEngine.EventSystems;

namespace WMSModManager.UI
{
    public delegate void buttonClick();
    public class UIButton : MonoBehaviour, IPointerClickHandler
    {
        public buttonClick leftClick;
        public buttonClick rightClick;

        public TextMeshProUGUI buttonText;

        public void OnPointerClick(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left && leftClick != null)
            {
                leftClick();
            }
            if (data.button == PointerEventData.InputButton.Right && rightClick != null)
            {
                rightClick();
            }
        }
    }
}
