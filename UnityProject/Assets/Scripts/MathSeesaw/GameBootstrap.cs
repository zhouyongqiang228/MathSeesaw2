using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MathSeesaw
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Level Data (L_1)")]
        public int curLevel = 1;
        public int[] numbers = { 1, 4, 2, 3 };

        [Header("Seesaw Mode")]
        public SeesawMode seesawMode = SeesawMode.Single;

        [Header("Level Database")]
        public LevelDatabase levelDatabase;

        [Header("Scene References")]
        public Light directionalLight;
        public Transform environmentRoot;
        public Transform putMansRoot;
        public LevelController levelController;
        public GameUI gameUI;
        public GameObject putManTemplate;

        [Header("Runtime Scene Policy")]
        public bool allowRuntimeSceneFallback;

        [Header("Layout")]
        public float beamLength = 6.6f;
        public float panOffsetX = 2.5f;
        [Min(0)] public int defaultLeftSeatCount = 4;
        [Min(0)] public int defaultRightSeatCount = 4;
        public float manHeight = 1.16f;
        public Vector3 manPickPadding = new Vector3(0.36f, 0.12f, 0.36f);

        static readonly Color IceColor = new Color(0.86f, 0.94f, 1f);
        static readonly Color IceDarkColor = new Color(0.62f, 0.78f, 0.92f);
        static readonly Color PanColor = new Color(0.72f, 0.42f, 0.92f);
        static readonly Color SeatColor = new Color(0.88f, 0.66f, 1f);
        static readonly Color GrassColor = new Color(0.4f, 0.76f, 0.36f);
        static readonly Color RockColor = new Color(0.48f, 0.5f, 0.54f);

        static readonly Color[] ManColors =
        {
            new Color(0.95f, 0.28f, 0.28f),
            new Color(0.25f, 0.56f, 1f),
            new Color(0.32f, 0.83f, 0.35f),
            new Color(1f, 0.74f, 0.12f),
            new Color(0.72f, 0.42f, 0.95f),
            new Color(1f, 0.5f, 0.15f),
        };

        Material m_baseLitMat;
        Font m_font;

        void Awake()
        {
            CacheAssets();
            LoadLevelData();

            if (!BindSceneReferences())
            {
                if (allowRuntimeSceneFallback)
                {
                    BuildEditableSceneFallback();
                }
                else
                {
                    Debug.LogError("GameBootstrap is missing scene references. Runtime scene building is disabled, so the hand-authored Unity scene will not be modified.");
                    enabled = false;
                    return;
                }
            }

            ConfigureLevelController();
            ConfigurePutMans();
            ApplyRuntimeVisualAdjustments();
            ConfigureUI();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayGameMusic();
        }

        [ContextMenu("Build Editable Scene Fallback")]
        public void BuildEditableSceneFallback()
        {
            Debug.LogWarning("Building fallback scene objects from code. Use only as an editor/dev bootstrap, then save or prefab the result before shipping.");
            CacheAssets();
            directionalLight = BuildLight();
            environmentRoot = BuildEnvironment();

            levelController = GetComponent<LevelController>();
            if (levelController == null)
                levelController = gameObject.AddComponent<LevelController>();
            if (Camera.main != null)
                levelController.cam = Camera.main;
            levelController.seesawMode = seesawMode;
            levelController.seesaws.Clear();
            levelController.putMans.Clear();
            BuildSeesaws(levelController);
            BuildPutMans(levelController);

            gameUI = GetComponent<GameUI>();
            if (gameUI == null)
                gameUI = gameObject.AddComponent<GameUI>();
            gameUI.Build(curLevel, seesawMode, OnSeesawModeChanged);
            levelController.ui = gameUI;
        }

        void CacheAssets()
        {
            var resources = SeesawResourcesManager.Instance;
            if (resources == null)
            {
                Debug.LogError("SeesawResourcesManager is missing from the scene.");
                return;
            }

            if (m_baseLitMat == null)
                m_baseLitMat = resources.SeesawMaterial;
            if (m_font == null)
                m_font = resources.Font;

            if (m_baseLitMat == null)
                Debug.LogError("SeesawResourcesManager is missing the seesaw material reference.");
            if (m_font == null)
                Debug.LogError("SeesawResourcesManager is missing the font reference.");
        }

        void LoadLevelData()
        {
            if (GameProgressManager.Instance != null)
                curLevel = GameProgressManager.Instance.CurrentLevel;

            if (levelDatabase != null)
            {
                var levelData = levelDatabase.GetLevel(curLevel);
                if (levelData != null)
                {
                    numbers = levelData.numbers;
                    seesawMode = levelData.seesawMode;
                }
            }
        }

        bool BindSceneReferences()
        {
            if (directionalLight == null)
                directionalLight = FindObjectOfType<Light>();
            if (environmentRoot == null)
            {
                var environment = GameObject.Find("Environment");
                if (environment != null)
                    environmentRoot = environment.transform;
            }
            if (putMansRoot == null)
            {
                var root = GameObject.Find("putMans");
                if (root != null)
                    putMansRoot = root.transform;
            }
            if (levelController == null)
                levelController = GetComponent<LevelController>();
            if (gameUI == null)
                gameUI = GetComponent<GameUI>();

            return Camera.main != null && levelController != null && levelController.seesaws.Count > 0 && putMansRoot != null;
        }

        void ConfigureLevelController()
        {
            if (levelController == null)
                levelController = gameObject.AddComponent<LevelController>();
            if (Camera.main != null)
                levelController.cam = Camera.main;
            levelController.seesawMode = seesawMode;
            levelController.activeSeesawCount = seesawMode == SeesawMode.Double ? 2 : 1;

            if (levelController.seesaws.Count == 0)
                levelController.seesaws.AddRange(FindObjectsOfType<Blance>());
            levelController.seesaws.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            foreach (var seesaw in levelController.seesaws)
            {
                if (seesaw == null)
                    continue;
                if (seesaw.leftSeatCount <= 0)
                    seesaw.leftSeatCount = defaultLeftSeatCount;
                if (seesaw.rightSeatCount <= 0)
                    seesaw.rightSeatCount = defaultRightSeatCount;
                seesaw.ApplySeatCounts();
            }

            levelController.SwitchSeesawMode(seesawMode);
        }

        void ConfigurePutMans()
        {
            if (levelController == null)
                return;

            levelController.putMans.Clear();
            if (putMansRoot == null)
            {
                var root = GameObject.Find("putMans");
                if (root != null)
                    putMansRoot = root.transform;
            }

            var sceneMans = putMansRoot != null
                ? new List<PutMan>(putMansRoot.GetComponentsInChildren<PutMan>(true))
                : new List<PutMan>();
            sceneMans.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

            if (putManTemplate == null && sceneMans.Count > 0)
                putManTemplate = sceneMans[0].gameObject;

            while (sceneMans.Count < numbers.Length && putManTemplate != null)
            {
                var clone = Instantiate(putManTemplate, putMansRoot);
                clone.name = $"PutMan_{sceneMans.Count}";
                clone.SetActive(true);
                var man = clone.GetComponent<PutMan>();
                sceneMans.Add(man);
            }

            var nums = new List<int>(numbers);
            for (int i = 0; i < nums.Count; i++)
            {
                int j = Random.Range(i, nums.Count);
                (nums[i], nums[j]) = (nums[j], nums[i]);
            }

            for (int i = 0; i < sceneMans.Count; i++)
            {
                var man = sceneMans[i];
                bool active = i < nums.Count;
                man.gameObject.SetActive(active);
                if (!active)
                    continue;

                man.CurPlace = null;
                EnsureGroundSeat(man);
                ApplyManInteractionSize(man);
                man.Init(nums[i], false);
                man.SaveInitState();
                man.PlayIdle();
                if (man.avatarAnimator != null && man.avatarAnimator.Animator != null)
                    man.avatarAnimator.Animator.Play("idle", 0, Random.value);
                levelController.putMans.Add(man);
            }
        }

        void ConfigureUI()
        {
            if (gameUI == null)
                gameUI = GetComponent<GameUI>();
            if (gameUI == null)
                gameUI = gameObject.AddComponent<GameUI>();

            gameUI.Initialize(curLevel, seesawMode, OnSeesawModeChanged);
            if (levelController != null)
                levelController.ui = gameUI;
        }

        void OnSeesawModeChanged(SeesawMode mode)
        {
            seesawMode = mode;
            if (levelController != null)
                levelController.SwitchSeesawMode(mode);
        }

        Light BuildLight()
        {
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = Color.white;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.35f;
            go.transform.rotation = Quaternion.Euler(48f, -22f, 0f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.6f, 0.66f);
            return light;
        }

        Transform BuildEnvironment()
        {
            var root = new GameObject("Environment").transform;

            var stage = CreatePart(PrimitiveType.Cylinder, root, "Stage", IceColor,
                new Vector3(0f, -1.35f, 0.8f), new Vector3(11.5f, 0.75f, 16f));
            stage.GetComponent<Renderer>().material.SetFloat("_Smoothness", 0.25f);

            CreatePart(PrimitiveType.Cylinder, root, "StageBase", IceDarkColor,
                new Vector3(0f, -2.3f, 0.8f), new Vector3(12.8f, 0.65f, 17.92f));

            BuildSmallDecorations(root);

            string[] cloudNames = { "Prefabs/Cloud_0", "Prefabs/Cloud_1", "Prefabs/Cloud_2" };
            Vector3[] cloudPos = { new Vector3(-5.4f, 4.6f, 15f), new Vector3(5.4f, 5.2f, 17f), new Vector3(0f, 6.2f, 19.5f) };
            var cloudMat = MakeLit(Color.white);
            cloudMat.SetFloat("_Smoothness", 0f);
            for (int i = 0; i < 3; i++)
            {
                var prefab = Resources.Load<GameObject>(cloudNames[i]);
                if (prefab == null) continue;
                var cloud = Instantiate(prefab, root);
                NormalizeSize(cloud.transform, 2.1f + i * 0.3f, Axis.X);
                cloud.transform.position = cloudPos[i];
                cloud.AddComponent<CloudDrift>().speed = 0.12f + i * 0.05f;
                StripColliders(cloud);
                foreach (var r in cloud.GetComponentsInChildren<Renderer>())
                    r.sharedMaterial = cloudMat;
            }
            return root;
        }

        void BuildSmallDecorations(Transform root)
        {
            Vector3[] grassPositions =
            {
                new Vector3(-4.8f, -0.92f, -1.1f),
                new Vector3(4.9f, -0.92f, -0.8f),
                new Vector3(-5.2f, -0.9f, 4.1f),
                new Vector3(5.1f, -0.9f, 4.4f),
            };
            foreach (var pos in grassPositions)
            {
                var tuft = new GameObject("GrassTuft").transform;
                tuft.SetParent(root, false);
                tuft.localPosition = pos;
                CreatePart(PrimitiveType.Cylinder, tuft, "GrassStem", GrassColor,
                    new Vector3(-0.08f, 0.18f, 0f), new Vector3(0.055f, 0.36f, 0.055f));
                CreatePart(PrimitiveType.Cylinder, tuft, "GrassStem", GrassColor,
                    new Vector3(0.08f, 0.15f, 0.04f), new Vector3(0.05f, 0.3f, 0.05f));
                CreatePart(PrimitiveType.Cylinder, tuft, "GrassStem", GrassColor,
                    new Vector3(0f, 0.2f, -0.06f), new Vector3(0.05f, 0.4f, 0.05f));
            }

            Vector3[] rockPositions =
            {
                new Vector3(-4.1f, -0.95f, 1.5f),
                new Vector3(4.25f, -0.96f, 1.8f),
                new Vector3(-3.9f, -0.94f, 6.2f),
                new Vector3(4.0f, -0.95f, 6.4f),
            };
            for (int i = 0; i < rockPositions.Length; i++)
            {
                var rock = CreatePart(PrimitiveType.Sphere, root, "Rock", RockColor,
                    rockPositions[i], new Vector3(0.46f + i % 2 * 0.08f, 0.22f, 0.34f));
                rock.transform.localRotation = Quaternion.Euler(0f, i * 27f, 0f);
            }
        }

        void BuildSeesaws(LevelController level)
        {
            var seesaw1 = BuildSeesaw(new Vector3(0f, 0f, 1.2f), 0);
            var seesaw2 = BuildSeesaw(new Vector3(0f, 0f, 6.5f), 1);
            level.seesaws.Add(seesaw1);
            level.seesaws.Add(seesaw2);

            if (seesawMode == SeesawMode.Single)
            {
                seesaw1.transform.position = new Vector3(0f, -0.6f, 1.2f);
                seesaw2.gameObject.SetActive(false);
            }
            else
            {
                seesaw1.gameObject.SetActive(true);
                seesaw2.gameObject.SetActive(true);
            }
        }

        Blance BuildSeesaw(Vector3 offset, int index)
        {
            var root = new GameObject($"Seesaw_{index}").transform;
            root.position = new Vector3(0f, -0.6f, 1.2f) + offset;

            CreatePart(PrimitiveType.Cube, root, "Base", PanColor,
                new Vector3(0f, 0.15f, 0f), new Vector3(2f, 0.3f, 1.2f));

            BuildFulcrum(root);

            var up = new GameObject("up").transform;
            up.SetParent(root, false);
            up.localPosition = new Vector3(0f, 1.85f, 0f);

            float beamTop = BuildBeam(up);

            var blance = root.gameObject.AddComponent<Blance>();
            blance.upComponent = up;
            blance.leftSeatCount = defaultLeftSeatCount;
            blance.rightSeatCount = defaultRightSeatCount;
            blance.leftPan = BuildPan(up, true, new Vector3(-panOffsetX, beamTop, 0f));
            blance.rightPan = BuildPan(up, false, new Vector3(panOffsetX, beamTop, 0f));
            blance.ApplySeatCounts();

            return blance;
        }

        void BuildFulcrum(Transform root)
        {
            var go = new GameObject("Fulcrum", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            go.GetComponent<MeshFilter>().mesh = BuildPrismMesh(1.8f, 1.55f, 0.8f);
            go.GetComponent<MeshRenderer>().material = m_baseLitMat;
        }

        float BuildBeam(Transform up)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/BalanceBeam");
            if (prefab != null)
            {
                var beam = Instantiate(prefab, up);
                StripColliders(beam);
                var b = GetBounds(beam.transform);
                if (b.size.z > b.size.x)
                    beam.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                b = GetBounds(beam.transform);
                float k = beamLength / Mathf.Max(b.size.x, 0.001f);
                beam.transform.localScale *= k;
                b = GetBounds(beam.transform);
                beam.transform.position += up.position - b.center;
                b = GetBounds(beam.transform);
                return b.max.y - up.position.y;
            }
            CreatePart(PrimitiveType.Cube, up, "Beam", PanColor,
                Vector3.zero, new Vector3(beamLength, 0.25f, 0.7f));
            return 0.125f;
        }

        NumContainerPan BuildPan(Transform up, bool isLeft, Vector3 localPos)
        {
            var panGo = new GameObject(isLeft ? "LeftPan" : "RightPan");
            panGo.transform.SetParent(up, false);
            panGo.transform.localPosition = localPos;
            var pan = panGo.AddComponent<NumContainerPan>();

            int maxSeats = Mathf.Max(defaultLeftSeatCount, defaultRightSeatCount, 1);
            pan.seatSpacing = 0.6f;
            float startX = -(maxSeats - 1) * pan.seatSpacing * 0.5f;
            for (int i = 0; i < maxSeats; i++)
            {
                float x = startX + i * pan.seatSpacing;
                var seat = CreatePart(PrimitiveType.Cylinder, panGo.transform, "Seat", SeatColor,
                    new Vector3(x, 0.12f, 0f), new Vector3(0.5f, 0.055f, 0.5f));
                var stand = new GameObject("StandPoint").transform;
                stand.SetParent(panGo.transform, false);
                stand.localPosition = new Vector3(x, 0.17f, 0f);

                var place = panGo.AddComponent<ManPlace>();
                place.container = pan;
                place.isLeft = isLeft;
                place.standPoint = stand;
                place.seatRenderer = seat.GetComponent<Renderer>();
                pan.places.Add(place);
            }

            pan.textTotal = BuildScoreBubble(panGo.transform);
            return pan;
        }

        TextMesh BuildScoreBubble(Transform pan)
        {
            var holder = new GameObject("ScoreBubble");
            holder.transform.SetParent(pan, false);
            holder.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            holder.AddComponent<FaceCamera>();

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            StripColliders(quad);
            quad.name = "Bg";
            quad.transform.SetParent(holder.transform, false);
            quad.transform.localScale = new Vector3(1f, 0.66f, 1f);
            var mat = MakeLit(Color.white);
            mat.SetFloat("_Smoothness", 0f);
            quad.GetComponent<Renderer>().material = mat;

            var text = CreateTextMesh(holder.transform, "0", 0.55f, Color.black);
            text.transform.localPosition = new Vector3(0f, 0f, -0.03f);
            return text;
        }

        void BuildPutMans(LevelController level)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/SeesawAvatar");
            var root = new GameObject("putMans").transform;
            putMansRoot = root;

            int count = numbers.Length;
            float spacing = 1.15f;
            float x0 = -(count - 1) * spacing * 0.5f;

            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(prefab, root);
                go.name = $"PutMan_{i}";
                go.transform.position = new Vector3(x0 + i * spacing, -0.6f, -1.7f);
                go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                var man = go.AddComponent<PutMan>();
                man.avatarAnimator = go.GetComponent<SeesawAvatarAnimator>();
                EnsureGroundSeat(man);

                var rend = man.avatarAnimator.MeshRenderer;
                rend.material.color = ManColors[i % ManColors.Length];

                ApplyManInteractionSize(man);

                var holder = new GameObject("NumHolder");
                holder.transform.SetParent(go.transform, false);
                holder.transform.localPosition = new Vector3(0f, manHeight * 0.52f / go.transform.localScale.y, 0f);
                holder.transform.localScale = Vector3.one / go.transform.localScale.y;
                holder.AddComponent<FaceCamera>();
                man.textNum = CreateTextMesh(holder.transform, "0", 0.42f, Color.white);
                man.textNum.transform.localPosition = new Vector3(0f, 0f, -0.28f);

                man.Init(numbers[i], false);
                man.SaveInitState();
                man.PlayIdle();
                if (man.avatarAnimator != null && man.avatarAnimator.Animator != null)
                    man.avatarAnimator.Animator.Play("idle", 0, Random.value);

                level.putMans.Add(man);
            }

            if (level.putMans.Count > 0)
                putManTemplate = level.putMans[0].gameObject;
        }

        void ApplyManInteractionSize(PutMan man)
        {
            if (man == null)
                return;

            var b = GetManVisualBounds(man);
            if (b.size.y > 0.0001f)
                man.transform.localScale *= manHeight / b.size.y;

            var box = man.pickCollider as BoxCollider;
            if (box == null)
                box = man.GetComponent<BoxCollider>();
            if (box == null)
                box = man.gameObject.AddComponent<BoxCollider>();

            b = GetManVisualBounds(man);
            box.center = man.transform.InverseTransformPoint(b.center);
            Vector3 size = man.transform.InverseTransformVector(b.size);
            box.size = new Vector3(
                Mathf.Abs(size.x) + manPickPadding.x,
                Mathf.Abs(size.y) + manPickPadding.y,
                Mathf.Abs(size.z) + manPickPadding.z);
            man.pickCollider = box;
        }

        void ApplyRuntimeVisualAdjustments()
        {
            if (levelController != null)
            {
                foreach (var seesaw in levelController.seesaws)
                {
                    if (seesaw == null)
                        continue;
                    seesaw.ApplySeatCounts();
                    AdjustPanVisuals(seesaw.leftPan);
                    AdjustPanVisuals(seesaw.rightPan);
                }
            }

            if (putMansRoot != null)
            {
                foreach (var man in putMansRoot.GetComponentsInChildren<PutMan>(true))
                    EnsureGroundSeat(man);
            }
        }

        void AdjustPanVisuals(NumContainerPan pan)
        {
            if (pan == null)
                return;

            var staleParts = new List<GameObject>();
            foreach (Transform child in pan.transform)
                if (child.name.StartsWith("Tray"))
                    staleParts.Add(child.gameObject);
            foreach (var part in staleParts)
                DestroyUnityObject(part);

            if (pan.textTotal != null)
            {
                var holder = pan.textTotal.transform.parent;
                if (holder != null)
                    holder.localPosition = new Vector3(0f, -0.55f, 0f);
            }
        }

        void EnsureGroundSeat(PutMan man)
        {
            if (man == null)
                return;

            var existing = man.transform.Find("GroundSeat");
            if (existing != null)
            {
                man.groundSeat = existing;
                return;
            }

            var seat = CreatePart(PrimitiveType.Cylinder, man.transform, "GroundSeat", SeatColor,
                new Vector3(0f, 0.03f, 0f), new Vector3(0.5f, 0.055f, 0.5f));
            man.groundSeat = seat.transform;
        }

        static Bounds GetManVisualBounds(PutMan man)
        {
            if (man.avatarAnimator != null && man.avatarAnimator.MeshRenderer != null)
                return man.avatarAnimator.MeshRenderer.bounds;
            return GetBounds(man.transform);
        }

        GameObject CreatePart(PrimitiveType type, Transform parent, string name, Color color, Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(type);
            StripColliders(go);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().material = MakeLit(color);
            return go;
        }

        Material MakeLit(Color color)
        {
            if (m_baseLitMat == null)
                return new Material(Shader.Find("Universal Render Pipeline/Lit"));

            var mat = new Material(m_baseLitMat.shader);
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0.15f);
            mat.SetFloat("_Metallic", 0f);
            return mat;
        }

        TextMesh CreateTextMesh(Transform parent, string content, float worldSize, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var tm = go.AddComponent<TextMesh>();
            tm.text = content;
            tm.font = m_font;
            tm.fontSize = 80;
            tm.characterSize = worldSize * 0.1f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.fontStyle = FontStyle.Bold;
            go.GetComponent<MeshRenderer>().material = m_font.material;
            return tm;
        }

        enum Axis { X, Y }

        static void NormalizeSize(Transform t, float target, Axis axis)
        {
            var b = GetBounds(t);
            float size = axis == Axis.X ? b.size.x : b.size.y;
            if (size > 0.0001f)
                t.localScale *= target / size;
        }

        static Bounds GetBounds(Transform t)
        {
            var rends = t.GetComponentsInChildren<Renderer>();
            var b = new Bounds(t.position, Vector3.zero);
            bool first = true;
            foreach (var r in rends)
            {
                if (first) { b = r.bounds; first = false; }
                else b.Encapsulate(r.bounds);
            }
            return b;
        }

        static void StripColliders(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>())
                DestroyUnityObject(c);
        }

        static void DestroyUnityObject(Object obj)
        {
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        static Mesh BuildPrismMesh(float width, float height, float depth)
        {
            float hw = width * 0.5f, hd = depth * 0.5f;
            var p0 = new Vector3(-hw, 0f, -hd);
            var p1 = new Vector3(hw, 0f, -hd);
            var p2 = new Vector3(0f, height, -hd);
            var p3 = new Vector3(-hw, 0f, hd);
            var p4 = new Vector3(hw, 0f, hd);
            var p5 = new Vector3(0f, height, hd);

            var verts = new List<Vector3>();
            var tris = new List<int>();

            void AddTri(Vector3 a, Vector3 b, Vector3 c)
            {
                int i = verts.Count;
                verts.Add(a); verts.Add(b); verts.Add(c);
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
            }
            void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                AddTri(a, b, c);
                AddTri(a, c, d);
            }

            AddTri(p0, p2, p1);
            AddTri(p3, p4, p5);
            AddQuad(p0, p3, p5, p2);
            AddQuad(p1, p2, p5, p4);
            AddQuad(p0, p1, p4, p3);

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
