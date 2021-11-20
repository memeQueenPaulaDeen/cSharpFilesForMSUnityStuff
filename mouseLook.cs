using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseLook : MonoBehaviour
{
    // Start is called before the first frame update
    public float mouseSensitvity = 100f;
    public Transform playerBody;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitvity * Time.deltaTime;
        float mousey = Input.GetAxis("Mouse Y")* mouseSensitvity * Time.deltaTime;

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
