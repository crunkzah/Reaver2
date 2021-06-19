using UnityEngine;

public class RagdollOwner : MonoBehaviour
{
    public ObjectPoolKey ragdollPoolKey = ObjectPoolKey.Chaser2_ragdoll;
    RagdollController3 puppet;
    
    public Transform head;
    public Transform pelvis;
    public Transform torso;
    
    public Transform arm1_l;
    public Transform arm2_l;
    
    public Transform arm1_r;
    public Transform arm2_r;
    
    public Transform leg1_l;
    public Transform leg2_l;
    
    public Transform leg1_r;
    public Transform leg2_r;
    
    bool isInitialized = false;
    
    void FindRagdollFromPool()
    {
        GameObject ragdollObject = ObjectPool2.s().Get(ragdollPoolKey, true);
        RagdollController3 ragdollController = ragdollObject.GetComponent<RagdollController3>();
        if(ragdollController)
        {
            puppet = ragdollController;
            
            InitWithRagdollController(puppet);
        }
    }
    
    public void ApplyForceToHead(Vector3 force)
    {
        if(!isInitialized)
        {
            FindRagdollFromPool();
        }            
            
        puppet.ApplyForceToHead(force);
    }
    
    public void InitWithRagdollController(RagdollController3 ragdoll)
    {
        puppet = ragdoll;
        puppet.ragdollMaster = this;
        
        ragdoll.targetHead = head;
        ragdoll.targetPelvis = pelvis;
        ragdoll.targetTorso = torso;
        
        ragdoll.targetArm1_l = arm1_l;
        ragdoll.targetArm2_l = arm2_l;
        
        ragdoll.targetArm1_r = arm1_r;
        ragdoll.targetArm2_r = arm2_r;
        
        ragdoll.targetLeg1_l = leg1_l;
        ragdoll.targetLeg2_l = leg2_l;
        
        ragdoll.targetLeg1_r = leg1_r;
        ragdoll.targetLeg2_r = leg2_r;
        
        isInitialized = true;
    }
}
