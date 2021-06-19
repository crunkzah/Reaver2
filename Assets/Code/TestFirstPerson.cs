using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFirstPerson : MonoBehaviour
{
	public float sens = 3;
    public float moveSpeed = 8;
    
    public float horizontalSpeed = 2.0f;
    public float verticalSpeed = 2.0f;
    
    public Vector2 vh = new Vector2(0,0);

	void Update () 
    {
		float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = verticalSpeed * Input.GetAxis("Mouse Y");

        vh.x = h;
        vh.y = v;
        transform.Rotate(v, h, 0);
        
        
        
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if(input.x * input.z != 0f)
        {
            input = Math.Normalized(input);
        }
        float dt  = UberManager.DeltaTime();
        
        Vector3 vel = transform.TransformDirection(input) * moveSpeed * dt;
        
        transform.Translate(vel , Space.World);
	}
}
