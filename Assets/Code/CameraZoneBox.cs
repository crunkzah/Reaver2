using UnityEngine;

public class CameraZoneBox : MonoBehaviour
{
    public float targetEulerY = 0f;
    public float shrinkAmountXZ = 6f;
    
#if UNITY_EDITOR
    
    string eulerString;
    float eulerCopy = 0f;

    void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 14;
        style.normal.textColor = Color.red;
        
        if(eulerCopy != targetEulerY)
        {
            eulerCopy = targetEulerY;
            eulerString = string.Format("<b>{0}</b>", eulerCopy.ToString());
        }
        
        if(string.IsNullOrEmpty(eulerString))
        {
            eulerString = eulerCopy.ToString();
        }
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, eulerString, style);
               
    }
#endif
    
}
