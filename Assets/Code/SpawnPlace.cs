using UnityEngine;

public class SpawnPlace : MonoBehaviour
{
    
    public bool isMainSpawn = true;
    
    
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = isMainSpawn ? Color.green : Color.blue;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.25f, "Spawn", style);
    }
#endif
    
    
}