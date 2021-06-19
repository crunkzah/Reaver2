using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public Vector3 velocity = new Vector3(0, 5, 1);
    
    public GameObject prefab;
    
    public void Test()
    {
        
    }
    
    // void Update()
    // {
    //     if(Input.GetMouseButtonDown(0))
    //     {
    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
    //         RaycastHit hit;
            
    //         if(Physics.Raycast(ray, out hit, 1000f, ~0))
    //         {
    //             GameObject bomb_obj = Instantiate(bomb_prefab);
                
    //             Bomb1 bomb = bomb_obj.GetComponent<Bomb1>();
                
    //             bomb.Launch(hit.point + new Vector3(0, 0.3f, 0), velocity);
                
    //         }
            
    //     }
    // }
}
