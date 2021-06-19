using UnityEngine;

public class MakeLine : MonoBehaviour
{
    
    LineRenderer lr;
    
    
    int groundMask = -1;
    
    const float groundOffset = 0.1f;
    
    void Start()
    {
        groundMask = LayerMask.GetMask("Ground", "Ceiling");
        
        lr = GetComponent<LineRenderer>();
        int childCount = transform.childCount;
        Vector3[] positions = new Vector3[childCount];
        RaycastHit hit;
        
        for(int i = 0; i < childCount; i++)
        {
            positions[i] = transform.GetChild(i).position;
            if(Physics.Raycast(positions[i] + Vector3.up * 0.33f, Vector3.down, out hit, 3, groundMask))
            {
                positions[i].y = hit.point.y + groundOffset;
            }
        }
        lr.positionCount = childCount;
        lr.SetPositions(positions);
    }
}
