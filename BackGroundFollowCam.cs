using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundFollowCam : MonoBehaviour
{
    public Camera Camera;

    public float offSet;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y - offSet,
            Camera.transform.position.z);
    }
}
