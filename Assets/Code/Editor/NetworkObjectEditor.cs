#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetworkObjectsManager))]
public class NetworkObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Assign network IDs"))
        {
            NetworkObjectsManager networkObjectsManager = (NetworkObjectsManager)target;
            networkObjectsManager.ResetStaticRegisteredObjects();
          
            NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>();
            for(int i = 0; i < networkObjects.Length; i++)
            {
                //NetworkObjectsManager.RegisterNetObject(networkObjects[i]);
                // 0 - 1024 // [0, 1024]
                NetworkObjectsManager.AssignStaticNetId(networkObjects[i]);
                EditorUtility.SetDirty(networkObjects[i]);
            }
            
            EditorUtility.SetDirty(networkObjectsManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("Registered " + networkObjects.Length + " networkObjects. LastNetId is " + networkObjectsManager.lastStaticNetId);
        }


        if (GUILayout.Button("Reset to -1 network IDs"))
        {
            NetworkObjectsManager networkObjectsManager = (NetworkObjectsManager)target;
            networkObjectsManager.ResetStaticRegisteredObjects();

            NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>();
            
            for (int i = 0; i < networkObjects.Length; i++)
            {
                networkObjects[i].networkId = -1;
                EditorUtility.SetDirty(networkObjects[i]);
            }

            EditorUtility.SetDirty(networkObjectsManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("Reset all network IDs.");
        }
    }
}
#endif