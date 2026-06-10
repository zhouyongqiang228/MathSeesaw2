using UnityEngine;

namespace MathSeesaw
{
    public class PutMan : MonoBehaviour
    {
        public int InitNum { get; private set; }
        public bool HideNum { get; private set; }
        public ManPlace CurPlace { get; set; }

        public SeesawAvatarAnimator avatarAnimator;
        public TextMesh textNum;
        public Collider pickCollider;

        Vector3 m_initPos;
        Quaternion m_initRot;
        Transform m_initParent;
        bool m_revealed;

        public void Init(int num, bool hideNum)
        {
            InitNum = num;
            HideNum = hideNum;
            RefreshText();
        }

        public int GetScore()
        {
            int score = InitNum;
            if (CurPlace != null && CurPlace.buffMultiplier != 1)
                score *= CurPlace.buffMultiplier;
            return score;
        }

        public bool NumVisible => !HideNum || m_revealed;

        public void RefreshText()
        {
            int score = GetScore();
            textNum.text = NumVisible ? score.ToString() : "?";
            float s = score >= 100 ? 0.6f : score >= 10 ? 0.8f : 1f;
            textNum.transform.localScale = Vector3.one * s;
        }

        public void Reveal()
        {
            m_revealed = true;
            RefreshText();
        }

        public void SaveInitState()
        {
            m_initPos = transform.position;
            m_initRot = transform.rotation;
            m_initParent = transform.parent;
        }

        public void RestoreInitState()
        {
            transform.SetParent(m_initParent, true);
            transform.SetPositionAndRotation(m_initPos, m_initRot);
        }

        public void PlayIdle() => avatarAnimator.PlayIdle();
        public void PlayVictory() => avatarAnimator.PlayVictory();
    }
}
