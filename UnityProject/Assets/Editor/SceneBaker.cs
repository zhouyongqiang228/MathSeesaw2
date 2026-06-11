using MathSeesaw;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneBaker
{
    const string GameScenePath = "Assets/Scenes/Game.unity";

    [MenuItem("MathSeesaw/Scene/Rebuild Editable Game Scene")]
    public static void RebuildEditableGameScene()
    {
        var scene = EditorSceneManager.OpenScene(GameScenePath);
        var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
        if (bootstrap == null)
        {
            var root = new GameObject("GameRoot");
            bootstrap = root.AddComponent<GameBootstrap>();
        }

        EnsureRootComponent<LevelController>(bootstrap.gameObject, c => bootstrap.levelController = c);
        EnsureRootComponent<GameUI>(bootstrap.gameObject, c => bootstrap.gameUI = c);

        DeleteIfExists("Main Camera");
        DeleteIfExists("Directional Light");
        DeleteIfExists("Environment");
        DeleteIfExists("Seesaw_0");
        DeleteIfExists("Seesaw_1");
        DeleteIfExists("putMans");
        DeleteChildIfExists(bootstrap.transform, "Canvas_Battle");
        DeleteChildIfExists(bootstrap.transform, "EventSystem");

        bootstrap.buildMissingSceneObjects = false;
        bootstrap.BuildEditableScene();

        EditorUtility.SetDirty(bootstrap);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    public static void RebuildEditableGameSceneBatch()
    {
        RebuildEditableGameScene();
        EditorApplication.Exit(0);
    }

    static void EnsureRootComponent<T>(GameObject root, System.Action<T> assign) where T : Component
    {
        var component = root.GetComponent<T>();
        if (component == null)
            component = root.AddComponent<T>();
        assign(component);
    }

    static void DeleteIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null)
            Object.DestroyImmediate(go);
    }

    static void DeleteChildIfExists(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child != null)
            Object.DestroyImmediate(child.gameObject);
    }
}
