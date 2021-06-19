using System.Collections.Generic;
using UnityEngine;

public enum AMMO { small, medium, heavy}

public class Inventory : MonoBehaviour {

    public Weapon slot1;
    public Weapon slot2;

    public int smallAmmo = 0;
    public int mediumAmmo = 0;
    public int heavyAmmo = 0;


    public int GetAmmoInfo(AMMO ammo_type)
    {
        int ammoNumber = 0;
        switch(ammo_type)
        {
            case AMMO.small:
                ammoNumber = smallAmmo;
                break;
            case AMMO.medium:
                ammoNumber = mediumAmmo;
                break;
            case AMMO.heavy:
                ammoNumber = heavyAmmo;
                break;

        }
        return ammoNumber;
    }

}
