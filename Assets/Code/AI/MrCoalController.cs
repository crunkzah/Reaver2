using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum MrCoalState : int
{
    Sleeping,
    Appearing,
    Walking
}

public class MrCoalController : MonoBehaviour, INetworkObject, Interactable
{
    NetworkObject net_comp;
    AudioSource audioSrc;
    NavMeshAgent agent;
    Collider col;
    Transform thisTransform;
    
    
    public SkinnedMeshRenderer meshRenderer;
    public Transform skeleton_root;
    
    
    Animator anim;
    
    public MrCoalState state;
    
    public VerticalGateController gate;
    
    
    public Transform path_holder;
    Vector3[] path;
    int nextPathIndex = 0;
    
    
    int moveSpeedHash;
    
    
    Collider[] limbs_cols;
    DamagableLimb[] limbs;
    
    void Awake()
    {
        thisTransform = transform;
        anim = GetComponent<Animator>();
        net_comp = GetComponent<NetworkObject>();
        audioSrc = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<CapsuleCollider>();
        
        moveSpeedHash = Animator.StringToHash("MoveSpeed");
        
        limbs = GetComponentsInChildren<DamagableLimb>();
        int len = limbs.Length;
        limbs_cols = new Collider[len];
        for(int i = 0; i < len; i++)
        {
            limbs_cols[i] = limbs[i].GetComponent<Collider>();
            limbs_cols[i].enabled = false;
        }
        
        agent.updateUpAxis = false;
    }
    
