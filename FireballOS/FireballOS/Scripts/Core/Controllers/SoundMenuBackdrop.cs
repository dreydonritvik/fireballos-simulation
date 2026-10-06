using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FireballOS
{
    public class SoundMenuBackdrop : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private SoundMenuController controller;

        public void SetController(SoundMenuController targetController)
        {
            controller = targetController;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Close sound popup immediately
            if (controller != null)
            {
                controller.CloseMenu();
            }

            // Raycast behind to forward click to desktop/taskbar icons
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject != gameObject &&
                    (controller == null || !result.gameObject.transform.IsChildOf(controller.transform)))
                {
                    // If clicked object is the sound tray button or child of it, do not forward.
                    // User clicked the tray button to close the menu, which was already closed by CloseMenu().
                    if (controller != null && controller.IsSoundButton(result.gameObject))
                    {
                        break;
                    }

                    ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.pointerClickHandler);
                    break;
                }
            }
        }
    }
}
