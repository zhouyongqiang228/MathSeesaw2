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

        public void Build(int curLevel)
        {
            m_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

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

            BuildProgressBar(canvasGo.transform, curLevel);
            BuildReplayButton(canvasGo.transform);
            BuildWinPanel(canvasGo.transform);
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
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.62f);
            trt.sizeDelta = new Vector2(1000f, 160f);
            title.fontStyle = FontStyle.Bold;

            var btnImg = CreateImage(overlay.transform, "BtnNext", HighlightColor);
            var brt = btnImg.rectTransform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.42f);
            brt.sizeDelta = new Vector2(440f, 130f);
            var btn = btnImg.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(Replay);
            var bt = CreateText(btnImg.transform, "PLAY AGAIN", 54, Color.white);
            Stretch(bt.rectTransform);
            bt.fontStyle = FontStyle.Bold;

            m_winPanel = overlay.gameObject;
            m_winPanel.SetActive(false);
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
