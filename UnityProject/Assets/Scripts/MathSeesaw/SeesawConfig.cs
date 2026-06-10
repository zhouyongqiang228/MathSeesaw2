using UnityEngine;

namespace MathSeesaw
{
    [System.Serializable]
    public class SeesawConfig
    {
        public Vector3 position;
        public Camera.GateFitMode cameraFitMode;

        public SeesawConfig(Vector3 pos)
        {
            position = pos;
        }
    }
}
