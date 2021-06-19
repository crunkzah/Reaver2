using UnityEngine;

[System.Serializable]
public struct SwingPoint
{
    public Transform tr;
    public bool isWorking;
    public Vector3 vel;
}

public class MeleeSwingController : MonoBehaviour
{
    public SwingPoint[] points;
    Vector3[] pointsPositions;
    public LineRenderer lr;
    
    int resolution = 8;
    
    void Awake()
    {
        pointsPositions = new Vector3[resolution];
        points = new SwingPoint[resolution];
        
        for(int i = 0; i < resolution; i++)
        {
            GameObject point = new GameObject("SwingBladePoint_" + i);
            point.transform.SetParent(this.transform);
            points[i].tr = point.transform;
            points[i].isWorking = false;
            points[i].vel = new Vector3(0, 0, 0);
        }
    }
    
    public void Launch(Vector3 pos, Vector3 dir)
    {
        
    }
    
    
    // void Update()
    // {
    //     poi
        
        
    //     int len = points.Length;
    //     for(int i = 0; i < len; i++)
    //     {
    //         lr.SetPositions()
    //     }
    // }
    
    
    
    
    
    
}
