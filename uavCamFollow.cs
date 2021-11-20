using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uavCamFollow : MonoBehaviour
{



    public Transform playerLoc;
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(playerLoc.position.x, transform.position.y, playerLoc.position.z);
    }
}
