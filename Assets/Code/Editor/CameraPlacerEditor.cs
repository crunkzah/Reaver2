#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraPlacer))]
public class CameraPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        
        if(GUILayout.Button("Particles palette"))
        {
            CameraPlacer t = (CameraPlacer)target;
            
            t.prevPos = t.transform.position;
            t.transform.position = new Vector3(-189.9f, 2.52f, 152);
            t.defaultPos = false;
        }
        
        
        if(GUILayout.Button("Default pos"))
        {
            CameraPlacer t = (CameraPlacer)target;
            
            t.prevPos = t.transform.position;
            t.transform.position = new Vector3(-1.9f, 4.4f, 113);
            t.defaultPos = false;
        }
        
        if(GUILayout.Button("Bulding 1"))
        {
            CameraPlacer t = (CameraPlacer)target;
            
            t.prevPos = t.transform.position;
            t.transform.position = new Vector3(-51.36f, 4.4f, 118.9f);
            t.defaultPos = false;
        }
        
        if(GUILayout.Button("Train platform"))
        {
            CameraPlacer t = (CameraPlacer)target;
            
            t.prevPos = t.transform.position;
            t.transform.position = new Vector3(53.6f, 4.4f, 88.28f);
            t.defaultPos = false;
        }
        
        
        if(GUILayout.Button("Building 7"))
        {
            CameraPlacer t = (CameraPlacer)target;
            
            t.prevPos = t.transform.position;
            t.transform.position = new Vector3(53.6f, 4.4f, 19.5f);
            t.defaultPos = false;
        }
        
        if(GUILayout.Button("Housing 1"))
        {
            CameraPlacer t = (CameraPlacer)target;
            
            t.prevPos = t.transform.position;
            t.transform.position = new Vector3(112.5f, 8.3f, 93.83f);
            t.defaultPos = false;
        }
        
    }
}
#endif