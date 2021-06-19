using UnityEngine;


public class CrossbowBoltController : MonoBehaviour, IPooledObject
{

    public Bullet bullet;
    // public int ownerPhotonViewId = -1;

    float lifeTimer = 0f;
    public Transform trail_renderer_holder;
    
    Vector3 direction = new Vector3(0, 0, 1);
    int collisionMask;
    
    Transform thisTransform;
    NetObjectOwner netObjectOwner;
    
    float currentSpeed = 1;
    
    ParticleSystem ps;
    
    int hitColliderID = 0;
    
    bool isNailed = false;
    
    public void InitialState()
    {
        this.gameObject.SetActive(true);
        // currentSpeed = bullet.flySpeed;
        lifeTimer = 0f;
        isNailed = false;
        
        if(ps)
            ps.Clear();
    }

    void Awake()
    {
        thisTransform = transform;
        ps = GetComponent<ParticleSystem>();
        netObjectOwner = GetComponent<NetObjectOwner>();
    }
    
    // ParticleTrail pt;
    
    void SetParticleTrail()
    {
        InGameConsole.LogOrange("SetParticleTrail() for bolt");
        ParticlesManager.SetParticleTrail(ParticleType.bolt_pt, transform, 0.6f, 2);
    }
    
    void ClearEffects()
    {
        if(ps)
        {
            ps.Clear();
            ps.Play();
        }
        // if(tr)
        // {
        //     tr.Clear();
        // }
    }
    
    public void Launch(Vector3 pos, Vector3 _direction, int _collisionMask)
    {
        
        this.direction = _direction;
        // this.netObjectOwner.ownerId  = _ownerId;
        this.collisionMask = _collisionMask;
        
        thisTransform.localPosition = pos;
        thisTransform.forward = _direction;
        
        ClearEffects();
        SetParticleTrail();
    }

    void Update()
    {
        if(!isNailed)
        {
            float dt = UberManager.DeltaTime();
            
            RaycastHit hit;
            Ray ray = new Ray(thisTransform.position - direction * 0.35f, direction);
            float distanceThisFrame = currentSpeed * dt;
            
#if UNITY_EDITOR  
            Color rayColor = Color.red;
#endif
            

            if(Physics.Raycast(ray, out hit, distanceThisFrame, collisionMask))
            {
                OnHit(hit.point, -hit.normal, hit.collider);
                
#if UNITY_EDITOR
                rayColor = Color.green;
#endif
            }
            else
            {
                Vector3 newPosition = thisTransform.localPosition + direction * distanceThisFrame;
                transform.localPosition = newPosition;
            }
            
#if false && UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * distanceThisFrame, rayColor, 0f, true);
#endif
            


            CheckLifeTimer(dt);
        }
    }

    void CheckLifeTimer(float dt)
    {
        if(isNailed)
            return;
        
        if(lifeTimer >= GameSettings.bulletLifeTime)
        {
            this.gameObject.SetActive(false);
        }
        lifeTimer += dt;
    }

    void OnHit(Vector3 point, Vector3 direction, Collider collider = null)
    {
        if (collider != null)
        {
            // int colliderInstanceID = collider.GetInstanceID();
            
            // if(colliderInstanceID == hitColliderID)
            // {
            //     return;
            // }
            // else
            //     hitColliderID = collider.GetInstanceID();
            
            
            
            NetworkObject targetNetworkObject = collider.GetComponent<NetworkObject>();
            if(targetNetworkObject != null)
            {
                IDamagableLocal idl = targetNetworkObject.GetComponent<IDamagableLocal>();
                if(idl != null)
                {
                    this.gameObject.SetActive(false);
                    // SpawnAnotherBolt(thisTransform.position, thisTransform.forward);
                    // idl.TakeDamageLocally(bullet.damage, direction, bullet.forceZ, bullet.forceY);
                    // NetworkObjectsManager.PackNetworkCommand(targetNetworkObject.networkId, NetworkCommand.TakeDamage, 
                    //                                         bullet.damage, netObjectOwner.ownerId);
                }
                else
                {
                    //ParticlesManager.PlayPooled(ParticleType.bullet_decal1, point, -direction);
                    GetNailed(point);
                }    
            }
            else
            {
                //ParticlesManager.PlayPooled(ParticleType.bullet_decal1, point, -direction);
                GetNailed(point);
            }
            
        }
        
        // ParticlesManager.Play(2, point, -direction);
        ParticlesManager.PlayPooled(ParticleType.bolt_col, point, -direction);
        
        
        
        
    }
    
    void GetNailed(Vector3 point)
    {
        thisTransform.localPosition = point;
        isNailed = true;
    }
    
    void SpawnAnotherBolt(Vector3 pos, Vector3 dir)
    {
        // GameObject bolt1 = Instantiate(WeaponManager.Singleton().GetWeapon(EntityType.CROSSBOW_ENTITY).bulletPrefab, pos, Quaternion.identity);
        // GameObject bolt2 = Instantiate(WeaponManager.Singleton().GetWeapon(EntityType.CROSSBOW_ENTITY).bulletPrefab, pos, Quaternion.identity);
        
        Vector2 shotDirectionXZZero = new Vector2(dir.x, dir.z);
        pos += new Vector3(shotDirectionXZZero.x, 0f, shotDirectionXZZero.y) * 1.5f;
        
        GameObject bolt1 = ObjectPool.s().Get(ObjectPoolKey.Bolt);
        bolt1.transform.position = pos;
        
        GameObject bolt2 = ObjectPool.s().Get(ObjectPoolKey.Bolt);
        bolt2.transform.position = pos;
        
        bolt1.GetComponent<NetObjectOwner>().ownerId = this.netObjectOwner.ownerId;
        bolt2.GetComponent<NetObjectOwner>().ownerId = this.netObjectOwner.ownerId;
        
        bolt1.GetComponent<CrossbowBoltController>().hitColliderID = hitColliderID;
        bolt2.GetComponent<CrossbowBoltController>().hitColliderID = hitColliderID;
        
                
                
        Vector2 shotDir1 = Math.GetVectorRotated(shotDirectionXZZero, 8f);
        Vector2 shotDir2 = Math.GetVectorRotated(shotDirectionXZZero, -8f);
        
        bolt1.transform.forward = new Vector3(shotDir1.x, 0f, shotDir1.y);
        bolt2.transform.forward = new Vector3(shotDir2.x, 0f, shotDir2.y);
    }

}
