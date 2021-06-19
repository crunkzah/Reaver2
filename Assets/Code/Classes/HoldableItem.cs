using UnityEngine;

public class HoldableItem : MonoBehaviour
{
    public virtual void UseOnMouseDown()
    {
        Debug.Log("UseOnMouseDown " + this.name + " item");
    }

    public virtual void UseOnMouseHeld()
    {
        Debug.Log("UseOnMouseHeld " + this.name + " item");
    }

    public virtual void UseOnMouseUp()
    {
        Debug.Log("UseOnMouseUp " + this.name + " item");
    }

    public virtual void OnItemEquip()
    {
        Debug.Log("OnItemEquip " + this.name + " item");
    }

    public virtual void OnItemUnequip()
    {
        Debug.Log("OnItemUnequip " + this.name + " item");
    }
    
    public virtual void OnReload()
    {
        Debug.Log("OnReload " + this.name + " item");
    }
}
