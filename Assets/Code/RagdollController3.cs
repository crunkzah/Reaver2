using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TransformRb
{
    public Transform tr;
    public Rigidbody rb;
    public CharacterJoint joint;
    
    public Vector3 initLocalPosition;
    public Quaternion initLocalRotation;
}

public class RagdollController3 : MonoBehaviour, IPooledObject
{
    public RagdollOwner ragdollMaster;
    
    public TransformRb head;
    public TransformRb pelvis;
    public TransformRb torso;
    
    public TransformRb arm1_l;
    public TransformRb arm2_l;
    
    public TransformRb arm1_r;
    public TransformRb arm2_r;
    
    public TransformRb leg1_l;
    public TransformRb leg2_l;
    
    public TransformRb leg1_r;
    public TransformRb leg2_r;
    
    
    [HideInInspector] public Transform targetHead;
    [HideInInspector] public Transform targetPelvis;
    [HideInInspector] public Transform targetTorso;
    
    [HideInInspector] public Transform targetArm1_l;
    [HideInInspector] public Transform targetArm2_l;
    
    [HideInInspector] public Transform targetArm1_r;
    [HideInInspector] public Transform targetArm2_r;
    
    [HideInInspector] public Transform targetLeg1_l;
    [HideInInspector] public Transform targetLeg2_l;
    
    [HideInInspector] public Transform targetLeg1_r;
    [HideInInspector] public Transform targetLeg2_r;
    
    public void InitialState()
    {
        InitRigidbodies();
        this.gameObject.SetActive(false);
    }
    
    void SynchronizeTransforms(Transform source, ref TransformRb dest)
    {
        //dest.tr.localPosition = dest.initLocalPosition;
        //dest.tr.localRotation = dest.initLocalRotation;
        dest.rb.MovePosition(source.position);// = source.position;
        dest.rb.MoveRotation(source.rotation);// = source.rotation;
    }
    
    bool shouldActivate = false;
    
    void ActivateRigidbodyNextFrame(Rigidbody _rb)
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        
        
        _rb.isKinematic = false;
        _rb.detectCollisions = true;
    }
    
    // void ActivateLimb(ref TransformRb x)
    // {
    //     x.rb.velocity = Vector3.zero;
    //     x.rb.angularVelocity = Vector3.zero;
        
    //     x.rb.isKinematic = false;
    //     x.rb.detectCollisions = true;
        
    //     // x.joint.
    // }
    
    
    
    public void Synchronize()
    {
        SynchronizeTransforms(targetHead, ref head);
        SynchronizeTransforms(targetPelvis, ref pelvis);
        SynchronizeTransforms(targetTorso, ref torso);
        
        SynchronizeTransforms(targetArm1_l, ref arm1_l);
        SynchronizeTransforms(targetArm2_l, ref arm2_l);
        
        SynchronizeTransforms(targetArm1_r, ref arm1_r);
        SynchronizeTransforms(targetArm2_r, ref arm2_r);
        
        SynchronizeTransforms(targetLeg1_l, ref leg1_l);
        SynchronizeTransforms(targetLeg2_l, ref leg2_l);
        
        SynchronizeTransforms(targetLeg1_r, ref leg1_r);
        SynchronizeTransforms(targetLeg2_r, ref leg2_r);
        
        ActivateRigidbodyNextFrame(head.rb);
        ActivateRigidbodyNextFrame(pelvis.rb);
        ActivateRigidbodyNextFrame(torso.rb);
        
        ActivateRigidbodyNextFrame(arm1_l.rb);
        ActivateRigidbodyNextFrame(arm2_l.rb);
        
        ActivateRigidbodyNextFrame(arm1_r.rb);
        ActivateRigidbodyNextFrame(arm2_r.rb);
        
        ActivateRigidbodyNextFrame(leg1_l.rb);
        ActivateRigidbodyNextFrame(leg2_l.rb);
        
        ActivateRigidbodyNextFrame(leg1_r.rb);
        ActivateRigidbodyNextFrame(leg2_r.rb);
        
        timer = 0f;
    }
    
    void InitStruct(ref TransformRb x)
    {
        x.rb = x.tr.GetComponent<Rigidbody>();
        //x.rb.useGravity = false;
        x.initLocalPosition = x.tr.localPosition;
        x.initLocalRotation = x.tr.localRotation;
        
        allRbs.Add(x.rb);
        
        CharacterJoint _joint;
        _joint = x.rb.GetComponent<CharacterJoint>();
        if(_joint != null)
        {
            x.joint = _joint;
        }
    }
    
    void InitRigidbodies()
    {
        InitStruct(ref head);
        InitStruct(ref pelvis);
        InitStruct(ref torso);
        
        InitStruct(ref arm1_l);
        InitStruct(ref arm2_l);
        
        InitStruct(ref arm1_r);
        InitStruct(ref arm2_r);
        
        InitStruct(ref leg1_l);
        InitStruct(ref leg2_l);
        
        InitStruct(ref leg1_r);
        InitStruct(ref leg2_r);
    }
    
    const float timeBeingRagdoll = 1f;
    float timer = 10f;
    
    void Update()
    {
        // if(timer < timeBeingRagdoll)
        // {
        //     float dt = UberManager.DeltaTime();
        //     timer += dt;
        //     if(timer > timeBeingRagdoll)
        //     {
        //         int count = allRbs.Count;
        //         for(int i = 0; i < count; i++)
        //         {
        //             allRbs[i].angularVelocity = Vector3.zero;
        //             allRbs[i].velocity = Vector3.zero;
                    
        //             allRbs[i].isKinematic = true;
        //         }
        //     }
        // }
    }
    
    void DoExplosionNearRagdoll()
    {
        
    }
    
    public void ApplyForceToHead(Vector3 force)
    {
        if(pelvis.rb != null)
        {
            transform.SetPositionAndRotation(ragdollMaster.transform.position, ragdollMaster.transform.rotation);
            
            this.gameObject.SetActive(true);
            Synchronize();
            //Vector3 rand = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
            
            
            head.rb.AddForce(force * 10, ForceMode.Impulse);
            
        }
        else
        {
            InGameConsole.LogError("Head rigidbody is null on " + this.gameObject.name);
        }
    }
    
    List<Rigidbody> allRbs = new List<Rigidbody>();
    
   
   
    
}
