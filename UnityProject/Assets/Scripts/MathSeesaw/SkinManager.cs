using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MathSeesaw
{
    public class SkinManager : MonoBehaviour
    {
        const string PrefKey = "ms2_skin";

        struct Skin
        {
            public string name;
            public string bgTexture;
            public Color sky;
            public Color stage;
            public Color stageBase;
            public Color seesaw;
            public Color pan;
            public Color light;
            public float lightIntensity;
            public Color ambient;
            public bool showClouds;
        }

        static readonly Skin[] Skins =
        {
            new Skin { name = "经典晴空", bgTexture = null,
                sky = new Color(0.4235f, 0.8392f, 0.9961f),
                stage = new Color(0.86f, 0.94f, 1f), stageBase = new Color(0.62f, 0.78f, 0.92f),
                seesaw = new Color(0.78f, 0.42f, 0.95f), pan = new Color(0.72f, 0.42f, 0.92f),
                light = Color.white, lightIntensity = 1.15f,
                ambient = new Color(0.55f, 0.6f, 0.66f), showClouds = true },
            new Skin { name = "极光冰原", bgTexture = "Backgrounds/bg_aurora",
                sky = new Color(0.18f, 0.16f, 0.42f),
                stage = new Color(0.85f, 0.92f, 1f), stageBase = new Color(0.55f, 0.68f, 0.95f),
                seesaw = new Color(0.55f, 0.38f, 0.98f), pan = new Color(0.45f, 0.5f, 0.98f),
                light = new Color(0.82f, 0.88f, 1f), lightIntensity = 1.05f,
                ambient = new Color(0.42f, 0.46f, 0.62f), showClouds = false },
            new Skin { name = "沙漠绿洲", bgTexture = "Backgrounds/bg_desert",
                sky = new Color(0.35f, 0.72f, 0.98f),
                stage = new Color(0.97f, 0.88f, 0.62f), stageBase = new Color(0.85f, 0.64f, 0.42f),
                seesaw = new Color(1f, 0.55f, 0.2f), pan = new Color(0.95f, 0.45f, 0.25f),
                light = new Color(1f, 0.96f, 0.86f), lightIntensity = 1.25f,
                ambient = new Color(0.62f, 0.58f, 0.5f), showClouds = false },
            new Skin { name = "森林河流", bgTexture = "Backgrounds/bg_forest",
                sky = new Color(0.3f, 0.7f, 0.95f),
                stage = new Color(0.62f, 0.85f, 0.5f), stageBase = new Color(0.45f, 0.42f, 0.38f),
                seesaw = new Color(1f, 0.5f, 0.3f), pan = new Color(0.95f, 0.42f, 0.32f),
                light = new Color(1f, 0.98f, 0.92f), lightIntensity = 1.2f,
                ambient = new Color(0.5f, 0.6f, 0.5f), showClouds = false },
            new Skin { name = "热带泻湖", bgTexture = "Backgrounds/bg_lagoon",
                sky = new Color(0.25f, 0.75f, 0.95f),
                stage = new Color(0.99f, 0.95f, 0.85f), stageBase = new Color(0.35f, 0.78f, 0.85f),
                seesaw = new Color(1f, 0.44f, 0.38f), pan = new Color(0.98f, 0.38f, 0.42f),
                light = new Color(1f, 0.98f, 0.94f), lightIntensity = 1.25f,
                ambient = new Color(0.58f, 0.64f, 0.62f), showClouds = false },
            new Skin { name = "冬日雪河", bgTexture = "Backgrounds/bg_winter",
                sky = new Color(0.45f, 0.8f, 0.98f),
                stage = new Color(0.96f, 0.98f, 1f), stageBase = new Color(0.72f, 0.85f, 0.98f),
                seesaw = new Color(0.3f, 0.5f, 0.97f), pan = new Color(0.25f, 0.45f, 0.92f),
                light = new Color(0.95f, 0.97f, 1f), lightIntensity = 1.2f,
                ambient = new Color(0.55f, 0.6f, 0.68f), showClouds = false },
        };

        Camera m_cam;
        Light m_light;
        Renderer m_stage;
        Renderer m_stageBase;
        Renderer m_fulcrum;
        Renderer m_base;
        Renderer[] m_trays;
        CloudDrift[] m_clouds;
        Renderer m_bgQuad;
        Text m_skinLabel;
        int m_index;
        float m_bgTextureAspect = 1f;
        float m_lastBgAspect = -1f;
        float m_lastBgOrthoSize = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (FindFirstObjectByType<GameBootstrap>() == null)
                return;
            if (FindFirstObjectByType<SkinManager>() != null)
                return;
            new GameObject("SkinManager").AddComponent<SkinManager>();
        }

        void Start()
        {
            m_cam = Camera.main;
            m_light = FindFirstObjectByType<Light>();
            m_stage = FindRenderer("Stage");
            m_stageBase = FindRenderer("StageBase");
            m_fulcrum = FindRenderer("Fulcrum");
            m_base = FindRenderer("Base");
            m_clouds = FindObjectsByType<CloudDrift>(FindObjectsSortMode.None);

            var trays = new System.Collections.Generic.List<Renderer>();
            foreach (var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                if (r.name.StartsWith("Tray"))
                    trays.Add(r);
            m_trays = trays.ToArray();

            BuildBackgroundQuad();
            BuildUI();

            m_index = Mathf.Clamp(PlayerPrefs.GetInt(PrefKey, 0), 0, Skins.Length - 1);
            Apply(m_index);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.bKey.wasPressedThisFrame)
                Next();

            UpdateBackgroundQuadLayout();
        }

        static Renderer FindRenderer(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<Renderer>() : null;
        }

        void BuildBackgroundQuad()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            foreach (var c in go.GetComponents<Collider>())
                Destroy(c);
            go.name = "SkyBackdrop";
            go.transform.SetParent(m_cam.transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, 45f);

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            m_bgQuad = go.GetComponent<Renderer>();
            m_bgQuad.material = new Material(shader);
            m_bgQuad.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            UpdateBackgroundQuadLayout(true);
        }

        void UpdateBackgroundQuadLayout(bool force = false)
        {
            if (m_cam == null || m_bgQuad == null)
                return;

            float aspect = Mathf.Max(m_cam.aspect, 0.01f);
            float orthographicSize = m_cam.orthographic ? m_cam.orthographicSize : -1f;
            if (!force &&
                Mathf.Approximately(aspect, m_lastBgAspect) &&
                Mathf.Approximately(orthographicSize, m_lastBgOrthoSize))
                return;

            m_lastBgAspect = aspect;
            m_lastBgOrthoSize = orthographicSize;

            float height;
            float width;
            if (m_cam.orthographic)
            {
                height = m_cam.orthographicSize * 2f;
                width = height * aspect;
            }
            else
            {
                const float dist = 45f;
                height = 2f * dist * Mathf.Tan(m_cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                width = height * aspect;
            }

            float viewAspect = width / Mathf.Max(height, 0.01f);
            float bgWidth = width;
            float bgHeight = height;
            if (m_bgTextureAspect > viewAspect)
                bgWidth = bgHeight * m_bgTextureAspect;
            else
                bgHeight = bgWidth / Mathf.Max(m_bgTextureAspect, 0.01f);

            const float bleed = 1.08f;
            m_bgQuad.transform.localPosition = new Vector3(0f, height * 0.08f, 45f);
            m_bgQuad.transform.localScale = new Vector3(bgWidth * bleed, bgHeight * bleed, 1f);
        }

        void BuildUI()
        {
            var canvasGo = new GameObject("SkinCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var btnGo = new GameObject("BtnSkin", typeof(Image), typeof(Button));
            btnGo.transform.SetParent(canvasGo.transform, false);
            var img = btnGo.GetComponent<Image>();
            img.color = new Color(0.2f, 0.7f, 0.95f);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(30f, -60f);
            rt.sizeDelta = new Vector2(230f, 110f);
            btnGo.GetComponent<Button>().onClick.AddListener(Next);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var textGo = new GameObject("Label", typeof(Text));
            textGo.transform.SetParent(btnGo.transform, false);
            m_skinLabel = textGo.GetComponent<Text>();
            m_skinLabel.font = font;
            m_skinLabel.fontSize = 40;
            m_skinLabel.fontStyle = FontStyle.Bold;
            m_skinLabel.color = Color.white;
            m_skinLabel.alignment = TextAnchor.MiddleCenter;
            m_skinLabel.raycastTarget = false;
            var lrt = m_skinLabel.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
        }

        public void Next()
        {
            m_index = (m_index + 1) % Skins.Length;
            PlayerPrefs.SetInt(PrefKey, m_index);
            Apply(m_index);
        }

        void Apply(int index)
        {
            var skin = Skins[index];

            m_cam.backgroundColor = skin.sky;
            if (string.IsNullOrEmpty(skin.bgTexture))
            {
                m_bgQuad.gameObject.SetActive(false);
            }
            else
            {
                var tex = Resources.Load<Texture2D>(skin.bgTexture);
                m_bgQuad.gameObject.SetActive(tex != null);
                if (tex != null)
                {
                    m_bgQuad.material.SetTexture("_BaseMap", tex);
                    m_bgTextureAspect = tex.width / (float)Mathf.Max(tex.height, 1);
                    UpdateBackgroundQuadLayout(true);
                }
            }

            if (m_light != null)
            {
                m_light.color = skin.light;
                m_light.intensity = skin.lightIntensity;
            }
            RenderSettings.ambientLight = skin.ambient;

            Tint(m_stage, skin.stage);
            Tint(m_stageBase, skin.stageBase);
            Tint(m_fulcrum, skin.seesaw);
            Tint(m_base, skin.seesaw);
            foreach (var t in m_trays)
                Tint(t, skin.pan);

            foreach (var c in m_clouds)
                if (c != null)
                    c.gameObject.SetActive(skin.showClouds);

            if (m_skinLabel != null)
                m_skinLabel.text = skin.name + " >";
        }

        static void Tint(Renderer r, Color c)
        {
            if (r == null)
                return;
            r.material.SetColor("_BaseColor", c);
            if (r.material.HasProperty("_BaseMap"))
                r.material.SetTexture("_BaseMap", null);
        }
    }
}
