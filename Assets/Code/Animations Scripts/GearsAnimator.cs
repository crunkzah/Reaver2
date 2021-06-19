using UnityEngine;

public class GearsAnimator : MonoBehaviour
{
    public float baseAngleSpeed = 40f;
    public Transform gearsHolder;
    Transform[] gears;
    
    void Awake()
    {
        if(gearsHolder)
        {
            int gearsNum = gearsHolder.childCount;
            gears = new Transform[gearsNum];
            for(int i = 0; i < gearsNum; i++)
            {
                gears[i] = gearsHolder.GetChild(i);                
            }
        }
    }
    
    static Vector3 rotationAxis = new Vector3(0, 0, 1);
    
    public int countInOneLine = 4;
    public float lineMultiplier = 1.2f;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        float dAngle = baseAngleSpeed * dt;
        Vector3 dRotation = rotationAxis * dAngle;
        
        int num = gears.Length;
        
        // int lineMultiplier = 0;
        
        for(int i = 0; i < num; i++)
        {
            // if(i / countInOneLine)
            // {
                
            // }
            gears[i].Rotate(dRotation, Space.Self);
        }
    }
    
    
    
}
