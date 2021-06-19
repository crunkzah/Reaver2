using UnityEngine;
using System.Collections.Generic;

public class BulletsManager : MonoBehaviour
{
    
    static List<BulletController> bullets = new List<BulletController>(256);
    static BulletsManager _instance;
    
    
    public static void RegisterBullet(BulletController bullet)
    {
        bullets.Add(bullet);
    }
    
    
    void Update()
    {
        int len = bullets.Count;
        for(int i = 0; i < len; i++)
        {
            bullets[i].UpdateMe();
        }
    }
    
    
}
