using UnityEngine;

public enum TakeDamageEffect : int
{
    NPC_1,
    player_1
}

public class OnDamageTakenEffects : MonoBehaviour
{
    public TakeDamageEffect mode;
    
    Renderer obj_renderer;
    Material original_material;
    public Material hurt1_material;
    
    public AudioClip clip1;
    AudioSource audioSource;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    public void SetupObject(Renderer rend, Material orig_material)
    {
        original_material = orig_material;
        obj_renderer = rend;    
    }
    
    const float audioTimeCD = 0.1F;
    float audioTimer = 0;
    
    public void OnTakeDamage(Vector3 pos, Vector3 dir)
    {
        //InGameConsole.LogOrange("OnTakeDamage()");
        switch(mode)
        {
            case(TakeDamageEffect.NPC_1):
            {
                effect_NPC_1(pos, dir);
                break;
            }
        }
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        audioTimer -= dt;
        if(audioTimer <= 0)
        {
            audioTimer = 0;
        }
    }
    
    const float effect_npc_1_time = 0.125f;
    
    void effect_NPC_1(Vector3 pos, Vector3 dir)
    {
         if(audioTimer == 0)
         {
            audioTimer += audioTimeCD;
            float pitch = Random.Range(0.95f, 1.05f);
            AudioManager.Play3D(SoundType.hurt_generic1, pos, 1, pitch);
            // audioSource.PlayOneShot(clip1);
         }
         
         FlyingGib gib = ObjectPool.s().Get(ObjectPoolKey.FlyingGib1).GetComponent<FlyingGib>();
         gib.Launch(pos, dir * 35);
    }
    
    void effect_player_1(Vector3 pos, Vector3 dir)
    {
        //isWorking = true;
        //obj_renderer.sharedMaterial = hurt1_material;
        //timer = effect_npc_1_time;
        
        //AudioManager.PlayClip(SoundType.player_take_damage_1, 0.6f, 1f);
        
        ParticlesManager.PlayPooled(ParticleType.annihilate_1_ps, pos, dir);
    }
    
    float timer = 0f;
    bool isWorking = false;
    
    // void Update()
    // {
    //     if(isWorking)
    //     {
    //         float dt = UberManager.DeltaTime();
    //         timer -= dt;
            
    //         if(timer < 0)
    //         {
    //             obj_renderer.sharedMaterial = original_material;
    //             isWorking = false;
    //         }
    //     }
    // }
}
