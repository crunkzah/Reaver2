using UnityEngine;
using TMPro;

public class ItemInfo3dLabel : MonoBehaviour
{
    static ItemInfo3dLabel _instance;
    public static ItemInfo3dLabel singleton{
        get{
            if(_instance == null)
                _instance = FindObjectOfType<ItemInfo3dLabel>();
            return _instance;
        }
    }

    TextMeshPro tmp;
    Renderer rend;


    void Start()
    {
        tmp = GetComponent<TextMeshPro>();
        rend = GetComponent<Renderer>();
    }

    public void Hide()
    {
        rend.enabled = false;
    }

    public void PopUp(Vector3 pos, AmmoType ammoType)
    {
        pos.y += 1.5f;
        this.transform.position = pos;
        this.transform.localRotation = Quaternion.Euler(70f, transform.localEulerAngles.y, transform.localEulerAngles.z);
        
        tmp.text = ammoType.ToString();
        
        rend.enabled = true;
    }

    public void PopUp(Vector3 pos, Weapon weaponInfo)
    {
        
        pos.y += 1.5f;
        this.transform.position = pos;
        
        this.transform.localRotation = Quaternion.Euler(70f, transform.localEulerAngles.y, transform.localEulerAngles.z);
        
        tmp.text = weaponInfo.name;


        rend.enabled = true;
    }
    
    public void PopUpKey(Vector3 pos, EntityType keyType)
    {
        
        pos.y += 1.5f;
        this.transform.position = pos;
        
        this.transform.localRotation = Quaternion.Euler(70f, transform.localEulerAngles.y, transform.localEulerAngles.z);
        
        tmp.text = keyType.ToString();


        rend.enabled = true;
    }



}
