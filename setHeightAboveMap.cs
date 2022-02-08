using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mapbox.Map;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using SQLite4Unity3d;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System;
using System.Threading;

public class setHeightAboveMap : MonoBehaviour
{
    // Start is called before the first frame update
    public AbstractMap map;
    public Transform object2move;
    public float altitude = 200f;
    public int updateEverNFrames = 500;
    public bool onStartOnly = false;


    
    void Start()
    {
        
        
    }

    private int count = 1;
    private bool run = true;
    private float[] result = new float[1];
    //private ref float rr = ref result[0];
    private Thread t;
    private bool done = false;
    private bool ready = false;

    
    void LateUpdate()
    {

        if (!ready)
        {
            ready = map != null;
        }

        if (ready)
        {
            if (onStartOnly && !done)
            {
                Vector2d latLong = map.WorldToGeoPosition(object2move.position);
                float adj = map.QueryElevationInUnityUnitsAt(latLong);
                Vector3 pos = new Vector3(object2move.position.x, altitude + adj, object2move.position.z);
                Debug.Log(adj);
                object2move.position = pos;
                done = true;
            }

            if (!onStartOnly)
            {

                if (run)
                {
                    run = false;
                    Vector2d latLong = map.WorldToGeoPosition(object2move.position);
                    t = new Thread(new ThreadStart(() => updateAlt(ref result, map, latLong)));
                    t.Start();
                }


                if (count % updateEverNFrames == 0)
                {
                    t.Join();
                    float adj = result[0];
                    Vector3 pos = new Vector3(object2move.position.x, altitude + adj, object2move.position.z);
                    //Debug.Log(adj);
                    object2move.position = pos;
                    count = 0;
                    run = true;
                }

                count = count + 1;

            }
        }
    }

    // private void OnApplicationQuit()
    // {
    //     //throw new NotImplementedException();
    // }

    public static void updateAlt(ref float[] result, AbstractMap map,Vector2d latLong)
    {
        //Vector2d latLong = map.WorldToGeoPosition(objPose);
        result[0] = map.QueryElevationInUnityUnitsAt(latLong);
    }

    
    
}
