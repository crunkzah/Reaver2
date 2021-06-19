using UnityEngine;


public enum LavaBurstState : int
{
    Dead,
    Rolling
}


public class LavaBurst : MonoBehaviour, IPooledObject//, INetworkObject
{
    // NetworkObject net_comp;
    Transform thisTransform;
    
    
    AudioSource audioSrc;
    public ParticleSystem gfx_ps;
    public ParticleSystem gfx_ps_trail;
    
    LavaBurstState state = LavaBurstState.Dead;
    
    // public void ReceiveCommand(NetworkCommand command, params object[] args)
    // {
    //     switch(command)
    //     {
    //         default:
    //         {
    //             break;
    //         }
    //     }
    // }
    
    public Transform forwardSensor;
    
    CharacterController controller;
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        thisTransform = transform;        
        audioSrc = GetComponent<AudioSource>();
        // net_comp = GetComponent<NetworkObject>();
        
        if(mask == -1)
        {
            mask = LayerMask.GetMask("Ground", "NPC2");
        }
    }
    
    
    public void InitialState()
    {
        
    }
    
    const float gravity_Y = -9.81F * 1.3F;
    
    float timer = 0;
    const float time_to_be_alive = 5F;
    
    bool isMine = false;
    
    Vector3 initVel;
    bool touchedGround = false;
    
    public void Launch(Vector3 pos, Vector3 initialVel, bool _isMine)
    {
        touchedGround = false;
        timer = time_to_be_alive;
        state = LavaBurstState.Rolling;
                
        initVel = initialVel;
        velocity = initVel;
        
        thisTransform.forward = Math.GetXZ(initialVel).normalized;
        
        thisTransform.localPosition = pos;
        
        isMine = _isMine;
        
        flag = false;
        
        gfx_ps.Play();
        gfx_ps.Clear();
        gfx_ps_trail.Play(); 
        
        audioSrc.Play();
    }
    
    const float rollingSpeed = 28f;
    
    bool flag = false;
    
    Vector3 velocity;
    static readonly Vector3 vDown = new Vector3(0, -1, 0);
    
    const float sphereRadius = 0.15F;
    static int mask = -1;
    
    Vector3 rollingRayOffset = new Vector3(0, 0.2f, 0);
    
    const float MIN_GRAVITY = -1;
    
    
    void Update()
    {
        
        switch(state)
        {
            case(LavaBurstState.Dead):
            {
                break;
            }
            case(LavaBurstState.Rolling):
            {
                float dt = UberManager.DeltaTime();
                
                timer -= dt;
                if(timer <= 0)
                {
                    InGameConsole.LogFancy("Time out");
                    SwitchToBlowOff();
                }
                
                RaycastHit hit;
                
                gizmoPos = forwardSensor.position;
                
                if(Physics.SphereCast(forwardSensor.position, 0.5f, Math.Normalized(velocity), out hit, Math.Magnitude(velocity) * dt, mask))
                {
                    InGameConsole.LogFancy("Hit " + hit.collider.name + " with LavaBurst");
                    SwitchToBlowOff();
                }
                
                if(!flag || !controller.isGrounded)
                {
                    
                    velocity.y += gravity_Y * dt;
                }
                else
                {
                    
                    velocity = transform.forward * rollingSpeed;
                                       
                    velocity.y = MIN_GRAVITY;
                }
                
                if(!flag)
                {
                    flag = true;
                }
                
                controller.Move(velocity * dt);
                
                
                break;
            }
        }
    }
    
    void OnTouchGround()
    {
        
    }
    
    Vector3 gizmoPos;
    float gizmoRadius = 0.6f;
    
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawSphere(gizmoPos, gizmoRadius);
    // }
    
    void SwitchToBlowOff()
    {
        state = LavaBurstState.Dead;
        
        audioSrc.Stop();
        
        gfx_ps.Clear();
        gfx_ps.Stop();
        
        gfx_ps_trail.Stop();
        
        velocity = Vector3.zero;
        
        GameObject obj = ObjectPool.s().Get(ObjectPoolKey.Kaboom1, false);
        obj.GetComponent<Kaboom1>().ExplodeDamageHostile(thisTransform.localPosition, 6, 35f, 300);
    }
    
   
}
