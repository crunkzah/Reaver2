using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigTrainAnimator : MonoBehaviour, Interactable
{
    public float maxSpeed = 4f;
    public float acceleration = 2.2f;
    public float gearsSpeed = 1f;
    
    float currentSpeed;
    
    [Header("Scenario:")]
    public int nextWaypointIndex = 0;
    public Transform[] reachWaypoints;
    public Vector3[] reachPositions;
    
    public Gear[] gears;
    
    
    void UpdateGears()
    {
        float dt = UberManager.DeltaTime();
        
        for(int i = 0; i < gears.Length; i++)
        {
            gears[i].transform.Rotate(currentSpeed * gearsSpeed * Vector3.right * gears[i].ratio * dt, Space.Self);
        }
    }
    
    
    
    Vector3 direction;
    
    public bool isWorking = false;
    
    
    
    float distanceBetween;
    float distanceTravelled;
    
    
    void Start()
    {
        reachPositions = new Vector3[reachWaypoints.Length];
        for(int i = 0; i < reachWaypoints.Length; i++)
        {
            reachPositions[i] = reachWaypoints[i].position;    
        }
    }
    
    
    [Header("OnWorkFinished:")]
    public BigGates01Animator onWorkFinishedGatesAnimator;
    
    void OnWorkFinished()
    {
        isWorking = false;
        onWorkFinishedGatesAnimator.UnlockGates();
        InGameConsole.LogFancy(string.Format("<color=red>{0}</color> finished work. Next waypoint index: <color=red>{1}</color>", this.gameObject.name, nextWaypointIndex));
        if(nextWaypointIndex + 1 <= reachWaypoints.Length)
        {
            nextWaypointIndex++;
        }
    }
    
    void OnWorkStarted()
    {
        
    }
    
    public void Interact()
    {
        if(nextWaypointIndex == reachPositions.Length)
        {
            return;
        }
        
        if(isWorking == false)
        {
            isWorking = true;
            
            distanceTravelled = 0f;
            distanceBetween = Vector3.Distance(transform.position, reachPositions[nextWaypointIndex]);
            direction = (reachPositions[nextWaypointIndex] - transform.position).normalized;
        }
    }
    
    void Update()
    {
        if(isWorking)
        {
            float dt = UberManager.DeltaTime();
            
            currentSpeed += acceleration * dt;
            
            currentSpeed = (currentSpeed > maxSpeed) ? maxSpeed : currentSpeed;
            
            distanceTravelled += currentSpeed * dt;
            
            if(distanceTravelled > distanceBetween)
            {
                distanceTravelled = distanceBetween;
                
                transform.position = reachPositions[nextWaypointIndex];
                
                OnWorkFinished();
            }
            else
            {
                transform.Translate(direction * currentSpeed * dt, Space.World);
                UpdateGears();
            }
        }
    }
}
