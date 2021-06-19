#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetworkObjectsLevelManager))]
public class NetworkObjectsLevelManagerEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if(GUILayout.Button("Assign static network ids"))
        {
            NetworkObject[] net_objs = FindObjectsOfType<NetworkObject>();
            int len = net_objs.Length;
            
            Debug.Log(string.Format("<color=yellow>Assigning <color=green>{0}</color> static networkObjects!</color>", len));
            
            int staticLastNetId = 0;
            for(int i = 0; i < len; i++)
            {
                Debug.Log(string.Format("<color=yellow>Assigned id '<color=green>{0}</color>' for <color=blue>{1}</color></color>", staticLastNetId, net_objs[i].gameObject.name));
                net_objs[i].networkId = staticLastNetId;
                staticLastNetId++;
                
                EditorUtility.SetDirty(net_objs[i]);
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
        }
        
        
    }
}
#endif