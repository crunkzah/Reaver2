using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(RagdollController2))]
public class RagdollController2Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        RagdollController2 t = (RagdollController2)target;
        
        if(GUILayout.Button("Init ragdoll"))
        {
            t.Init();    
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        
        if(GUILayout.Button("Set to kinematic"))
        {
            t.SetToKinematic();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
