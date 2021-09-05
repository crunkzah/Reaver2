using UnityEngine;

public class HealthCrystalSmall : MonoBehaviour
{
    const float small_healing_radius = 2.85f;
    const float small_healing_radiusSquared = small_healing_radius * small_healing_radius;
    const int small_healing_amount = 1;
    
    public static void MakeSmallHealing(Vector3 pos, int times)
    {
        if(times <= 0)
            return;
        
        
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            float sqrDist = Math.SqrDistance(local_pc.GetHeadPosition(), pos);
            int healing = small_healing_amount * times;
            bool reachingPlayer = sqrDist < small_healing_radiusSquared;
            
            InGameConsole.LogFancy(string.Format("MakeSmallHealing(): <color=green>{0}</color>, dist: <color={1}>{2}</color>", healing, reachingPlayer ? "green" : "red", Mathf.Sqrt(sqrDist).ToString("f")));
            
            if(sqrDist < small_healing_radiusSquared)
            {
                local_pc.Heal(healing);
            }
        }
    }
    
    
}
