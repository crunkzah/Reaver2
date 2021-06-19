using UnityEngine;

public class GaugeAnimator : MonoBehaviour
{
    [Range(0f, 1f)]
    public float progress = 0f;
    public float maxDegree = 180f;

    [SerializeField] Vector3 minRotation, maxRotation;
    
    void Start()
    {
        minRotation = transform.rotation.eulerAngles;
        transform.Rotate(maxDegree * Vector3.left, Space.Self);
        maxRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(minRotation);
    }

    void Update()
    {
        transform.eulerAngles = Vector3.Slerp(minRotation, maxRotation, progress);
        //transform.Rotate(Vector3.left * 30f * Time.deltaTime, Space.Self);
        //transform.rotation = Quaternion.Euler(Vector3.Slerp(minRotation, maxRotation, progress));
        
    }
}
