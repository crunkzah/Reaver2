using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformTest : MonoBehaviour
{
    public List<Vector3> relativeWaypoints;
    List<Vector3> waypoints;
    
    public float speed = 2f;
    
    int currentIndex = 0;
    
    void Start()
    {
        waypoints = new List<Vector3>();
        for(int i = 0; i < relativeWaypoints.Count; i++)
        {
            waypoints.Add(transform.position + relativeWaypoints[i]);
        }
    }
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            currentIndex++;
            if(currentIndex >= waypoints.Count)
            {
                currentIndex = 0;
            }
        }
        
        int nextIndex = currentIndex + 1;
        if(nextIndex >= waypoints.Count)
        {
            nextIndex = 0;
        }
        
        Vector3 direction = (waypoints[nextIndex] - waypoints[currentIndex]).normalized;
        
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
