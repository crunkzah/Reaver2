using UnityEngine;
using Photon.Pun;

public enum ShootyThingBulletType
{
    Big,
    Medium
}

public class ShootyThing1 : MonoBehaviour, INetworkObject
{
    public ShootyThingBulletType type;
    
    
    public ParticleSystem ps_on_shot;
    
    Transform thisTransform;
    public Transform gunPoint;
    
    NetworkObject net_comp;
    public float projectileSpeed = 20;
    public float shootingRate = 2.5f;
    
    public bool isWorking = false;
    
    public AudioSource audioSrc;
    
    
    void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        net_comp = GetComponent<NetworkObject>();
        thisTransform = transform;
        if(playerGroundMask == -1)
        {
            playerGroundMask = LayerMask.GetMask("Player", "Ground");
        }
    }
    
    
    public void ReceiveCommand(NetworkCommand command,  params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.WakeUp):
            {
                StartWork();
                break;
            }
            case(NetworkCommand.Ability1):
            {
                StopWork();
                break;
            }
            case(NetworkCommand.Shoot):
            {
                Shoot();
                break;
            }
        }
    }
    
    public float projectileRadius = 0.33f;
    static int playerGroundMask = -1;
    
    void Shoot()
    {
        Vector3 pos = gunPoint.position;
        Vector3 dir = gunPoint.forward;
        
        switch(type)
        {
            case(ShootyThingBulletType.Big):
            {
                GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.ShootyThing_bullet1);
                bool isBulletMine = PhotonNetwork.IsMasterClient;
                bullet.GetComponent<BulletController>().LaunchAsSphere(pos, dir, projectileRadius, playerGroundMask, projectileSpeed, 50, isBulletMine);
                
                
                audioSrc.Play();
                break;
            }
            case(ShootyThingBulletType.Medium):
            {
                ps_on_shot.Play();
                GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.ShootyThing_bullet2);
                bool isBulletMine = PhotonNetwork.IsMasterClient;
                bullet.GetComponent<BulletController>().LaunchAsSphere(pos, dir, projectileRadius, playerGroundMask, projectileSpeed, 33, isBulletMine);
                
                audioSrc.Play();
                break;
            }
        }
        //AudioManager.Play3D(SoundType.projectile_launch1, pos, Random.Range(0.6f, 0.7f));
        
    }
    
    
    
    public void StartWork()
    {
        timer = 0 + timeDelay; 
        isWorking = true;
    }
    
    public void StopWork()
    {
        isWorking = false;
    }
    
    public float timeDelay = 1;
    float timer = 0;
    
    void UpdateBrain(float dt)
    {
        timer += dt;
        if(timer > shootingRate)
        {
            timer -= shootingRate;
            if(timer < 0)
            {
                timer = 0;
            }
            
            NetworkObjectsManager.ScheduleCommand(net_comp.networkId, UberManager.GetPhotonTimeDelayedBy(timeDelay + 0.2f), NetworkCommand.Shoot);
        }
    }
    
    void Update()
    {
        if(PhotonNetwork.IsMasterClient)        
        {
            if(isWorking)
            {
                float dt = UberManager.DeltaTime();
                UpdateBrain(dt);
            }
        }
    }
}
