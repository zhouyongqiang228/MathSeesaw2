using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MathSeesaw
{
    public class LevelController : MonoBehaviour
    {
        public Camera cam;
        public Blance blance;
        public NumContainerPan leftPan;
        public NumContainerPan rightPan;
        public List<PutMan> putMans = new List<PutMan>();
        public GameUI ui;

        public float snapDistance = 1.6f;
        public float dragFollowSpeed = 22f;
        public float dragLift = 0.45f;
        public float dragScale = 1.12f;

        PutMan m_dragMan;
        ManPlace m_hoverPlace;
        Vector3 m_dragTarget;
        Plane m_dragPlane;
        bool m_gameOver;

        void Start()
        {
            blance.onRotateOver = CheckAndDealGameOver;
            UpdateScore(true);
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
            foreach (var pan in new[] { leftPan, rightPan })
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
            return best;
        }

        public void UpdateScore(bool immediate = false)
        {
            leftPan.UpdateTotalScore();
            rightPan.UpdateTotalScore();
            blance.UpdateWeight(leftPan.TotalScore, rightPan.TotalScore, immediate);
        }

        void CheckAndDealGameOver()
        {
            if (m_gameOver)
                return;
            if (leftPan.TotalScore != rightPan.TotalScore)
                return;
            foreach (var man in putMans)
                if (man.CurPlace == null)
                    return;

            m_gameOver = true;
            leftPan.OnGameWin();
            rightPan.OnGameWin();
            foreach (var man in putMans)
                man.PlayVictory();
            if (ui != null)
                ui.ShowWin();
        }
    }
}
