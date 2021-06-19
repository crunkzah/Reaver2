using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomizeRotationHelper))]
public class RandomizeRotationHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Randomize children rotation"))
        {
            RandomizeRotationHelper t = (RandomizeRotationHelper)target;
            
            t.RandomizeRotationOfChildren();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
    
}
