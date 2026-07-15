using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StageGenerator))]
public class StageGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StageGenerator generator = (StageGenerator)target;

        GUILayout.Space(10);

        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Generate Stage", GUILayout.Height(35)))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Generate Stage");

            generator.Generate();

            EditorUtility.SetDirty(generator);
        }

        GUI.backgroundColor = Color.cyan;


        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("Clear Stage", GUILayout.Height(35)))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Clear Stage");

            generator.ClearMaze();

            EditorUtility.SetDirty(generator);
        }

        GUI.backgroundColor = Color.white;
    }
}