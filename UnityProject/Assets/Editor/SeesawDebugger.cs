using UnityEngine;
using UnityEditor;
using MathSeesaw;

[InitializeOnLoad]
public static class SeesawDebugger
{
    static SeesawDebugger()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.delayCall += () =>
            {
                var level = Object.FindFirstObjectByType<LevelController>();
                if (level != null)
                {
                    Debug.Log($"[SeesawDebugger] Current mode: {level.seesawMode}");
                    Debug.Log($"[SeesawDebugger] Seesaws count: {level.seesaws.Count}");
                    for (int i = 0; i < level.seesaws.Count; i++)
                    {
                        var seesaw = level.seesaws[i];
                        Debug.Log($"[SeesawDebugger] Seesaw {i}: Active={seesaw.gameObject.activeSelf}, Position={seesaw.transform.position}");
                    }
                }
            };
        }
    }

    [MenuItem("MathSeesaw/Debug Seesaws")]
    static void DebugSeesaws()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Please enter Play Mode first");
            return;
        }

        var level = Object.FindFirstObjectByType<LevelController>();
        if (level == null)
        {
            Debug.LogError("LevelController not found");
            return;
        }

        Debug.Log("=== Seesaw Debug Info ===");
        Debug.Log($"Mode: {level.seesawMode}");
        Debug.Log($"Total seesaws: {level.seesaws.Count}");

        for (int i = 0; i < level.seesaws.Count; i++)
        {
            var seesaw = level.seesaws[i];
            Debug.Log($"\nSeesaw {i}:");
            Debug.Log($"  Name: {seesaw.gameObject.name}");
            Debug.Log($"  Active: {seesaw.gameObject.activeSelf}");
            Debug.Log($"  Position: {seesaw.transform.position}");
            Debug.Log($"  Left Pan: {seesaw.leftPan.TotalScore}");
            Debug.Log($"  Right Pan: {seesaw.rightPan.TotalScore}");
        }
    }
}
