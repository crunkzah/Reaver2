using UnityEngine;

[System.Serializable]
public class Cover
{
    public bool isOccupied = false;
    public Transform spot;
    public int spot_id = -1;
    public CoverForNPC master_cover = null;
}

public class CoverForNPC : MonoBehaviour
{
    public bool isAvailable = true;
    public Cover[] spots;
    public Vector3 worldPos;
    
    public NetworkObject net_comp;
    
    public bool HasFreeSpots()
    {
        if(!isAvailable)
            return false;
        
        int len = spots.Length;
        
        for(int i = 0; i < len; i++)
        {
            if(!spots[i].isOccupied)
            {
                return true;
            }
        }
        
        return false;
    }
    
    public void OccupieSpot(int spotIndex)
    {
        if(spotIndex >= 0 && spotIndex < spots.Length)
        {
            // if(spots[spotIndex].isOccupied)
            // {
            //     InGameConsole.LogError(string.Format("<color=yellow>OccupieSpot()</color>: Trying to occupie an occupied spot " + 
            //     "on cover with netId <color=blue>{0}</color> on spot with index <color=yellow>{1}</color>", net_comp.networkId, spotIndex));
            // }
            // else
            // {
                // InGameConsole.LogFancy("Success!");
                spots[spotIndex].isOccupied = true;
            // }
        }
        else
        {
            InGameConsole.LogError(string.Format("<color=yellow>OccupieSpot()</color>: invalid spotIndex for" + 
            "cover with netId <color=blue>{0}</color> on spot with index <color=yellow>{1}</color>", net_comp.networkId, spotIndex));
        }
    }
    
    void Awake()
    {
        worldPos = transform.position;
        net_comp = GetComponent<NetworkObject>();
        
        int len = spots.Length;
        for(int i = 0; i < len; i++)
        {
            spots[i].master_cover = this;
            spots[i].spot_id = i;
        }
    }
    
    void Start()
    {
        if(spots.Length == 0)
        {
            InGameConsole.LogWarning(string.Format("Cover <color=green>{0}</color> has <color=yellow>0</color> spots for cover", this.gameObject.name));
            return;
        }
        
        if(NPCManager.Singleton())
        {
            NPCManager.Singleton().AddCover(this);
        }
    }
    
#if UNITY_EDITOR
    //public Mesh arrowMesh_editor;

    void OnDrawGizmos()
    {
        
            
        Gizmos.color = Colors.LightBlue;
        
        if(spots != null && spots.Length > 0)
        {
            int len = spots.Length;
            
            for(int i = 0; i < len; i++)
            {
                if(spots[i].spot)
                {
                    Vector3 pos = spots[i].spot.position;
                    pos.y += 1;
                    // Vector3 dir = cover_spots[i].spot.forward;
                    Quaternion rot = spots[i].spot.rotation;
                    if(spots[i].isOccupied)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Colors.LightBlue;
                    }
                    //Gizmos.DrawMesh(arrowMesh_editor, 0, pos, rot);
                    // Gizmos.DrawIcon(cover_spots[i].spot.position, "Cover");
                }
            }
        }
    }
#endif
}
