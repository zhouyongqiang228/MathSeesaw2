using UnityEngine;
using UnityEngine.InputSystem;

namespace MathSeesaw
{
    /// <summary>
    /// 输入调试工具 - 显示输入状态
    /// </summary>
    public class InputDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] bool m_showDebugInfo = true;
        [SerializeField] KeyCode m_toggleKey = KeyCode.F1;

        string m_debugText = "";

        void Update()
        {
            if (Input.GetKeyDown(m_toggleKey))
            {
                m_showDebugInfo = !m_showDebugInfo;
            }

            if (m_showDebugInfo)
            {
                UpdateDebugInfo();
            }
        }

        void UpdateDebugInfo()
        {
            m_debugText = "=== INPUT DEBUG ===\n\n";

            // 触摸输入
            if (Touchscreen.current != null)
            {
                m_debugText += "TOUCHSCREEN:\n";
                m_debugText += $"  Available: Yes\n";
                m_debugText += $"  Touch Count: {Touchscreen.current.touches.Count}\n";
                var primaryTouch = Touchscreen.current.primaryTouch;
                m_debugText += $"  Primary Pressed: {primaryTouch.press.isPressed}\n";
                if (primaryTouch.press.isPressed)
                {
                    var pos = primaryTouch.position.ReadValue();
                    m_debugText += $"  Position: ({pos.x:F1}, {pos.y:F1})\n";
                }
                m_debugText += $"  Just Pressed: {primaryTouch.press.wasPressedThisFrame}\n";
                m_debugText += $"  Just Released: {primaryTouch.press.wasReleasedThisFrame}\n";
            }
            else
            {
                m_debugText += "TOUCHSCREEN: Not Available\n";
            }

            m_debugText += "\n";

            // 鼠标输入
            if (Mouse.current != null)
            {
                m_debugText += "MOUSE:\n";
                m_debugText += $"  Available: Yes\n";
                m_debugText += $"  Left Button: {Mouse.current.leftButton.isPressed}\n";
                var pos = Mouse.current.position.ReadValue();
                m_debugText += $"  Position: ({pos.x:F1}, {pos.y:F1})\n";
                m_debugText += $"  Just Pressed: {Mouse.current.leftButton.wasPressedThisFrame}\n";
                m_debugText += $"  Just Released: {Mouse.current.leftButton.wasReleasedThisFrame}\n";
            }
            else
            {
                m_debugText += "MOUSE: Not Available\n";
            }

            m_debugText += "\n";

            // Pointer (通用)
            if (Pointer.current != null)
            {
                m_debugText += "POINTER:\n";
                m_debugText += $"  Type: {Pointer.current.GetType().Name}\n";
                m_debugText += $"  Pressed: {Pointer.current.press.isPressed}\n";
                var pos = Pointer.current.position.ReadValue();
                m_debugText += $"  Position: ({pos.x:F1}, {pos.y:F1})\n";
            }
            else
            {
                m_debugText += "POINTER: Not Available\n";
            }

            m_debugText += "\n";

            // LevelController 状态
            var level = FindObjectOfType<LevelController>();
            if (level != null)
            {
                m_debugText += "LEVEL CONTROLLER:\n";
                m_debugText += $"  Active: Yes\n";
                m_debugText += $"  Camera: {(level.cam != null ? "OK" : "NULL")}\n";
                m_debugText += $"  Seesaws: {level.seesaws.Count}\n";
                m_debugText += $"  PutMans: {level.putMans.Count}\n";
            }
            else
            {
                m_debugText += "LEVEL CONTROLLER: Not Found\n";
            }

            m_debugText += $"\nPress {m_toggleKey} to toggle debug info";
        }

        void OnGUI()
        {
            if (!m_showDebugInfo)
                return;

            // 背景框
            var bgRect = new Rect(10, 10, 400, 500);
            GUI.Box(bgRect, "");

            // 文字
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            style.padding = new RectOffset(10, 10, 10, 10);

            var textRect = new Rect(20, 20, 380, 480);
            GUI.Label(textRect, m_debugText, style);
        }
    }
}
