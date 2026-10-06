using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FireballOS
{
    public class Window : MonoBehaviour, IPointerDownHandler
    {
        [Header("Window Info")]
        [SerializeField] private string windowTitle = "Window";
        [SerializeField] private Sprite windowIcon;

        [Header("Resize & Maximize Settings")]
        [SerializeField] private RectTransform rectTransform;

        [Tooltip("Open() büyütülmüş açar. Görev çubuğundan geri getirmek kullanıcının boyutunu korur.")]
        [SerializeField] private bool openMaximized;

        [Header("Maximize Button UI")]
        [SerializeField] private TextMeshProUGUI maximizeButtonText;
        [SerializeField] private string maximizeIcon = "\uD83D\uDDD6"; // 🗖 (Segoe UI Symbol Maximize)
        [SerializeField] private string restoreIcon = "\uD83D\uDDD7";  // 🗗 (Segoe UI Symbol Restore)
        [SerializeField] private TMP_FontAsset symbolFont;

        public string MaximizeIcon
        {
            get => string.IsNullOrEmpty(maximizeIcon) ? "\uD83D\uDDD6" : maximizeIcon;
            set { maximizeIcon = value; UpdateMaximizeButtonIcon(); }
        }

        public string RestoreIcon
        {
            get => string.IsNullOrEmpty(restoreIcon) ? "\uD83D\uDDD7" : restoreIcon;
            set { restoreIcon = value; UpdateMaximizeButtonIcon(); }
        }

        private bool isMaximized = false;
        public bool IsMaximized => isMaximized; // Dışarıdan kontrol için

        public string WindowTitle => windowTitle;
        public Sprite WindowIcon => windowIcon;

        private Vector2 normalSize;
        private Vector2 normalPosition;
        private Vector2 normalAnchorMin;
        private Vector2 normalAnchorMax;
        private TaskbarManager taskbarManager;

        private void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            GetTaskbar();
            SaveNormalState();
            UpdateMaximizeButtonIcon();
        }

        private TaskbarManager GetTaskbar()
        {
            if (taskbarManager == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    taskbarManager = canvas.GetComponentInChildren<TaskbarManager>(true);
                }
                if (taskbarManager == null)
                {
                    taskbarManager = TaskbarManager.Instance;
                }
            }
            return taskbarManager;
        }

        private void Start()
        {
            // Sahne başladığında pencere açıksa görev çubuğuna ekle
            var tb = GetTaskbar();
            if (gameObject.activeSelf && tb != null)
            {
                tb.RegisterWindow(this, GetTitle(), windowIcon);
            }
        }

        private void OnDestroy()
        {
            var tb = GetTaskbar();
            if (tb != null)
            {
                tb.UnregisterWindow(this);
            }
        }

        private void SaveNormalState()
        {
            normalSize = rectTransform.sizeDelta;
            normalPosition = rectTransform.anchoredPosition;
            normalAnchorMin = rectTransform.anchorMin;
            normalAnchorMax = rectTransform.anchorMax;
        }

        public string GetTitle()
        {
            return string.IsNullOrEmpty(windowTitle) ? gameObject.name : windowTitle;
        }

        public void Open()
        {
            gameObject.SetActive(true);
            BringToFront();
            if (openMaximized && !isMaximized) ToggleMaximize();

            // Görev çubuğuna butonu kaydet / ekle
            var tb = GetTaskbar();
            if (tb != null)
            {
                tb.RegisterWindow(this, GetTitle(), windowIcon);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);

            // Pencere tamamen kapandığı için butonunu Taskbar'dan sil
            var tb = GetTaskbar();
            if (tb != null)
            {
                tb.UnregisterWindow(this);
            }
        }

        public void Minimize()
        {
            gameObject.SetActive(false);
        }

        public void ToggleMaximize()
        {
            if (!isMaximized)
            {
                SaveNormalState();

                rectTransform.sizeDelta = new Vector2(1920f, 1020f);
                rectTransform.anchoredPosition = new Vector2(0f, 30f);

                isMaximized = true;
            }
            else
            {
                rectTransform.sizeDelta = normalSize;
                rectTransform.anchoredPosition = normalPosition;

                isMaximized = false;
            }

            UpdateMaximizeButtonIcon();
            BringToFront();
        }

        private bool IsValidFontAsset(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null) return false;
            try
            {
                if (fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0) return false;
                if (fontAsset.atlasTextures[0] == null) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateMaximizeButtonIcon()
        {
            if (maximizeButtonText != null)
            {
                if (symbolFont == null)
                {
                    var loaded = Resources.Load<TMP_FontAsset>("Fonts & Materials/SegoeUISymbol SDF");
                    if (IsValidFontAsset(loaded))
                    {
                        symbolFont = loaded;
                    }
                }

                if (IsValidFontAsset(symbolFont))
                {
                    if (maximizeButtonText.font != symbolFont)
                    {
                        maximizeButtonText.font = symbolFont;
                    }
                    maximizeButtonText.text = isMaximized ? RestoreIcon : MaximizeIcon;
                }
                else
                {
                    // Fallback to standard square characters supported by default font
                    maximizeButtonText.text = isMaximized ? "\u25A0" : "\u25A1";
                }

                maximizeButtonText.ForceMeshUpdate();
            }
        }

        public void Toggle()
        {
            if (gameObject.activeSelf)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }

        public void BringToFront()
        {
            transform.SetAsLastSibling();
        }

        public bool IsFrontmost()
        {
            if (transform.parent == null) return true;

            // Sadece hiyerarşideki aktif olan kardeş pencereler arasında en üstte mi kontrol et
            int myIndex = transform.GetSiblingIndex();
            for (int i = myIndex + 1; i < transform.parent.childCount; i++)
            {
                Transform sibling = transform.parent.GetChild(i);
                if (sibling.gameObject.activeSelf)
                {
                    return false;
                }
            }
            return true;
        }
    }
}