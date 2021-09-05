using UnityEngine;
using Photon.Pun;

public enum AxeTrapState : byte
{
    Disabled,
    Swinging,
    Enabled
}

public class AxeTrap : MonoBehaviour, INetworkObject
{
    public AxeTrapState state;
    
    Transform thisTransform;
    
    const float apexAngle = 80F;
    const float acceleration = 80F;
    public float speed = 0;
    const float constantSpeed = 420f;
    public float dir = 1;
    
    //RotationAnimator[] rotationAnimators;
    
    
    public float delay = 0f;
    
    
    const int damage = 40;
    const float damageRadius = 1.25F;
    
    NetworkObject net_comp;
    
    bool canSendCommands = true;
    
    void Start()
    {
        thisTransform.localEulerAngles = new Vector3(0, 0, apexAngle);
    }
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            
            case(NetworkCommand.Attack):
            {
                UnlockSendingCommands();
                
                Invoke(nameof(OnAttack), delay);
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    void OnAttack()
    {
        state = AxeTrapState.Swinging;
        didDamageLocally = false;
        speed = 0;
        angle = dir * apexAngle;
        dir = -dir;
        //for(int i = 0; i < rotationAnimators.Length; i++)
            //rotationAnimators[i].enabled = true;
        //audioSrc.PlayOneShot(swingClip, 0.7f);
        audioSrc.PlayOneShot(launchClip);
    }
    
    public AudioSource audioSrc;
    
    
    void Awake()
    {
        thisTransform = transform;
        net_comp = GetComponent<NetworkObject>();
        //rotationAnimators = GetComponentsInChildren<RotationAnimator>();
    }


    const float swingRate = 2.0F;
    public float brainTimer = 0;
    
    public Transform bladeTransform;
    bool didDamageLocally = false;
    // Update is called once per frame
    
    
    
    float angle = 80F;
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(AxeTrapState.Disabled):
            {
                break;
            }
            case(AxeTrapState.Swinging):
            {
                //speed += dir * acceleration * dt;
                // Vector3 localEulers = thisTransform.localEulerAngles;
                
                // localEulers.z += speed * dt;
                // speed = Mathf.MoveTowards(speed, dir * float.MaxValue, acceleration * dt);
                // angle = Mathf.MoveTowards(angle, dir * apexAngle, dt * speed);
                angle = Mathf.MoveTowards(angle, dir * apexAngle, dt * constantSpeed);
                
                
                
                thisTransform.localEulerAngles = new Vector3(0, 0, angle);
                
                //thisTransform.localEulerAngles = Vector3.MoveTowards(thisTransform.localEulerAngles, new Vector3(0, 0, dir * apexAngle), dt * speed);
                
                if(angle * dir > 0)
                {
                    if(Math.Abs(angle) >= Math.Abs(dir * apexAngle * 0.99f))
                    {
                        state = AxeTrapState.Enabled;
                        //for(int i = 0; i < rotationAnimators.Length; i++)
                            //rotationAnimators[i].enabled = false;
                        speed = 0;
                        
                        // if(PhotonNetwork.IsMasterClient && canSendCommands)
                        // {
                        //     LockSendingCommands();
                            
                        // }
                    }
                }
                
                PlayerController local_pc = PhotonManager.GetLocalPlayer();
                
                if(local_pc && !didDamageLocally)
                {
                    Vector3 bladePos = bladeTransform.position;
                    if(Math.SqrDistance(bladePos, local_pc.GetCenterPosition()) < damageRadius * damageRadius)
                    {
                        didDamageLocally = true;
                        local_pc.TakeDamage(damage);
                    }
                    
                }
                
                // if((dir * localEulers.z > 0) && Math.Abs(localEulers.z) > apexAngle)
                // {
                //     localEulers.z = dir * apexAngle;
                //     state = AxeTrapState.Enabled;
                // }
                
                //thisTransform.localEulerAngles = localEulers;
                
                break;
            }
            case(AxeTrapState.Enabled):
            {
                break;
            }
        }    
    }
    
    public AudioClip launchClip;
    //public AudioClip swingClip;
    
    void UpdateBrain(float dt)
    {
        switch(state)
        {
            case(AxeTrapState.Enabled):
            {
                if(canSendCommands)
                {
                    brainTimer += dt;
                    if(brainTimer > swingRate)
                    {
                        brainTimer -= swingRate;
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Attack);
                        //NetworkObjectsManager.ScheduleCommand(net_comp.networkId, UberManager.GetPhotonTimeDelayedBy(delay), NetworkCommand.Attack);
                    }
                }
                break;
            }
            case(AxeTrapState.Swinging):
            {
                break;
            }
            case(AxeTrapState.Disabled):
            {
                break;
            }
        }
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        if(PhotonNetwork.IsMasterClient)
        {
            UpdateBrain(dt);
        }
        
        UpdateBrainLocally(dt);
    }
}
