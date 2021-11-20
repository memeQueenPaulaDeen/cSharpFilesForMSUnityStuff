using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class getPlayerImageCoordinates : MonoBehaviour
{

    public bool connectToServer = false;
    public Transform playerLoc;
    private Camera cam;
    public int updateEveryNFrames = 10;
    private int frames;
    private TCPMessenger messenger;
    
    public int camResWidth = 240; 
    public int camResHeight = 240;


    private Vector3 convertToOpenCVCord(Vector3 unityImageCord)
    {
        return new Vector3(unityImageCord.x, cam.pixelHeight - unityImageCord.y, unityImageCord.z);
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        frames = 0;
        if (connectToServer)
        {
            messenger = new TCPMessenger();
            Debug.Log("attepting to connect");
        }
    }

    // Update is called once per frame
    
    void Update()
    {
        frames = frames + 1;
        if (frames % updateEveryNFrames == 0)
        {
            //NOTE: The BOTTOM-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight)
            Vector3 pixelCoord = cam.WorldToScreenPoint(playerLoc.position);
            pixelCoord = convertToOpenCVCord(pixelCoord);
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
                messenger.sendLocMsg(pixelCoord.ToString());
                messenger.sendPicMsg(bytes);
            }


            frames = 0;
        }
        
    }
}
