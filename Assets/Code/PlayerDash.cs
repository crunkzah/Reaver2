using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    Transform thisTransform;
    Transform target;
    ParticleSystem ps;
    
    Vector3 offset = new Vector3(0, 0.5f, 0);
    
    void Awake()
    {
        thisTransform = transform;
        ps = GetComponent<ParticleSystem>();
    }
    
    public void SetTarget(Transform _target)
    {
        target = _target;
    }
    
    public void Play()
    {
        //ps.Clear();
        ps.Play();
    }
    
    public void Stop()
    {
        ps.Stop();
    }
    
    void Update()
    {
        if(target)
            thisTransform.localPosition = target.position + offset;
    }
}
