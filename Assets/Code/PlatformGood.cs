using UnityEngine;

public enum PlatformType : int
{
    Ping_pong,
    Loop
}

public class PlatformGood : MonoBehaviour, INetworkObject
{
    NetworkObject net_comp;
    
    public Transform[] waypoints;
    public PlatformType type;
    
    int currentWaypointIndex = -1;
    
    
    public float speed = 4f;
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        
        if(waypoints.Length > 1)
        {
            currentWaypointIndex = 0;
        }
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Ability1):
            {
                
                break;
            }
        }
    }
    
    Vector3 GetPointOnLine(Vector3 start, Vector3 end, float t)
    {
        return Vector3.Lerp(start, end, t);
    }
    
    int NextWaypointIndex()
    {
        if(currentWaypointIndex + 1 >= waypoints.Length)
        {
            return 0;
        }
        else
        {
            return currentWaypointIndex + 1;
        }
    }
    
    void Update()
    {
        if(currentWaypointIndex > -1)
        {
            Vector3 destination = waypoints[currentWaypointIndex].position;
            
            
        }
    }
}
