using UnityEngine;

namespace MathSeesaw
{
    /// <summary>
    /// 触觉反馈管理器
    /// </summary>
    public class HapticManager : MonoBehaviour
    {
        public static HapticManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 轻微震动（按钮点击、拾取小人）
        /// </summary>
        public void LightImpact()
        {
            if (!IsVibrationEnabled())
                return;

#if UNITY_IOS && !UNITY_EDITOR
            iOSHapticFeedback.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactLight);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// 中等震动（放置小人）
        /// </summary>
        public void MediumImpact()
        {
            if (!IsVibrationEnabled())
                return;

#if UNITY_IOS && !UNITY_EDITOR
            iOSHapticFeedback.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactMedium);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// 强烈震动（关卡完成）
        /// </summary>
        public void HeavyImpact()
        {
            if (!IsVibrationEnabled())
                return;

#if UNITY_IOS && !UNITY_EDITOR
            iOSHapticFeedback.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactHeavy);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// 成功反馈（天平平衡、胜利）
        /// </summary>
        public void Success()
        {
            if (!IsVibrationEnabled())
                return;

#if UNITY_IOS && !UNITY_EDITOR
            iOSHapticFeedback.Trigger(iOSHapticFeedback.iOSFeedbackType.Success);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        bool IsVibrationEnabled()
        {
            return GameProgressManager.Instance == null || GameProgressManager.Instance.VibrationEnabled;
        }
    }

#if UNITY_IOS && !UNITY_EDITOR
    public static class iOSHapticFeedback
    {
        public enum iOSFeedbackType
        {
            ImpactLight,
            ImpactMedium,
            ImpactHeavy,
            Success,
            Warning,
            Error,
            Selection
        }

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _TriggerImpactFeedback(int style);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _TriggerNotificationFeedback(int type);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _TriggerSelectionFeedback();

        public static void Trigger(iOSFeedbackType type)
        {
            switch (type)
            {
                case iOSFeedbackType.ImpactLight:
                    _TriggerImpactFeedback(0);
                    break;
                case iOSFeedbackType.ImpactMedium:
                    _TriggerImpactFeedback(1);
                    break;
                case iOSFeedbackType.ImpactHeavy:
                    _TriggerImpactFeedback(2);
                    break;
                case iOSFeedbackType.Success:
                    _TriggerNotificationFeedback(0);
                    break;
                case iOSFeedbackType.Warning:
                    _TriggerNotificationFeedback(1);
                    break;
                case iOSFeedbackType.Error:
                    _TriggerNotificationFeedback(2);
                    break;
                case iOSFeedbackType.Selection:
                    _TriggerSelectionFeedback();
                    break;
            }
        }
    }
#endif
}
