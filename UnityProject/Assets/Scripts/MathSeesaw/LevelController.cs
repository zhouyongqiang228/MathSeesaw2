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
        public float dragFollowSpeed = 22f;
        public float dragLift = 0.45f;
        public float dragScale = 1.12f;

        PutMan m_dragMan;
        ManPlace m_hoverPlace;
        Vector3 m_dragTarget;
        Plane m_dragPlane;
        bool m_gameOver;

        Vector3 m_singleCameraPos = new Vector3(0f, 3.4f, -9.5f);
        Vector3 m_singleCameraRot = new Vector3(13f, 0f, 0f);
        Vector3 m_doubleCameraPos = new Vector3(0f, 7f, -7f);
        Vector3 m_doubleCameraRot = new Vector3(30f, 0f, 0f);

        int m_moveCount;
        float m_startTime;

        void Start()
        {
            foreach (var seesaw in seesaws)
                seesaw.onRotateOver = CheckAndDealGameOver;
            UpdateScore(true);
            ApplyCameraSettings();

            m_moveCount = 0;
            m_startTime = Time.time;
        }

        public void SwitchSeesawMode(SeesawMode mode)
        {
            seesawMode = mode;
            ApplyCameraSettings();

            // Show/hide seesaws based on mode
            if (mode == SeesawMode.Single)
            {
                if (seesaws.Count > 0)
                {
                    seesaws[0].gameObject.SetActive(true);
                    seesaws[0].transform.position = new Vector3(0f, -0.6f, 1.2f); // Center position
                }
                if (seesaws.Count > 1)
                {
                    seesaws[1].gameObject.SetActive(false);
                }
            }
            else // Double mode
            {
                if (seesaws.Count > 0)
                {
                    seesaws[0].gameObject.SetActive(true);
                    seesaws[0].transform.position = new Vector3(0f, -0.6f, 1.2f); // Front position (original)
                }
                if (seesaws.Count > 1)
                {
                    seesaws[1].gameObject.SetActive(true);
                    seesaws[1].transform.position = new Vector3(0f, -0.6f, 6.5f); // Back position (far back)
                }
            }

            // Update scores after switching
            UpdateScore(true);
        }

        void ApplyCameraSettings()
        {
            if (seesawMode == SeesawMode.Single)
            {
                cam.transform.position = m_singleCameraPos;
                cam.transform.rotation = Quaternion.Euler(m_singleCameraRot);
            }
            else
            {
                cam.transform.position = m_doubleCameraPos;
                cam.transform.rotation = Quaternion.Euler(m_doubleCameraRot);
            }
        }

        void Update()
        {
            if (m_gameOver)
                return;
            var pointer = Pointer.current;
            if (pointer == null)
                return;

            Vector2 pos = pointer.position.ReadValue();
            bool pressed = pointer.press.isPressed;

            if (pressed && m_dragMan == null && pointer.press.wasPressedThisFrame)
                TryPick(pos);
            else if (pressed && m_dragMan != null)
                DragMove(pos);
            else if (!pressed && m_dragMan != null)
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
            man.transform.SetParent(null, true);
            man.transform.localScale *= dragScale;
            m_dragPlane = new Plane(-cam.transform.forward, man.transform.position + Vector3.up * dragLift);
            m_dragTarget = man.transform.position;

            // 播放拾取音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.PickupMan);
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

            var place = FindNearestEmptyPlace(t.position);
            if (place != m_hoverPlace)
            {
                if (m_hoverPlace != null) m_hoverPlace.SetHighlight(false);
                m_hoverPlace = place;
                if (m_hoverPlace != null) m_hoverPlace.SetHighlight(true);
            }
        }

        void Drop()
        {
            var man = m_dragMan;
            m_dragMan = null;
            man.transform.localScale /= dragScale;

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
            }
            else
            {
                man.RestoreInitState();
            }
            UpdateScore();
        }

        ManPlace FindNearestEmptyPlace(Vector3 worldPos)
        {
            ManPlace best = null;
            float bestDist = snapDistance * snapDistance;
            foreach (var seesaw in seesaws)
            {
                if (!seesaw.gameObject.activeSelf) continue;
                foreach (var pan in new[] { seesaw.leftPan, seesaw.rightPan })
                {
                    foreach (var p in pan.places)
                    {
                        if (!p.IsEmpty) continue;
                        float d = (p.standPoint.position - worldPos).sqrMagnitude;
                        if (d < bestDist)
                        {
                            bestDist = d;
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
