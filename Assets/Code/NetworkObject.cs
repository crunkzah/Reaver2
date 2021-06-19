using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    public int networkId = -1;

    void OnEnable()
    {
        //Dynamic object case:
        if(networkId < 0)
        {
            //NetworkObjectsManager.RegisterNetObject(this);
            // InGameConsole.LogWarning(string.Format("<color=yellow>Invalid netId on <color=red>{0}</color> Assigning dynamic id:(<color=green>{1}</color>)</color>", this.gameObject.name, networkId));
        }
        else 
        {
            //Static object case:
            if(NetworkObjectsManager.Singleton())
            {
                if(NetworkObjectsManager.Singleton().runtimePool.ContainsKey(networkId))
                {
                    InGameConsole.LogFancy(string.Format("<color=orange>Replaced <color=red>{0}</color> with <color=green>{1}</color> in runtimePool of netObjects!</color>", this.gameObject.name, NetworkObjectsManager.Singleton().runtimePool[networkId].gameObject.name));
                }
                
                
                NetworkObjectsManager.Singleton().runtimePool[networkId] = this;
            }
            
            // if(NetworkObjectsManager.Singleton().runtimePool.ContainsKey(this.networkId) == false)
            // {
            //     // Debug.Log("Registering net object from scene " + this.gameObject.name);
            //     NetworkObjectsManager.RegisterStaticNetObject(this);
            // }
            // else
            // {
            //     NetworkObject another_net_object = NetworkObjectsManager.Singleton().runtimePool[networkId];
            //     InGameConsole.LogError("<color=red>Panic!!!</color>");
                
            //     InGameConsole.LogError("<color=red>NetworkId collision! for <color=blue>" + this.gameObject.name + "</color>"
            //                     + " and <color=orange>" + another_net_object.gameObject.name + "</color> ID: " + networkId + "</color>");
            //     InGameConsole.LogError("<color=red>Panic!!!</color>");
            // }
        }
    }


    void OnDisable()
    {
        NetworkObjectsManager.UnregisterNetObject(this);
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.cyan;
        string label = networkId > 1024 ?  ("Dynamic\n netId " + networkId.ToString()) : ("Staticv\n netId " + networkId.ToString());
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.25f, "netId " + networkId.ToString(), style);
    }
#endif
}
