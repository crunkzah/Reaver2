using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CargoLineAnimator))]
public class CargoLineAnimatorEditor : Editor
{
    
    
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CargoLineAnimator t = (CargoLineAnimator)target;
        
        if(GUILayout.Button("Remove populated objects"))
        {
            t.DestroyPlacedObjects();
            
            EditorUtility.SetDirty(t);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        if(GUILayout.Button("Place objects at Path"))
        {
            t.PlaceObjectsAtPath();
            
            EditorUtility.SetDirty(t);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        if(GUILayout.Button("Place cargos"))
        {
            float pathLength = t.GetComponent<PathCreation.PathCreator>().path.length;
            //Debug.Log("PathLength: " + pathLength);
            t.PlaceCargos(pathLength);
            
            EditorUtility.SetDirty(t);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
    }
}
