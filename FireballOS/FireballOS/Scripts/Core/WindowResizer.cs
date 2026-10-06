using UnityEngine;
using UnityEngine.EventSystems;

namespace FireballOS
{
    public class WindowResizer : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private RectTransform windowRect;
        [SerializeField] private Vector2 minSize = new Vector2(300f, 200f);
        [SerializeField] private Vector2 maxSize = new Vector2(1920f, 1020f);

        public Vector2 MinSize
        {
            get => minSize;
            set => minSize = value;
        }

        public Vector2 MaxSize
        {
            get => maxSize;
            set => maxSize = value;
        }

        private Window windowComponent;

        private void Awake()
        {
            if (windowRect == null)
                windowRect = GetComponentInParent<RectTransform>();

            windowComponent = windowRect.GetComponent<Window>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Tıklandığında pencereyi en öne getir
            if (windowComponent != null)
            {
                windowComponent.BringToFront();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (windowRect == null) return;

            // Farenin anlık hareket farkı (delta)
            Vector2 delta = eventData.delta;

            Vector2 currentSize = windowRect.sizeDelta;
            Vector2 newSize = new Vector2(
                Mathf.Clamp(currentSize.x + delta.x, minSize.x, maxSize.x),
                Mathf.Clamp(currentSize.y - delta.y, minSize.y, maxSize.y)
            );

            // Gerçekte değişen miktar
            Vector2 appliedDelta = new Vector2(
                newSize.x - currentSize.x,
                newSize.y - currentSize.y
            );

            // Boyutu güncelle
            windowRect.sizeDelta = newSize;

            // Pivot merkezde (0.5, 0.5) olduğu için sağa ve aşağı doğru kaydır
            windowRect.anchoredPosition += new Vector2(appliedDelta.x * 0.5f, -appliedDelta.y * 0.5f);
        }
    }
}
