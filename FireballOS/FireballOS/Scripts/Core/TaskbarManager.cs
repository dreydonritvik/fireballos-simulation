using System.Collections.Generic;
using UnityEngine;

namespace FireballOS
{
    public class TaskbarManager : MonoBehaviour
    {
        public static TaskbarManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Transform runningAppsContainer; // Butonların dizileceği alan
        [SerializeField] private GameObject taskbarItemPrefab;     // Buton Prefab'ı

        private readonly Dictionary<Window, GameObject> activeButtons = new Dictionary<Window, GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterWindow(Window window, string title, Sprite icon = null)
        {
            if (activeButtons.ContainsKey(window)) return;

            if (taskbarItemPrefab != null && runningAppsContainer != null)
            {
                GameObject itemObj = Instantiate(taskbarItemPrefab, runningAppsContainer);
                TaskbarItem item = itemObj.GetComponent<TaskbarItem>();
                if (item != null)
                {
                    item.Setup(window, title, icon);
                }

                activeButtons.Add(window, itemObj);
            }
        }

        public void UnregisterWindow(Window window)
        {
            if (activeButtons.TryGetValue(window, out GameObject itemObj))
            {
                Destroy(itemObj);
                activeButtons.Remove(window);
            }
        }
    }
}