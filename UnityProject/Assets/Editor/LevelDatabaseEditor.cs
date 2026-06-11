using UnityEditor;
using UnityEngine;

namespace MathSeesaw
{
    [CustomEditor(typeof(LevelDatabase))]
    public class LevelDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelDatabase database = (LevelDatabase)target;

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Level Management", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Default Levels", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Create Default Levels",
                    "This will clear all existing levels and create 20 default levels. Continue?",
                    "Yes", "Cancel"))
                {
                    database.CreateDefaultLevels();
                    EditorUtility.SetDirty(database);
                    Debug.Log("Created 20 default levels");
                }
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Add New Level", GUILayout.Height(30)))
            {
                int nextLevel = database.GetTotalLevels() + 1;
                database.levels.Add(new LevelData(nextLevel, new int[] { 1, 2, 3 }));
                EditorUtility.SetDirty(database);
            }

            if (database.levels.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Total Levels: {database.GetTotalLevels()}", EditorStyles.helpBox);
            }
        }
    }
}
