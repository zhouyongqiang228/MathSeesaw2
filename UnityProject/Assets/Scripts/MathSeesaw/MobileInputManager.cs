using UnityEngine;
using UnityEngine.InputSystem;

namespace MathSeesaw
{
    /// <summary>
    /// 移动端优化的输入管理器，支持触摸和鼠标
    /// </summary>
    public class MobileInputManager : MonoBehaviour
    {
        public static MobileInputManager Instance { get; private set; }

        public bool IsTouching { get; private set; }
        public Vector2 TouchPosition { get; private set; }
        public bool TouchBegan { get; private set; }
        public bool TouchEnded { get; private set; }

        Touchscreen m_touchscreen;
        Mouse m_mouse;
        bool m_wasTouching;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            m_touchscreen = Touchscreen.current;
            m_mouse = Mouse.current;
        }

        void Update()
        {
            bool touching = false;
            Vector2 position = Vector2.zero;

            // 优先使用触摸输入
            if (m_touchscreen != null && m_touchscreen.primaryTouch.press.isPressed)
            {
                touching = true;
                position = m_touchscreen.primaryTouch.position.ReadValue();
            }
            // 回退到鼠标输入（编辑器测试用）
            else if (m_mouse != null && m_mouse.leftButton.isPressed)
            {
                touching = true;
                position = m_mouse.position.ReadValue();
            }

            TouchBegan = touching && !m_wasTouching;
            TouchEnded = !touching && m_wasTouching;
            IsTouching = touching;
            TouchPosition = position;

            m_wasTouching = touching;
        }

        /// <summary>
        /// 获取当前是否在 UI 上
        /// </summary>
        public bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
    }
}
