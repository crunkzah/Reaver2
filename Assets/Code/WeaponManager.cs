using UnityEngine;
using System.Collections.Generic;



[System.Serializable]
public struct WeaponInfo
{
    public EntityType entityType;
    public Weapon weapon;
}

public class WeaponManager : MonoBehaviour
{
    #region Singleton
    static WeaponManager _instance;
    public static WeaponManager Singleton()
    {
        if (_instance == null)
            _instance = FindObjectOfType<WeaponManager>();
            
        return _instance;
    }
    #endregion
    
    [Header("Predefined weapons array:")]
    public WeaponInfo[] weapons;

    [HideInInspector] public List<Transform> presentItemsOnGround = new List<Transform>();
    Dictionary<int, Weapon> all_weapons = new Dictionary<int, Weapon>();

    public void RegisterItemOnGround(Transform newItemOnGround)
    {
        int instance_id = newItemOnGround.GetInstanceID();
        
        
        if(!ContainsItem(instance_id))
        {
            presentItemsOnGround.Add(newItemOnGround);
        }
    }
    
    public void ResetItemsOnGround()
    {
        InGameConsole.LogFancy("<color=green>ResetItemsOnGround()</color>");
        
        int len = presentItemsOnGround.Count;
        
        for(int i = 0; i < len; i++)
        {
            presentItemsOnGround[i].position = new Vector3(2000, 2000, 2000);
        }
    }
    
    
    public bool ContainsItem(int instance_id)
    {
        int len = presentItemsOnGround.Count;
        
        for(int i = 0; i < len; i++)
        {
            int id = presentItemsOnGround[i].GetInstanceID();
            
            if(instance_id == id)
            {
                return true;
            }
        }
        
        return false;
    }

    public void  GetNearestPickUpableWeapons(Vector3 position, float range, ref List<Transform> weapons)
    {
        weapons.Clear();
        
        for(int i = 0; i < presentItemsOnGround.Count; i++)
        {
            if(presentItemsOnGround[i] != null)
            {
                // if(Vector3.SqrMagnitude(position - presentItemsOnGround[i].position) <= range * range)
                if(Math.SqrDistance(position, presentItemsOnGround[i].position) <= range * range)
                    weapons.Add(presentItemsOnGround[i]);
            }
        }

        // return weapons;
    }
    
    Vector3 vUp = new Vector3(0, 1, 0);
    
    bool shouldRotateItemsOnGround = true;
    
    void Update()
    {
        if(shouldRotateItemsOnGround)
        {
            int count = presentItemsOnGround.Count;
            
            float dt = UberManager.DeltaTime();
            
            float rotationAmountDt = 55f * dt;
            
            for(int i = 0; i < count; i++)
            {
                presentItemsOnGround[i].Rotate(vUp, rotationAmountDt);
            }
        }
    }

   

    public void UnregisterItemOnGround(Transform itemOnGround)
    {
        presentItemsOnGround.Remove(itemOnGround);
    }

    private void Awake()
    {
        InitWeaponPool();
    }

    

    public Weapon GetWeapon(EntityType weaponType)
    {
        int weaponKey = (int)weaponType;
        if(all_weapons.ContainsKey(weaponKey) == false)
        {
            return null;
        }

        return all_weapons[weaponKey];
    }


    void InitWeaponPool()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("Predefined weapon array is not set");
            return;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            int weaponKey = (int)weapons[i].entityType;
            all_weapons.Add(weaponKey, weapons[i].weapon);
        }

    }

    
    


}
