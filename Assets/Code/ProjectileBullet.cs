using UnityEngine;
using Photon.Pun;

public class ProjectileBullet : MonoBehaviour
{
    public float radius = 0.25F;
    
    static int player_bullet_mask;
    static int npc_bullet_mask;
    
    float speed;
    Vector3 flyDir;
    float lifeTimer = 0f;
    float timeToBeAlive = 10;
    bool isWorking = false;
    
    Transform thisTransform;
    
    
    
    void Awake()
    {
        thisTransform = transform;
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        Vector3 currentPos = thisTransform.localPosition;
        Vector3 dV = flyDir * speed * dt;
        
            
    }
    
}
