using UnityEngine;
using UnityEngine.AI;

public class NPC_Controller : MonoBehaviour {
    NavMeshAgent agent;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    public bool hasPath = false;
    public LayerMask mask;
    public float stepHeight = 0.75f;

    

    public bool IsDestinationReachable(Vector3 dest)
    {
        bool result = true;
        Vector3 v = dest - (transform.position - Vector3.up * agent.baseOffset);
        print(v);
        if (Mathf.Abs(v.y) > stepHeight)
            result = false;
        
        return result;
    }

    private void Update()
    {
        hasPath = agent.hasPath;
        if(Input.GetMouseButtonDown(0))
        {
           
            Ray ray = FindObjectOfType<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 1000f, mask))
            {
                if (IsDestinationReachable(hit.point))
                {
                    agent.SetDestination(hit.point);
                }
                else
                    Debug.Log("Destination not reachable");
            }
        }
    }


}
