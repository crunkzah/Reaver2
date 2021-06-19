using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PosRotScale
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
}

public class RagdollController2 : MonoBehaviour
{
    public Rigidbody[] rbs;
    public Transform[] boneParents;
    PosRotScale[] bonesProperties;
    public bool isRagdollActive = false;
    
    SkinnedMeshRenderer smr;
    
    Animator anim;
    
    public void Init()
    {
        anim = GetComponent<Animator>();
        rbs = GetComponentsInChildren<Rigidbody>();
        
        smr = GetComponentInChildren<SkinnedMeshRenderer>();
        
        // if(smr = null)
        // {
        //     Debug.LogError("Skinned mesh renderer not found!!!");
        // }
        
        
        int len = rbs.Length;
        boneParents = new Transform[len];
        
        bonesProperties = new PosRotScale[len];
        
        for(int i = 0; i < len; i++)
        {
            rbs[i].angularDrag = 2F;
            rbs[i].drag = 1F;
            rbs[i].isKinematic = true;
            rbs[i].detectCollisions = false;
            rbs[i].interpolation = RigidbodyInterpolation.None;
            
            boneParents[i] = rbs[i].transform.parent;    
            
            bonesProperties[i].pos   = rbs[i].transform.localPosition;
            bonesProperties[i].rot   = rbs[i].transform.localRotation;
            bonesProperties[i].scale = rbs[i].transform.localScale;
        }
        
        isRagdollActive = false;
    }
    
    public void SetToKinematic()
    {
        if(rbs == null)
        {
            Debug.LogError("Rbs are not initialized");
            return;
        }
        
        int len = rbs.Length;
        
        for(int i = 0; i < len; i++)
        {
            rbs[i].isKinematic = true;
            rbs[i].detectCollisions = false;
            //Debug.Log(string.Format("Bone <color=green>{0}</color>: property isKinematic is now <color=yellow>{1}</color>", rbs[i].gameObject.name, rbs[i].isKinematic));
        }
        
        //Debug.Log(string.Format("Rigidbodies for {0} are now <color=yellow>kinematic!</color>", this.gameObject.name));
    }
    
    public void DeactivateRagdoll()
    {
        
        if(smr != null)
        {
            smr.updateWhenOffscreen = false;
        }
        for(int i = 0; i < rbs.Length; i++)
        {
            rbs[i].isKinematic = true;
            rbs[i].useGravity = false;
            rbs[i].detectCollisions = false;
            rbs[i].interpolation = RigidbodyInterpolation.None;
            
            rbs[i].transform.SetParent(boneParents[i]);
            
            rbs[i].transform.localPosition = bonesProperties[i].pos;
            rbs[i].transform.localRotation = bonesProperties[i].rot;
            rbs[i].transform.localScale = bonesProperties[i].scale;
        }
        
        isRagdollActive = false;
        
        if(anim != null)
            anim.enabled = true;
    }
    
    
    
    public void ActivateRagdoll()
    {
        if(smr != null)
        {
            smr.updateWhenOffscreen = true;
        }
        
        if(anim != null)
            anim.enabled = false;
        
        for(int i = 0; i < rbs.Length; i++)
        {
            rbs[i].isKinematic = false;
            rbs[i].useGravity = true;
            rbs[i].detectCollisions = true;
            rbs[i].interpolation = RigidbodyInterpolation.Interpolate;
            
            rbs[i].transform.SetParent(null);
        }
        
        isRagdollActive = true;
    }
    
    
    
    public void ApplyForce(Vector3 force, float explosionForce = 4)
    {   
        if(isRagdollActive == false)
        {
            return;
        }
        int len = rbs.Length;
        
        
                
        for(int i = 0; i < len; i++)
        {
            rbs[i].AddTorque(force, ForceMode.Impulse); 
            rbs[i].AddForce(force, ForceMode.Impulse);
            rbs[i].AddExplosionForce(explosionForce, transform.position, 3, 1, ForceMode.Impulse);
        }
    }
    
    void Awake()
    {
        Init();
    }
    
}
