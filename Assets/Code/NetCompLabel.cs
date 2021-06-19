using UnityEngine;
using TMPro;

public enum LabelType
{
    netComp,
    hitpoints
}

public class NetCompLabel : MonoBehaviour
{
    public LabelType type;
    TextMeshPro label;
    
    NetworkObject net_comp;
    IDamagableLocal idl;
    int hitpoints;
    int net_id = -2;
    
    void Awake()
    {
        label = GetComponentInChildren<TextMeshPro>();
        net_comp = transform.parent.GetComponent<NetworkObject>();
        idl = transform.parent.GetComponent<IDamagableLocal>();
    }
    
    
    
    void Update()
    {
        
        switch(type)
        {
            case(LabelType.netComp):
            {
                if(net_comp.networkId != net_id)
                {
                    net_id =net_comp.networkId;
                    label.color = Color.cyan;
                    label.SetText(net_id.ToString());
                }
                break;
            }
            case(LabelType.hitpoints):
            {
                if(idl != null)
                {
                    int _hp = idl.GetCurrentHP();
                    if(_hp != hitpoints)
                    {
                        hitpoints = _hp;
                        label.color = Color.green;
                        label.SetText(idl.GetCurrentHP().ToString());
                    }
                }
                break;
            }
        }
    }
    
}
