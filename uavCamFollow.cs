using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class uavCamFollow : MonoBehaviour

{
    


    public Transform playerLoc;
    public bool useRLFollow;
    private Vector3 initialCamPos;
    public int updateRLposEverNframes = 10;
    private int frames;
    
    private void Start()
    {
        initialCamPos = transform.position;
        frames = 1;
    }

    void Update()
    {
        if (!useRLFollow)
        {
            transform.position = new Vector3(playerLoc.position.x, transform.position.y, playerLoc.position.z);
        }
        else
        {
            if (frames % updateRLposEverNframes == 0)
            {
                bool reset = TCPMessenger.act.getReset();
                if (reset)
                {
                    transform.position = initialCamPos;
                }
                else
                {
                    float heading = TCPMessenger.act.getHeading();
                    float force = TCPMessenger.act.getForce();
                    force = force * 300f;

                
                    //0 degress forward, 90 right, 180 back, and 270 left
                    float x = (float) Math.Sin(heading*(Math.PI/180))*force*Time.deltaTime;
                    float y = (float) Math.Cos(heading*(Math.PI/180))*force*Time.deltaTime; 
                
                    Vector3 delta = new Vector3(x,0,y);
                    transform.position = transform.position + delta;    
                    
                }
                
            }

            frames = frames + 1;

        }
    }
}
