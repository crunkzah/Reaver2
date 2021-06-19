using UnityEngine;

public class TriggerTrackable : MonoBehaviour
{
    void Start()
    {
        DistanceTrigger.targets.Add(this);
        DistanceTriggerNet.targets.Add(this);
    }    
    
    void OnDestroy()
    {
        DistanceTrigger.targets.Remove(this);
        DistanceTriggerNet.targets.Remove(this);
    }
}
