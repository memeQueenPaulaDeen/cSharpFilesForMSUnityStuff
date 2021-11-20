using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{

    public float speed = 12f;
    public CharacterController Controller;
    public float gravity = -9.81f;

    public Transform groundCheck;
    private float groundDistance = 2f;
    public LayerMask groundMask;
    
    

    private Vector3 velocity;
    private bool isGrounded;
   
    // Update is called once per frame
    void Update()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        
        //gravity effect foobared bc mapbox is dum and children dont seem to inherit layer props
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        Controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        Controller.Move(velocity * Time.deltaTime);
    }
}
