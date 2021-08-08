using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public class JumpPad : MonoBehaviour, IActivatable
{
    NetworkObject net_comp;
    Transform thisTransform;
    
    public Transform active_transform;
    public Transform[] gears;
    
    float gear_mult = -5.0f;
    
    public BoosterState state = BoosterState.Ready;
    
    Quaternion apexRotation;
    Quaternion readyRotation;
    
    public TrailRenderer[] trails;
    
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        
        thisTransform = transform;
        
        
        readyRotation = active_transform.localRotation;
        apexRotation = readyRotation * Quaternion.Euler(Vector3.right * apexAngleOffset);
        
        readyAngle_x = active_transform.localRotation.eulerAngles.x;
        apexAngle_x = readyAngle_x + apexAngleOffset;
    }
    
    [Header("Boost velocity in local coordinates:")]
    public Vector3 boostVelocityInLocal = new Vector3(0, 8, -8);
    
    public bool standingOn = false;
    
    void Update()
    {
        if(PhotonNetwork.IsMasterClient && state == BoosterState.Ready)
        {
            bool isStandingOnNow = CheckPressure();
            
            if(isStandingOnNow)
            {
                if(!standingOn)
                {
                    standingOn = true;
                    
                    PlayerController player_on_booster = standings[0].GetComponent<PlayerController>();
                    
                    if(player_on_booster && player_on_booster.pv.IsMine && player_on_booster.aliveState != PlayerAliveState.Dashing)
                    {
                        Vector3 global_velocity = thisTransform.TransformVector(boostVelocityInLocal);
                        
                        //player_on_booster.GetComponent<PhotonView>().RPC("BoostVelocity", RpcTarget.AllViaServer, global_velocity);
                        player_on_booster.BoostVelocity(global_velocity);
                        
                        NetworkObjectsManager.Activate(net_comp.networkId);
                    
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
                    
                    NetworkObjectsManager.Deactivate(net_comp.networkId);
                }                
            }
        }
        
        ProcessState();
    }
    
    float apexAngleOffset = -70f;
    float liftSpeedAngle = 360f;
    float deescAccelerationAngle = 62f;
    
    float deescSpeed = 0f;
    
    float apexAngle_x  = 0;
    float readyAngle_x = 0;
    
    void SetState(BoosterState _state)
    {
        // InGameConsole.LogOrange(string.Format("Set state {0} on JumpPad", state));
        
        state = _state;
        
        if(trails == null)
            return;
        
        switch(_state)
        {
            case(BoosterState.Ready):
            {
                for(int i = 0; i < trails.Length; i++)
                {
                    trails[i].emitting = true;
                }
                
                break;
            }
            case(BoosterState.Deescalating):
            {
                for(int i = 0; i < trails.Length; i++)
                {
                    trails[i].emitting = false;
                }
                
                break;
            }
        }
        
    }
    
    void ProcessState()
    {
        
        float dt = UberManager.DeltaTime();
        
        switch(state)
        {
            case BoosterState.Ready:
            {
                break;
            }
            case BoosterState.Working:
            {
                Quaternion currentRotation = active_transform.localRotation;
                
                if(currentRotation != apexRotation)
                {
                    Quaternion liftedRotation = Quaternion.RotateTowards(currentRotation, apexRotation, liftSpeedAngle * dt);
                    
                    active_transform.localRotation = liftedRotation;
                    
                    RotateGears(dt * liftSpeedAngle);
                }
                else
                {
                    SetState(BoosterState.Deescalating);
                }
                
                break;
            }
            case BoosterState.Deescalating:
            {
                Quaternion currentRotation = active_transform.localRotation;
                
                if(currentRotation != readyRotation)
                {
                    deescSpeed += deescAccelerationAngle * dt;

                    Quaternion deescalatedRotation = Quaternion.RotateTowards(currentRotation, readyRotation, deescSpeed * dt);
                    
                    active_transform.localRotation = deescalatedRotation;
                    
                    RotateGears(dt * deescSpeed);
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
    
    static Vector3 vUp = new Vector3(0, 0, 1);
    
    void RotateGears(float dAngle)
    {
        int len = gears.Length;
        
        Vector3 dv = vUp * dAngle * gear_mult;
        
        for(int i = 0; i < len; i++)
        {
            gears[i].Rotate(dv);
        }            
    }
    
    
    public void Activate()
    {
        SetState(BoosterState.Working);
        
        OnJumpHappened();
    }
    
    public void Deactivate()
    {
        OnJumpEnded();
    }
    
    void OnJumpHappened()
    {
        // InGameConsole.Log("OnJumpHappened()");
        
        // rend.sharedMaterial = mat2;
    }
    
    void OnJumpEnded()
    {
        // InGameConsole.Log("OnJumpEnded()");
        
        // rend.sharedMaterial = mat1;
    }
    
    
    public LayerMask mask;
    
    static float radius = 1.2f;
    static Vector3 spherePos = new Vector3(0, 0.5f, 0);
    
    Collider[] standings = new Collider[1];
    
    bool CheckPressure()
    {
        bool Result = false;
        
        Vector3 pos = thisTransform.position + spherePos;
        
        int numStandings = Physics.OverlapSphereNonAlloc(pos, radius, standings, mask);
        
        if(numStandings > 0)
        {
            Result = true;
        }
        
        return Result;
    }
    
#if UNITY_EDITOR
    GUIStyle style = new GUIStyle();

    void OnDrawGizmos()
    {
        if(Application.isPlaying && transform != null)
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(transform.position + spherePos, radius);
            
            string txt = string.Format("Velocity: {0}\n, state: {1}", boostVelocityInLocal, state);
            //string txt = string.Format("Is standing on: {0}", standingOn);
            
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
