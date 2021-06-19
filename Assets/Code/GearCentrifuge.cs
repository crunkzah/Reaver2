using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

[System.Serializable]
public struct GearAndHolder
{
    public Transform holder;
    public Transform gear;
}

public class GearCentrifuge : MonoBehaviour, Interactable
{
    static readonly Vector3 axis = new Vector3(0, 1, 0);
    
    [Header("Centrifuge:")]
    public float maxSpeedAngle = 90f;
    public float accelerationAngle = 25f;
    float currentSpeedAngle;            
    
    [Header("Gears:")]
    public GearAndHolder[] gears;
    public float gearsAccelerationAngle = 180f;
    public float gearsMaxSpeedAngle = 360f;
    
    float gearsCurrentSpeedAngle;
        
    
    public enum CentrifugeState
    {
        Off,
        TurningOn,
        Working,
        TurningOff,
    }
    
    public CentrifugeState state;
    
    void Update()
    {
        switch(state)
        {
            case CentrifugeState.Off:
            {
                
                
                break;
            }
            case CentrifugeState.TurningOn:
            {
                TurningOn();
                
                break;
            }
            case CentrifugeState.Working:
            {
                DoWork(UberManager.DeltaTime());
                
                break;
            }   
            case CentrifugeState.TurningOff:
            {
                TurningOff();
                
                break;
            }
        }        
        
        if(Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
        
    }
    
    void Awake()
    {
        mask = (int)hitMask;
    }
    
    void Start()
    {
        InitialState();
        
    }
    
    public void Interact()
    {
        switch(state)
        {
            case CentrifugeState.Off:
            {
                state = CentrifugeState.TurningOn;
                OnTurningOnStarted();
                
                break;
            }
            case CentrifugeState.TurningOn:
            {
                state = CentrifugeState.TurningOff;
                OnTurningOffStarted();
                
                break;
            }
            case CentrifugeState.Working:
            {
                state = CentrifugeState.TurningOff;
                OnTurningOffStarted();
                
                break;
            }
            case CentrifugeState.TurningOff:
            {
                state = CentrifugeState.TurningOn;
                OnTurningOnStarted();
                
                break;
            }
        }
    }
    
    void OnTurningOnStarted()
    {
        InGameConsole.LogFancy(string.Format("{0}: OnTurning <color=green>On</color> Started()", this.gameObject.name));
    }
    
    void OnTurnedOn()
    {
        InGameConsole.LogFancy(string.Format("{0}: OnTurned <color=green>On</color> On()", this.gameObject.name));
    }
    
    
    void OnTurningOffStarted()
    {
        InGameConsole.LogFancy(string.Format("{0}: OnTurning <color=red>Off</color> Started()", this.gameObject.name));
    }

    void OnTurnedOff()
    {
        InGameConsole.LogFancy(string.Format("{0}: OnTurned <color=red>Off</color> Off()", this.gameObject.name));
    }
    
    public float rayLength = 1f;
    public LayerMask hitMask;
    int mask;
    float timeWhenCanHit;
    
    static readonly Vector3 gearRayOffset = new Vector3(0, 1f, 0);
    static readonly Vector3 offPosition = new Vector3(0, -3, 0);
    static readonly Vector3 onPosition = new Vector3(0, 0, 0);
    
    const float damageCD = 0.15f;
    
    
    
    
    void TurningOn()
    {
        if(transform.position == onPosition)
        {
            state = CentrifugeState.Working;
            OnTurnedOn();
            
            return;
        }
        
        transform.position = Vector3.MoveTowards(transform.position, onPosition, 2f * UberManager.DeltaTime());
    }
    
    void TurningOff()
    {
        if(transform.position == offPosition)
        {
            state = CentrifugeState.Off;
            OnTurnedOff();
            
            return;
        }
        
        transform.position = Vector3.MoveTowards(transform.position, offPosition, 2f * UberManager.DeltaTime());
        
    }
    
    public void InitialState()
    {
        state = CentrifugeState.Off;
        this.transform.position = offPosition;
        this.transform.rotation = Quaternion.identity;
        
        currentSpeedAngle = 0f;   
        gearsCurrentSpeedAngle = 0f;
    }
            
    void DoWork(float dt)
    {
        float uberTime = UberManager.TimeSinceStart();
        
        //Checking target times:
        {
           
        }
        
        
        //Gears:
        {
            gearsCurrentSpeedAngle += gearsAccelerationAngle * dt;
            if(gearsCurrentSpeedAngle > gearsMaxSpeedAngle)
            {
                gearsCurrentSpeedAngle = gearsMaxSpeedAngle;
            }
            
            int len = gears.Length;
            
            if(uberTime > timeWhenCanHit && gearsCurrentSpeedAngle > 0)
            {
                CheckGearsHit();
            }
            
            for(int i = 0; i < len; i++)
            {
                gears[i].gear.Rotate(axis * gearsCurrentSpeedAngle * dt, Space.Self);
                
            }
        }        
       
        //Centrifuge: 
        {
            currentSpeedAngle += accelerationAngle * dt;
            
            if(currentSpeedAngle > maxSpeedAngle)
                currentSpeedAngle = maxSpeedAngle;
            
            transform.Rotate(axis * currentSpeedAngle * dt, Space.Self);
        }
    }    
    
    
    void CheckGearsHit()
    {
        float uberTime = UberManager.TimeSinceStart();
        RaycastHit hit;
        Ray ray;
        
        int len = gears.Length;
        
        for(int i = 0; i < len; i++)
        {
            // ray = new Ray(gears[i].position + gears[i].InverseTransformPoint(gearRayOffset), gears[i].forward);
            
            Vector3 forwardDir = -gears[i].holder.forward;
            
            ray = new Ray(gears[i].holder.position + gearRayOffset, forwardDir);
            
            Color col = Color.green;
            
            if(Physics.Raycast(ray, out hit, rayLength, mask))
            {
                col = Color.red;
                timeWhenCanHit = uberTime + damageCD;
                
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                if(player)
                {
                    
                    player.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllViaServer, 500);
                    
                    InGameConsole.LogOrange(string.Format("{0} has hit {1}", gears[i].gear.name, hit.collider.name));
                }
                // else
                // {
                //     IDamagableLocal idl = hit.collider.GetComponent<IDamagableLocal>();
                //     if(idl != null)
                //     {
                //         idl.TakeDamageLocally(500, ray.direction, 50, 3);
                //     }
                // }
                
            }
            #if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * rayLength, col, 0f);
            #endif
        }
    }
        
}
