using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mapbox.Examples;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using DefaultNamespace;



public class AutoCapture : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera rgbCam;
    public Camera segCam;
    public AbstractMap map;
    public int camResWidth = 480;
    public int camResHeight = 480;
    
    public int changePositionEveryXframes = 20;
    public int strideRight = 50;
    public int strideUp = 50;

    public int rasterLengthRight = 20;
    public int rasterLengthUp = 2;

    public String dataFolder;
    public bool isTrainMode = true;
    public bool doFullAutoTest;
    public bool randomRotAug = false;
    public bool manOverRideRaster = false;
    
    public static bool fullAutoTest;
    
    
    

    private bool ready = false;
    private bool done = false;
    private int count = 0;
    private bool takeScreenShot = false;
    
    private List<Vector3> rasterPoseList;
    public static int poseIdx = 0;
    private int cityLocIdx = 0;
    private string currentLocationName;
    public static TCPMessenger messenger;

    private GameObject player;

    private Quaternion initRot;

    void Start()
    {
        initRot = rgbCam.transform.rotation;
        fullAutoTest = doFullAutoTest;
        if (isTrainMode)//collect and save images to train CNN
        {
            //all training locations found at bottom of file can add and remove as needed
            currentLocationName = locations[cityLocIdx].Item1; //the string name of the city is prepended to the file name
            map.SetCenterLatitudeLongitude(locations[cityLocIdx].Item2); // set the lat long for the map 
        }
        else //run the testing mode first step is to collect a panorama over the effected area
        {
            currentLocationName = "test";
            if (fullAutoTest)
            {
                //dont need to render player during pano collection
                player = GameObject.Find("player");
                player.SetActive(false); //when active script on player will force cam to directly overhead
                GameObject.Find("Main Camera").GetComponent<sendMainCamToPython>().enabled = false;//send main cam to python streams at fixed frame rate. Instead we want to capture once per unique pose.
                GameObject.Find("Main Camera").GetComponent<uavCamFollow>().enabled = false; //when active script on player will force cam to directly overhead
                messenger = new TCPMessenger();
                Debug.Log("attepting to connect");
            }
        }

        if (manOverRideRaster)
        {
            changePositionEveryXframes = 1;// allow manual flight to be most responsive
        }
    }
    

    
    static float nextFloat(float min, float max){//get a random float in the range
        System.Random random = new System.Random();
        double val = (random.NextDouble() * (max - min) + min);
        return (float)val;
    }

    private void Update()
    {
        if (!ready)//need to wait for map to init
        {
            ready = map != null;
        }

        if (ready) //once map has init can begin either gather training images or test nav
        {
            //Dont move this updates 1 frame behind all others?
            count = count + 1;
            
            // time to update the pose
            if (count % changePositionEveryXframes == 0)
            {
                count = 0;
                //init the list if first pass
                if (rasterPoseList == null)
                {
                    rasterPoseList = raster();
                }
                //check if there is still part of the raster to do
                //maybe worth refactoring the rastering to a class

                if (randomRotAug) //can apply a random rotation to simulate pitch and roll. Need gimbal to avoid homogaphy plane assumption
                {
                    rgbCam.transform.rotation = initRot;
                    rgbCam.transform.Rotate(0f,0f,nextFloat(-30f,30f));
                    //rgbCam.transform.Rotate(nextFloat(-2f,2f),nextFloat(-2f,2f),nextFloat(-2f,2f));
                }

                if (isTrainMode)
                {
                    uavCamTrainMode();
                }
                else
                {
                    if (manOverRideRaster)
                    {
                        uavCamTestModeManOverRide();
                    }
                    else
                    {
                        uavCamTestModeGatherGlobal();
                    }

                    // uavCamTestModeGatherGlobal();
                }



                //next step is to add logic to jump the map to a new location all together
                //then repeat raster
                




            }

            

        }

    }


    private void uavCamTestModeGatherGlobal()
    {
        if (poseIdx < rasterPoseList.Count)
        {
            rgbCam.transform.position = rgbCam.transform.position + rasterPoseList[poseIdx];
            //Also want to add logic for saving pic and its seg map
                    
            poseIdx = poseIdx + 1;
            takeScreenShot = true;
        }
        else
        {
            Debug.Log("all images captured");
            if (fullAutoTest)
            {
                Debug.Log("Begin Navigation phase");
                
                player.SetActive(true);
                GameObject.Find("Main Camera").GetComponent<uavCamFollow>().enabled = true;
                GameObject.Find("Main Camera").GetComponent<sendMainCamToPython>().enabled = true;

                //Stop the random rotation durring template matching 
                randomRotAug = false;



            }
        }
    }

    private void uavCamTrainMode()
    {
        if (poseIdx < rasterPoseList.Count) //still more locations at this area to consume
        {
            rgbCam.transform.position = rgbCam.transform.position + rasterPoseList[poseIdx];
            //Also want to add logic for saving pic and its seg map
                    
            poseIdx = poseIdx + 1;
            takeScreenShot = true; //indicate time to capture training pair and save to disk after screen renders [in late update funciton]
        }
        else if (cityLocIdx < locations.Count-1) //move to the next city area and restart raster
        {
            poseIdx = 0; //restart raster
            cityLocIdx++; //move to next city area
                    
            currentLocationName = locations[cityLocIdx].Item1; //update prefix

                    
            // map.SetCenterLatitudeLongitude(locations[cityLocIdx].Item2);
            // ReloadMap r = new ReloadMap();

            int numKids = map.transform.childCount;
            map.UpdateMap(locations[cityLocIdx].Item2,18); //sets new loc in mapbox
            rgbCam.transform.position = new Vector3(0, rgbCam.transform.position.y, 0);
        }
    }

    private List<Vector3> raster() //fly the raster patern
    {
        List<Vector3> result = new List<Vector3>(rasterLengthUp*rasterLengthRight);
        Vector3 Delta;
        for (int j = 0; j < rasterLengthUp; j++)
        {
            for (int i = 0; i < rasterLengthRight; i++) //move left to right then up then right to left and repeat
            {
                if (j % 2 == 0)
                {
                    Delta =  (rgbCam.transform.right * strideRight);
                }
                else
                {
                    Delta = (rgbCam.transform.right * -strideRight);
                }

                result.Add(Delta);

            }
            Delta = (rgbCam.transform.up * strideUp);
            result.Add(Delta);
        }

        // stop the last move up from happening
        result.RemoveAt(result.Count-1);
        return result;
    }
    
    
    //this is where images are either sent to python durring the stitching phase of testing 
    // or where the training images are saved to the disk
    void LateUpdate() {
        
        if (takeScreenShot) //UAV is in position and ready to capture a training pair ar send an image to be stitched
        {
            string fname = currentLocationName + "_" +poseIdx.ToString() + ".png"; //create unique name for each image

            //images and masks are saved with the same name to corresponing folders
            //tulsa_1.png in folder x will have a mask saved called tulsa_1.png in folder y
            string xpath = System.IO.Path.Combine(dataFolder, "x"); //save x image to disk
            string ypath = System.IO.Path.Combine(dataFolder, "y"); //save seg mask to disk
            
            if(isTrainMode || !fullAutoTest){ //default to save the images if nothing is checked or save images if train mode is checked
                screenShot(rgbCam,xpath,fname);
                screenShot(segCam,ypath,fname);
            }
            else // case where only do full auto test is checked under autocap in editor fly the raster pattern and send to python for stitching phase of nav test
            {
                RenderTexture rt = new RenderTexture(camResWidth, camResHeight, 24);
                rgbCam.targetTexture = rt;
                Texture2D screenShot = new Texture2D(camResWidth, camResHeight, TextureFormat.RGB24, false);
                rgbCam.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, camResWidth, camResHeight), 0, 0);
                rgbCam.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                Destroy(rt);
                byte[] bytes = screenShot.EncodeToPNG();
                
                messenger.sendPicStitchMsg(bytes);
            }

            takeScreenShot = false;
        }
    }

    void screenShot(Camera camera,string filePath,string filename)
    {
        RenderTexture rt = new RenderTexture(camResWidth, camResHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(camResWidth, camResHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, camResWidth, camResHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        
        string fullPath = System.IO.Path.Combine(filePath,filename);
        System.IO.File.WriteAllBytes(fullPath, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", fullPath));
    }

    private void uavCamTestModeManOverRide() //rather than fly rastern patern control the uav manually durring the nav test
    {
        if (Input.GetKeyDown("q"))
        {
            Debug.Log("all images captured");
            if (fullAutoTest)
            {
                Debug.Log("Begin Navigation phase");
                
                player.SetActive(true);
                GameObject.Find("Main Camera").GetComponent<uavCamFollow>().enabled = true;
                GameObject.Find("Main Camera").GetComponent<sendMainCamToPython>().enabled = true;

                //Stop the random rotation durring template matching 
                randomRotAug = false;
                
            }
        }
        else
        {
            
            // rgbCam.transform.position = rgbCam.transform.position + rasterPoseList[poseIdx];
            // //Also want to add logic for saving pic and its seg map
            //         
            // poseIdx = poseIdx + 1;
            if (Input.GetKeyDown("space"))
            {
                
                takeScreenShot = true;
                poseIdx = poseIdx + 1;
            }

        }
    }

    // list of all locations where training data was gathered
    private static List<Tuple<string, Vector2d>> locations = new List<Tuple<string, Vector2d>>
    {
        
        new Tuple<string, Vector2d>("Tulsa",new Vector2d(36.119803,-95.918574)),
        new Tuple<string, Vector2d>("Houston", new Vector2d(29.7650639, -95.6377411)),
        new Tuple<string, Vector2d>("NewOrleans", new Vector2d(29.9922571,-90.210429)),
        new Tuple<string, Vector2d>("Miami", new Vector2d(25.7194804,-80.2902936)),
        new Tuple<string, Vector2d>("Olathe", new Vector2d(38.8793528,-94.831577)),
        new Tuple<string, Vector2d>("Seattle", new Vector2d(47.4095234,-122.213649)),
        new Tuple<string, Vector2d>("Eugene", new Vector2d(44.0478014,-123.111087)),
        new Tuple<string, Vector2d>("Fresno", new Vector2d(36.809862,-119.802066)),
        new Tuple<string, Vector2d>("SanJose", new Vector2d(37.33079,-121.89364)),
        new Tuple<string, Vector2d>("Reno", new Vector2d(39.5033288,-119.814619)),
        new Tuple<string, Vector2d>("LasVegas", new Vector2d(36.1059792,-115.17269)),
        new Tuple<string, Vector2d>("Phoenix", new Vector2d(33.4683015,-112.078925)),
        new Tuple<string, Vector2d>("Atlanta", new Vector2d(33.7412596,-84.441618)),
        new Tuple<string, Vector2d>("Benson", new Vector2d(31.9692569,-110.300303)),
        new Tuple<string, Vector2d>("Encino", new Vector2d(34.6518326,-105.46732)),
        new Tuple<string, Vector2d>("Taos", new Vector2d(36.390933,-105.587367)),
        new Tuple<string, Vector2d>("Anderson", new Vector2d(40.1101184,-85.666375)),
        new Tuple<string, Vector2d>("AngelFire", new Vector2d(36.369917,-105.28993)),
        new Tuple<string, Vector2d>("SaltLakeCity", new Vector2d(40.6789975,-111.989250)),
        new Tuple<string, Vector2d>("TwinFalls", new Vector2d(42.5594059,-114.462959)),
        new Tuple<string, Vector2d>("MilesCity", new Vector2d(46.4045927,-105.837697)),
        new Tuple<string, Vector2d>("Bartlesville", new Vector2d(36.7444419,-95.9817277)),
        new Tuple<string, Vector2d>("Lawton", new Vector2d(34.6061023,-98.41906)),
        new Tuple<string, Vector2d>("Chillicothe", new Vector2d(39.3354899,-82.999484)),
        new Tuple<string, Vector2d>("Madison", new Vector2d(43.0586054,-89.489191)),
        new Tuple<string, Vector2d>("Jackson", new Vector2d(39.0495999,-82.64607)),
        new Tuple<string, Vector2d>("Charleston", new Vector2d(38.3680128,-81.656397)),
        new Tuple<string, Vector2d>("Huntington", new Vector2d(38.4167189,-82.438926)),
        new Tuple<string, Vector2d>("Scranton", new Vector2d(41.4073355,-75.683582)),
        new Tuple<string, Vector2d>("NewYork", new Vector2d(40.7192769,-74.071145)),
        new Tuple<string, Vector2d>("Chantilly", new Vector2d(38.8786886,-77.446673)),
        new Tuple<string, Vector2d>("FortWayne", new Vector2d(41.0607323,-85.225359)),
        new Tuple<string, Vector2d>("Indianapolis", new Vector2d(39.7397752,-86.219786)),
        new Tuple<string, Vector2d>("Memphis", new Vector2d(35.0919845,-89.9509)),
        new Tuple<string, Vector2d>("LebanonTN", new Vector2d(36.214709,-86.331207)),
        new Tuple<string, Vector2d>("ThreeMileIsland", new Vector2d(40.1516408,-76.724547)),
        new Tuple<string, Vector2d>("Destin", new Vector2d(30.3946611,-86.479875)),
        new Tuple<string, Vector2d>("Pensacola", new Vector2d(30.4440823,-87.273829)),
        new Tuple<string, Vector2d>("Homosassa", new Vector2d(28.7819681,-82.6156547)),
        new Tuple<string, Vector2d>("Blacksburg", new Vector2d(37.2277556,-80.42290)),
        new Tuple<string, Vector2d>("Christiansburg", new Vector2d(37.1449295,-80.42499)),
        new Tuple<string, Vector2d>("Soldotna", new Vector2d(60.4859128,-151.07928)),
        new Tuple<string, Vector2d>("CambridgeBay", new Vector2d(69.1388574,-105.31782)),
        new Tuple<string, Vector2d>("Anchorage", new Vector2d(61.1629428,-149.88524)),
        new Tuple<string, Vector2d>("Fairbanks", new Vector2d(64.8211921,-147.7442494)),
        new Tuple<string, Vector2d>("Brandon", new Vector2d(49.8636212,-99.976921)),
        new Tuple<string, Vector2d>("Bottineau", new Vector2d(48.8231833,-100.4438289)),
        new Tuple<string, Vector2d>("Branson", new Vector2d(36.6384881,-93.276220)),
        new Tuple<string, Vector2d>("Poteau", new Vector2d(35.0447904,-94.6262877)),
        new Tuple<string, Vector2d>("StLouis", new Vector2d(38.6244255,-90.18536)),
        new Tuple<string, Vector2d>("Lawrence", new Vector2d(38.9587496,-95.277979)),
        new Tuple<string, Vector2d>("BrokenBow", new Vector2d(41.4024487,-99.64361)),
        new Tuple<string, Vector2d>("RapidCity", new Vector2d(44.0628404,-103.224620)),
        new Tuple<string, Vector2d>("Galveston", new Vector2d(29.2868257,-94.8130089)),
        new Tuple<string, Vector2d>("Casper", new Vector2d(42.8281839,-106.3766184)),
        new Tuple<string, Vector2d>("BigTimber", new Vector2d(45.8321807,-109.9509134)),
        new Tuple<string, Vector2d>("Denver", new Vector2d(39.7393659,-104.989446)),
        new Tuple<string, Vector2d>("Durango", new Vector2d(37.2769781,-107.8754912)),
        new Tuple<string, Vector2d>("Minneapolis", new Vector2d(44.940069,-93.27071)),
        new Tuple<string, Vector2d>("MasonCity", new Vector2d(43.1440155,-93.21850)),
        new Tuple<string, Vector2d>("SiloamSprings", new Vector2d(36.1826465,-94.533445)),
        new Tuple<string, Vector2d>("LittleRock", new Vector2d(34.7492902,-92.36034)),
        new Tuple<string, Vector2d>("Mobile", new Vector2d(30.6666151,-88.07907)),
        new Tuple<string, Vector2d>("GrandIsle", new Vector2d(29.2352169,-89.99562)),
        new Tuple<string, Vector2d>("MorganCity", new Vector2d(29.697928,-91.19506)),
        new Tuple<string, Vector2d>("Biloxi", new Vector2d(30.4265168,-88.960698)),
        new Tuple<string, Vector2d>("Louisville", new Vector2d(38.1795413,-85.698337)),
        new Tuple<string, Vector2d>("Detroit", new Vector2d(42.3799282,-82.9367707)),
        new Tuple<string, Vector2d>("Chicago", new Vector2d(41.8639324,-87.649704)),
        new Tuple<string, Vector2d>("MyrtleBeach", new Vector2d(33.6848757,-78.907104)),
        new Tuple<string, Vector2d>("VirginiaBeach", new Vector2d(36.7999207,-76.091429)),
        new Tuple<string, Vector2d>("AtlanticCity", new Vector2d(39.3900032,-74.52544)),
        new Tuple<string, Vector2d>("RehobothBeach", new Vector2d(38.7096073,-75.099573)),
        new Tuple<string, Vector2d>("Boston", new Vector2d(42.3584986,-71.059035)),
        new Tuple<string, Vector2d>("Portland", new Vector2d(43.6506631,-70.2728082)),
        new Tuple<string, Vector2d>("Melbourne", new Vector2d(-37.8981436,145.004673)),
        new Tuple<string, Vector2d>("MountIsa", new Vector2d(-20.7258195,139.4907035)),
        new Tuple<string, Vector2d>("Fukaya", new Vector2d(36.1721291,139.231259)),
        new Tuple<string, Vector2d>("Tokyo", new Vector2d(35.6312503,139.650232)),
        new Tuple<string, Vector2d>("Tokyo1", new Vector2d(35.7112122,139.67846)),
        new Tuple<string, Vector2d>("Okegawa", new Vector2d(35.9996904,139.545372)),
        new Tuple<string, Vector2d>("Osaka", new Vector2d(34.6715112,135.509)),
        new Tuple<string, Vector2d>("Hiroshima", new Vector2d(34.3956482,132.45362)),
        new Tuple<string, Vector2d>("Fukuoka", new Vector2d(33.5612071,130.372834)),
        new Tuple<string, Vector2d>("Nagasaki", new Vector2d(32.8072134,130.1674)),
        new Tuple<string, Vector2d>("Miyazaki", new Vector2d(31.8811875,131.423011)),
        new Tuple<string, Vector2d>("Akita", new Vector2d(39.7133785,140.105653)),
        new Tuple<string, Vector2d>("Dublin", new Vector2d(53.3458952,-6.262388)),
        new Tuple<string, Vector2d>("Istanbul", new Vector2d(41.0083371,28.96173)),
        new Tuple<string, Vector2d>("Pyongyang", new Vector2d(39.0093386,125.72667)),
        new Tuple<string, Vector2d>("Casablanca", new Vector2d(33.5508845,-7.6061484)),
        new Tuple<string, Vector2d>("Catania", new Vector2d(37.4962079,15.08425)),
        new Tuple<string, Vector2d>("Venice", new Vector2d(45.4363048,12.32709)),
        new Tuple<string, Vector2d>("Florence", new Vector2d(43.773874,11.24528)),
        new Tuple<string, Vector2d>("Trieste", new Vector2d(45.6427782,13.77319)),
        new Tuple<string, Vector2d>("Graz", new Vector2d(47.0565758,15.427616)),
        new Tuple<string, Vector2d>("Hamburg", new Vector2d(53.5531488,9.962490)),
        new Tuple<string, Vector2d>("Brest", new Vector2d(48.3854583,-4.5181163)),
        new Tuple<string, Vector2d>("Amsterdam", new Vector2d(52.3724207,4.896662)),
        new Tuple<string, Vector2d>("Geneva", new Vector2d(46.2008808,6.13415)),
        
        
    };


}
