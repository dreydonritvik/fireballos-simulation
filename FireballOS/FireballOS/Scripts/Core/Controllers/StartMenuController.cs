using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FireballOS
{
    public class StartMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject startMenuPanel;
        [SerializeField] private GameObject backdrop;
        [SerializeField] private Button startButton;

        private bool isOpening = false;
        private int lastActionFrame = -1;

        public bool IsOpen => startMenuPanel != null ? startMenuPanel.activeSelf : gameObject.activeSelf;

        private void Awake()
        {
            if (startMenuPanel == null)
            {
                startMenuPanel = gameObject;
            }

            if (startButton != null)
            {
                // Buton tıklandıktan sonra odaklanıp koyu kalmasın
                Navigation nav = startButton.navigation;
                nav.mode = Navigation.Mode.None;
                startButton.navigation = nav;

                // Inspector'da persistent listener eklenmemişse dinamik olarak ekle
                bool hasPersistent = false;
                for (int i = 0; i < startButton.onClick.GetPersistentEventCount(); i++)
                {
                    if (startButton.onClick.GetPersistentMethodName(i) == nameof(ToggleMenu))
                    {
                        hasPersistent = true;
                        break;
                    }
                }

                startButton.onClick.RemoveListener(ToggleMenu);
                if (!hasPersistent)
                {
                    startButton.onClick.AddListener(ToggleMenu);
                }
            }

            // Eğer Awake, OpenMenu() tarafından tetiklenmediyse ve sahne başlangıcında açıksa menüyü kapat
            if (!isOpening)
            {
                CloseMenu();
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseMenu();
            }
#endif
        }

        public void OpenMenu()
        {
            lastActionFrame = Time.frameCount;
            isOpening = true;
            try
            {
                if (startMenuPanel != null) startMenuPanel.SetActive(true);
                else gameObject.SetActive(true);

                if (backdrop != null) backdrop.SetActive(true);
            }
            finally
            {
                isOpening = false;
            }
        }

        public void CloseMenu()
        {
            lastActionFrame = Time.frameCount;
            if (startMenuPanel != null) startMenuPanel.SetActive(false);
            else gameObject.SetActive(false);

            if (backdrop != null) backdrop.SetActive(false);
        }

        public void ToggleMenu()
        {
            if (Time.frameCount == lastActionFrame) return;

            if (IsOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        public bool IsStartButton(GameObject go)
        {
            if (startButton == null || go == null) return false;
            return go == startButton.gameObject || go.transform.IsChildOf(startButton.transform);
        }

        // Aliases for compatibility
        public void Toggle() => ToggleMenu();
        public void Open() => OpenMenu();
        public void Close() => CloseMenu();
    }
}