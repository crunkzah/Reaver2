using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformAnimator : MonoBehaviour, Interactable
{
    
    public Transform reachWaypoint;
    
    public FloorLeverAnimator leverCallback;
    
    public enum PlatformState
    {
        Idle,
        PositiveWork,
        NegativeWork,
        Waiting
    }
    
    public PlatformState state = PlatformState.Idle;
    
    public float waitingTime = 1.2f;
    
    float waitingTimer = 0f;
    

    public bool isWorking = false;
    public float speed = 2f;
    public float distanceTravelled = 0f;
    
    Vector3[] waypoints;
    int currentIndex = 0;
    
    float distanceBetween = 0f;
    
    
    Vector3 direction;
    
    
    void Start()
    {
        waypoints = new Vector3[2];
        
        waypoints[0] = transform.position;
        waypoints[1] = reachWaypoint.position;
        
        distanceBetween = Vector3.Distance(waypoints[0], waypoints[1]);
        direction = (waypoints[1] - waypoints[0]).normalized;
    }
    
    public void Interact()
    {
        // isWorking = true;
        // IncrementIndex();
        
        switch(state)
        {
            case(PlatformState.Idle):
            {
                state = PlatformState.PositiveWork;   
                break;
            }
            case(PlatformState.PositiveWork):
            {
                state = PlatformState.NegativeWork;
                break;
            }
            case(PlatformState.NegativeWork):
            {
                state = PlatformState.PositiveWork;
                break;
            }
            case(PlatformState.Waiting):
            {
                state = PlatformState.NegativeWork;
                break;
            }
        }
        
    }
    
    void IncrementIndex()
    {
        currentIndex++;
        if(currentIndex > 1)
            currentIndex = 0;
    }
    
    
    //When platform reached destination
    void OnPositiveWorkFinished()
    {
        IncrementIndex();
        isWorking = true;
        
        waitingTimer = 0f;
        state = PlatformState.Waiting;
        
        
        
        
        InGameConsole.Log("<color=blue>OnPositiveWorkFinished()</color>");
    }
    
    //When platform returned to original position
    void OnNegativeWorkFinished()
    {
        isWorking = false;
        
        state = PlatformState.Idle;
        
        InGameConsole.Log("<color=blue>OnNegativeWorkFinished()</color>");
    }
    
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.T))
        // {
        //     Interact();
        // }
        
        
        
        switch(state)
        {
            case(PlatformState.Idle):
            {
                
                break;
            }
            case(PlatformState.PositiveWork):
            {
                float dir = 1f;
                
                distanceTravelled += dir * speed * Time.deltaTime;
            
                if(distanceTravelled > distanceBetween)
                {
                    OnPositiveWorkFinished();
                    distanceTravelled = distanceBetween;
                }
                else
                {
                    transform.Translate(dir * direction * speed * Time.deltaTime);
                }
                
                break;
            }
            case(PlatformState.NegativeWork):
            {
                float dir = -1f;
                
                distanceTravelled += dir * speed * Time.deltaTime;
            
                if(distanceTravelled < 0f)
                {
                    OnNegativeWorkFinished();
                    distanceTravelled = 0f;
                }
                else
                {
                    transform.Translate(dir * direction * speed * Time.deltaTime);
                }
                
                break;
            }
            case(PlatformState.Waiting):
            {
                waitingTimer += Time.deltaTime;
                
                if(waitingTimer > waitingTime)
                {
                    waitingTimer = 0f;
                    
                    // state = PlatformState.NegativeWork;
                    leverCallback.SendMessage("Interact");
                }
                break;
            }
        }
        
        // if(isWorking)
        // {
        //     float dir = (currentIndex == 0) ? -1f : 1f;
            
        //     distanceTravelled += dir * speed * Time.deltaTime;
            
        //     if(distanceTravelled > distanceBetween)
        //     {
        //         OnPositiveWorkFinished();
        //         distanceTravelled = distanceBetween;
        //     }
        //     else
        //     {
        //         if(distanceTravelled < 0f)
        //         {
        //             OnNegativeWorkFinished();
        //             distanceTravelled = 0f;
        //         }
        //         else
        //         {
        //             transform.Translate(dir * direction * speed * Time.deltaTime);
                    
        //         }
        //     }
        // }        
    }
}
