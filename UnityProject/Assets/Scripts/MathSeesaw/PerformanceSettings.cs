using UnityEngine;

namespace MathSeesaw
{
    public class PerformanceSettings : MonoBehaviour
    {
        const int HighRefreshRate = 120;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            if (FindObjectOfType<PerformanceSettings>() != null)
                return;

            var settings = new GameObject("Performance Settings");
            settings.AddComponent<PerformanceSettings>();
            DontDestroyOnLoad(settings);
        }

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = GetTargetFrameRate();
        }

        static int GetTargetFrameRate()
        {
            float refreshRate = (float)Screen.currentResolution.refreshRateRatio.value;
            if (refreshRate <= 0f)
                refreshRate = Screen.currentResolution.refreshRate;

            if (refreshRate >= HighRefreshRate - 1f)
                return HighRefreshRate;

            return Mathf.Max(60, Mathf.RoundToInt(refreshRate));
        }
    }
}
