using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Bomb1 : MonoBehaviour, IPooledObject
{
    Transform thisTransform;
    
    static int hitMask;
    static int groundMask;
    
    static Vector3[] circle_positions_8;
    
    static void InitCirclePositions(int points, ref Vector3[] positions)
    {
        float step = 360 / points * Mathf.Deg2Rad;
        float angle = 0;
        
        positions = new Vector3[points];
        
        for(int i = 0; i < points; i++)
        {
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            
            positions[i].x = cos;
            positions[i].z = sin;
            
            angle += step;
        }
    }
    
    void Awake()
    {
        if(circle_positions_8 == null)
        {
            InitCirclePositions(8, ref circle_positions_8);
        }
        
        thisTransform = transform;
        
        // hitMask = LayerMask.GetMask("Ground", "Player");
        hitMask = LayerMask.GetMask("Ground");
        groundMask = LayerMask.GetMask("Ground");
    }
    
    
    
    const float radius = 0.45f;
    const float explosionRadius = 1.75f;
    static Vector3 vZero = new Vector3(0, 0, 0);
    const float simulationMultiplier = 2f;
    
    public void InitialState()
    {
        isFlying = false;
        velocity = vZero;
    }
    
    bool isFlying = false;
    Vector3 velocity;
    float lifeTimer = 0f;
    const float lifeTime = 12F;
    int explosionMask = -1;
    
    public void Launch(Vector3 pos, Vector3 velocity, int damageMask)
    {
        explosionMask = damageMask;
        lifeTimer = 0f;
        isFlying = true;
        thisTransform.position = pos;
        this.velocity = velocity;
        this.gameObject.SetActive(true);
        
        SetupEffects();
    }
    
    void SetupEffects()
    {
        ParticlesManager.SetParticleTrail(ParticleType.bomb1_pt, transform, 0.4f, 1);
    }
    
    Collider[] collisions = new Collider[1];
    
    void Update()
    {
        if(isFlying)
        {
            
            float dt = UberManager.DeltaTime() * simulationMultiplier;
            lifeTimer += dt;
            
            if(lifeTimer > lifeTime)
            {
                // InGameConsole.LogOrange("LifeTimer: " + lifeTimer);
                OnHit();                
            
            }
            else
            {
                velocity.y -= 9.81f * dt;
                
                Vector3 updatedPos = thisTransform.position;
                updatedPos += velocity * dt;
                
                int touchingItems = Physics.OverlapSphereNonAlloc(updatedPos, radius, collisions, hitMask);
                
                if(touchingItems > 0)
                {
                    velocity = Vector3.zero;
                    isFlying = false;
                    
                    OnHit();
                }
                else
                {
                    thisTransform.localPosition = updatedPos;
                }
            }
            
        }
    }
    
    Collider[] explosionColliders = new Collider[8];
    
    void DoExplosionDamage(Vector3 pos, float radius, int mask, int baseDamage)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            int count = Physics.OverlapSphereNonAlloc(pos, radius, explosionColliders);
            
            if(count > 0)
            {
                PhotonView playerPv;
                for(int i = 0; i < count; i++)
                {
                    playerPv = explosionColliders[i].GetComponent<PhotonView>();
                    if(playerPv)
                    {
                        playerPv.RPC("TakeDamage", RpcTarget.AllViaServer, baseDamage);
                    }
                }
            }
        }
    }
    
    void OnHit()
    {
        // InGameConsole.LogOrange("OnHit()");
        DoExplosionDamage(thisTransform.position, explosionRadius * 1.2f, explosionMask, 25);
        
        isFlying = false;
        
        Vector3 centrePos = thisTransform.position;
        
        ParticlesManager.Play(ParticleType.bomb1_explosion, centrePos, thisTransform.forward);
        
        int len = circle_positions_8.Length;
        
        Ray ray = new Ray(centrePos, new Vector3(0, -1, 0));
        
        for(int i = 0; i < len; i++)
        {
            Vector3 pos = centrePos + circle_positions_8[i] * explosionRadius;
            
            float y = pos.y;
            pos += Random.insideUnitSphere * 0.65f;
            pos.y = y - 0.15f;
            
            ray.origin = pos - new Vector3(0, 0.2f, 0);
            
            bool hitSomething = Physics.Raycast(ray, 0.3f, groundMask);
            
            // Color col = hitSomething ? Color.red : Color.blue;
            
            // Debug.DrawRay(ray.origin, ray.direction * 0.3f, col, 2f);
            
            if(hitSomething)
            {
                ParticlesManager.Play(ParticleType.bomb1_explosion, pos, thisTransform.forward);
            }
            
        }
        
        FollowingCamera.ShakeY(Random.Range(8, 9));
        float pitch = Random.Range(0.8f, 1.3f);
        AudioManager.PlayClip(SoundType.Explosion_1, 1f, pitch);
        this.gameObject.SetActive(false);
    }
}
