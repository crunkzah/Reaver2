using UnityEngine;

public class GearRotationRandomizer : MonoBehaviour
{
    public Transform[] gearsToRandomize;
    public float[] ratios;
    
    public Vector3 axis = new Vector3(0, 0, 1);
    
    void Start()
    {
        int len = gearsToRandomize.Length;
        
        float randomAngle = Random.Range(0f, 360f);
        
        for(int i = 0; i < len; i++)
        {
            gearsToRandomize[i].Rotate(axis * randomAngle * ratios[i], Space.Self);
        }
    }
}
