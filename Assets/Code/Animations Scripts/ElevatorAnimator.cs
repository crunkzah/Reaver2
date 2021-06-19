using UnityEngine;
using System.Collections.Generic;



public class ElevatorAnimator : MonoBehaviour, Interactable
{
    public enum ElevatorState {Stationary, Moving, GateOpening, GateClosing}
    public ElevatorState state;
    public float speed = 5f;
    public int currentFloor;
    public List<Transform> destinations;

    [SerializeField] float zeroHeight;

    [Header("Gears settings:")]
    public Transform  mainGear;
    public float mainGearSpeed = 60f;
    public Transform gateGear;
    public float gateGearSpeed  = 60f;

    [Header("Gate settings:")]
    public Transform gate;
    public float timeToOpen = 1f;
    public float openDistance = 4f;
    float gateSpeed;
    public Vector3 localOpenPos, localClosedPos;
    
    [Header("Additional interactables:")]
    public GameObject[] interactables_additional;

    void Start()
    {
        gateSpeed = openDistance / timeToOpen;
        localClosedPos = gate.localPosition - Vector3.right * openDistance;
        localOpenPos = gate.localPosition;
    }

    public void SetFloor(int floor)
    {
        if(state != ElevatorState.Stationary || floor == currentFloor)
            return;
        
        
        floor = Mathf.Clamp(floor, 0, destinations.Count - 1);
        currentFloor = floor;
        state = ElevatorState.GateClosing;
        
        OnGateWorkStarted();
    }
    
    public int floorToSet = 0;

    void Update()
    {
        ProcessState();   
    }

    void ProcessState()
    {
        float dt = UberManager.DeltaTime();
        
        switch(state)
        {
            case ElevatorState.Stationary:
            {
                break;
            }
            case ElevatorState.GateOpening:
            {
                if(gate.localPosition == localOpenPos)
                {
                    OnGateWorkFinished();
                    state = ElevatorState.Stationary;
                    // interactiveByPlayer = true;
                }

                gateGear.Rotate(Vector3.forward * gateGearSpeed * dt, Space.Self);

                gate.localPosition = Vector3.MoveTowards(gate.localPosition, localOpenPos, gateSpeed * dt);
                
                break;
            }
            case ElevatorState.GateClosing:
            {
                if(gate.localPosition == localClosedPos)
                {
                    OnGateWorkFinished();
                    state = ElevatorState.Moving;
                }

                gateGear.Rotate(Vector3.forward * -gateGearSpeed * dt , Space.Self);

                gate.localPosition = Vector3.MoveTowards(gate.localPosition, localClosedPos, gateSpeed * dt);

                break;
            }
            case ElevatorState.Moving:
            {
                if(transform.position.y == destinations[currentFloor].position.y)
                {
                    OnElevatorWorkFinished();
                    state = ElevatorState.GateOpening;
                }



                float yCoord = transform.position.y;
                yCoord = Mathf.MoveTowards(yCoord, destinations[currentFloor].position.y, speed * dt);
                float sign_dir = yCoord > transform.position.y ? -1f : 1f;
                mainGear.Rotate(Vector3.forward * sign_dir *  mainGearSpeed * dt , Space.Self);

                transform.position = new Vector3(transform.position.x, yCoord, transform.position.z);
                break;
            }

        }
    }

    void InteractWithAdditionals()
    {
        if(interactables_additional != null)
        {
            for(int i = 0; i < interactables_additional.Length; i++)
            {
                //interactables_additional[i].GetComponent<Interactable>().Interact();
                interactables_additional[i].SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void OnGateWorkStarted()
    {
        //Play sound of gate starting to work??
        InteractWithAdditionals();
    }


    void OnElevatorWorkFinished()
    {
        //Play the sound(probably)
        InteractWithAdditionals();
    }

    void OnGateWorkFinished()
    {
        //Play sound and some other stuff
        
    }
    
    public void Interact()
    {
        
        Debug.Log("Interacting with elevator");
        
                
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        UnityEditor.Handles.Label(this.transform.position + Vector3.up * 0.5f, this.gameObject.name + "\nFloor: " + currentFloor.ToString() + "\n" + state.ToString(), style);
    }
    #endif



    

}
