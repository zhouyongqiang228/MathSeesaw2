using UnityEngine;

namespace MathSeesaw
{
    public class CloudDrift : MonoBehaviour
    {
        public float speed = 0.25f;
        public float range = 3f;

        Vector3 m_center;
        float m_phase;

        void Start()
        {
            m_center = transform.position;
            m_phase = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            float x = Mathf.Sin(Time.time * speed + m_phase) * range;
            transform.position = m_center + new Vector3(x, 0f, 0f);
        }
    }
}
