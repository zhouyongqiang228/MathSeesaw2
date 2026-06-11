using UnityEngine;

namespace MathSeesaw
{
    /// <summary>
    /// 放置预览助手 - 显示小人将要被放置的位置
    /// </summary>
    public class PlacementPreview : MonoBehaviour
    {
        static Material s_ghostMaterial;

        GameObject m_ghostObject;
        Transform m_originalTransform;

        public void Initialize(GameObject original)
        {
            if (s_ghostMaterial == null)
            {
                CreateGhostMaterial();
            }

            // 复制原始对象
            m_originalTransform = original.transform;
            m_ghostObject = new GameObject("PlacementGhost");
            m_ghostObject.transform.SetParent(transform, false);

            // 复制所有 MeshRenderer
            CopyMeshes(original, m_ghostObject);

            // 设置半透明材质
            ApplyGhostMaterial(m_ghostObject);

            m_ghostObject.SetActive(false);
        }

        void CopyMeshes(GameObject source, GameObject target)
        {
            var meshFilters = source.GetComponentsInChildren<MeshFilter>();
            foreach (var sourceMF in meshFilters)
            {
                var go = new GameObject(sourceMF.name);
                go.transform.SetParent(target.transform, false);
                go.transform.localPosition = sourceMF.transform.localPosition;
                go.transform.localRotation = sourceMF.transform.localRotation;
                go.transform.localScale = sourceMF.transform.localScale;

                var mf = go.AddComponent<MeshFilter>();
                mf.mesh = sourceMF.sharedMesh;

                var mr = go.AddComponent<MeshRenderer>();
                mr.material = s_ghostMaterial;
            }

            // 也复制 SkinnedMeshRenderer（如果有）
            var skinnedRenderers = source.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var sourceSMR in skinnedRenderers)
            {
                var go = new GameObject(sourceSMR.name);
                go.transform.SetParent(target.transform, false);
                go.transform.localPosition = sourceSMR.transform.localPosition;
                go.transform.localRotation = sourceSMR.transform.localRotation;
                go.transform.localScale = sourceSMR.transform.localScale;

                // 转换为普通 MeshRenderer
                var mf = go.AddComponent<MeshFilter>();
                var mesh = new Mesh();
                sourceSMR.BakeMesh(mesh);
                mf.mesh = mesh;

                var mr = go.AddComponent<MeshRenderer>();
                mr.material = s_ghostMaterial;
            }
        }

        void ApplyGhostMaterial(GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = s_ghostMaterial;
            }
        }

        static void CreateGhostMaterial()
        {
            // 创建半透明材质
            s_ghostMaterial = new Material(Shader.Find("Standard"));
            s_ghostMaterial.SetFloat("_Mode", 3); // Transparent
            s_ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            s_ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            s_ghostMaterial.SetInt("_ZWrite", 0);
            s_ghostMaterial.DisableKeyword("_ALPHATEST_ON");
            s_ghostMaterial.EnableKeyword("_ALPHABLEND_ON");
            s_ghostMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            s_ghostMaterial.renderQueue = 3000;

            // 设置颜色为半透明白色
            Color ghostColor = new Color(1f, 1f, 1f, 0.3f);
            s_ghostMaterial.SetColor("_Color", ghostColor);
            s_ghostMaterial.SetColor("_EmissionColor", new Color(0.5f, 0.8f, 1f) * 0.3f);
            s_ghostMaterial.EnableKeyword("_EMISSION");
        }

        public void Show(Vector3 position, Quaternion rotation)
        {
            if (m_ghostObject == null)
                return;

            m_ghostObject.SetActive(true);
            m_ghostObject.transform.position = position;
            m_ghostObject.transform.rotation = rotation;
        }

        public void Hide()
        {
            if (m_ghostObject != null)
            {
                m_ghostObject.SetActive(false);
            }
        }

        void OnDestroy()
        {
            if (m_ghostObject != null)
            {
                Destroy(m_ghostObject);
            }
        }
    }
}
