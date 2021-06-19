using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectSpawner))]
public class ObjectSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        
        
        
        
       
        if(GUILayout.Button("(Global)Save names"))
        {
            ObjectSpawner[] spawners = FindObjectsOfType<ObjectSpawner>();
            
            int len = spawners.Length;
            
            for(int i = 0; i < len; i++)
            {
                spawners[i].gameObject.name = string.Format("Spawner {0} ({1})", spawners[i].obj_key, i);
                EditorUtility.SetDirty(spawners[i]);
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
            
        }
    }
}
