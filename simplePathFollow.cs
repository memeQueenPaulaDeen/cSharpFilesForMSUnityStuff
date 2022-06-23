using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefaultNamespace;
using System;



public class simplePathFollow : MonoBehaviour
{
    public Transform playerTransform;
    public Camera uavCamera;
    public int updateposEverNframes = 10;
    
    private int frames;

    // Start is called before the first frame update
    void Start()
    {
        frames = 1;
    }



    // Update is called once per frame
    void LateUpdate()
    {
        if (frames % updateposEverNframes == 0)
        {
            bool reset = TCPMessenger.act.getReset();
            if (reset)
            {
                //transform.position = initialPlayerPose;
            }
            else
            {
                float heading = TCPMessenger.act.getHeading();
                float force = TCPMessenger.act.getForce();
                force = force * 30f;


                //0 degress forward, 90 right, 180 back, and 270 left
                float x = (float) Math.Sin(heading * (Math.PI / 180)) * force * Time.deltaTime;
                float y = (float) Math.Cos(heading * (Math.PI / 180)) * force * Time.deltaTime;

                Vector3 delta = new Vector3(x, 0, y);
                playerTransform.position = playerTransform.position + delta;

            }
        }
        frames = frames + 1;
    }
}
