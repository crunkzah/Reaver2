using UnityEngine;

public class PlayerBlob : MonoBehaviour
{
    Transform thisTransform;
    Transform target;
    
    readonly static Vector3 vDown = new Vector3(0, -1, 0);
    readonly static Vector3 vUp = new Vector3(0, 1, 0);
    static int groundMask = -1;
    
    void Awake()
    {
        if(groundMask == -1)
        {
            groundMask = LayerMask.GetMask("Ground");
        }
        
        thisTransform = transform;
        thisTransform.localScale = new Vector3(0.66f, 0.66f, 0.66f);
    }
    
    public void SetTarget(Transform t)
    {
        target = t;    
    }
    
    float sqrDistanceToGround = 0f;
    
    void Update()
    {
        if(target)
        {
            RaycastHit hit;
            Ray ray = new Ray(target.position + new Vector3(0, 1.25f, 0), vDown);
            
            if(Physics.Raycast(ray, out hit, 128f, groundMask))
            {
                thisTransform.localPosition = hit.point;
                
                sqrDistanceToGround = Math.SqrDistance(target.position, thisTransform.position);
            }
            else
            {
                thisTransform.localPosition = target.position + new Vector3(0, 10, 0);
            }
            
            float dt = UberManager.DeltaTime();
            float t = Mathf.InverseLerp(0, 16 * 16, sqrDistanceToGround);
            float angleSpeed = Mathf.Lerp(90f, 1440f, t);
            
            transform.Rotate(vUp * dt * angleSpeed);
            
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
}
