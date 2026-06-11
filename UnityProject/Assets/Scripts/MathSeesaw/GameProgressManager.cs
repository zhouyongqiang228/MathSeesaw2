using System;
using System.Collections.Generic;
using UnityEngine;

namespace MathSeesaw
{
    /// <summary>
    /// 游戏进度数据
    /// </summary>
    [Serializable]
    public class GameProgress
    {
        public int currentLevel = 1;
        public int maxUnlockedLevel = 1;
        public List<int> completedLevels = new List<int>();
        public Dictionary<int, int> levelBestMoves = new Dictionary<int, int>();
        public Dictionary<int, float> levelBestTime = new Dictionary<int, float>();

        // 设置
        public bool musicEnabled = true;
        public bool soundEnabled = true;
        public bool vibrationEnabled = true;
        public float musicVolume = 0.7f;
        public float soundVolume = 1.0f;
    }

    /// <summary>
    /// 游戏进度管理器 - 使用 PlayerPrefs 保存
    /// </summary>
    public class GameProgressManager : MonoBehaviour
    {
        public static GameProgressManager Instance { get; private set; }

        const string SaveKey = "MathSeesaw_Progress";
        GameProgress m_progress;

        public int CurrentLevel => m_progress.currentLevel;
        public int MaxUnlockedLevel => m_progress.maxUnlockedLevel;
        public bool MusicEnabled => m_progress.musicEnabled;
        public bool SoundEnabled => m_progress.soundEnabled;
        public bool VibrationEnabled => m_progress.vibrationEnabled;
        public float MusicVolume => m_progress.musicVolume;
        public float SoundVolume => m_progress.soundVolume;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgress();
        }

        void LoadProgress()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                m_progress = JsonUtility.FromJson<GameProgress>(json);
            }
            else
            {
                m_progress = new GameProgress();
                SaveProgress();
            }
        }

        void SaveProgress()
        {
            string json = JsonUtility.ToJson(m_progress);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void CompleteLevel(int level, int moves, float time)
        {
            if (!m_progress.completedLevels.Contains(level))
                m_progress.completedLevels.Add(level);

            // 更新最佳记录
            if (!m_progress.levelBestMoves.ContainsKey(level) || moves < m_progress.levelBestMoves[level])
                m_progress.levelBestMoves[level] = moves;

            if (!m_progress.levelBestTime.ContainsKey(level) || time < m_progress.levelBestTime[level])
                m_progress.levelBestTime[level] = time;

            // 解锁下一关
            if (level >= m_progress.maxUnlockedLevel)
                m_progress.maxUnlockedLevel = level + 1;

            SaveProgress();
        }

        public void SetCurrentLevel(int level)
        {
            m_progress.currentLevel = level;
            SaveProgress();
        }

        public bool IsLevelUnlocked(int level)
        {
            return level <= m_progress.maxUnlockedLevel;
        }

        public bool IsLevelCompleted(int level)
        {
            return m_progress.completedLevels.Contains(level);
        }

        public void SetMusicEnabled(bool enabled)
        {
            m_progress.musicEnabled = enabled;
            SaveProgress();
        }

        public void SetSoundEnabled(bool enabled)
        {
            m_progress.soundEnabled = enabled;
            SaveProgress();
        }

        public void SetVibrationEnabled(bool enabled)
        {
            m_progress.vibrationEnabled = enabled;
            SaveProgress();
        }

        public void SetMusicVolume(float volume)
        {
            m_progress.musicVolume = Mathf.Clamp01(volume);
            SaveProgress();
        }

        public void SetSoundVolume(float volume)
        {
            m_progress.soundVolume = Mathf.Clamp01(volume);
            SaveProgress();
        }

        public void ResetProgress()
        {
            m_progress = new GameProgress();
            SaveProgress();
        }

        public int GetLevelBestMoves(int level)
        {
            return m_progress.levelBestMoves.ContainsKey(level) ? m_progress.levelBestMoves[level] : -1;
        }

        public float GetLevelBestTime(int level)
        {
            return m_progress.levelBestTime.ContainsKey(level) ? m_progress.levelBestTime[level] : -1f;
        }
    }
}
