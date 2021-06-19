using System.Collections.Generic;
using UnityEngine;

public struct AnimatedTransformFloat
{
    public Transform tr;
    public float offset;
    public float scaleMult;
    public float freq;
}

public struct AnimatedTransformRotation
{
    public Transform tr;
    public float rotationSpeed;
    public Vector3 axis;
}

public class TransformAnimatorManager : MonoBehaviour
{
    static TransformAnimatorManager instance;
    
    public static TransformAnimatorManager Singleton()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<TransformAnimatorManager>();
        }
        
        return instance;
    }    
    
    const float scale = 0.3f;
    // const float freq = 1f;
    List<AnimatedTransformFloat> things_to_float = new List<AnimatedTransformFloat>(128);
    List<AnimatedTransformRotation> things_to_rotate = new List<AnimatedTransformRotation>(64);
    
    public void RegisterTransformFloat(AnimateTransform thing)
    {
        AnimatedTransformFloat at = new AnimatedTransformFloat();
        at.tr = thing.transform;
        at.offset = thing.offset;
        at.scaleMult = thing.scaleMult;
        at.freq = thing.freq;
        
        things_to_float.Add(at);
    }
    
    // public void RegisterTransformRotation(RotationAnimator thing)
    // {
    //     AnimatedTransformRotation at = new AnimatedTransformRotation();
        
    //     at.tr = thing.transform;
    //     at.axis = thing.
    // }
    
   
    
    void Update()
    {
        
        if(things_to_float != null)
        {
            float dt = UberManager.DeltaTime();
            
            Vector3 vUpDelta = new Vector3(0, 0, 0);
            
            float time = Time.time;
            
            int len = things_to_float.Count;
            for(int i = 0; i < len; i++)
            {
                float cos = Mathf.Cos((things_to_float[i].offset + time) * things_to_float[i].freq);
                vUpDelta.y = cos * scale * dt * things_to_float[i].scaleMult;
                things_to_float[i].tr.localPosition += vUpDelta;
            }
        }
    }
}
