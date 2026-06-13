using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MathSeesaw
{
    public class SkinManager : MonoBehaviour
    {
        const string PrefKey = "ms2_skin";

        [Header("Scene Background")]
        public Transform backgroundRoot;

        struct Skin
        {
            public string name;
            public Color sky;
            public Color stage;
            public Color stageBase;
            public Color seesaw;
            public Color seat;
            public Color pan;
            public Color light;
            public float lightIntensity;
            public Color ambient;
            public bool showClouds;
        }

        static readonly Skin[] Skins =
        {
            new Skin { name = "经典晴空",
                sky = new Color(0.4235f, 0.8392f, 0.9961f),
                stage = new Color(0.86f, 0.94f, 1f), stageBase = new Color(0.62f, 0.78f, 0.92f),
                seesaw = new Color(0.78f, 0.42f, 0.95f), seat = new Color(0.88f, 0.66f, 1f), pan = new Color(0.72f, 0.42f, 0.92f),
                light = Color.white, lightIntensity = 1.15f,
                ambient = new Color(0.55f, 0.6f, 0.66f), showClouds = true },
            new Skin { name = "极光冰原",
                sky = new Color(0.18f, 0.16f, 0.42f),
                stage = new Color(0.85f, 0.92f, 1f), stageBase = new Color(0.55f, 0.68f, 0.95f),
                seesaw = new Color(0.55f, 0.38f, 0.98f), seat = new Color(0.62f, 0.82f, 1f), pan = new Color(0.45f, 0.5f, 0.98f),
                light = new Color(0.82f, 0.88f, 1f), lightIntensity = 1.05f,
                ambient = new Color(0.42f, 0.46f, 0.62f), showClouds = true },
            new Skin { name = "沙漠绿洲",
                sky = new Color(0.35f, 0.72f, 0.98f),
                stage = new Color(0.97f, 0.88f, 0.62f), stageBase = new Color(0.85f, 0.64f, 0.42f),
                seesaw = new Color(1f, 0.55f, 0.2f), seat = new Color(1f, 0.78f, 0.36f), pan = new Color(0.95f, 0.45f, 0.25f),
                light = new Color(1f, 0.96f, 0.86f), lightIntensity = 1.25f,
                ambient = new Color(0.62f, 0.58f, 0.5f), showClouds = true },
            new Skin { name = "森林河流",
                sky = new Color(0.3f, 0.7f, 0.95f),
                stage = new Color(0.62f, 0.85f, 0.5f), stageBase = new Color(0.45f, 0.42f, 0.38f),
                seesaw = new Color(1f, 0.5f, 0.3f), seat = new Color(0.52f, 0.86f, 0.44f), pan = new Color(0.95f, 0.42f, 0.32f),
                light = new Color(1f, 0.98f, 0.92f), lightIntensity = 1.2f,
                ambient = new Color(0.5f, 0.6f, 0.5f), showClouds = true },
            new Skin { name = "热带泻湖",
                sky = new Color(0.25f, 0.75f, 0.95f),
                stage = new Color(0.99f, 0.95f, 0.85f), stageBase = new Color(0.35f, 0.78f, 0.85f),
                seesaw = new Color(1f, 0.44f, 0.38f), seat = new Color(0.38f, 0.9f, 0.86f), pan = new Color(0.98f, 0.38f, 0.42f),
                light = new Color(1f, 0.98f, 0.94f), lightIntensity = 1.25f,
                ambient = new Color(0.58f, 0.64f, 0.62f), showClouds = true },
            new Skin { name = "冬日雪河",
                sky = new Color(0.45f, 0.8f, 0.98f),
                stage = new Color(0.96f, 0.98f, 1f), stageBase = new Color(0.72f, 0.85f, 0.98f),
                seesaw = new Color(0.3f, 0.5f, 0.97f), seat = new Color(0.72f, 0.88f, 1f), pan = new Color(0.25f, 0.45f, 0.92f),
                light = new Color(0.95f, 0.97f, 1f), lightIntensity = 1.2f,
                ambient = new Color(0.55f, 0.6f, 0.68f), showClouds = true },
        };

        Camera m_cam;
        Light m_light;
        Renderer m_stage;
        Renderer m_stageBase;
        Renderer m_fulcrum;
        Renderer m_base;
        Renderer[] m_trays;
        Renderer[] m_seats;
        CloudDrift[] m_clouds;
        Text m_skinLabel;
        int m_index;

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
            RemoveBackgroundReferences();
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

            var seats = new System.Collections.Generic.List<Renderer>();
            foreach (var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                if (r.name.StartsWith("Seat") || r.name.StartsWith("GroundSeat"))
                    seats.Add(r);
            m_seats = seats.ToArray();

            BuildUI();

            m_index = Mathf.Clamp(PlayerPrefs.GetInt(PrefKey, 0), 0, Skins.Length - 1);
            Apply(m_index);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.bKey.wasPressedThisFrame)
                Next();
        }

        static Renderer FindRenderer(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<Renderer>() : null;
        }

        void RemoveBackgroundReferences()
        {
            if (backgroundRoot == null)
            {
                var root = GameObject.Find("Scene Background");
                if (root != null)
                    backgroundRoot = root.transform;
            }

            if (backgroundRoot != null)
            {
                backgroundRoot.gameObject.SetActive(false);
            }
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
            rt.anchoredPosition = new Vector2(330f, -60f);
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

            if (m_cam != null)
                m_cam.backgroundColor = skin.sky;

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
            foreach (var s in m_seats)
                Tint(s, skin.seat);

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
