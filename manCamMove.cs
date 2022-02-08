using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manCamMove : MonoBehaviour
{

    public Transform camLoc;
    public float sensitivity = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private float x;
    private float z;
    void Update()
    {
        Vector3 move;
        
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        move = transform.right * x + transform.up * z;
        move = move * sensitivity;
        camLoc.position = camLoc.position + move;
    }
}
