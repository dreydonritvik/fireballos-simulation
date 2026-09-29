using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireballOS
{
    public class TaskbarItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button button;

        private Window linkedWindow;

        public void Setup(Window window, string title, Sprite icon = null)
        {
            linkedWindow = window;

            if (titleText != null)
                titleText.text = title;

            if (iconImage != null)
            {
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnItemClicked);
            }
        }

        private void OnItemClicked()
        {
            if (linkedWindow == null) return;

            // 1. Durum: Pencere küçültülmüşse (kapalıysa) geri aç ve öne getir
            if (!linkedWindow.gameObject.activeSelf)
            {
                linkedWindow.gameObject.SetActive(true);
                linkedWindow.BringToFront();
            }
            // 2. Durum: Pencere zaten açıksa ama arkada kalmışsa öne getir
            else if (!linkedWindow.IsFrontmost())
            {
                linkedWindow.BringToFront();
            }
            // 3. Durum: Pencere zaten en öndeyse tekrar basıldığında simge durumuna küçült
            else
            {
                linkedWindow.Minimize();
            }
        }
    }
}