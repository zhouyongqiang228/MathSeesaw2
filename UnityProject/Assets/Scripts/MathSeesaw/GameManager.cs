using UnityEngine;
using UnityEngine.SceneManagement;

namespace MathSeesaw
{
    /// <summary>
    /// 游戏管理器 - 协调所有游戏系统
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] GameObject m_progressManagerPrefab;
        [SerializeField] GameObject m_audioManagerPrefab;
        [SerializeField] GameObject m_inputManagerPrefab;
        [SerializeField] GameObject m_hapticManagerPrefab;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeManagers();
        }

        void InitializeManagers()
        {
            // 初始化进度管理器
            if (GameProgressManager.Instance == null && m_progressManagerPrefab != null)
            {
                Instantiate(m_progressManagerPrefab);
            }
            else if (GameProgressManager.Instance == null)
            {
                var go = new GameObject("GameProgressManager");
                go.AddComponent<GameProgressManager>();
                DontDestroyOnLoad(go);
            }

            // 初始化音频管理器
            if (AudioManager.Instance == null && m_audioManagerPrefab != null)
            {
                Instantiate(m_audioManagerPrefab);
            }
            else if (AudioManager.Instance == null)
            {
                var go = new GameObject("AudioManager");
                go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
            }

            // 初始化输入管理器
            if (MobileInputManager.Instance == null && m_inputManagerPrefab != null)
            {
                Instantiate(m_inputManagerPrefab);
            }
            else if (MobileInputManager.Instance == null)
            {
                var go = new GameObject("MobileInputManager");
                go.AddComponent<MobileInputManager>();
                DontDestroyOnLoad(go);
            }

            // 初始化触觉反馈管理器
            if (HapticManager.Instance == null && m_hapticManagerPrefab != null)
            {
                Instantiate(m_hapticManagerPrefab);
            }
            else if (HapticManager.Instance == null)
            {
                var go = new GameObject("HapticManager");
                go.AddComponent<HapticManager>();
                DontDestroyOnLoad(go);
            }
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void LoadGame()
        {
            SceneManager.LoadScene("Game");
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadNextLevel()
        {
            if (GameProgressManager.Instance != null)
            {
                int nextLevel = GameProgressManager.Instance.CurrentLevel + 1;
                GameProgressManager.Instance.SetCurrentLevel(nextLevel);
                LoadGame();
            }
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
