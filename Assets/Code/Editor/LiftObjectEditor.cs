using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LiftObject))]
public class LiftObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Lift Objects"))
        {
            LiftObject t = (LiftObject)target;
            
            LiftObject[] objects_to_lift = FindObjectsOfType<LiftObject>();
            MeshFilter mf;
            
            for(int i = 0; i < objects_to_lift.Length; i++)
            {
                mf = objects_to_lift[i].GetComponent<MeshFilter>();
                if(mf)
                {
                    
                    int randomIndex = Random.Range(0, objects_to_lift[i].meshes.Length);
                        
                    mf.sharedMesh = objects_to_lift[i].meshes[randomIndex];
                    EditorUtility.SetDirty(objects_to_lift[i].gameObject);
                }
            }
            
            Debug.Log("<color=#59a832>Modified lifted objects</color>");
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        if(GUILayout.Button("Revert"))
        {
            LiftObject t = (LiftObject)target;
            
            LiftObject[] objects_to_lift = FindObjectsOfType<LiftObject>();
            MeshFilter mf;
            
            for(int i = 0; i < objects_to_lift.Length; i++)
            {
                mf = objects_to_lift[i].GetComponent<MeshFilter>();
                if(mf)
                {
                    int Index = 0;
                    mf.sharedMesh = objects_to_lift[i].meshes[Index];
                    EditorUtility.SetDirty(objects_to_lift[i].gameObject);
                }
            }
            
            Debug.Log("<color=#32a894>Reverted lifted objects back to mesh 0</color>");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
    
}
