using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class sendMainCamToPython : MonoBehaviour
{

    public bool connectToServer = false;
    private Camera cam;
    public int updateEveryNFrames = 10;
    private int frames;
    private TCPMessenger messenger;
    private Vector3 playerOriginalPosition;
    
    public int camResWidth = 240; 
    public int camResHeight = 240;

    public bool sendOnKeyPress = false;


    private Vector3 convertToOpenCVCord(Vector3 unityImageCord)
    {
        return new Vector3(unityImageCord.x, cam.pixelHeight - unityImageCord.y, unityImageCord.z);
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        frames = 0;
        
        //to set in unity editor game view resolution use plus sign to add a fixed res
        Screen.SetResolution(camResWidth, camResHeight, FullScreenMode.Windowed);
        
        if (connectToServer)
        {
            if (AutoCapture.fullAutoTest)
            {
                messenger = AutoCapture.messenger;
            }
            else // just send the RGB cam to python 
            {
                messenger = new TCPMessenger();
                Debug.Log("attepting to connect");    
            }

            
        }
    }


    void send()
    {
        //NOTE: The BOTTOM-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight)
        //Debug.Log(pixelCoord);
        //Debug.Log(cam.WorldToViewportPoint(playerLoc.position).ToString());
            
        RenderTexture rt = new RenderTexture(camResWidth, camResHeight, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(camResWidth, camResHeight, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, camResWidth, camResHeight), 0, 0);
        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
            
            
        if (connectToServer)
        {
                
            messenger.sendPicMsg(bytes);
                
        }
    }

    // Update is called once per frame
    
    void Update()
    {
        if (sendOnKeyPress)
        {
            if (Input.GetKeyDown("space"))
            {
                send();
            }
        }
        else
        {
            frames = frames + 1;
            if (frames % updateEveryNFrames == 0)
            {
                send();
                frames = 0;
            }    
        }


        
        
    }
}
