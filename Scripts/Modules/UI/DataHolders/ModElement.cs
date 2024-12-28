using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WMSModManager.UI
{
    public class ModElement : MonoBehaviour, IPointerClickHandler
    {
        // Start is called before the first frame update
        public string modID;
        public TextMeshProUGUI ModName;
        public TextMeshProUGUI ModStatus;
        public TextMeshProUGUI ModAuthor;
        public TextMeshProUGUI ModVersion;

        public float startPos = 290.0f;

        void Start()
        {

        }

        public void fixTransform()
        {
            startPos = gameObject.transform.localPosition.y;
        }

        public void setProgress(float progress, float sz)
        {
            transform.SetLocalPositionAndRotation(new Vector3(
                    transform.localPosition.x,
                    startPos + progress*sz,
                    transform.localPosition.z
                ),
                transform.localRotation);
        }

        public void UpdateStatus()
        {
            if (LifeCycle.mods[modID].restart)
            {
                ModStatus.text = "<color=\"red\">Restart required</color>";
                return;
            }
            Status modStatus = LifeCycle.mods[modID].status;
            if (modStatus == Status.Loaded)
            {
                ModStatus.text = "<color=\"green\">Enabled</color>";
            }
            else if (modStatus == Status.Disabled)
            {
                ModStatus.text = "<color=\"red\">Disabled</color>";
            }
            else if (modStatus == Status.Update)
            {
                ModStatus.text = "<color=\"yellow\">Update available</color>";
            }
            else if (modStatus == Status.Failed)
            {
                ModStatus.text = "<color=\"red\">Failed to load</color>";
            }
            else if (modStatus == Status.Crashed)
            {
                ModStatus.text = "<color=\"red\">Crashed</color>";
            }
            else
            {
                ModStatus.text = "<color=\"red\">Status unknown...</color>";
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Right || eventData.button == PointerEventData.InputButton.Left)
            {
                UIManager.ShowDropdown(this, eventData, ModName.text);
            }
        }
    }
}
