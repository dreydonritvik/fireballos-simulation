using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace FireballOS
{
    public class LoginScreenController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button exitScreenButton;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private StartMenuController startMenuController;

        [Header("Configuration")]

        [Header("Events")]
        [SerializeField] private UnityEvent onLoginSuccess;
        [SerializeField] private UnityEvent onExitScreen;

        public bool IsActive => loginPanel != null ? loginPanel.activeSelf : gameObject.activeSelf;

        private void Awake()
        {
            if (loginPanel == null)
            {
                loginPanel = gameObject;
            }

            // Always ensure the login screen starts active and on top
            EnsureFirstOnScreen();

            if (loginButton != null)
            {
                loginButton.onClick.RemoveListener(Login);
                loginButton.onClick.AddListener(Login);
            }

            if (exitScreenButton != null)
            {
                exitScreenButton.onClick.RemoveListener(ExitScreen);
                exitScreenButton.onClick.AddListener(ExitScreen);
            }
        }

        private void Start()
        {
            EnsureFirstOnScreen();
        }

        private void OnEnable()
        {
            EnsureFirstOnScreen();
        }

        public void EnsureFirstOnScreen()
        {
            if (loginPanel != null)
            {
                loginPanel.SetActive(true);
            }
            transform.SetAsLastSibling();

            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                {
                    Login();
                }
                else if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    ExitScreen();
                }
            }
        }

        public void Login()
        {

            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }

            if (loginPanel != null)
            {
                loginPanel.SetActive(false);
            }

            onLoginSuccess?.Invoke();
        }

        public void LogOut()
        {
            if (startMenuController != null && startMenuController.IsOpen)
            {
                startMenuController.CloseMenu();
            }

            EnsureFirstOnScreen();
        }

        public void ExitScreen()
        {
            Debug.Log("[FireballOS] Exit screen requested.");

            EnsureFirstOnScreen();

            // The game listens to this event to take the player away from the computer
            onExitScreen?.Invoke();
        }
    }
}
