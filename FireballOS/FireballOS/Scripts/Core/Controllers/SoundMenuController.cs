using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FireballOS
{
    public class SoundMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject soundMenuPanel;
        [SerializeField] private GameObject backdrop;
        [SerializeField] private Button soundButton;

        [Header("Volume Controls")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TextMeshProUGUI volumeText;
        [SerializeField] private Button muteButton;
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private Image muteIcon;
        [SerializeField] private TextMeshProUGUI muteText;

        [Header("Slider Visuals (Line and Bar)")]
        [SerializeField] private Image sliderBackground;
        [SerializeField] private Image sliderFill;
        [SerializeField] private Image sliderHandle;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color mutedColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        [Header("Icons / Sprites (Optional)")]
        [SerializeField] private Sprite normalSoundSprite;
        [SerializeField] private Sprite mutedSoundSprite;

        private bool isOpening = false;
        private int lastActionFrame = -1;
        private bool isUpdatingUI = false;

        public bool IsOpen => soundMenuPanel != null ? soundMenuPanel.activeSelf : gameObject.activeSelf;

        private void Awake()
        {
            if (soundMenuPanel == null)
            {
                soundMenuPanel = gameObject;
            }

            if (soundButton != null)
            {
                Navigation nav = soundButton.navigation;
                nav.mode = Navigation.Mode.None;
                soundButton.navigation = nav;

                soundButton.onClick.RemoveListener(ToggleMenu);
                soundButton.onClick.AddListener(ToggleMenu);
            }

            if (volumeSlider != null)
            {
                volumeSlider.minValue = 0f;
                volumeSlider.maxValue = 1f;
                volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
                volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
                ResolveSliderVisuals();
            }

            if (muteButton != null)
            {
                muteButton.onClick.RemoveListener(ToggleMute);
                muteButton.onClick.AddListener(ToggleMute);
            }

            if (muteToggle != null)
            {
                muteToggle.onValueChanged.RemoveListener(OnMuteToggleChanged);
                muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
            }

            if (!isOpening)
            {
                CloseMenu();
            }
        }

        private void Start()
        {
            SyncWithSoundManager();
        }

        private void OnEnable()
        {
            if (SystemSoundManager.Instance != null)
            {
                SystemSoundManager.Instance.OnVolumeChanged += OnSystemVolumeChanged;
            }
            SyncWithSoundManager();
        }

        private void OnDisable()
        {
            if (SystemSoundManager.Instance != null)
            {
                SystemSoundManager.Instance.OnVolumeChanged -= OnSystemVolumeChanged;
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && IsOpen)
            {
                CloseMenu();
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape) && IsOpen)
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
#if UNITY_2023_1_OR_NEWER
                var startMenu = FindAnyObjectByType<StartMenuController>();
#else
                var startMenu = FindObjectOfType<StartMenuController>();
#endif
                if (startMenu != null && startMenu.IsOpen)
                {
                    startMenu.CloseMenu();
                }

                if (soundMenuPanel != null) soundMenuPanel.SetActive(true);
                else gameObject.SetActive(true);

                if (backdrop != null) backdrop.SetActive(true);

                SyncWithSoundManager();
            }
            finally
            {
                isOpening = false;
            }
        }

        public void CloseMenu()
        {
            lastActionFrame = Time.frameCount;
            if (soundMenuPanel != null) soundMenuPanel.SetActive(false);
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

        public void OnSliderValueChanged(float value)
        {
            if (isUpdatingUI) return;

            if (SystemSoundManager.Instance != null)
            {
                if (SystemSoundManager.Instance.IsMuted && value > 0f)
                {
                    SystemSoundManager.Instance.SetMute(false);
                }
                SystemSoundManager.Instance.SetVolume(value);
            }

            bool currentMute = SystemSoundManager.Instance != null ? SystemSoundManager.Instance.IsMuted : (muteToggle != null ? muteToggle.isOn : false);
            UpdateDisplay(value, currentMute);
        }

        public void ToggleMute()
        {
            bool currentMute = SystemSoundManager.Instance != null ? SystemSoundManager.Instance.IsMuted : (muteToggle != null && muteToggle.isOn);
            bool newMute = !currentMute;

            if (SystemSoundManager.Instance != null)
            {
                SystemSoundManager.Instance.SetMute(newMute);
            }
            if (muteToggle != null)
            {
                muteToggle.SetIsOnWithoutNotify(newMute);
            }

            float currentVol = SystemSoundManager.Instance != null ? SystemSoundManager.Instance.Volume : (volumeSlider != null ? volumeSlider.value : 1f);
            UpdateDisplay(currentVol, newMute);
        }

        public void OnMuteToggleChanged(bool isMuted)
        {
            if (isUpdatingUI) return;

            if (SystemSoundManager.Instance != null)
            {
                SystemSoundManager.Instance.SetMute(isMuted);
            }

            float currentVol = SystemSoundManager.Instance != null ? SystemSoundManager.Instance.Volume : (volumeSlider != null ? volumeSlider.value : 1f);
            UpdateDisplay(currentVol, isMuted);
        }

        private void OnSystemVolumeChanged(float vol, bool muted)
        {
            SyncWithSoundManager();
        }

        private void SyncWithSoundManager()
        {
            float currentVol = SystemSoundManager.Instance != null ? SystemSoundManager.Instance.Volume : (volumeSlider != null ? volumeSlider.value : 1f);
            bool currentMute = SystemSoundManager.Instance != null && SystemSoundManager.Instance.IsMuted;

            isUpdatingUI = true;
            try
            {
                if (volumeSlider != null)
                {
                    volumeSlider.value = currentVol;
                }
                UpdateDisplay(currentVol, currentMute);
            }
            finally
            {
                isUpdatingUI = false;
            }
        }

        private void UpdateDisplay(float vol, bool muted)
        {
            int percent = Mathf.RoundToInt(vol * 100f);
            if (volumeText != null)
            {
                volumeText.text = muted ? $"{percent}% (Muted)" : $"{percent}%";
            }

            if (muteToggle != null)
            {
                muteToggle.SetIsOnWithoutNotify(muted);
            }

            if (muteText != null)
            {
                muteText.text = muted ? "Unmute" : "Mute";
            }

            if (muteIcon != null)
            {
                if (muted && mutedSoundSprite != null)
                {
                    muteIcon.sprite = mutedSoundSprite;
                }
                else if (!muted && normalSoundSprite != null)
                {
                    muteIcon.sprite = normalSoundSprite;
                }
                muteIcon.color = muted ? new Color(1f, 0.4f, 0.4f, 1f) : Color.white;
            }

            // Update slider visuals (line and bar)
            ResolveSliderVisuals();
            Color targetColor = muted ? mutedColor : activeColor;

            if (sliderBackground != null) sliderBackground.color = targetColor;
            if (sliderFill != null) sliderFill.color = targetColor;
            if (sliderHandle != null) sliderHandle.color = targetColor;

            if (volumeSlider != null)
            {
                var colors = volumeSlider.colors;
                colors.normalColor = targetColor;
                colors.highlightedColor = muted ? new Color(0.65f, 0.65f, 0.65f, 1f) : new Color(0.95f, 0.95f, 0.95f, 1f);
                colors.pressedColor = muted ? new Color(0.45f, 0.45f, 0.45f, 1f) : new Color(0.78f, 0.78f, 0.78f, 1f);
                colors.selectedColor = targetColor;
                volumeSlider.colors = colors;
            }
        }

        private void ResolveSliderVisuals()
        {
            if (volumeSlider == null) return;

            if (sliderHandle == null && volumeSlider.handleRect != null)
            {
                sliderHandle = volumeSlider.handleRect.GetComponent<Image>();
            }

            if (sliderFill == null && volumeSlider.fillRect != null)
            {
                sliderFill = volumeSlider.fillRect.GetComponent<Image>();
            }

            if (sliderBackground == null)
            {
                var bg = volumeSlider.transform.Find("Background");
                if (bg != null) sliderBackground = bg.GetComponent<Image>();
            }
        }

        public bool IsSoundButton(GameObject go)
        {
            if (soundButton == null || go == null) return false;
            return go == soundButton.gameObject || go.transform.IsChildOf(soundButton.transform);
        }

        // Aliases for compatibility
        public void Toggle() => ToggleMenu();
        public void Open() => OpenMenu();
        public void Close() => CloseMenu();
    }
}
