using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;


public class NPCManager : MonoBehaviour {

    #region Singleton
    static NPCManager _instance;
    public static NPCManager Singleton()
    {
        if (_instance == null)
            _instance = FindObjectOfType<NPCManager>();
        return _instance;
    }
    #endregion

    //public Transform[] aiTargets;
    public List<Transform> aiTargets = new List<Transform>();
    
    
    public List<CoverForNPC> covers = new List<CoverForNPC>(16);
    
    public const int killablesMaxNum = 128;
    //static bool wasKillablesInit = false;
    static int currentKillablesIndex = 0;
    public static List<IKillableThing> killables = new List<IKillableThing>(killablesMaxNum);
    
    void InitKillablesWithNulls()
    {
        killables.Clear();
        for(int i = 0; i < killablesMaxNum; i++)
        {
            killables.Add(null);
        }
        
      //  wasKillablesInit = true;
    }
    
    public static void RegisterKillable(IKillableThing _killable)
    {
        killables[currentKillablesIndex] = _killable;
        currentKillablesIndex++;
        if(currentKillablesIndex >= killablesMaxNum)
        {
            currentKillablesIndex = 0;
        }
    }
    
    public static void UnregisterKillable(IKillableThing _killable)
    {
        killables.Remove(_killable);
    }
    
    public void AddCover(CoverForNPC _cover)
    {
        covers.Add(_cover);
    }
    
    
    public static ref List<Transform> AITargets()
    {
        return ref Singleton().aiTargets;
    }
    
    public bool GetClosestCover(Vector3 hiderPos, Transform target, ref Cover coverSpot, float maxDistance)
    {
        if(target == null)
        {
            return false;
        }
        
        CoverForNPC closestCover = null;
        int len = covers.Count;
        
        float closestDistanceSqr = maxDistance * maxDistance;
        
        for(int i = 0; i < len; i++)
        {
            float distanceSqr = Math.SqrMagnitude(covers[i].worldPos - hiderPos);
            
            if(distanceSqr < closestDistanceSqr && covers[i].HasFreeSpots())
            {
                closestDistanceSqr = distanceSqr;
                closestCover = covers[i];
            }
        }
        
        if(closestCover != null)
        {
            len = closestCover.spots.Length;
            
            float dot = -1;
            
            Vector3 targetToCoverDir = (closestCover.worldPos - target.position);
            
            for(int i = 0; i < len; i++)
            {
                if(closestCover.spots[i].isOccupied)
                    continue;
                
                dot = Vector3.Dot(targetToCoverDir, closestCover.spots[i].spot.transform.forward);
                if(dot > 0)
                {
                    coverSpot = closestCover.spots[i];
                    // InGameConsole.LogFancy("Found <color=yellow>spot</color>!");
                    return true;
                }
            }
        }
        
        // InGameConsole.LogFancy("<color=yellow>Spot</color> was <color=red>not</color> found.");
        return false;
    }
    
    void Update()
    {
        int len = aiTargets.Count;
        for(int i = 0; i < len; i++)
        {
            if(aiTargets[i] == null)
            {
                aiTargets.RemoveAt(i);
                InGameConsole.LogFancy("Removed PlayerController from AI targets list");
            }
        }
    }
        
