using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MathSeesaw
{
    /// <summary>
    /// 主菜单 UI 管理器
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        Font m_font;
        GameObject m_mainPanel;
        GameObject m_levelSelectPanel;
        GameObject m_settingsPanel;

        void Start()
        {
            m_font = SeesawResourcesManager.GetFont();
            BuildMainMenu();
        }

        void BuildMainMenu()
        {
            // 创建 Canvas
            var canvasGo = new GameObject("Canvas_MainMenu", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            // 主面板
            m_mainPanel = BuildMainPanel(canvasGo.transform);
            m_levelSelectPanel = BuildLevelSelectPanel(canvasGo.transform);
            m_settingsPanel = BuildSettingsPanel(canvasGo.transform);

            ShowMainPanel();
        }

        GameObject BuildMainPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "MainPanel", new Color(0.4235f, 0.8392f, 0.9961f));

            // 标题
            var title = CreateText(panel.transform, "MATH SEESAW", 120, Color.white);
            var trt = title.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.7f);
            trt.sizeDelta = new Vector2(900f, 200f);
            title.fontStyle = FontStyle.Bold;

            float yPos = 0.45f;
            float spacing = 0.12f;

            // 开始游戏按钮
            CreateButton(panel.transform, "START GAME", new Vector2(0.5f, yPos), new Vector2(600f, 150f),
                new Color(0f, 0.9f, 1f), () => {
                    AudioManager.Instance?.PlaySound(SoundType.ButtonClick);
                    StartGame();
                });

            // 关卡选择按钮
            CreateButton(panel.transform, "LEVEL SELECT", new Vector2(0.5f, yPos - spacing), new Vector2(600f, 150f),
                new Color(0.55f, 0.4f, 0.95f), () => {
                    AudioManager.Instance?.PlaySound(SoundType.ButtonClick);
                    ShowLevelSelect();
                });

            // 设置按钮
            CreateButton(panel.transform, "SETTINGS", new Vector2(0.5f, yPos - spacing * 2), new Vector2(600f, 150f),
                new Color(0.72f, 0.42f, 0.92f), () => {
                    AudioManager.Instance?.PlaySound(SoundType.ButtonClick);
                    ShowSettings();
                });

            return panel;
        }

        GameObject BuildLevelSelectPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "LevelSelectPanel", new Color(0.4235f, 0.8392f, 0.9961f));

            // 标题
            var title = CreateText(panel.transform, "SELECT LEVEL", 90, Color.white);
            var trt = title.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.88f);
            trt.sizeDelta = new Vector2(900f, 150f);
            title.fontStyle = FontStyle.Bold;

            // 关卡网格（滚动视图）
            BuildLevelGrid(panel.transform);

            // 返回按钮
            CreateButton(panel.transform, "BACK", new Vector2(0.5f, 0.08f), new Vector2(400f, 120f),
                new Color(0.7f, 0.7f, 0.7f), () => {
                    AudioManager.Instance?.PlaySound(SoundType.ButtonClick);
                    ShowMainPanel();
                });

            return panel;
        }

        void BuildLevelGrid(Transform parent)
        {
            // 创建滚动视图容器
            var scrollRect = new GameObject("LevelScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollRect.transform.SetParent(parent, false);
            var scrollRt = scrollRect.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.1f, 0.15f);
            scrollRt.anchorMax = new Vector2(0.9f, 0.8f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
            scrollRect.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);

            var scroll = scrollRect.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            // 内容容器
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(scrollRect.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);

            scroll.content = contentRt;

            // 创建关卡按钮（假设20关）
            int totalLevels = 20;
            int columns = 4;
            int rows = Mathf.CeilToInt(totalLevels / (float)columns);
            float buttonSize = 150f;
            float spacing = 30f;

            contentRt.sizeDelta = new Vector2(0f, rows * (buttonSize + spacing) + spacing);

            for (int i = 0; i < totalLevels; i++)
            {
                int level = i + 1;
                int row = i / columns;
                int col = i % columns;

                bool unlocked = GameProgressManager.Instance == null || GameProgressManager.Instance.IsLevelUnlocked(level);
                bool completed = GameProgressManager.Instance != null && GameProgressManager.Instance.IsLevelCompleted(level);

                Color btnColor = completed ? new Color(0.3f, 0.9f, 0.4f) :
                                unlocked ? new Color(0f, 0.9f, 1f) :
                                new Color(0.5f, 0.5f, 0.5f);

                var btn = CreateLevelButton(content.transform, level.ToString(),
                    new Vector2(col * (buttonSize + spacing) + buttonSize / 2 + spacing,
                               -row * (buttonSize + spacing) - buttonSize / 2 - spacing),
                    new Vector2(buttonSize, buttonSize), btnColor, level, unlocked);
            }
        }

        GameObject CreateLevelButton(Transform parent, string label, Vector2 pos, Vector2 size, Color color, int level, bool unlocked)
        {
            var btnImg = CreateImage(parent, $"LevelBtn_{level}", color);
            var rt = btnImg.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            if (unlocked)
            {
                var btn = btnImg.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => {
                    AudioManager.Instance?.PlaySound(SoundType.ButtonClick);
                    LoadLevel(level);
                });
            }

            var t = CreateText(btnImg.transform, unlocked ? label : "🔒", 70, Color.white);
            Stretch(t.rectTransform);
            t.fontStyle = FontStyle.Bold;

            return btnImg.gameObject;
        }

        GameObject BuildSettingsPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "SettingsPanel", new Color(0.4235f, 0.8392f, 0.9961f));

            // 标题
            var title = CreateText(panel.transform, "SETTINGS", 90, Color.white);
            var trt = title.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.85f);
            trt.sizeDelta = new Vector2(900f, 150f);
            title.fontStyle = FontStyle.Bold;

            float yPos = 0.65f;
            float spacing = 0.12f;

            // 音乐开关
            CreateToggle(panel.transform, "Music", new Vector2(0.5f, yPos),
                GameProgressManager.Instance?.MusicEnabled ?? true, (value) => {
                    AudioManager.Instance?.ToggleMusic(value);
                });

            // 音效开关
            CreateToggle(panel.transform, "Sound", new Vector2(0.5f, yPos - spacing),
                GameProgressManager.Instance?.SoundEnabled ?? true, (value) => {
                    AudioManager.Instance?.ToggleSound(value);
                });

            // 震动开关
            CreateToggle(panel.transform, "Vibration", new Vector2(0.5f, yPos - spacing * 2),
                GameProgressManager.Instance?.VibrationEnabled ?? true, (value) => {
                    if (GameProgressManager.Instance != null)
                        GameProgressManager.Instance.SetVibrationEnabled(value);
                });

            // 返回按钮
            CreateButton(panel.transform, "BACK", new Vector2(0.5f, 0.15f), new Vector2(400f, 120f),
                new Color(0.7f, 0.7f, 0.7f), () => {
                    AudioManager.Instance?.PlaySound(SoundType.ButtonClick);
                    ShowMainPanel();
                });

            return panel;
        }

        void CreateToggle(Transform parent, string label, Vector2 anchorPos, bool defaultValue, UnityAction<bool> onValueChanged)
        {
            var container = new GameObject("Toggle_" + label, typeof(RectTransform));
            container.transform.SetParent(parent, false);
            var rt = container.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchorPos;
            rt.sizeDelta = new Vector2(600f, 100f);

            // 标签
            var labelText = CreateText(container.transform, label, 60, Color.white);
            var lrt = labelText.rectTransform;
            lrt.anchorMin = new Vector2(0f, 0.5f);
            lrt.anchorMax = new Vector2(0.5f, 0.5f);
            lrt.pivot = new Vector2(0f, 0.5f);
            lrt.anchoredPosition = Vector2.zero;
            lrt.sizeDelta = new Vector2(300f, 100f);
            labelText.alignment = TextAnchor.MiddleLeft;

            // 切换按钮
            var toggleBg = CreateImage(container.transform, "ToggleBg", new Color(0.7f, 0.7f, 0.7f));
            var tbgRt = toggleBg.rectTransform;
            tbgRt.anchorMin = new Vector2(1f, 0.5f);
            tbgRt.anchorMax = new Vector2(1f, 0.5f);
            tbgRt.pivot = new Vector2(1f, 0.5f);
            tbgRt.anchoredPosition = Vector2.zero;
            tbgRt.sizeDelta = new Vector2(160f, 80f);

            var toggle = toggleBg.gameObject.AddComponent<Toggle>();
            toggle.isOn = defaultValue;
            toggle.onValueChanged.AddListener(onValueChanged);

            var checkmark = CreateImage(toggleBg.transform, "Checkmark", new Color(0f, 0.9f, 1f));
            checkmark.rectTransform.sizeDelta = new Vector2(120f, 60f);
            toggle.graphic = checkmark.GetComponent<Image>();
        }

        void ShowMainPanel()
        {
            m_mainPanel.SetActive(true);
            m_levelSelectPanel.SetActive(false);
            m_settingsPanel.SetActive(false);
        }

        void ShowLevelSelect()
        {
            m_mainPanel.SetActive(false);
            m_levelSelectPanel.SetActive(true);
            m_settingsPanel.SetActive(false);
        }

        void ShowSettings()
        {
            m_mainPanel.SetActive(false);
            m_levelSelectPanel.SetActive(false);
            m_settingsPanel.SetActive(true);
        }

        void StartGame()
        {
            int currentLevel = GameProgressManager.Instance?.CurrentLevel ?? 1;
            LoadLevel(currentLevel);
        }

        void LoadLevel(int level)
        {
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.SetCurrentLevel(level);
            }
            SceneManager.LoadScene("Game");
        }

        GameObject CreatePanel(Transform parent, string name, Color bgColor)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Stretch(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = bgColor;
            return panel;
        }

        Image CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        Text CreateText(Transform parent, string content, int size, Color color)
        {
            var go = new GameObject("Text", typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = m_font;
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            return t;
        }

        void CreateButton(Transform parent, string label, Vector2 anchorPos, Vector2 size, Color color, System.Action onClick)
        {
            var btnImg = CreateImage(parent, "Btn_" + label, color);
            var rt = btnImg.rectTransform;
            rt.anchorMin = rt.anchorMax = anchorPos;
            rt.sizeDelta = size;

            var btn = btnImg.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick());

            var t = CreateText(btnImg.transform, label, 54, Color.white);
            Stretch(t.rectTransform);
            t.fontStyle = FontStyle.Bold;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
