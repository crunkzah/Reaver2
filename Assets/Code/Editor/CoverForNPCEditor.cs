using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CoverForNPC))]
public class CoverForNPCEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var style = new GUIStyle(GUI.skin.button);
        
        style.normal.textColor = Colors.Orange;
        
        if(GUILayout.Button("Assign covers from children", style))
        {
            CoverForNPC cover = (CoverForNPC)target;
            
            int childCount = cover.transform.childCount;
            
            if(childCount == 0)
            {
                return;
            }
            
            cover.spots = new Cover[childCount];
            
            for(int i = 0; i < childCount; i++)
            {
                Transform spot = cover.transform.GetChild(i);
                
                // if(cover.spots[i] == null)
                // {
                //     Debug.Log(string.Format("cover.spots[{0}] is null", i));
                // }
                
                cover.spots[i] = new Cover();              
                            
                cover.spots[i].spot = spot;                
            }                        
            
            EditorUtility.SetDirty(cover);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
