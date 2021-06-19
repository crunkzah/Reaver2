using UnityEngine;

public class ParticleTrail : MonoBehaviour
{
    
    public Vector3 prevPos;
    
    float minDistance = 1;
    float sqrMinDistance = 1;
    
    
    
    Transform target;
    Transform thisTransform;
    
    ParticleSystem ps;
    
    int count_per_emit = 1;
    
    void Awake()
    {
        thisTransform = transform;
        ps = GetComponent<ParticleSystem>();
    }
    
    public void SetTrail(Transform _target, float _minDistance, int particles_count)
    {
        ps.Clear();
        
        target = _target;
        
        prevPos = target.position;
        thisTransform.position = prevPos;
        thisTransform.forward = -_target.forward;
        
        count_per_emit = particles_count;
        
        minDistance = _minDistance;
        sqrMinDistance = minDistance * minDistance;
    }
    
    public void Stop()
    {
        ps.Stop(true);
    }
    
    public void Play()
    {
        ps.Play(true);
    }
    
    public void ClearTarget()
    {
        target = null;
    }
    
    void Update()
    {
        if(target == null)
            return;
        
        float dt = UberManager.DeltaTime();
        
        
        if(Math.SqrDistance(prevPos, target.position) > sqrMinDistance)
        {
            thisTransform.localPosition = target.position;
            prevPos = thisTransform.position;
            thisTransform.forward = -target.forward;
            
            
            ps.Emit(count_per_emit);
        }
    }
    
    
    
}
