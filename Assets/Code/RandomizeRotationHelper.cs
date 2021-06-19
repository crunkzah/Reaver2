#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RandomizeRotationHelper : MonoBehaviour
{
    public Vector3 axis = new Vector3(0, 0, 1);
    
    public Vector2 range = new Vector2(-180f, 180f);
    
    
    
        
    public void RandomizeRotationOfChildren()
    {
        int childCount = transform.childCount;
        
        for(int i = 0; i < childCount; i++)
        {
            transform.GetChild(i).Rotate(axis * Random.Range(range.x, range.y), Space.Self);
        }
        
        
        
    }
    
}
#endif