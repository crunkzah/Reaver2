using UnityEngine;

public class CameraZonesController : MonoBehaviour
{
    // [System.Serializable]
    public struct CameraZone
    {
        public Bounds positiveBounds;
        public Bounds negativeBounds;
        public float targetEulerY;
    }
    
    CameraZone[] zones;
    int zoneCount;
    
    
    
    public bool containsPositive;
    public bool containsNegative;
    
    void Awake()
    {
        zones = new CameraZone[16];
    }
    
    void Start()
    {
        zoneCount = transform.childCount;
        
        
            
        for(int i = 0; i < zoneCount; i++)
        {
            Transform child = transform.GetChild(i);
            zones[i].positiveBounds = zones[i].negativeBounds = child.GetComponent<BoxCollider>().bounds;
            
            CameraZoneBox box = child.GetComponent<CameraZoneBox>();
            Vector3 shrinkAmountOnAxis = new Vector3(box.shrinkAmountXZ, 0f, box.shrinkAmountXZ);
            
            zones[i].negativeBounds.Expand(shrinkAmountOnAxis);
            
            zones[i].targetEulerY = box.targetEulerY;
            
            Destroy(child.gameObject);
        }
    }
    
    bool TestZone(ref Bounds bounds, Vector3 point)
    {
        return bounds.Contains(point);
    }
    
    public Transform localPlayer;
    
    void Update()
    {
#if true
        containsPositive = false;
        containsNegative = false;
        float eulerY = 0f;
        
        if(localPlayer != null)
        {
            
            for(int i = 0; i < zoneCount; i++)
            {
                if(TestZone(ref zones[i].positiveBounds, localPlayer.position))
                {
                    containsPositive = true;
                    eulerY = zones[i].targetEulerY;
                    break;
                }
            }
            if(!containsPositive)
            {
                for(int i = 0; i < zoneCount; i++)
                {
                    if(TestZone(ref zones[i].negativeBounds, localPlayer.position))
                    {
                        containsNegative = true;
                        break;
                    }
                }
            }
            
            
            if(containsPositive)
            {
                if(FollowingCamera.Singleton().cameraTargetRotationY != eulerY)
                {
                    FollowingCamera.Singleton().SetCameraEulerY(eulerY);
                    InGameConsole.Log(string.Format("<color={0}>Entered Camera Zone !</color>", Colors.OrangeHex));
                }
            }
            else
            {
                if(!containsNegative)
                {
                    if(FollowingCamera.Singleton().cameraTargetRotationY != eulerY)
                    {
                        FollowingCamera.Singleton().SetCameraEulerY(eulerY);
                        InGameConsole.Log(string.Format("<color={0}>Left Camera Zone !</color>", Colors.LightBlueHex));
                    }                    
                }
            }
            
        }
        else
        {
            
            if(PhotonManager.Singleton() && PhotonManager.Singleton().local_player_gameObject != null)
            {
                localPlayer = PhotonManager.Singleton().local_player_gameObject.transform;
            }
        }
#endif
    }

#if UNITY_EDITOR    
    void OnDrawGizmos()
    {
        for(int i = 0; i < zoneCount; i++)
        {
            Gizmos.color = Colors.Orange;
            Gizmos.DrawWireCube(zones[i].positiveBounds.center, zones[i].positiveBounds.size);
            Gizmos.color = Colors.LightBlue;
            Gizmos.DrawWireCube(zones[i].negativeBounds.center, zones[i].negativeBounds.size);
            //zones[i].
        }        
    }
#endif
}
