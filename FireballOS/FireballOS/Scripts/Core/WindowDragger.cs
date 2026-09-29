using UnityEngine;
using UnityEngine.EventSystems;

namespace FireballOS
{
    public class WindowDragger : MonoBehaviour, IDragHandler
    {
        [SerializeField] private RectTransform targetWindow;
        [SerializeField] private Window window;

        private void Awake()
        {
            if (targetWindow == null)
                targetWindow = GetComponentInParent<RectTransform>();

            if (window == null && targetWindow != null)
                window = targetWindow.GetComponent<Window>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Eğer pencere tam ekrandaysa sürüklemeyi doğrudan engelle
            if (window != null && window.IsMaximized) 
                return;

            if (targetWindow != null)
            {
                targetWindow.anchoredPosition += eventData.delta;
            }
        }
    }
}