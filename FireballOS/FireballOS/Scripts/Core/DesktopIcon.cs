using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DesktopIcon : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private UnityEvent onDoubleClick;
    [SerializeField] private float doubleClickThreshold = 0.3f;

    private float lastClickTime;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        float timeSinceLastClick = Time.unscaledTime - lastClickTime;

        if (timeSinceLastClick <= doubleClickThreshold)
        {
            onDoubleClick?.Invoke();
            lastClickTime = 0f;
        }
        else
        {
            lastClickTime = Time.unscaledTime;
        }
    }
}