using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParticleSystemController))]
public class ParticleSystemControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        ParticleSystemController t = (ParticleSystemController)target;
        
        if(GUILayout.Button("Print current density"))
        {
            Debug.Log("<color=yellow>Print current density</color>");
        }
    }
}
