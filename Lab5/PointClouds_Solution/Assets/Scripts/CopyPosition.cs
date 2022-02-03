using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CopyPosition : MonoBehaviour
{
    [SerializeField]
    Transform transTarget;
    
    [SerializeField]
    private Vector3 offset = new Vector3(0,-2.75f,9.6f); // Offset to apply to the camera to keep it sufficently far from the player

    // Update is called once per frame
    void Update()
    {
        // Apply the offset to the  camera with respect to the target
        transform.position = transTarget.position - offset;   
    }
}
