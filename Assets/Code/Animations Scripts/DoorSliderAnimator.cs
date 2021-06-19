using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DoorSliderAnimator : MonoBehaviour, Interactable
{
    public enum DoorState { Closed, Opening, Open, Closing }
    public Vector3 closedPos, openPos;

    

    public float timeToAnimate = 0.327f;
    float animSpeed = 1f;
    AudioSource source;
    public AudioClip clip;

    const float extencionOfCollider = 0.9f;
    

    public DoorState state          = DoorState.Closed;
    public DoorState expected_state = DoorState.Closed;

    

    //Can it be interacted by player?
    public bool isLocked = false;

    public Transform status_transform;
    GameObject[] statusLights;


    // Note : Assume door is open(animation state) at start.
    void Start()
    {
        closedPos = transform.position;
        openPos = transform.position + transform.forward * GetComponent<BoxCollider>().size.z * extencionOfCollider;
        
        animSpeed = (closedPos - openPos).magnitude / timeToAnimate;

        if(status_transform != null)
        {
            statusLights = new GameObject[status_transform.childCount];
            for(int i = 0; i < status_transform.childCount; i++)
                statusLights[i] = status_transform.GetChild(i).gameObject;

            if(isLocked)
                HandleStatusLights(ref statusLights, 0);
            else
                HandleStatusLights(ref statusLights, 1);
        }
        source = GetComponent<AudioSource>();
    }
    

    


    void OnOpenBegin()
    {
        HandleStatusLights(ref this.statusLights, 2);
        source.PlayOneShot(clip);
        // print("OnOpenBegin()");
    }

    void OnCloseBegin()
    {
        HandleStatusLights(ref this.statusLights, 1);
        source.PlayOneShot(clip);
        // print("OnCloseBegin()");
    }

    void OnCloseEnd()
    {
        HandleStatusLights(ref this.statusLights, 1);
        // print("OnCloseEnd()");
    }

    void OnOpenEnd()
    {
        HandleStatusLights(ref this.statusLights, 2);
        // print("OnOpenEnd()");
    }

    int GetStatusIndex()
    {
        int result = -1;
        
        
        switch(state)
        {
            case DoorState.Closed:
                result = 1;
            break;

            case DoorState.Open:
                result = 2;
            break;

            default:
                result = 0;
            break;
        }
        return result;
    }

    //TODO: This is all ugly
    void HandleStatusLights(ref GameObject[] lights, int index)
    {
        if(lights ==  null)
            return;

        lights[index].SetActive(true);
        for(int i = 0; i < lights.Length; i++)
        {
            if(i == index)
                continue;

            lights[i].SetActive(false);
        }
        // print("HandleStatusLights");
        // if(lights[index].activeSelf == false)
        // {   
        //     lights[index].SetActive(true);
        //     for(int i = 0; i < lights.Length; i++)
        //     {
        //         if(i == index) continue;

        //         lights[index].SetActive(false);
        //     }
        // }
    }

    
    //TODO: Fix this fucking doors
    void HandleState()
    {
        switch(state)
        {
            case DoorState.Closed:

                if(expected_state == DoorState.Open)
                {
                    state = DoorState.Opening;
                    OnOpenBegin();
                }

                break;
            case DoorState.Opening:
                
                // if(transform.position == openPos)
                if (MathHelper.V3Approx(transform.position, openPos))
                {
                    state = DoorState.Open;
                    OnOpenEnd();
                }
                else
                    transform.position = Vector3.MoveTowards(transform.position, openPos, animSpeed * Time.deltaTime);

                break;
            case DoorState.Open:
                if(expected_state == DoorState.Closed)
                {
                    state = DoorState.Closing;
                    OnCloseBegin();
                }

                break;
            case DoorState.Closing:
                // if(transform.position == closedPos)
                if (MathHelper.V3Approx(transform.position, closedPos))
                {
                    state = DoorState.Closed;
                    OnCloseEnd();
                }
                else
                    transform.position = Vector3.MoveTowards(transform.position, closedPos, animSpeed * Time.deltaTime);

                break;    
        }
    }

    public void Interact()
    {
       if(state == DoorState.Closed || state == DoorState.Closing)
       {
           expected_state = DoorState.Open;
           return;
       }

       if(state == DoorState.Open || state == DoorState.Opening)
       {
           expected_state = DoorState.Closed;
           return;
       }
    }

    void Update()
    {
        HandleState();
    }  
}
