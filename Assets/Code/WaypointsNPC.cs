using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsNPC : MonoBehaviour
{
    [HideInInspector] public bool hasWaypoints = false;
    [HideInInspector] public Vector3[] waypoints;
    
    public int waypointsNum = 0;
    
    public void SetWaypoints(ref Vector3[] _waypoints)
    {
        waypointsNum = _waypoints.Length;
        waypoints = new Vector3[waypointsNum];
        hasWaypoints = true;
        
        for(int i = 0; i < waypointsNum; i++)
        {
            // InGameConsole.LogFancy("Index: " + i);
            waypoints[i] = _waypoints[i];
        }
    }
}
