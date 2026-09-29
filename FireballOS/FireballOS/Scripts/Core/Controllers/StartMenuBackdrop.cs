using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FireballOS
{
    public class StartMenuBackdrop : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private StartMenuController controller;

        public void SetController(StartMenuController targetController)
        {
            controller = targetController;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Menüyü ve backdrop'ı hemen kapat
            if (controller != null)
            {
                controller.CloseMenu();
            }

            // Tıklamayı arkadaki masaüstü ikonuna ilet
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject != gameObject &&
                    (controller == null || !result.gameObject.transform.IsChildOf(controller.transform)))
                {
                    // Eğer tıklanan nesne StartButton veya onun bir parçasıysa tıklamayı iletme.
                    // Kullanıcı menüyü kapatmak için StartButton'a tıkladı ve menü zaten CloseMenu() ile kapandı.
                    if (controller != null && controller.IsStartButton(result.gameObject))
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