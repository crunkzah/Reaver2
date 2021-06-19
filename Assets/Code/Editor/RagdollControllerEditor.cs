using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(RagdollController))]
public class RagdollControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        RagdollController t = (RagdollController)target;
        if(GUILayout.Button("Init ragdoll controller"))
        {
            t.FindRbsAndColliders();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        if (GUILayout.Button("Enable"))
        {
            t.ToggleRagdoll(false);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        if (GUILayout.Button("Disable"))
        {
            t.ToggleRagdoll(true);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
