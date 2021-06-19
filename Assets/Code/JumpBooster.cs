using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public enum BoosterState : int
{
    Ready,
    Working,
    Deescalating,
    Off
}

public class JumpBooster : MonoBehaviour, IActivatable, INetworkObject
{
    NetworkObject net_comp;
    Transform thisTransform;
    AudioSource audioSource;    
    
    
    public Transform active_transform;
    public Transform animated_transform;
     
    public BoosterState state = BoosterState.Ready;
    
    public ParticleSystem ps;
    
    public void ReceiveCommand(NetworkCommand cmd, params object[] args)
    {
        switch(cmd)
        {
            default:
            {
                break;
            }
        }
    }
    
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        rend = GetComponent<MeshRenderer>();
        
        audioSource = GetComponent<AudioSource>();
        
        thisTransform = transform;
        
        readyYLocal = active_transform.localPosition.y;
        apexYLocal = readyYLocal + apexHeight;
        
        bounds = pressureBoxCollider.bounds;
        Destroy(pressureBoxCollider);
    }
    
    public Vector3 boostVelocity = new Vector3(0, 12, 0);
    
    public bool standingOn = false;
    
    float animatedTransformSpeed;
    const float idleAnimatedTransformSpeed = 30F;
    const float idleDecelerationSpeed = 540F;
    public bool inLocalSpace = false;
    
    static readonly Vector3 vUp = new Vector3(0, 1, 0);
    
    void AnimatedTransform()
    {
        float dt = UberManager.DeltaTime();
        
        if(animated_transform)
        {
            animatedTransformSpeed = Mathf.MoveTowards(animatedTransformSpeed, idleAnimatedTransformSpeed, dt * idleDecelerationSpeed);
        }
        
        animated_transform.Rotate(vUp * animatedTransformSpeed * dt);
    }
    
    void Update()
    {
        //if(PhotonNetwork.IsMasterClient && state == BoosterState.Ready)
        {
            bool isStandingOnNow = CheckPressure();
            
            if(isStandingOnNow)
            {
                if(!standingOn)
                {
                    standingOn = true;
                    
                    PlayerController player_on_booster = PhotonManager.Singleton().local_controller;
                    
                    if(player_on_booster && player_on_booster.photonView.IsMine)
                    {
                        // player_on_booster.GetComponent<PhotonView>().RPC("BoostVelocity", RpcTarget.AllViaServer, boostVelocity);
                        if(inLocalSpace)
                        {
                            Vector3 vel_in_localSpace = transform.TransformVector(boostVelocity);
                            player_on_booster.BoostVelocity(vel_in_localSpace);
                        }
                        else
                        {
                            player_on_booster.BoostVelocity(boostVelocity);
                        }
                            
                        
                        NetworkObjectsManager.Activate(net_comp.networkId, RpcTarget.All);
                    
                        // player_on_booster.velocity.y = boostVelocity.y;
                        // player_on_booster.externalVelocity = new Vector3(boostVelocity.x, 0, boostVelocity.z);
                    }
                    
                }
            }
            else
            {
                if(standingOn)
                {
                    standingOn = false;
                    
                    NetworkObjectsManager.Deactivate(net_comp.networkId,RpcTarget.All);
                }                
            }
        }
        
        ProcessState();
        
        AnimatedTransform();
    }
    
    const float apexHeight = 1.65f;
    const float liftSpeed = 13f;
    const float deescAcceleration = -7f;
    
    float deescSpeed = 0f;
    
    float apexYLocal  = 0;
    float readyYLocal = 0;
    
    void SetState(BoosterState _state)
    {
        // InGameConsole.LogOrange(string.Format("Set state {0} on JumpBooster", state));
        
        state = _state;
    }
    
    public float active_y = 0;
    
    void ProcessState()
    {
        active_y = active_transform.localPosition.y;
        
        float dt = UberManager.DeltaTime();
        
        switch(state)
        {
            case BoosterState.Ready:
            {
                break;
            }
            case BoosterState.Working:
            {
                float delta = liftSpeed * dt;
                
                if(active_y + delta > apexYLocal)
                {
                    active_y = apexYLocal;
                    
                    Vector3 liftedPos = active_transform.localPosition;
                    liftedPos.y = active_y;
                    active_transform.localPosition = liftedPos;
                    
                    
                    SetState(BoosterState.Deescalating);
                }
                else
                {
                    active_y += delta;
                    
                    Vector3 liftedPos = active_transform.localPosition;
                    liftedPos.y = active_y;
                    active_transform.localPosition = liftedPos;
                }
                break;
            }
            case BoosterState.Deescalating:
            {
                if(active_transform.localPosition.y != readyYLocal)
                {
                    float currentY = active_transform.localPosition.y;
                    
                    deescSpeed += deescAcceleration * dt;
                    
                    if(deescSpeed < -99f)
                    {
                        deescSpeed = -99f;
                    }
                    
                    float deescalatedY = Mathf.MoveTowards(currentY, readyYLocal, Math.Abs(deescSpeed) * dt);
                    
                    Vector3 deescalatedPos = active_transform.localPosition;
                    deescalatedPos.y = deescalatedY;
                    
                    active_transform.localPosition = deescalatedPos;
                }
                else
                {
                    deescSpeed = 0f;
                    SetState(BoosterState.Ready);
                }
                
                break;
            }
            case BoosterState.Off:
            {
                break;
            }
        }
    }
    
    public Material mat1, mat2;
    MeshRenderer rend;
    
    public void Activate()
    {
        active_transform.localPosition = new Vector3(active_transform.localPosition.x, readyYLocal, active_transform.localPosition.z);
        
        SetState(BoosterState.Working);
        
        OnJumpHappened();
    }
    
    public void Deactivate()
    {
        OnJumpEnded();
    }
    
    void OnJumpHappened()
    {
        animatedTransformSpeed = 720;
        ps.Play();
        audioSource.Play();
        // InGameConsole.Log("OnJumpHappened()");
        
        // rend.sharedMaterial = mat2;
    }
    
    void OnJumpEnded()
    {
        // InGameConsole.Log("OnJumpEnded()");
        
        // rend.sharedMaterial = mat1;
    }
    
    
    public LayerMask mask;
    
    public float radius = 1.55f;
    const float sphereOffset = 1.3f;
    
    public BoxCollider pressureBoxCollider;
    Bounds bounds;
    
    
    bool CheckPressure()
    {
        bool Result = false;
        
        if(PhotonManager.Singleton())
        {
            PlayerController localPlayer = PhotonManager.Singleton().local_controller;
            if(localPlayer)
            {
                Vector3 playerPos = localPlayer.GetGroundPosition();
                if(bounds.Contains(playerPos))
                {
                    Result = true;
                }
            }
        }
        
        // Vector3 pos = thisTransform.position + thisTransform.up * sphereOffset;
        
        // int numStandings = Physics.OverlapSphereNonAlloc(pos, radius, standings, mask);
        
        // if(numStandings > 0)
        // {
        //     Result = true;
        // }
        
        return Result;
    }
    
#if UNITY_EDITOR
    GUIStyle style = new GUIStyle();

    void OnDrawGizmos()
    {
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(transform.position + transform.up * sphereOffset, radius);
        if(Application.isPlaying && transform != null)
        {
            
            // string txt = string.Format("Vel: {0}\n, state: {1}", boostVelocity, state);
            string txt = string.Format("Vel: {0}\n", boostVelocity);
            
            
            style.alignment = TextAnchor.MiddleCenter;
            style.richText = true;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.green;
            
            UnityEditor.Handles.Label(thisTransform.position + 2 * Vector3.up , txt, style);
        }
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(destination, 0.8f);
    }
#endif
}
