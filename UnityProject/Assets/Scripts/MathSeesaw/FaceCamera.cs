using UnityEngine;

namespace MathSeesaw
{
    public class FaceCamera : MonoBehaviour
    {
        Camera m_cam;

        void LateUpdate()
        {
            if (m_cam == null)
                m_cam = Camera.main;
            if (m_cam != null)
                transform.rotation = m_cam.transform.rotation;
        }
    }
}
