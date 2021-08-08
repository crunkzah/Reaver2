using UnityEngine;

public class GroundStomp : MonoBehaviour
{
    ParticleSystem ps1;
    
    
    public AudioSource audio_src;
    
    
    float force;    
    float radius;
    int damage;
    
    const float dmg_height_threshold = 0.25F;
    
    //bool isWorking;
    
    const float time_to_be_alive = 10;
    float lifeTimer = 0;
    
    public void MakeStomp(float _force, int _damage, float _radius)
    {
        force = _force;
        damage = _damage;
        radius = _radius;
        ps1 = GetComponent<ParticleSystem>();
        ps1.Play();
        
        audio_src.Play();
        
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            if(Vector3.Distance(local_pc.GetCenterPosition(), transform.localPosition) < radius)
            {
                local_pc.TakeDamage(damage);
                Vector3 force_dir = (local_pc.GetCenterPosition() - transform.localPosition).normalized;
                force_dir += new Vector3(0, 0.33f, 0);
                Vector3 _f = force_dir.normalized * force;
                local_pc.BoostVelocity(_f);
            }
        }
        
        isWorking = true;
    }
    bool isWorking = false;
    
    void Update()
    {
        if(!isWorking)
            return;
        
        float dt = UberManager.DeltaTime();
        if(lifeTimer < time_to_be_alive)
        {
            lifeTimer += dt;
        }
        else
        {
            isWorking = false;
            Destroy(this.gameObject);
        }
    }
}