    void Awake()
    {
        //if(!wasKillablesInit)
        //{
        InitKillablesWithNulls();
        //}
        
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    
    public void UnregisterAiTarget(Transform targetToRemove)
    {
        if(targetToRemove == null)
        {
            return;
        }
        //InGameConsole.LogOrange(string.Format("UnregisterAiTarget({0})", targetToRemove.gameObject.name));
        if(aiTargets != null)
        {
            if(aiTargets.Contains(targetToRemove))
            {
                InGameConsole.LogOrange(string.Format("Removed <color=blue>{0}</color> from AI targets", targetToRemove.gameObject.name));
                aiTargets.Remove(targetToRemove);
            }
            else
            {
                InGameConsole.Log(string.Format("<color=red>NPCManager does not contain <color=blue>{0}</color> in AI targets</color>", targetToRemove.gameObject.name));
            }
        }
    }
    
    public void RegisterAiTarget(Transform targetToAdd)
    {
        InGameConsole.LogOrange("TryingToRegister" + targetToAdd.gameObject.name);
        if(!aiTargets.Contains(targetToAdd))
        {
            aiTargets.Add(targetToAdd);
        }
    }

   

    // void FindAiTargets(out Transform[] targets)
    // {
    //     GameObject[] targetsObj = GameObject.FindGameObjectsWithTag("Player");
    //     targets = new Transform[targetsObj.Length];
    //     for(int i = 0; i < targets.Length; i++)
    //     {
    //         targets[i] = targetsObj[i].transform;
    //         Debug.Log("Found " + targets[i].name + " as target for AI");
    //     }
        
    // }
    
    static Vector3 targetOffsetPos = new Vector3(0f, 0.65f, 0f);
    
    public static void CheckVision(Transform seeker,
                                    Transform target,
                                    LayerMask mask,
                                    out float sqrDistance, out float dotXZ)
    {
        sqrDistance = (target.position - seeker.position).sqrMagnitude;
        
        
        Vector2 dirTowardsTarget = (new Vector2(target.position.x, target.position.z) 
                                  - new Vector2(seeker.position.x, seeker.position.z)).normalized;
        
        dotXZ     = Vector2.Dot(new Vector2(seeker.forward.x, seeker.forward.z),
                                            dirTowardsTarget);
    }
    
    
    public static bool CheckDirectVisionWithOverlapSphere(Vector3 seekerPos, Transform target, LayerMask mask, float maxDistance, float sphereRadius, int sphereMask)
    {
        bool result = false;
        
        if(Math.SqrDistance(target.position, seekerPos) > maxDistance * maxDistance)
        {
            return false;
        }

        Vector3 targetAdjustedPos = target.position + targetOffsetPos;
        Vector3 dir = (targetAdjustedPos - seekerPos);
        dir.Normalize();
        
        
        if(Physics.CheckSphere(seekerPos, sphereRadius, sphereMask))
        {
            return false;
        }
        
        RaycastHit hit;
        Ray ray = new Ray(seekerPos, dir);
        if(Physics.Raycast(ray, out hit, maxDistance, mask))
        {
            if (hit.collider.gameObject.GetInstanceID() == target.gameObject.GetInstanceID())
            {
                result = true;
            }
           
        }

        // float distance = result ? hit.distance : maxDistance;

        return result;
    }
    
    public static bool CheckObscuranceWithOverlapSphere(Vector3 seekerPos, Transform target, LayerMask mask, float maxDistance, float sphereRadius, int sphereMask)
    {
        bool result = false;
        
        if(Math.SqrDistance(target.position, seekerPos) > maxDistance * maxDistance)
        {
            return false;
        }

        Vector3 targetAdjustedPos = target.position + targetOffsetPos;
        Vector3 dir = Math.Normalized(targetAdjustedPos - seekerPos);
        
        if(Physics.CheckSphere(seekerPos, sphereRadius, sphereMask))
        {
            return false;
        }
        
        RaycastHit hit;
        Ray ray = new Ray(seekerPos, dir);
        if(Physics.Raycast(ray, out hit, maxDistance, mask))
        {
            if (hit.collider.gameObject.GetInstanceID() == target.gameObject.GetInstanceID())
            {
                result = true;
            }
        }
        else
        {
            return true;
        }

        // float distance = result ? hit.distance : maxDistance;

        return result;
    }
    

    public static bool CheckDirectVision(Vector3 seekerPos, Transform target, LayerMask mask, float maxDistance)
    {
        bool result = false;
        
        if(Math.SqrDistance(target.position, seekerPos) > maxDistance * maxDistance)
        {
            return false;
        }

        Vector3 targetAdjustedPos = target.position + targetOffsetPos;
        Vector3 dir = (targetAdjustedPos - seekerPos);
        dir.Normalize();
        
        RaycastHit hit;
        Ray ray = new Ray(seekerPos, dir);
        if(Physics.Raycast(ray, out hit, maxDistance, mask))
        {
            if (hit.collider.gameObject.GetInstanceID() == target.gameObject.GetInstanceID())
            {
                result = true;
            }
           
        }

        Color col = result ? Color.red : Color.blue;
        float distance = result ? hit.distance : maxDistance;

        return result;
    }

    public static bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 offset = Random.insideUnitSphere * range;
            Vector3 randomPoint = center + offset;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
    
    static Vector3 V3Up = Vector3.up;
    
    public static bool CheckTargetVisibility(Transform seekerTransform, Transform t,
                                             float sqrFatalVisionRadius, float sqrVisionRadius, 
                                             float cosVisionFov, 
                                             float seekerEyesOffsetY, float targetHeadOffsetY)
    {
        bool Result = false;
        
        float sqrDist = Math.SqrDistance(t.position, seekerTransform.position);
                    
                    
        if(Math.Abs(t.position.y - seekerTransform.position.y) > 4.5f)
        {
            Result = false;
            return Result;
        }
        
        
        if(sqrDist < sqrFatalVisionRadius)
        {
            Result = true;
            return Result;
        }  

        if(sqrDist < sqrVisionRadius)
        {
            Vector3 dirToTarget = (t.position - seekerTransform.position);
            dirToTarget.y = 0f;
            dirToTarget.Normalize();
            
#if UNITY_EDITOR
            Debug.DrawRay(seekerTransform.position, dirToTarget, Color.red);
#endif
            
            float cosAngle = Vector3.Dot(seekerTransform.forward, dirToTarget);
            
            if(cosAngle > cosVisionFov)
            {
                Vector3 rayDir = (t.position + V3Up * targetHeadOffsetY) - (seekerTransform.position + V3Up * seekerEyesOffsetY);
                rayDir.Normalize();
                
                Ray ray = new Ray(seekerTransform.position + V3Up * seekerEyesOffsetY + seekerTransform.forward * 1f, rayDir);
                
                Color col = Color.blue;
                float distanceToTarget = Mathf.Sqrt(sqrDist);
                if(!Physics.Raycast(ray, distanceToTarget, GameSettings.Singleton().enemyVisionMaskWithoutPlayer))
                {
                    col = Color.red;
                    
                    Result = true;
                }
                
#if UNITY_EDITOR                               
                Debug.DrawRay(ray.origin, ray.direction * distanceToTarget, col);
#endif
            }
        }
        
        return Result;
    }

#if UNITY_EDITOR
    BoundingSphere gizmoSphere;
    float gizmoDuration = 4f;
    float gizmoTimeEnd = 0f;
    void OnDrawGizmos()
    {
        if(Time.time < gizmoTimeEnd)
        {
            Gizmos.color = new Color(0f, 65f/255f, 95f/255f);// 0, 65, 95, 0)
            Gizmos.DrawWireSphere(gizmoSphere.position, gizmoSphere.radius);
        }
    }
    
    
#endif

    public void NotifyAi(Vector3 center, float radius)
    {
#if UNITY_EDITOR
        gizmoSphere = new BoundingSphere(center, radius);
        gizmoTimeEnd = Time.time + gizmoDuration;
#endif
        
        if(Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            Collider[] collidersAI = Physics.OverlapSphere(center, radius, GameSettings.Singleton().notifySphereMask);
            for (int i = 0; i < collidersAI.Length; i++)
            {
                
                int netId = collidersAI[i].GetComponent<NetworkObject>().networkId;
                
                NetworkObjectsManager.PackNetworkCommand(netId, NetworkCommand.GetNotified, center);
                
            }
        }
        
        
        
    }

}
