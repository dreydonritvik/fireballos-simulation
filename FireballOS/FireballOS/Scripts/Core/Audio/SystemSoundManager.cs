using UnityEngine;

namespace FireballOS
{
    /// <summary>
    /// Manages volume and playback for FireballOS system/UI sounds.
    /// Strictly isolated from browser/website audio (SiteSound.cs) and global AudioListener.
    /// </summary>
    public class SystemSoundManager : MonoBehaviour
    {
        private static SystemSoundManager instance;
        public static SystemSoundManager Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    instance = Object.FindAnyObjectByType<SystemSoundManager>();
#else
                    instance = Object.FindObjectOfType<SystemSoundManager>();
#endif
                }
                return instance;
            }
            private set => instance = value;
        }

        private const string PREFS_VOLUME = "FireballOS_SystemVolume";
        private const string PREFS_MUTE = "FireballOS_SystemMute";

        [Header("Audio Source")]
        [SerializeField] private AudioSource audioSource;

        [Header("System Audio Clips (Optional Defaults)")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip notificationSound;

        [Header("Current State")]
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;
        [SerializeField] private bool isMuted = false;

        public float Volume => volume;
        public bool IsMuted => isMuted;
        public float EffectiveVolume => isMuted ? 0f : volume;

        public event System.Action<float, bool> OnVolumeChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound

            LoadPreferences();
            UpdateAudioSource();
        }

        private void LoadPreferences()
        {
            volume = PlayerPrefs.GetFloat(PREFS_VOLUME, 1f);
            isMuted = PlayerPrefs.GetInt(PREFS_MUTE, 0) == 1;
        }

        private void SavePreferences()
        {
            PlayerPrefs.SetFloat(PREFS_VOLUME, volume);
            PlayerPrefs.SetInt(PREFS_MUTE, isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            SavePreferences();
            UpdateAudioSource();
            OnVolumeChanged?.Invoke(volume, isMuted);
        }

        public void SetMute(bool mute)
        {
            isMuted = mute;
            SavePreferences();
            UpdateAudioSource();
            OnVolumeChanged?.Invoke(volume, isMuted);
        }

        public void ToggleMute()
        {
            SetMute(!isMuted);
        }

        private void UpdateAudioSource()
        {
            if (audioSource != null)
            {
                audioSource.volume = EffectiveVolume;
                audioSource.mute = isMuted;
            }
        }

        /// <summary>
        /// Plays a system audio clip at the current system volume level.
        /// </summary>
        public void PlaySound(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null || EffectiveVolume <= 0f) return;

            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale) * EffectiveVolume);
            }
        }

        public void PlayClick() => PlaySound(clickSound);
        public void PlayError() => PlaySound(errorSound);
        public void PlayNotification() => PlaySound(notificationSound);

        /// <summary>
        /// Helper to adjust external UI AudioSources to match system volume.
        /// </summary>
        public void ApplyToSource(AudioSource targetSource, float baseVolume = 1f)
        {
            if (targetSource != null)
            {
                targetSource.volume = EffectiveVolume * Mathf.Clamp01(baseVolume);
                targetSource.mute = isMuted;
            }
        }
    }
}
