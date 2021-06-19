using UnityEngine;

public class DisableOnPlay : MonoBehaviour
{
    void Start()
    {
        this.gameObject.SetActive(false);
    }
}
