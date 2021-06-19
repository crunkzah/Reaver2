using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SinclaireController))]
public class SinclaireEditor : Editor
{
    // public override void OnInspectorGUI()
    // {
    //     base.OnInspectorGUI();
    //     if(GUILayout.Button("Kill all"))
    //     {
    //         SinclaireController[] sinclaires = FindObjectsOfType<SinclaireController>();
    //         for(int i = 0; i < sinclaires.Length; i++)
    //         {
    //             sinclaires[i].Die(Vector3.one);
    //         }
    //     }
    //     if(GUILayout.Button("Freeze all"))
    //     {
    //         SinclaireController[] sinclaires = FindObjectsOfType<SinclaireController>();
    //         for(int i = 0; i < sinclaires.Length; i++)
    //         {
    //             sinclaires[i].DisableSkeleton();
    //         }
    //     }
    // }
}
