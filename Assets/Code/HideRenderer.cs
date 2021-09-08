using UnityEngine;

public class HideRenderer : MonoBehaviour
{
    static bool invisible = true;
    void OnEnable()
    {
        Renderer rend = GetComponent<Renderer>();
        if(rend)
            GetComponent<Renderer>().enabled = !invisible;
    }
}
