using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestManager))]
public class TestManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if(GUILayout.Button("Test something"))
        {
            
        }
        
    }
}
