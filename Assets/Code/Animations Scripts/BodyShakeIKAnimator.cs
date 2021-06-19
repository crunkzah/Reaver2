using UnityEngine;


[System.Serializable]
public struct Bone
{
    public Transform transform;
    [HideInInspector]
    public Quaternion originalLocalRotation;
    [HideInInspector]
    public Quaternion modifiedLocalRotation;
    public Vector3 trauma;
}

public class BodyShakeIKAnimator : MonoBehaviour
{
    public Bone[] bones;
    
    public float springSpeed = 60f;    
    
    public bool workStandalone = true;
    
    public void MakeTrauma(float amountPercent)
    {
        if(bones != null)
        {
            amountPercent = Math.Clamp01(amountPercent);
            
            for(int i = 0; i < bones.Length; i++)
            {
                bones[i].modifiedLocalRotation = Quaternion.Euler(bones[i].trauma * amountPercent) * bones[i].originalLocalRotation;
            }
        }
    }
    
        
    void LateUpdate()
    {
        
        if(workStandalone)
        {
            Evaluate();
        }
        
        
    }
    
    public void Evaluate()
    {
        if(bones != null)
            {
                float dt = UberManager.DeltaTime();
                
                for(int i = 0; i < bones.Length; i++)
                {
                    bones[i].originalLocalRotation = bones[i].transform.localRotation;
                    bones[i].modifiedLocalRotation = Quaternion.RotateTowards(bones[i].modifiedLocalRotation, bones[i].originalLocalRotation, springSpeed * dt);
                    
                    bones[i].transform.localRotation = bones[i].modifiedLocalRotation;
                }
                
                
            }
    }
}
