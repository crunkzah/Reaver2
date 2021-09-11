using UnityEngine;
using Photon.Pun;

public class AnnihilateWithPlayer : MonoBehaviour
{
    
    Collider col;
    Bounds bounds;
    
    float timer = 0f;
    
    const float cooldown = 0.4f;
    
    void Awake()
    {
        col = GetComponent<Collider>();
        bounds = col.bounds;
        
    }
    
    ParticleSystem ps;
       
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        if(timer > 0)
        {
            timer -= dt;
        }
        
        canAnnihilate = timer <= 0;
        
        if(PhotonManager.Singleton())
        {
            int len = UberManager.Singleton().players.Count;
            
            for(int i = 0; i < len; i++)
            {
                GameObject player = UberManager.Singleton().players[i];
                if(player)
                {
                    if(bounds.Contains(player.transform.position))
                    {
                        if(canAnnihilate)
                        {
                            PlayerController pc = player.GetComponent<PlayerController>();
                            if(pc.isAlive && pc.aliveState != PlayerAliveState.Dashing)
                            {
                                timer = cooldown;
                                
                                ps = ParticlesManager.PlayPooled(ParticleType.annihilate_1_ps, pc.transform.position, transform.parent.forward);
                                ps.Clear();
                                
                                
                                if(pc.pv.IsMine)
                                {
                                    //pc.TakeDamageOnline(50);                            
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    bool canAnnihilate = true;
   
}
