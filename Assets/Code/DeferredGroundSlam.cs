using UnityEngine;
using Photon.Pun;

public class DeferredGroundSlam : MonoBehaviour
{
    Transform thisTransform;
    
    static int npc2Mask = -1;
    static int slamMask = -1;
    
    bool isMine = false;
    
    void Awake()
    {
        thisTransform = transform;
        
        if(npc2Mask == -1)
            npc2Mask = LayerMask.GetMask("NPC2");
        if(slamMask == -1)
            slamMask = LayerMask.GetMask("NPC");
    }
    
    
    Collider[] slammed_cols_rb = new Collider[64];
    Collider[] slammed_cols_npcs = new Collider[32];
    
    bool isWorking = false;
    float serverTimeStampWhenSlam = 0;
    float force;
    
    public void DoDeferredSlam(Vector3 pos, float timeStampWhenSlam, float _force, bool _isMine)
    {
        serverTimeStampWhenSlam = timeStampWhenSlam;
        
        //InGameConsole.LogFancy("DeferredGroundSlam: delay: " + (timeStampWhenSlam - UberManager.GetPhotonTime()).ToString());
        if(UberManager.GetPhotonTime() - serverTimeStampWhenSlam > 0.250F)
        {
            InGameConsole.LogFancy("DeferredGroundSlam: slam took too long and we discard it due to being late...");
            return;
        }
        
        force = _force;
        thisTransform.localPosition = pos;
        isMine = _isMine;
        
        isWorking = true;
    }
    
    void Update()
    {
        if(isWorking)
        {
            if(serverTimeStampWhenSlam < UberManager.GetPhotonTime())
            {
                DoActualSlam();
                isWorking = false;
            }
        }
    }
    
    const float slamPhysicsForce = 2;
    
    const float box_size_x = 4.5F;
    const float box_size_y = 0.5F;
    const float box_size_z = 4.5F;
    
    void DoActualSlam()
    {
        Vector3 slam_pos = thisTransform.localPosition;
        
        int slammed_rbs_len = Physics.OverlapBoxNonAlloc(slam_pos, new Vector3(box_size_x * 1.25f, box_size_y, box_size_z * 1.25f), slammed_cols_rb, thisTransform.localRotation, slamMask);
            
        for(int  i = 0; i < slammed_rbs_len; i++)
        {
            Rigidbody rb;
            rb = slammed_cols_rb[i].GetComponent<Rigidbody>();
            if(rb && rb.isKinematic == false)
            {
                //InGameConsole.LogOrange("Trying to add force to rigidbody");
                rb.AddForce(Vector3.up * slamPhysicsForce * force, ForceMode.Impulse);
            }
        }
        
        if(isMine)
        {
            int slammed_npcs_len = Physics.OverlapBoxNonAlloc(slam_pos, new Vector3(box_size_x, box_size_y, box_size_z), slammed_cols_npcs, thisTransform.localRotation, npc2Mask);
            
            for(int i = 0; i < slammed_npcs_len; i++)
            {
                if(slammed_cols_npcs[i].transform.localPosition.y > thisTransform.localPosition.y - 0.25f)
                {
                    ILaunchableAirbourne ila = slammed_cols_npcs[i].GetComponent<ILaunchableAirbourne>();
                    if(ila != null && ila.CanBeLaunchedUp())
                    {
                        InGameConsole.LogFancy(string.Format("Trying to launch airbourne {0}", slammed_cols_npcs[i].name));
                        NetworkObject net_comp = slammed_cols_npcs[i].GetComponent<NetworkObject>();
                        Vector3 npcLaunchPos = slammed_cols_npcs[i].transform.localPosition;
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand    .LaunchAirborneUp, npcLaunchPos, force);
                    }
                }
            }
        }
        
        
        
    }
    
}
