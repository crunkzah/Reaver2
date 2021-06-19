using UnityEngine;
using System.Collections.Generic;

public class RotationManager : MonoBehaviour
{
    
    static RotationManager _instance;
    public static RotationManager Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<RotationManager>();
        }
        
        return _instance;
    }
    
    List<RotationAnimator> rotators = new List<RotationAnimator>(64);
    
    
    public void RegisterRotator(RotationAnimator rotator)
    {
        if(!rotators.Contains(rotator))
            rotators.Add(rotator);
    }
    
    
    // void Update()
    // {
    //     float dt = UberManager.DeltaTime();
    //     int len = rotators.Count;
    //     for(int i = 0; i < len; i++)
    //     {
    //         rotators[i].UpdateMe(dt);
    //     }
    // }
    
    
}
