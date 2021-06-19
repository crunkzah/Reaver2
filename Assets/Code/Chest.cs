using UnityEngine;
using Photon.Pun;



public enum ChestState : int
{
    Closed,
    Opening,
    Open
}

[System.Serializable]
public struct ChestItem
{
    public ObjectPoolKey pool_key;
    public Vector3 velocity;
    
    public int ammo_num;
}

public class Chest : MonoBehaviour, Interactable
{
    public Transform cap;
    public Transform gear;
    public Transform gear_inner;
    public Transform drop_start_spot;
    
    public ParticleSystem ps;
    
    Quaternion closed_rotation;
    Quaternion open_rotation;    
    
    // Vector3 closed_rotation;
    // Vector3 open_rotation;    
    
    Transform thisTransform;
    
    
    public ChestItem[] items_to_drop;
    
    
    
    // public Transform[] dropPositions;
    
    public ChestState state = ChestState.Closed;
    
    void Awake()
    {
        thisTransform = transform;
        // closed_rotation = cap.localRotation;
        closed_rotation = Quaternion.identity;
        open_rotation = closed_rotation * Quaternion.Euler(100, 0, 0);
        
        
        // closed_rotation = cap.localRotation.eulerAngles;
        // open_rotation = closed_rotation + Vector3.right * 110;
    }
    
    
    public void Interact()
    {
        switch(state)
        {
            case ChestState.Closed:
            {
                state = ChestState.Opening;
                
                OnOpenStarted();
                
                break;
            }
            case ChestState.Opening:
            {
                
                break;
            }
            case ChestState.Open:
            {
                break;
            }
        }
    }
    
    static Vector3 upV = new Vector3(0, 0, 1);
    static Vector3 rightV = new Vector3(1, 0, 0);
    
    bool droppedAllItems = false;
    const float droppingFreq = 0.2f;
    float droppingTimer = 0f;
    int currentItemToDrop = -1;
    
    
    
    void Update()
    {
        switch(state)
        {
            case ChestState.Closed:
            {
                
                
                break;
            }
            case ChestState.Opening:
            {
                float dt = UberManager.DeltaTime();
                
                Vector3 dV = upV * dt * 220;
                
                gear.Rotate(dV, Space.Self);
                gear_inner.Rotate(-dV, Space.Self);
                
                if(cap.localRotation != open_rotation)
                {
                    Quaternion currentRot = cap.localRotation;
                    
                    currentRot = Quaternion.RotateTowards(currentRot, open_rotation, 110f * dt);
                    
                    cap.localRotation = currentRot;
                }
                else
                {
                    state = ChestState.Open;
                    
                    
                    
                    OnOpen();
                }
                
                break;
            }
            case ChestState.Open:
            {
                float dt = UberManager.DeltaTime();
                
                
                gear.Rotate(upV * dt * 60, Space.Self);
                gear_inner.Rotate(upV * dt * -60, Space.Self);
                
                if(PhotonNetwork.IsMasterClient)
                {
                    if(!droppedAllItems)
                    {
                        droppingTimer += dt;
                        int i = currentItemToDrop;
                        
                        if(i == 0 || droppingTimer > droppingFreq)
                        {
                            Vector3 transformed_velocity = transform.TransformDirection(items_to_drop[i].velocity + i * new Vector3(0, 0, 1f));
                    
                            currentItemToDrop++;
                            droppingTimer = 0;
                            if(currentItemToDrop >= items_to_drop.Length)
                            {
                                droppedAllItems = true;
                            }
                            
                                    
                            NetworkObjectsManager.SpawnNewItem(items_to_drop[i].pool_key, drop_start_spot.position, transformed_velocity);
                        }
                        
                    }
                }
                
                break;
            }
        }
    }
    
    void OnOpenStarted()
    {
        // InGameConsole.LogFancy("OnOpenStarted()");
    }
    
    void OnOpen()
    {
        currentItemToDrop = 0;
        
        ps.Play();        
        //Sounds and animation here:
        // InGameConsole.LogFancy("OnOpen()");
    }
    
}
