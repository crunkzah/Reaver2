using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEffectsManager : MonoBehaviour
{
 
    PlatformEffect1[] effects1;
    
    static Vector3 effects1_offset = new Vector3(-2.25f, 0, -2.25f);
    
    void Awake()
    {
        effects1 = FindObjectsOfType<PlatformEffect1>();
        playerMask = LayerMask.GetMask("Player");
    }
    
    static int playerMask;
    
    static Vector3 halfExtents1 = new Vector3(4.5f, 4, 4.5f);
    static Quaternion qZero = Quaternion.identity;
    static Vector3 vForward = new Vector3(0, 0, 1);
    
    void Effects1()
    {
        int len = effects1.Length;

        Vector3 pos;

        for(int i = 0; i < len; i++)
        {
            pos = effects1[i].world_static_pos;
            if(Physics.CheckBox(pos, halfExtents1, qZero, playerMask))
            {
                if(!effects1[i].is_player_standing_on)
                {
                    effects1[i].attached_ps = ParticlesManager.GetNextPooledParticleSystem(ParticleType.platform_effect1).GetComponent<ParticleSystem>();
                    ParticlesManager.PlayPooled(ParticleType.platform_effect1, pos, vForward);
                    
                    effects1[i].is_player_standing_on = true;
                }
            }
            else
            {
                if(effects1[i].is_player_standing_on)
                {
                    if(effects1[i].attached_ps)
                    {
                        effects1[i].attached_ps.Stop();
                        effects1[i].attached_ps = null;
                    }
                    
                    effects1[i].is_player_standing_on = false;
                }
            }
            // if(effects1[i].isActive)
        }
    }
    
    void Update()
    {
        Effects1();
    }

 }
