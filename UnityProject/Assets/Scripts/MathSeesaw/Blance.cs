using System;
using System.Collections;
using UnityEngine;

namespace MathSeesaw
{
    public class Blance : MonoBehaviour
    {
        public const float MaxAngle = 13f;
        public const float AngleScale = 1f;
        public const float RotateDuration = 1.5f;

        public Transform upComponent;
        public NumContainerPan leftPan;
        public NumContainerPan rightPan;
        [Min(0)] public int leftSeatCount = 4;
        [Min(0)] public int rightSeatCount = 4;

        public Action onRotateOver;

        int m_leftWeight;
        int m_rightWeight;
        bool m_initialized;
        Coroutine m_rotateCo;

        void Awake()
        {
            ApplySeatCounts();
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
                ApplySeatCounts();
        }

        public void ApplySeatCounts()
        {
            if (leftPan != null)
                leftPan.ApplySeatCount(leftSeatCount);
            if (rightPan != null)
                rightPan.ApplySeatCount(rightSeatCount);
        }

        public static float GetAngleByWeight(int left, int right)
        {
            if (left == 0 && right == 0)
                return 0f;
            int max = Mathf.Max(left, right);
            int min = Mathf.Min(left, right);
            float percent = min > 0 ? (max - min) / (float)min * 100f : 100f;
            percent *= AngleScale;
            percent = Mathf.Min(percent, MaxAngle);
            if (left < right)
                percent = -percent;
            return percent;
        }

        public void UpdateWeight(int left, int right, bool immediate = false)
        {
            if (m_initialized && left == m_leftWeight && right == m_rightWeight)
                return;
            m_initialized = true;
            m_leftWeight = left;
            m_rightWeight = right;
            float target = GetAngleByWeight(left, right);

            if (m_rotateCo != null)
                StopCoroutine(m_rotateCo);

            if (immediate)
            {
                upComponent.localEulerAngles = new Vector3(0f, 0f, target);
                onRotateOver?.Invoke();
                return;
            }
            m_rotateCo = StartCoroutine(RotateTo(target));
        }

        IEnumerator RotateTo(float target)
        {
            float start = upComponent.localEulerAngles.z;
            if (start > 180f) start -= 360f;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / RotateDuration;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                upComponent.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(start, target, k));
                yield return null;
            }
            m_rotateCo = null;
            onRotateOver?.Invoke();
        }
    }
}
