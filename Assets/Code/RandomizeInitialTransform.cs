using UnityEngine;

public class RandomizeInitialTransform : MonoBehaviour
{
    [Header("Rotation:")]
    public bool randomRotation = true;
    public Vector3 axis = Vector3.up;    
    
    void Start()
    {
        transform.Rotate(axis * Random.Range(-180, 180), Space.Self);
        Destroy(this);
    }
}
