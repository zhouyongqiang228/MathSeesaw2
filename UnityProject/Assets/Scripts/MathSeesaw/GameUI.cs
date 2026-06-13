using System;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MathSeesaw
{
    public class GameUI : MonoBehaviour
    {
        static readonly Color HighlightColor = new Color(0f, 0.9f, 1f);

        Font m_font;
        GameObject m_winPanel;
        Text m_fpsText;
        Action<SeesawMode> m_onModeChanged;
        float m_fpsTimer;
        int m_fpsFrames;

        public void Initialize(int curLevel, SeesawMode currentMode, Action<SeesawMode> onModeChanged)
        {
            m_font = SeesawResourcesManager.GetFont();
            m_onModeChanged = onModeChanged;

            var canvas = transform.Find("Canvas_Battle");
            if (canvas == null)
            {
                Build(curLevel, currentMode, onModeChanged);
                return;
            }

            BindSeesawModeToggle(canvas, currentMode);
            BindButton(canvas, "BtnReplay", Replay);
            BindButton(canvas, "BtnMenu", GoToMainMenu);
            BindWinPanel(canvas);
            BindOrBuildFps(canvas);
        }

        public void Build(int curLevel, SeesawMode currentMode, Action<SeesawMode> onModeChanged)
        {
            m_font = SeesawResourcesManager.GetFont();
            m_onModeChanged = onModeChanged;

            var canvasGo = new GameObject("Canvas_Battle", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
            es.transform.SetParent(transform, false);

            BuildSeesawModeToggle(canvasGo.transform, currentMode);
            BuildProgressBar(canvasGo.transform, curLevel);
            BuildReplayButton(canvasGo.transform);
            BuildMenuButton(canvasGo.transform);
            BuildFpsCounter(canvasGo.transform);
            BuildWinPanel(canvasGo.transform);
        }

        void Update()
        {
            if (m_fpsText == null)
                return;

            m_fpsFrames++;
            m_fpsTimer += Time.unscaledDeltaTime;
            if (m_fpsTimer < 0.35f)
                return;

            float fps = m_fpsFrames / Mathf.Max(m_fpsTimer, 0.0001f);
            m_fpsText.text = $"FPS {Mathf.RoundToInt(fps)}";
            m_fpsFrames = 0;
            m_fpsTimer = 0f;
        }

        void BindSeesawModeToggle(Transform canvas, SeesawMode currentMode)
        {
            var container = canvas.Find("SeesawModeToggle");
            if (container == null)
                return;

            var btn1Img = container.Find("Btn1")?.GetComponent<Image>();
            var btn2Img = container.Find("Btn2")?.GetComponent<Image>();
            var btn1 = btn1Img != null ? btn1Img.GetComponent<Button>() : null;
            var btn2 = btn2Img != null ? btn2Img.GetComponent<Button>() : null;
            if (btn1Img == null || btn2Img == null || btn1 == null || btn2 == null)
                return;

            btn1.onClick.RemoveAllListeners();
            btn2.onClick.RemoveAllListeners();
            btn1.onClick.AddListener(() => SwitchMode(SeesawMode.Single, btn1Img, btn1));
            btn2.onClick.AddListener(() => SwitchMode(SeesawMode.Double, btn2Img, btn2));
            SetModeVisual(container, currentMode);
        }

        void BindButton(Transform canvas, string name, UnityEngine.Events.UnityAction action)
        {
            var button = canvas.Find(name)?.GetComponent<Button>();
            if (button == null)
                return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        void BindWinPanel(Transform canvas)
        {
            m_winPanel = canvas.Find("WinPanel")?.gameObject;
            if (m_winPanel == null)
                return;

            BindButton(m_winPanel.transform, "BtnNext", NextLevel);
            BindButton(m_winPanel.transform, "BtnReplay", Replay);
            BindButton(m_winPanel.transform, "BtnMenu", GoToMainMenu);
            m_winPanel.SetActive(false);
        }

        void BindOrBuildFps(Transform canvas)
        {
            m_fpsText = canvas.Find("FpsCounter")?.GetComponent<Text>();
            if (m_fpsText == null)
                BuildFpsCounter(canvas);
        }

        void BuildFpsCounter(Transform parent)
        {
            var t = CreateText(parent, "FPS --", 30, new Color(0.12f, 0.16f, 0.2f));
            t.name = "FpsCounter";
            t.alignment = TextAnchor.MiddleRight;
            t.fontStyle = FontStyle.Bold;

            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-160f, -82f);
            rt.sizeDelta = new Vector2(180f, 48f);
            m_fpsText = t;
        }

        void BuildMenuButton(Transform parent)
        {
            var btnImg = CreateImage(parent, "BtnMenu", new Color(0.95f, 0.5f, 0.15f));
            var rt = btnImg.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(30f, -200f);
            rt.sizeDelta = new Vector2(110f, 110f);
            var btn = btnImg.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(GoToMainMenu);
            var t = CreateText(btnImg.transform, "M", 56, Color.white);
            Stretch(t.rectTransform);
            t.fontStyle = FontStyle.Bold;
        }

        void GoToMainMenu()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        void BuildSeesawModeToggle(Transform parent, SeesawMode currentMode)
        {
            var container = CreateImage(parent, "SeesawModeToggle", new Color(1f, 1f, 1f, 0.92f));
            var rt = container.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(30f, -60f);
            rt.sizeDelta = new Vector2(280f, 110f);

            // Button for 1 seesaw
            var btn1Img = CreateImage(container.transform, "Btn1", currentMode == SeesawMode.Single ? HighlightColor : new Color(0.7f, 0.7f, 0.7f));
            var btn1Rt = btn1Img.rectTransform;
            btn1Rt.anchorMin = new Vector2(0f, 0.5f);
            btn1Rt.anchorMax = new Vector2(0f, 0.5f);
            btn1Rt.pivot = new Vector2(0f, 0.5f);
            btn1Rt.anchoredPosition = new Vector2(10f, 0f);
            btn1Rt.sizeDelta = new Vector2(125f, 90f);
            var btn1 = btn1Img.gameObject.AddComponent<Button>();
            btn1.onClick.AddListener(() => SwitchMode(SeesawMode.Single, btn1Img, btn1));
            var t1 = CreateText(btn1Img.transform, "1", 52, currentMode == SeesawMode.Single ? Color.white : new Color(0.3f, 0.3f, 0.3f));
            Stretch(t1.rectTransform);
            t1.fontStyle = FontStyle.Bold;

            // Button for 2 seesaws
            var btn2Img = CreateImage(container.transform, "Btn2", currentMode == SeesawMode.Double ? HighlightColor : new Color(0.7f, 0.7f, 0.7f));
            var btn2Rt = btn2Img.rectTransform;
            btn2Rt.anchorMin = new Vector2(1f, 0.5f);
            btn2Rt.anchorMax = new Vector2(1f, 0.5f);
            btn2Rt.pivot = new Vector2(1f, 0.5f);
            btn2Rt.anchoredPosition = new Vector2(-10f, 0f);
            btn2Rt.sizeDelta = new Vector2(125f, 90f);
            var btn2 = btn2Img.gameObject.AddComponent<Button>();
            btn2.onClick.AddListener(() => SwitchMode(SeesawMode.Double, btn2Img, btn2));
            var t2 = CreateText(btn2Img.transform, "2", 52, currentMode == SeesawMode.Double ? Color.white : new Color(0.3f, 0.3f, 0.3f));
            Stretch(t2.rectTransform);
            t2.fontStyle = FontStyle.Bold;
        }

        void SwitchMode(SeesawMode mode, Image btnImg, Button btn)
        {
            m_onModeChanged?.Invoke(mode);

            SetModeVisual(btnImg.transform.parent, mode);
        }

        void SetModeVisual(Transform container, SeesawMode mode)
        {
            var btn1Img = container.Find("Btn1").GetComponent<Image>();
            var btn2Img = container.Find("Btn2").GetComponent<Image>();
            var btn1Text = btn1Img.transform.GetChild(0).GetComponent<Text>();
            var btn2Text = btn2Img.transform.GetChild(0).GetComponent<Text>();

            btn1Img.color = mode == SeesawMode.Single ? HighlightColor : new Color(0.7f, 0.7f, 0.7f);
            btn2Img.color = mode == SeesawMode.Double ? HighlightColor : new Color(0.7f, 0.7f, 0.7f);
            btn1Text.color = mode == SeesawMode.Single ? Color.white : new Color(0.3f, 0.3f, 0.3f);
            btn2Text.color = mode == SeesawMode.Double ? Color.white : new Color(0.3f, 0.3f, 0.3f);
        }

        void BuildProgressBar(Transform parent, int curLevel)
        {
            var bar = CreateImage(parent, "LevelProgress", new Color(1f, 1f, 1f, 0.92f));
            var rt = bar.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -60f);
            rt.sizeDelta = new Vector2(760f, 110f);

            int startLevel = (curLevel - 1) / 5 * 5 + 1;
            for (int i = 0; i < 5; i++)
            {
                int lvl = startLevel + i;
                bool isCur = lvl == curLevel;
                string label = i == 4 ? "Boss" : lvl.ToString();

                if (isCur)
                {
                    var pill = CreateImage(bar.transform, "CurPill", HighlightColor);
                    var prt = pill.rectTransform;
                    prt.anchorMin = prt.anchorMax = new Vector2((i + 0.5f) / 5f, 0.5f);
                    prt.sizeDelta = new Vector2(110f, 86f);
                }
                var t = CreateText(bar.transform, label, 44, isCur ? Color.white : new Color(0.45f, 0.45f, 0.5f));
                var trt = t.rectTransform;
                trt.anchorMin = trt.anchorMax = new Vector2((i + 0.5f) / 5f, 0.5f);
                trt.sizeDelta = new Vector2(140f, 90f);
                t.fontStyle = FontStyle.Bold;
            }
        }

        void BuildReplayButton(Transform parent)
        {
            var btnImg = CreateImage(parent, "BtnReplay", new Color(0.55f, 0.4f, 0.95f));
            var rt = btnImg.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-30f, -60f);
            rt.sizeDelta = new Vector2(110f, 110f);
            var btn = btnImg.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(Replay);
            var t = CreateText(btnImg.transform, "R", 56, Color.white);
            Stretch(t.rectTransform);
            t.fontStyle = FontStyle.Bold;
        }

        void BuildWinPanel(Transform parent)
        {
            var overlay = CreateImage(parent, "WinPanel", new Color(0f, 0f, 0f, 0.55f));
            Stretch(overlay.rectTransform);

            var title = CreateText(overlay.transform, "LEVEL COMPLETE!", 90, Color.white);
            var trt = title.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.7f);
            trt.sizeDelta = new Vector2(1000f, 160f);
            title.fontStyle = FontStyle.Bold;

            // 下一关按钮
            var btnNextImg = CreateImage(overlay.transform, "BtnNext", HighlightColor);
            var brt = btnNextImg.rectTransform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(440f, 130f);
            var btnNext = btnNextImg.gameObject.AddComponent<Button>();
            btnNext.onClick.AddListener(NextLevel);
            var bt = CreateText(btnNextImg.transform, "NEXT LEVEL", 54, Color.white);
            Stretch(bt.rectTransform);
            bt.fontStyle = FontStyle.Bold;

            // 重玩按钮
            var btnReplayImg = CreateImage(overlay.transform, "BtnReplay", new Color(0.55f, 0.4f, 0.95f));
            var brt2 = btnReplayImg.rectTransform;
            brt2.anchorMin = brt2.anchorMax = new Vector2(0.5f, 0.35f);
            brt2.sizeDelta = new Vector2(440f, 130f);
            var btnReplay = btnReplayImg.gameObject.AddComponent<Button>();
            btnReplay.onClick.AddListener(Replay);
            var bt2 = CreateText(btnReplayImg.transform, "REPLAY", 54, Color.white);
            Stretch(bt2.rectTransform);
            bt2.fontStyle = FontStyle.Bold;

            // 主菜单按钮
            var btnMenuImg = CreateImage(overlay.transform, "BtnMenu", new Color(0.7f, 0.7f, 0.7f));
            var brt3 = btnMenuImg.rectTransform;
            brt3.anchorMin = brt3.anchorMax = new Vector2(0.5f, 0.2f);
            brt3.sizeDelta = new Vector2(440f, 130f);
            var btnMenu = btnMenuImg.gameObject.AddComponent<Button>();
            btnMenu.onClick.AddListener(GoToMainMenu);
            var bt3 = CreateText(btnMenuImg.transform, "MAIN MENU", 54, Color.white);
            Stretch(bt3.rectTransform);
            bt3.fontStyle = FontStyle.Bold;

            m_winPanel = overlay.gameObject;
            m_winPanel.SetActive(false);
        }

        void NextLevel()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadNextLevel();
            }
            else
            {
                Replay();
            }
        }

        public void ShowWin() => m_winPanel.SetActive(true);

        void Replay() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

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

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
