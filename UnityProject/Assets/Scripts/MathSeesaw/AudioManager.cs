using UnityEngine;

namespace MathSeesaw
{
    /// <summary>
    /// 音效类型
    /// </summary>
    public enum SoundType
    {
        ButtonClick,
        PickupMan,
        PlaceMan,
        SeesawBalance,
        LevelComplete,
        Victory,
        Unlock
    }

    /// <summary>
    /// 音频管理器
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] AudioSource m_musicSource;
        [SerializeField] AudioSource m_soundSource;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 创建 AudioSource 如果不存在
            if (m_musicSource == null)
            {
                m_musicSource = gameObject.AddComponent<AudioSource>();
                m_musicSource.loop = true;
                m_musicSource.playOnAwake = false;
            }

            if (m_soundSource == null)
            {
                m_soundSource = gameObject.AddComponent<AudioSource>();
                m_soundSource.loop = false;
                m_soundSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        void Start()
        {
            if (GameProgressManager.Instance != null && GameProgressManager.Instance.MusicEnabled)
            {
                PlayMenuMusic();
            }
        }

        public void PlayMenuMusic()
        {
            var menuMusic = SeesawResourcesManager.Instance != null ? SeesawResourcesManager.Instance.MenuMusic : null;
            if (menuMusic != null && m_musicSource.clip != menuMusic)
            {
                m_musicSource.clip = menuMusic;
                m_musicSource.Play();
            }
        }

        public void PlayGameMusic()
        {
            var gameMusic = SeesawResourcesManager.Instance != null ? SeesawResourcesManager.Instance.GameMusic : null;
            if (gameMusic != null && m_musicSource.clip != gameMusic)
            {
                m_musicSource.clip = gameMusic;
                m_musicSource.Play();
            }
        }

        public void PlaySound(SoundType soundType)
        {
            if (GameProgressManager.Instance != null && !GameProgressManager.Instance.SoundEnabled)
                return;

            var clip = GetSoundClip(soundType);
            if (clip != null)
            {
                m_soundSource.PlayOneShot(clip);
            }
        }

        AudioClip GetSoundClip(SoundType soundType)
        {
            var resources = SeesawResourcesManager.Instance;
            if (resources == null)
                return null;

            return soundType switch
            {
                SoundType.ButtonClick => resources.ButtonClickSound,
                SoundType.PickupMan => resources.PickupSound,
                SoundType.PlaceMan => resources.PlaceSound,
                SoundType.SeesawBalance => resources.BalanceSound,
                SoundType.LevelComplete => resources.CompleteSound,
                SoundType.Victory => resources.VictorySound,
                SoundType.Unlock => resources.UnlockSound,
                _ => null
            };
        }

        public void PlaySound(AudioClip clip)
        {
            if (GameProgressManager.Instance != null && !GameProgressManager.Instance.SoundEnabled)
                return;

            if (clip != null)
            {
                m_soundSource.PlayOneShot(clip);
            }
        }

        public void StopMusic()
        {
            m_musicSource.Stop();
        }

        public void UpdateVolumes()
        {
            if (GameProgressManager.Instance != null)
            {
                m_musicSource.volume = GameProgressManager.Instance.MusicVolume;
                m_soundSource.volume = GameProgressManager.Instance.SoundVolume;
                m_musicSource.mute = !GameProgressManager.Instance.MusicEnabled;
            }
        }

        public void SetMusicVolume(float volume)
        {
            m_musicSource.volume = Mathf.Clamp01(volume);
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.SetMusicVolume(volume);
            }
        }

        public void SetSoundVolume(float volume)
        {
            m_soundSource.volume = Mathf.Clamp01(volume);
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.SetSoundVolume(volume);
            }
        }

        public void ToggleMusic(bool enabled)
        {
            m_musicSource.mute = !enabled;
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.SetMusicEnabled(enabled);
            }
        }

        public void ToggleSound(bool enabled)
        {
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.SetSoundEnabled(enabled);
            }
        }
    }
}
