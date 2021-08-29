using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
    // Reference: https://forum.unity.com/threads/tab-between-input-fields.263779/page-2 from user merlin4
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            EventSystem system = EventSystem.current;
            GameObject curObj = system.currentSelectedGameObject;
            GameObject nextObj = null;
            if (!curObj)
            {
                nextObj = system.firstSelectedGameObject;
            }
            else
            {
                Selectable curSelect = curObj.GetComponent<Selectable>();
                Selectable nextSelect =
                    Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                        ? curSelect.FindSelectableOnUp()
                        : curSelect.FindSelectableOnDown();
                if (nextSelect)
                {
                    nextObj = nextSelect.gameObject;
                }
            }
            if (nextObj)
            {
                system.SetSelectedGameObject(nextObj, new BaseEventData(system));
            }
        }
    }
}