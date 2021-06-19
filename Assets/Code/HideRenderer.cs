using UnityEngine;

public class HideRenderer : MonoBehaviour
{
    static bool invisible = true;
    void OnEnable()
    {
        GetComponent<Renderer>().enabled = !invisible;
    }
}
