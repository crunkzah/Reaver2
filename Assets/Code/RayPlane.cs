using UnityEngine;

public class RayPlane : MonoBehaviour {
    public bool turnOffRenderingOnStart = true;

    private void OnEnable()
    {
        if(turnOffRenderingOnStart)
            GetComponent<Renderer>().enabled = false;
    }
}
