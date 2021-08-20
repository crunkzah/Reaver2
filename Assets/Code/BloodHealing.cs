
using UnityEngine;
using Photon.Pun;

public class BloodHealing : MonoBehaviour
{
    static BloodHealing _instance;
    
    public static BloodHealing Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<BloodHealing>();
        }
        
        return _instance;
    }
    
    const float healingRadius = 1.33f;
    const int healingOnDamage = 25;
    const int healingOnKill = 50;
    
    public static void MakeHealingAt(Vector3 pos)
    {
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            float sqrDistance = Math.SqrDistance(pos, local_pc.GetHeadPosition());
            if(sqrDistance < healingRadius * healingRadius)
            {
                local_pc.Heal(healingOnDamage);
            }
            
        }
    }
}
