using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectPool))]
public class ObjectPoolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        ObjectPool t = (ObjectPool)target;
        
        if(GUILayout.Button("Init pool"))
        {
            t.InitPoolOffline();
        }
        
        if(GUILayout.Button("Clear children (clear pool)"))
        {
            int len = t.transform.childCount;
            GameObject objToDelete;
            for(int i = 0; i < len; i++)
            {
                objToDelete = t.transform.GetChild(i).gameObject;
                DestroyImmediate(objToDelete);
            }
            
        }
        
        if(GUILayout.Button("Deactivate pooled objects"))
        {
            int len = t.transform.childCount;
            for(int i = 0; i < len; i++)
            {
                t.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
