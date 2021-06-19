﻿using UnityEngine;

public class SinclaireSword : MonoBehaviour
{
    bool isWielded = true;
    
    public Rigidbody rb;
    public Collider col;
    public TrailRendererController trail_controller;
    
    void Awake()
    {
        col.enabled = false;
    }
    
    public void OnSwing()
    {
        trail_controller.EmitFor(0.65f);
    }
    
    public void Drop()
    {
        transform.SetParent(null, true);
        isWielded = false;
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 8;
        rb.centerOfMass = new Vector3(0, 0.175f, 0);
           
        col.enabled = true;
        rb.detectCollisions = true;
    }
}
