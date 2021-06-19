using UnityEngine;

public class BulletControllerHurtless : MonoBehaviour
{
    Transform thisTransform;
    public float speed = 0;
    public float lifeTimer = 0;
    Vector3 direction;
    bool isWorking = false;
    
    
    bool hasEndToSet = false;
    Vector3 endPoint;
    
    
    public float shrinkSpeed = 1;
    
    public float startWidth = 0.3f;
    
    
    // public ParticleSystem psToTrigger;
    
    TrailRenderer tr;
    
    void Awake()
    {
        thisTransform = transform;
        tr = GetComponent<TrailRenderer>();
        
    }
    
    public void Launch(Vector3 pos, Vector3 dir, float _speed, float _time_to_be_alive)
    {
        //this.gameObject.SetActive(true);
        //tr.w startWidth
        tr.widthMultiplier = startWidth;
        thisTransform.position = pos;
        thisTransform.forward = dir;
        if(tr)
        {
            tr.Clear();
        }
        
        
        
        speed = _speed;
        direction = dir;
        
        lifeTimer = _time_to_be_alive;
        isWorking = true;
    }
    public void Launch2(Vector3 start, Vector3 end)
    {
        tr.widthMultiplier = startWidth;
        thisTransform.position = start;
        thisTransform.forward = Math.Normalized(end - start);
        
        endPoint = end;
        
        if(tr)
        {
            tr.Clear();
        }
        
       
        
        hasEndToSet = true;
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        // if(isWorking)
        // {
        //     lifeTimer -= dt;
            
        //     if(lifeTimer <= 0)
        //     {
        //         isWorking = false;
        //     }
        //     else
        //     {
        //         Vector3 updatedPos = thisTransform.localPosition + direction * speed * dt;
        //         thisTransform.localPosition = updatedPos;
        //     }
            
        // }
        
        if(hasEndToSet)
        {
            thisTransform.position = endPoint;
            hasEndToSet = false;
        }
        
        tr.widthMultiplier = Mathf.MoveTowards(tr.widthMultiplier, 0f, shrinkSpeed * dt);
    }
    
}
