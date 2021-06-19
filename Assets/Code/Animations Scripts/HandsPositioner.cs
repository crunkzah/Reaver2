using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsPositioner : MonoBehaviour
{
    void OnEnable()
    {
        transform.rotation = Quaternion.Euler(4f, -20f, 5f);
    }
}
