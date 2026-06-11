using UnityEngine;
using System.Collections.Generic;

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

        [Header("Music Clips")]
        [SerializeField] AudioClip m_menuMusic;
        [SerializeField] AudioClip m_gameMusic;

        [Header("Sound Clips")]
        [SerializeField] AudioClip m_buttonClickSound;
        [SerializeField] AudioClip m_pickupSound;
        [SerializeField] AudioClip m_placeSound;
        [SerializeField] AudioClip m_balanceSound;
        [SerializeField] AudioClip m_completeSound;
        [SerializeField] AudioClip m_victorySound;
        [SerializeField] AudioClip m_unlockSound;

        Dictionary<SoundType, AudioClip> m_soundClips;

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

            InitializeSoundDictionary();
            UpdateVolumes();
        }

        void Start()
        {
            if (GameProgressManager.Instance != null && GameProgressManager.Instance.MusicEnabled)
            {
                PlayMenuMusic();
            }
        }

        void InitializeSoundDictionary()
        {
            m_soundClips = new Dictionary<SoundType, AudioClip>
            {
                { SoundType.ButtonClick, m_buttonClickSound },
                { SoundType.PickupMan, m_pickupSound },
                { SoundType.PlaceMan, m_placeSound },
                { SoundType.SeesawBalance, m_balanceSound },
                { SoundType.LevelComplete, m_completeSound },
                { SoundType.Victory, m_victorySound },
                { SoundType.Unlock, m_unlockSound }
            };
        }

        public void PlayMenuMusic()
        {
            if (m_menuMusic != null && m_musicSource.clip != m_menuMusic)
            {
                m_musicSource.clip = m_menuMusic;
                m_musicSource.Play();
            }
        }

        public void PlayGameMusic()
        {
            if (m_gameMusic != null && m_musicSource.clip != m_gameMusic)
            {
                m_musicSource.clip = m_gameMusic;
                m_musicSource.Play();
            }
        }

        public void PlaySound(SoundType soundType)
        {
            if (GameProgressManager.Instance != null && !GameProgressManager.Instance.SoundEnabled)
                return;

            if (m_soundClips.TryGetValue(soundType, out AudioClip clip) && clip != null)
            {
                m_soundSource.PlayOneShot(clip);
            }
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
