using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MathSeesaw;

public static class SkinPreview
{
    const string Flag = "ms2_skin_preview";
    static double s_phaseStart;
    static int s_shot;

    public static void Run()
    {
        SessionState.SetBool(Flag, true);
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
        EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    static void Hook()
    {
        if (!SessionState.GetBool(Flag, false))
            return;
        s_phaseStart = EditorApplication.timeSinceStartup;
        s_shot = 0;
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate()
    {
        if (!EditorApplication.isPlaying)
            return;
        double elapsed = EditorApplication.timeSinceStartup - s_phaseStart;
        if (elapsed < 1.0)
            return;

        var mgr = Object.FindFirstObjectByType<SkinManager>();
        if (mgr == null)
            return;

        Capture($"/tmp/ms2_skin_{s_shot}.png");
        s_shot++;
        if (s_shot >= 6)
        {
            Debug.Log("[SkinPreview] done");
            SessionState.SetBool(Flag, false);
            EditorApplication.Exit(0);
            return;
        }
        mgr.Next();
        s_phaseStart = EditorApplication.timeSinceStartup;
    }

    static void Capture(string path)
    {
        var cam = Camera.main;
        int w = 900, h = 1600;
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log("[SkinPreview] captured " + path);
    }
}
