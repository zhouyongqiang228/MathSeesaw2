using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MathSeesaw;

public static class DemoValidator
{
    const string Flag = "ms2_validate";
    static double s_phaseStart;
    static int s_phase;

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
        s_phase = 0;
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate()
    {
        if (!EditorApplication.isPlaying)
            return;
        double elapsed = EditorApplication.timeSinceStartup - s_phaseStart;

        switch (s_phase)
        {
            case 0:
                if (elapsed < 1.0) return;
                Capture("/tmp/ms2_shot_start.png");
                AutoPlace();
                NextPhase();
                break;
            case 1:
                if (elapsed < 2.5) return;
                Capture("/tmp/ms2_shot_win.png");
                Verify();
                break;
        }
    }

    static void NextPhase()
    {
        s_phase++;
        s_phaseStart = EditorApplication.timeSinceStartup;
    }

    static void AutoPlace()
    {
        var level = Object.FindFirstObjectByType<LevelController>();
        var men = level.putMans;

        // Get the first active seesaw
        var seesaw = level.seesaws.Count > 0 ? level.seesaws[0] : null;
        if (seesaw == null)
        {
            Debug.LogError("[Validator] No seesaw found");
            return;
        }

        foreach (var man in men)
        {
            var pan = (man.InitNum == 1 || man.InitNum == 4) ? seesaw.leftPan : seesaw.rightPan;
            var place = pan.GetNearestEmptyPlace(man.transform.position);
            place.SetMan(man);
        }
        level.UpdateScore();
        Debug.Log($"[Validator] placed all, L={seesaw.leftPan.TotalScore} R={seesaw.rightPan.TotalScore}");
    }

    static void Verify()
    {
        var level = Object.FindFirstObjectByType<LevelController>();
        bool ok = true;

        // Get the first active seesaw
        var seesaw = level.seesaws.Count > 0 ? level.seesaws[0] : null;
        if (seesaw == null)
        {
            Debug.LogError("[Validator] No seesaw found");
            ok = false;
        }
        else
        {
            if (seesaw.leftPan.TotalScore != 5 || seesaw.rightPan.TotalScore != 5)
            {
                Debug.LogError($"[Validator] totals wrong: {seesaw.leftPan.TotalScore}/{seesaw.rightPan.TotalScore}");
                ok = false;
            }
            float angle = seesaw.upComponent.localEulerAngles.z;
            if (angle > 180f) angle -= 360f;
            if (Mathf.Abs(angle) > 0.5f)
            {
                Debug.LogError($"[Validator] beam not level: {angle}");
                ok = false;
            }
        }

        var winPanel = GameObject.Find("WinPanel");
        if (winPanel == null)
        {
            Debug.LogError("[Validator] win panel not shown");
            ok = false;
        }
        Debug.Log("[Validator] result=" + (ok ? "PASS" : "FAIL"));
        SessionState.SetBool(Flag, false);
        EditorApplication.Exit(ok ? 0 : 1);
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
        Debug.Log("[Validator] captured " + path);
    }
}
