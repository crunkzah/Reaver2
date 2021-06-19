using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    Transform thisTransform;
    Transform target;
    
    void Awake()
    {
        thisTransform = transform;
    }
    
    public void SetTarget(Transform _target)
    {
        target = _target;
    }
    
    void Update()
    {
        thisTransform.localPosition = target.position;
    }
    
}