    bool canSendCommands = true;
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    public void Interact()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if(state == MrCoalState.Sleeping && canSendCommands)
            {
                LockSendingCommands();
                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (int)MrCoalState.Appearing);
            }
        }
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                int arg = (int)args[0];
                
                
                MrCoalState  _state = (MrCoalState)arg;
                
                
                SetState(_state);
                if(_state == MrCoalState.Walking)
                {
                    Vector3 movePos = (Vector3)args[1];
                    MoveAgent(movePos);
                }
                
                break;
            }
            case(NetworkCommand.Move):
            {
                UnlockSendingCommands();
                Vector3 pos = (Vector3)args[0];
                MoveAgent(pos);
                break;
            }
            case(NetworkCommand.Ability1):
            {
                if(gate)
                {
                    gate.Open();
                }
                break;
            }
            case(NetworkCommand.Ability2):
            {
                if(gate)
                {
                    gate.Close();
                }
                break;
            }
            case(NetworkCommand.Ability3):
            {
                Die();
                break;
            }
            case(NetworkCommand.Die):
            {
                Invoke(nameof(Die), 2);
                break;
            }
            case(NetworkCommand.TakeDamage):
            {
                if(agent.speed != 18)
                    agent.speed = 18;
                // if(PhotonNetwork.IsMasterClient)
                // {
                    
                    // if(HitPoints == MaxHealth)
                    // {
                    // }
                    
                // }
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    const int MaxHealth = 500;
    int HitPoints;
    
    void MoveAgent(Vector3 pos)
    {
        agent.SetDestination(pos);
    }
    
    public ParticleSystem ps_explode;
    public ParticleSystem[] lasers_ps;
    //public AudioSource audioSrc_explode;
    
    void OnDie()
    {
        Vector3 pos = thisTransform.position + new Vector3(0, 2, 0) + 1.5f * Math.RandomVector();
        ParticlesManager.PlayPooled(ParticleType.hurt1_ps, thisTransform.position, new Vector3(0, 0, 1));
        ObjectPool.s().Get(ObjectPoolKey.BloodSprayer, false).GetComponent<BloodStainSprayer>().MakeStains(thisTransform.position + new Vector3(0, 1, 0));
    }
    
    void Die()
    {
        ps_explode.transform.SetParent(null);
        ps_explode.Play();
        ps_explode.GetComponent<AudioSource>().Play();
        
        Invoke(nameof(OnDie), 0.2f);
        Invoke(nameof(OnDie), 0.4f);
        Invoke(nameof(OnDie), 0.6f);
        
        if(lasers_ps != null)
        {
            for(int i = 0; i < lasers_ps.Length; i++)
            {
                lasers_ps[i].Play();
            }
        }
        
        meshRenderer.gameObject.SetActive(false);
        skeleton_root.gameObject.SetActive(false);
        col.enabled = false;
        agent.enabled = true;
        Destroy(this.gameObject, 7f);
    }
    
    void SetState(MrCoalState _state)
    {
        state = _state;
        
        switch(_state)
        {
            case(MrCoalState.Appearing):
            {
                audioSrc.Play();
                meshRenderer.gameObject.SetActive(true);
                skeleton_root.gameObject.SetActive(true);
                anim.Play("Base.Appearance", 0, 0);
                break;
            }
            case(MrCoalState.Walking):
            {   
                anim.Play("Base.Moving", 0, 0);
                int len = limbs_cols.Length;
                for(int i = 0; i < len; i++)
                {
                    limbs_cols[i].enabled = true;
                }
                col.enabled = true;
                break;
            }
        }
    }
    
    public void Landed()
    {
        InGameConsole.LogFancy("Landed");      
        ParticlesManager.PlayPooled(ParticleType.groundSlam1_ps, thisTransform.localPosition, new Vector3(0, 0, 1));
        AudioManager.Play3D(SoundType.Explosion_1, thisTransform.localPosition, 1f);
        
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            if(local_pc.IsGrounded())
            {
                local_pc.BoostVelocity(new Vector3(0, 13, 0));
            }
        }
        
    }
    
    public void AppearanceEnded()
    {
//        InGameConsole.LogFancy("AppearanceEnded");
        if(PhotonNetwork.IsMasterClient)
        {
            if(canSendCommands)
            {
                if(path != null && path.Length > 0)
                {
                    Vector3 movePos = path[nextPathIndex];
                    LockSendingCommands();
                    NetworkObjectsManager.ScheduleCommand(net_comp.networkId, UberManager.GetPhotonTimeDelayedBy(0.25f), NetworkCommand.SetState, (int)MrCoalState.Walking, movePos);
                }
            }
        }
    }
    
    void Start()
    {
        HitPoints = MaxHealth;
        col.enabled = false;
        meshRenderer.gameObject.SetActive(false);
        skeleton_root.gameObject.SetActive(false);
        // agent.Sample
        
        if(path_holder)
        {
            int len = path_holder.childCount;
            path = new Vector3[len];
            
            for(int i = 0; i < len; i++)
            {
                path[i] = path_holder.GetChild(i).position;
            }
        }
    }
    
    float brainTimer = 0;
    
    
    
    void UpdateBrain(float dt)
    {
        switch(state)
        {
            case(MrCoalState.Sleeping):
            {
                break;
            }
            case(MrCoalState.Appearing):
            {
                break;
            }
            case(MrCoalState.Walking):
            {
                
                if(agent.enabled && agent.remainingDistance < 0.1F)
                {
                    if(canSendCommands)
                    {
                        if(nextPathIndex < path.Length - 1)
                        {
                            // if(nextPathIndex == path.Length - 3)
                            // {
                            //     NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1);    
                            // }
                            // else
                            // if(nextPathIndex == path.Length - 2)
                            // {
                            //     NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability2);
                            // }
                            
                            nextPathIndex++;
                            Vector3 nextWaypointPos = path[nextPathIndex];
                            
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Move, nextWaypointPos);
                        }
                        else
                        {
                            Vector3 lastWaypointPos = path[nextPathIndex];
                            if(Math.SqrDistance(thisTransform.localPosition, lastWaypointPos) < 0.12F * 0.12F)
                            {
                                LockSendingCommands();
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Die);
                            }
                        }
                    }
                }
                
                break;
            }
        }    
    }
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(MrCoalState.Sleeping):
            {
                break;
            }
            case(MrCoalState.Appearing):
            {
                break;
            }
            case(MrCoalState.Walking):
            {
                float currentSpeed = Math.Magnitude(agent.velocity) / 4.7f;
                anim.SetFloat(moveSpeedHash, currentSpeed, 0.1f, dt);
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
