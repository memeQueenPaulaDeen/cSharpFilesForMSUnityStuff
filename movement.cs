using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class movement : MonoBehaviour
{

    public bool randAct = false;
    public int changeEveryNframes = 10;
    public float speed = 12f;
    public CharacterController Controller;
    public float gravity = -9.81f;

    public Transform groundCheck;
    private float groundDistance = 2f;
    public LayerMask groundMask;
    
    

    private Vector3 velocity;
    private bool isGrounded;
    private int frames = 0;
    private float x;
    private float z;
    
    
    // Update is called once per frame
    void Update()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        
        //gravity effect foobared bc mapbox is dum and children dont seem to inherit layer props
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        
        
        //Debug.Log("x is " + x.ToString());
        Vector3 move;

        if (!randAct)
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
            move = transform.right * x + transform.forward * z;
        }
        else
        {
            if (frames % changeEveryNframes == 0)
            {
                x = Random.Range(-1f, 1f);
                z = Random.Range(-1f, 1f);
                frames = 0;
            }
            move = transform.right * x + transform.forward * z;
            frames = frames + 1;
        }

        Controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        Controller.Move(velocity * Time.deltaTime);
    }
}
