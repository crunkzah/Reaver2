using UnityEngine;

public class PlatformPassiveAnimator01 : MonoBehaviour, ISceneSyncable
{
    
    public Transform[] waypointsTransforms;
    public Vector3[] waypoints;
    
    public float speed = 2.8f;
    
    public bool isWorking = true;
    
    public float distanceTravelled = 0f;
    
    public float distanceBetween = 0f;
    Vector3 direction;
    float dir = 1f;
    
    [Header("Optional Gears:")]
    public Gear[] gears;
    Quaternion[] gearsInitRotations;
    
    
    Transform thisTransform;
    
    
    void Start()
    {
        PhotonManager.Singleton().SubscribeToOnSceneSync(this.gameObject);
        
        InitialInit();        
        
        RevertToInitState();
    }
    
    void InitialInit()
    {
        
        thisTransform = transform;
        waypoints = new Vector3[waypointsTransforms.Length];
        for(int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = waypointsTransforms[i].position;
            waypointsTransforms[i].SetParent(null);
        }
        distanceBetween = Vector3.Distance(waypoints[0], waypoints[1]);
        direction = (waypoints[1] - waypoints[0]).normalized;
        
        if(gears != null)
        {
            gearsInitRotations = new Quaternion[gears.Length];
            
            for(int i = 0; i < gears.Length; i++)
            {
                gearsInitRotations[i] = gears[i].transform.rotation;
            }
        }
        
    }
    
    public void OnSceneSynchronization()
    {
        RevertToInitState();
    }
    
    public void RevertToInitState()
    {
        
        dir = 1f;
        distanceTravelled = 0f;
        
        transform.position = waypoints[0];
        
        if(gears != null)
        {
            for(int i = 0; i < gears.Length; i++)
            {
                gears[i].transform.rotation = gearsInitRotations[i];
            }
        }
    }
    
    void Update()
    {
        if(isWorking)
        {
            distanceTravelled += speed * Time.deltaTime;
            
            if(distanceTravelled > distanceBetween)
            {
                distanceTravelled = 0f;
                dir = -dir;
            }
            else
            {
                thisTransform.Translate(dir * direction * speed * Time.deltaTime);
                
                if(gears != null)
                {
                    for(int i = 0; i < gears.Length; i++)
                    {
                        gears[i].transform.Rotate(gears[i].axis * -dir * gears[i].ratio * Time.deltaTime, Space.Self);
                    }
                }
            }
        }
    }
}
