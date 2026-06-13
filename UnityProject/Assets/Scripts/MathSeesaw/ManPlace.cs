using UnityEngine;

namespace MathSeesaw
{
    public class ManPlace : MonoBehaviour
    {
        public NumContainerPan container;
        public bool isLeft;
        public int buffMultiplier = 1;
        public Transform standPoint;
        public Renderer seatRenderer;

        public PutMan PuttedMan { get; private set; }
        public bool Locked { get; set; }

        Color m_baseColor;
        Vector3 m_baseScale;
        bool m_inited;

        void CacheVisual()
        {
            if (m_inited) return;
            m_inited = true;
            m_baseColor = seatRenderer.material.color;
            m_baseScale = seatRenderer.transform.localScale;
        }

        public bool IsEmpty => PuttedMan == null && !Locked;

        public bool SetMan(PutMan man)
        {
            if (PuttedMan != null && PuttedMan != man)
                return false;
            if (man.CurPlace != null && man.CurPlace != this)
                man.CurPlace.ClearMan();
            PuttedMan = man;
            man.CurPlace = this;
            man.transform.SetParent(standPoint, false);
            man.transform.localPosition = Vector3.zero;
            man.transform.localRotation = Quaternion.identity;
            man.SetGroundSeatVisible(false);
            man.RefreshText();
            return true;
        }

        public void ClearMan()
        {
            if (PuttedMan != null)
            {
                PuttedMan.CurPlace = null;
                PuttedMan.SetGroundSeatVisible(true);
                PuttedMan.RefreshText();
                PuttedMan = null;
            }
        }

        public int GetScore() => PuttedMan != null ? PuttedMan.GetScore() : 0;

        public bool ScoreHidden => PuttedMan != null && !PuttedMan.NumVisible;

        public void SetHighlight(bool on)
        {
            CacheVisual();
            seatRenderer.material.color = on ? Color.Lerp(m_baseColor, Color.white, 0.18f) : m_baseColor;
            seatRenderer.transform.localScale = on ? m_baseScale * 1.06f : m_baseScale;
        }
    }
}
