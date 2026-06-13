using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MathSeesaw
{
    public class LevelController : MonoBehaviour
    {
        public Camera cam;
        public List<Blance> seesaws = new List<Blance>();
        public List<PutMan> putMans = new List<PutMan>();
        public GameUI ui;
        public SeesawMode seesawMode = SeesawMode.Single;

        public float snapDistance = 1.6f;
        public float snapScreenDistance = 120f;
        public float dragFollowSpeed = 22f;
        public float dragLift = 0.45f;
        public float dragScale = 1.12f;
        [Range(1, 2)] public int activeSeesawCount = 1;

        [Header("Camera Settings")]
        public Vector3 singleCameraPosition = new Vector3(0f, 7f, -7f);
        public Vector3 singleCameraRotation = new Vector3(30f, 0f, 0f);
        public float singleCameraSize = 6f;
        public Vector3 doubleCameraPosition = new Vector3(0f, 7.5f, -9f);
        public Vector3 doubleCameraRotation = new Vector3(34f, 0f, 0f);
        public float doubleCameraSize = 7.2f;

        [Header("Camera Framing")]
        public float designWidth = 2048f;
        public float designHeight = 2732f;
        public bool frameActiveSeesaws = true;
        public float cameraFramePadding = 1.35f;

        PutMan m_dragMan;
        ManPlace m_hoverPlace;
        Vector3 m_dragTarget;
        Plane m_dragPlane;
        bool m_gameOver;
        GameObject m_previewGhost;

        float m_baseOrthographicSize = 6f;
        readonly List<Vector3> m_initialSeesawPositions = new List<Vector3>();
        float m_lastAspect = -1f;

        int m_moveCount;
        float m_startTime;
        float m_lastHoverHapticTime = -1f;

        void Start()
        {
            CacheSeesawPositions();
            foreach (var seesaw in seesaws)
            {
                seesaw.ApplySeatCounts();
                seesaw.onRotateOver = CheckAndDealGameOver;
            }
            UpdateScore(true);
            ApplyCameraSettings();

            m_moveCount = 0;
            m_startTime = Time.time;
        }

        public void SwitchSeesawMode(SeesawMode mode)
        {
            seesawMode = mode;
            activeSeesawCount = mode == SeesawMode.Double ? 2 : 1;
            CacheSeesawPositions();

            int count = Mathf.Clamp(activeSeesawCount, 1, Mathf.Max(1, seesaws.Count));
            for (int i = 0; i < seesaws.Count; i++)
            {
                if (seesaws[i] == null)
                    continue;
                bool active = i < count;
                seesaws[i].gameObject.SetActive(active);
                if (i < m_initialSeesawPositions.Count)
                    seesaws[i].transform.position = m_initialSeesawPositions[i];
                if (active)
                    seesaws[i].ApplySeatCounts();
            }

            ApplyCameraSettings();
            UpdateScore(true);
        }

        void CacheSeesawPositions()
        {
            while (m_initialSeesawPositions.Count < seesaws.Count)
                m_initialSeesawPositions.Add(seesaws[m_initialSeesawPositions.Count].transform.position);
        }

        void ApplyCameraSettings()
        {
            if (cam == null)
                return;

            if (seesawMode == SeesawMode.Single)
            {
                cam.transform.position = singleCameraPosition;
                cam.transform.rotation = Quaternion.Euler(singleCameraRotation);
                m_baseOrthographicSize = singleCameraSize;
            }
            else
            {
                cam.transform.position = doubleCameraPosition;
                cam.transform.rotation = Quaternion.Euler(doubleCameraRotation);
                m_baseOrthographicSize = doubleCameraSize;
            }
            if (frameActiveSeesaws)
                FrameActiveSeesaws();
            ApplyResponsiveCameraSize(true);
        }

        void FrameActiveSeesaws()
        {
            if (cam == null || !cam.orthographic)
                return;

            Bounds bounds = default;
            bool hasBounds = false;
            foreach (var seesaw in seesaws)
            {
                if (seesaw == null || !seesaw.gameObject.activeSelf)
                    continue;

                foreach (var renderer in seesaw.GetComponentsInChildren<Renderer>())
                {
                    if (!renderer.enabled)
                        continue;
                    if (!hasBounds)
                    {
                        bounds = renderer.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            if (!hasBounds)
                return;

            Vector3 center = bounds.center;
            cam.transform.position = new Vector3(center.x, cam.transform.position.y, cam.transform.position.z);
            float depthSize = Mathf.Max(bounds.size.z * 0.55f, bounds.size.y) + cameraFramePadding;
            float widthSize = bounds.size.x / Mathf.Max(cam.aspect, 0.01f) * 0.55f + cameraFramePadding;
            m_baseOrthographicSize = Mathf.Max(m_baseOrthographicSize, depthSize, widthSize);
        }

        void ApplyResponsiveCameraSize(bool force = false)
        {
            if (cam == null || !cam.orthographic)
                return;

            float aspect = Mathf.Max(cam.aspect, 0.01f);
            if (!force && Mathf.Approximately(aspect, m_lastAspect))
                return;

            m_lastAspect = aspect;
            float designAspect = Mathf.Max(0.01f, designWidth) / Mathf.Max(0.01f, designHeight);
            float widthFitScale = Mathf.Max(1f, designAspect / aspect);
            cam.orthographicSize = m_baseOrthographicSize * widthFitScale;
        }

        void Update()
        {
            ApplyResponsiveCameraSize();

            if (m_gameOver)
                return;

            // 使用更可靠的输入检测
            bool pressed = false;
            Vector2 pos = Vector2.zero;

            // 优先检测触摸输入
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                pressed = true;
                pos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            // 回退到鼠标输入（编辑器测试）
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                pressed = true;
                pos = Mouse.current.position.ReadValue();
            }

            // 检测按下开始
            bool justPressed = false;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                justPressed = true;
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                justPressed = true;

            // 检测松开
            bool justReleased = false;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
                justReleased = true;
            else if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
                justReleased = true;

            if (pressed && m_dragMan == null && justPressed)
                TryPick(pos);
            else if (pressed && m_dragMan != null)
                DragMove(pos);
            else if (justReleased && m_dragMan != null)
                Drop();
        }

        void TryPick(Vector2 screenPos)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
                return;
            var man = hit.collider.GetComponentInParent<PutMan>();
            if (man == null)
                return;
            if (man.CurPlace != null && man.CurPlace.Locked)
                return;

            m_dragMan = man;
            if (man.CurPlace != null)
            {
                man.CurPlace.ClearMan();
                UpdateScore();
            }
            man.SetGroundSeatVisible(false);
            man.transform.SetParent(null, true);
            man.transform.localScale *= dragScale;
            m_dragPlane = new Plane(-cam.transform.forward, man.transform.position + Vector3.up * dragLift);
            m_dragTarget = man.transform.position;

            // 清理旧的预览
            if (m_previewGhost != null)
            {
                Destroy(m_previewGhost);
                m_previewGhost = null;
            }

            // 播放拾取音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.PickupMan);
            }

            // 播放触觉反馈
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.LightImpact();
            }
        }

        void DragMove(Vector2 screenPos)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            if (m_dragPlane.Raycast(ray, out float enter))
                m_dragTarget = ray.GetPoint(enter);

            var t = m_dragMan.transform;
            t.position = Vector3.Lerp(t.position, m_dragTarget, Time.deltaTime * dragFollowSpeed);
            t.rotation = Quaternion.Slerp(t.rotation, Quaternion.Euler(0f, 180f, 0f), Time.deltaTime * 10f);

            var place = FindNearestEmptyPlace(screenPos, t.position);
            if (place != m_hoverPlace)
            {
                if (m_hoverPlace != null) m_hoverPlace.SetHighlight(false);
                m_hoverPlace = place;
                if (m_hoverPlace != null)
                {
                    m_hoverPlace.SetHighlight(true);
                    if (HapticManager.Instance != null && Time.unscaledTime - m_lastHoverHapticTime > 0.18f)
                    {
                        HapticManager.Instance.Selection();
                        m_lastHoverHapticTime = Time.unscaledTime;
                    }
                }
            }

            // 更新半透明预览
            UpdatePreviewGhost();
        }

        void UpdatePreviewGhost()
        {
            if (m_hoverPlace != null && m_dragMan != null)
            {
                // 创建或显示预览
                if (m_previewGhost == null)
                {
                    m_previewGhost = CreatePreviewGhost(m_dragMan);
                }

                // 更新预览位置
                m_previewGhost.SetActive(true);
                m_previewGhost.transform.position = m_hoverPlace.standPoint.position;
                m_previewGhost.transform.rotation = Quaternion.identity;
            }
            else if (m_previewGhost != null)
            {
                // 隐藏预览
                m_previewGhost.SetActive(false);
            }
        }

        GameObject CreatePreviewGhost(PutMan original)
        {
            // 复制原始小人
            var ghost = Instantiate(original.gameObject);
            ghost.name = "PreviewGhost";

            // 移除不需要的组件
            if (ghost.GetComponent<PutMan>() is PutMan putman)
                Destroy(putman);
            if (ghost.GetComponent<Collider>() is Collider col)
                Destroy(col);

            // 设置半透明材质
            var renderers = ghost.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                {
                    // 设置为半透明
                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color c = mat.color;
                        c.a = 0.4f;
                        mat.color = c;
                    }

                    // 如果使用标准着色器，需要切换到透明模式
                    if (mat.HasProperty("_Mode"))
                    {
                        mat.SetFloat("_Mode", 3); // Transparent mode
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                }
            }

            ghost.SetActive(false);
            return ghost;
        }

        void Drop()
        {
            var man = m_dragMan;
            m_dragMan = null;
            man.transform.localScale /= dragScale;

            // 清理预览
            if (m_previewGhost != null)
            {
                Destroy(m_previewGhost);
                m_previewGhost = null;
            }

            if (m_hoverPlace != null)
            {
                m_hoverPlace.SetHighlight(false);
                m_hoverPlace.SetMan(man);
                m_hoverPlace = null;
                m_moveCount++;

                // 播放放置音效
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.PlaceMan);
                }

                // 播放触觉反馈
                if (HapticManager.Instance != null)
                {
                    HapticManager.Instance.LightImpact();
                }
            }
            else
            {
                man.RestoreInitState();
            }
            UpdateScore();
        }

        ManPlace FindNearestEmptyPlace(Vector2 screenPos, Vector3 worldPos)
        {
            ManPlace best = null;
            float bestScreenDist = snapScreenDistance * snapScreenDistance;
            float bestWorldDist = snapDistance * snapDistance;
            foreach (var seesaw in seesaws)
            {
                if (!seesaw.gameObject.activeSelf) continue;
                foreach (var pan in new[] { seesaw.leftPan, seesaw.rightPan })
                {
                    if (pan == null)
                        continue;
                    foreach (var p in pan.places)
                    {
                        if (p == null || !p.RuntimeAvailable)
                            continue;
                        if (!p.IsEmpty) continue;

                        Vector3 placeScreenPos = cam.WorldToScreenPoint(p.standPoint.position);
                        if (placeScreenPos.z > 0f)
                        {
                            float screenDist = ((Vector2)placeScreenPos - screenPos).sqrMagnitude;
                            if (screenDist < bestScreenDist)
                            {
                                bestScreenDist = screenDist;
                                bestWorldDist = (p.standPoint.position - worldPos).sqrMagnitude;
                                best = p;
                            }
                            continue;
                        }

                        float worldDist = (p.standPoint.position - worldPos).sqrMagnitude;
                        if (worldDist < bestWorldDist)
                        {
                            bestWorldDist = worldDist;
                            best = p;
                        }
                    }
                }
            }
            return best;
        }

        public void UpdateScore(bool immediate = false)
        {
            foreach (var seesaw in seesaws)
            {
                if (!seesaw.gameObject.activeSelf) continue;
                seesaw.leftPan.UpdateTotalScore();
                seesaw.rightPan.UpdateTotalScore();
                seesaw.UpdateWeight(seesaw.leftPan.TotalScore, seesaw.rightPan.TotalScore, immediate);
            }
        }

        void CheckAndDealGameOver()
        {
            if (m_gameOver)
                return;

            // Check all active seesaws are balanced
            foreach (var seesaw in seesaws)
            {
                if (!seesaw.gameObject.activeSelf) continue;
                if (seesaw.leftPan.TotalScore != seesaw.rightPan.TotalScore)
                    return;
            }

            // Check all putMans are placed
            foreach (var man in putMans)
                if (man.CurPlace == null)
                    return;

            m_gameOver = true;

            // 播放平衡音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.SeesawBalance);
            }

            foreach (var seesaw in seesaws)
            {
                if (!seesaw.gameObject.activeSelf) continue;
                seesaw.leftPan.OnGameWin();
                seesaw.rightPan.OnGameWin();
            }
            foreach (var man in putMans)
                man.PlayVictory();

            // 保存关卡进度
            if (GameProgressManager.Instance != null)
            {
                int currentLevel = 1; // 需要从 GameBootstrap 获取
                if (FindObjectOfType<GameBootstrap>() is GameBootstrap bootstrap)
                {
                    currentLevel = bootstrap.curLevel;
                }
                float completionTime = Time.time - m_startTime;
                GameProgressManager.Instance.CompleteLevel(currentLevel, m_moveCount, completionTime);
            }

            // 播放胜利音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.Victory);
            }

            if (ui != null)
                ui.ShowWin();
        }
    }
}
