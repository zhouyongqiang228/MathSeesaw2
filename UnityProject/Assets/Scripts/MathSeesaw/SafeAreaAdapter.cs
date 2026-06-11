using UnityEngine;

namespace MathSeesaw
{
    /// <summary>
    /// 安全区域适配器 - 处理刘海屏、挖孔屏等异形屏幕
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdapter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] bool m_adaptTop = true;
        [SerializeField] bool m_adaptBottom = true;
        [SerializeField] bool m_adaptLeft = true;
        [SerializeField] bool m_adaptRight = true;

        RectTransform m_rectTransform;
        Rect m_lastSafeArea;
        Vector2Int m_lastScreenSize;

        void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
        }

        void Start()
        {
            ApplySafeArea();
        }

        void Update()
        {
            // 检测屏幕变化
            if (m_lastSafeArea != Screen.safeArea ||
                m_lastScreenSize.x != Screen.width ||
                m_lastScreenSize.y != Screen.height)
            {
                ApplySafeArea();
            }
        }

        void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            m_lastSafeArea = safeArea;
            m_lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // 归一化到 0-1 范围
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // 根据设置应用安全区域
            if (!m_adaptLeft) anchorMin.x = 0f;
            if (!m_adaptBottom) anchorMin.y = 0f;
            if (!m_adaptRight) anchorMax.x = 1f;
            if (!m_adaptTop) anchorMax.y = 1f;

            m_rectTransform.anchorMin = anchorMin;
            m_rectTransform.anchorMax = anchorMax;
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Safe Area")]
        void ApplySafeAreaEditor()
        {
            ApplySafeArea();
        }
#endif
    }
}
