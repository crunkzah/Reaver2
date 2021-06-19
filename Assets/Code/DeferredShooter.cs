using UnityEngine;

public enum DeferredShooterMode : int
{
    Revolver_alt,
}

public class DeferredShooter : MonoBehaviour
{
    static int bulletMask = -1;
    static int hostileMask = -1;
    
    public AudioClip revolver_alt_clip;
    
    AudioSource audioSrc;
    
    int currentMask;
    
    SoundType shoot_clip_type;
    
    public DeferredShooterMode mode;
    
    Transform thisTransform;
    ObjectPoolKey bullet_key;
    
    public float fireRate = 0.15f;
    int shotsFired = 0;
    public int numberOfShots = 0;
    
    bool isMine = false;
    
    
    float bullet_speed = 70f;
    float bullet_radius = 0.5f;
    int bullet_damage = 1;
    float timer = 0;
    
    const float delay = 0.025f;
    
    void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        thisTransform = transform;
        bulletMask = LayerMask.GetMask("Ground", "NPC", "Ceiling");
        hostileMask = LayerMask.GetMask("Ground", "Player", "Ceiling");
    }
    
    
    public void Launch(DeferredShooterMode _mode, Vector3 pos, Vector3 dir, bool _isMine)
    {
        
        shotsFired = 0;
        mode = _mode;
        
        isMine = _isMine;
        
        transform.localPosition = pos;
        thisTransform.forward = dir;
        
        switch(mode)
        {
            case(DeferredShooterMode.Revolver_alt):
            {
                currentMask = bulletMask;
                bullet_key = ObjectPoolKey.Revolver_bullet_alt;
                fireRate = 0.13f;
                numberOfShots = 4;
                bullet_speed = 85;
                bullet_radius = 0.5f;
                bullet_damage = 700;
                shoot_clip_type = SoundType.revolver_alt_defer;
                
                audioSrc.clip = revolver_alt_clip;
                audioSrc.volume = 0.25f;
                audioSrc.pitch = 1.4f;
                
                timer = fireRate;
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    void Update()
    {
        if(shotsFired < numberOfShots)
        {
            float dt = UberManager.DeltaTime();
            timer += dt;
            
            
            if(timer > fireRate)
            {
                timer = 0;
                Shoot();
                shotsFired++;
            }
        }
    }
    
    void Shoot()
    {
        GameObject bullet_obj = ObjectPool.s().Get(bullet_key);
        BulletController bullet = bullet_obj.GetComponent<BulletController>();
        
        bullet.LaunchAsSphere(thisTransform.localPosition, thisTransform.forward, bullet_radius, currentMask, bullet_speed, bullet_damage, isMine);
        //AudioManager.Play3D(shoot_clip_type, thisTransform.localPosition, Random.Range(0.9f, 1.1f), 0.35f, 3);
        
        audioSrc.PlayOneShot(audioSrc.clip);
    }
    
    
}
