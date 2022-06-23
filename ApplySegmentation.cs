using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.MeshGeneration.Modifiers;
using UnityEditor.UIElements;
using UnityEngine;
using Object = UnityEngine.Object;
using TerrainLayer = Mapbox.Unity.Map.TerrainLayer;

public class ApplySegmentation : MonoBehaviour
{

    public Shader segmentShader;
    public Camera segmentCamera;
    public AbstractMap map;
    public MaterialModifier mod;
    public LayerModifier LayerModifierForBG;
    public int updateEveryXframes = 20;
    
    Dictionary<string, Color32> segmentDict = new Dictionary<string, Color32>();
    

    private System.Action mapInitEvent;
    private ArrayList children = new ArrayList();

    private TerrainFactoryBase tf = null;//will copy in terrain factory
    void Start()
    {
         //map.OnInitialized += doTheThing;
         
         
         segmentDict.Add("Building", new Color32(255, 0, 0, 255));
         segmentDict.Add("Road", new Color32(45, 45, 45, 255));
         segmentDict.Add("Parking", new Color32(255, 90, 0, 255));
         segmentDict.Add("nonFloodWater", new Color32(0, 0, 255, 255));
         //Warning flood water color actually seems to be set by the material props in unity
         //This should match the value set in the unity editor now. 111, 63, 12 on 2/16/2022
         segmentDict.Add("FloodWater", new Color32(111, 63, 12, 255));
         //segmentDict.Add("FloodWater", new Color32(114, 93, 71, 255));
         segmentDict.Add("map", new Color32(255, 255, 0, 255));
         
    }

    private bool ready = false;
    private bool done = false;
    private int count = 0;
    private void Update()
    {
        if (!ready)
        {
            ready = map != null; // wait for map to initialize
        }

        if (ready)// && ! done)
        {
            
            
            if (count % updateEveryXframes == 0)
            {
                // playing();
                applySegCamShader();
                //map.Terrain.AddToUnityLayer(8);
                count = 0;
            }
            count = count + 1;

            // doTheThing();
            // done = true;
        }
    }



    private static bool findTerrainFactory(AbstractTileFactory fact)
    {
        return fact is TerrainFactoryBase;
    }

    // void playing()//copy terrain factory and set clone in map layer apply coloring to map layer
    // {
    //     if (tf == null)//first time in need to copy the terrainFactory
    //     {
    //         Predicate<AbstractTileFactory> pred = findTerrainFactory;
    //         tf = (TerrainFactoryBase) map.MapVisualizer.Factories[map.MapVisualizer.Factories.FindIndex(pred)];
    //         TerrainFactoryBase tfClone = Object.Instantiate(tf);
    //
    //         tf.Properties.unityLayerOptions.layerId = 8;//map layer id is 8
    //         // tfClone.Properties.unityLayerOptions.layerId = 8;//map layer id is 8
    //         map.MapVisualizer.Factories.Add(tf);
    //
    //     }
    //
    // }


    //this has become much more complicated than intended and there is probably a better way
    //Map box generates a mesh object based on elevation data to simulate terain
    //This layer is not labeled or filterable and so the tag and layer modifiers are not straightforward to use
    //THis method attempts to create copies of all the tiles and put them in a layer only visable to the segmenation camera
    void ColorMapMesh() 
    {
        ArrayList childrenToRemoveNames = new ArrayList();
        
        int numKids = map.transform.childCount; //track the number of tiles in the map
        for (int i = 1; i < numKids; i++ )
        {
            GameObject c = map.transform.GetChild(i).gameObject;
            if (c.activeSelf && !c.name.Contains("Clone")) // dont clone the clones only dup og
            {
                
                //Mesh m = c.GetComponent<MeshFilter>().mesh;




                if (!children.Contains(c.name) )//there is a new tile rendered need to clone
                {
                    children.Add(c.name);

                    GameObject cloned = Object.Instantiate(map.transform.GetChild(i).gameObject, map.transform);
                    UnityTile tile = cloned.GetComponent<UnityTile>();;

                    UnityTile satTile = c.GetComponent<UnityTile>();//
                    Texture2D rd = satTile.GetRasterData();
                    if (rd == null)
                    {
                        Debug.Log("null tile " + tile.name);
                    }

                    Texture2D temp = new Texture2D(rd.width,rd.height,rd.format,rd.mipmapCount,true);
                    Graphics.CopyTexture(rd, temp);
                    
                    //copy over the orginal satilite texture
                    satTile.MeshRenderer.sharedMaterial.mainTexture = temp;
                    
                    //also need to copy the texture fo the kids
                    int numSatTileKids = satTile.transform.childCount;
                    for (int k = 0; k < numSatTileKids;k++)
                    {
                        
                        
                        GameObject satChild = satTile.transform.GetChild(k).gameObject;
                        if (satChild.name == "building")
                        {
                            MeshRenderer smr = satChild.GetComponent<MeshRenderer>();
                            Material[] mats = smr.sharedMaterials;
                            foreach (var mat in mats)
                            {
                                Texture srd = mat.mainTexture;
                                
                                if (mat.name.Contains("Satellite")){
                                    Texture2D stemp = new Texture2D(srd.width,srd.height,rd.format,srd.mipmapCount,true);
                                    Graphics.CopyTexture(srd, stemp);
                                    smr.sharedMaterial.mainTexture = stemp;       
                                }

                                 
                            }
                            
                            
                        }
                    }
                    
                    
                    
                    //tile.Destroy();
                    
                    MeshFilter mf = cloned.GetComponent<MeshFilter>();
                    MeshRenderer r = cloned.GetComponent<MeshRenderer>();
                    VectorEntity ve = new VectorEntity()
                    {
                        GameObject = cloned,
                        Transform = cloned.transform,
                        MeshFilter = mf,
                        MeshRenderer = r,
                        Mesh = mf.sharedMesh
                    };
                    
                    
                    mod.Run(ve, tile);
                    LayerModifierForBG.Run(ve,tile);
                    
                    var mpb = new MaterialPropertyBlock();
                    segmentDict.TryGetValue("map", out Color32 outColor);
                    mpb.SetColor("_SegmentColor", outColor);
                    r.SetPropertyBlock(mpb);
                    
                    
                    
                }

                
            }
            
        }

        ArrayList activeNonClones = new ArrayList();
        List<GameObject> toDestroy = new List<GameObject>();
        numKids = map.transform.childCount;
        
        for (int i = 1; i < numKids - 1; i++)
        {
            string name = map.transform.GetChild(i).gameObject.name;
            if (!name.Contains("Clone"))
            {
                activeNonClones.Add(name);
            }
        }
        
         // trying to avoid concurrent mod
        
         for (int i = 1; i < numKids - 1; i++)
         {
             GameObject c = map.transform.GetChild(i).gameObject;
             if (c.name.Contains("Clone"))
             {
                 string cloneName = c.name.Split('(')[0];
                 if (!activeNonClones.Contains(cloneName))
                 {
                     toDestroy.Add(c);
                     children.Remove(cloneName);
                 }
             }
         }

         //avoiding destriction of recent tiles seems to greatly allevate the blank tile issue.
         int size = toDestroy.Count;
         while (size > 53)//large prime number?
         {
             var c = toDestroy[0];
             c.SetActive(false);
             c.Destroy();
             toDestroy.RemoveAt(0);
             size = toDestroy.Count;
         }


         foreach (GameObject c in toDestroy)
         {
             c.SetActive(false);
             // string cloneName = c.name.Split('(')[0];
             // int x =  Int32.Parse(cloneName.Split('/')[1]);
             // int y =  Int32.Parse(cloneName.Split('/')[2]);
             // int z =  Int32.Parse(cloneName.Split('/')[0]);
             //
             //c.Destroy();
             //map.MapVisualizer.DisposeTile(new UnwrappedTileId(z,x,y));
         }

         //just giving up at this point and trying to brute force mange the old tiles
         // if ((AutoCapture.rasterLengthRight + 1) * AutoCapture.rasterLengthUp == AutoCapture.poseIdx)
         // {
         //     foreach (GameObject c in toDestroy)
         //     {
         //         c.Destroy();
         //     }
         // }

    }

    void applySegCamShader()
    {
        //Debug.Log("made it into event code");
        

        // Find all GameObjects with Mesh Renderer and add a color variable to be
        // used by the shader in it's MaterialPropertyBlock
        var renderers = FindObjectsOfType<MeshRenderer>();
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {
            //Debug.Log("Attempting " + r.name.ToString()); //.transform.tag.ToString());
            
            // mpb.SetColor("_SegmentColor", new Color32(0, 0, 255, 255));
            // r.SetPropertyBlock(mpb);

            if (segmentDict.TryGetValue(r.transform.tag, out Color32 outColor))
            {
                //Debug.Log("adding " + r.transform.tag.ToString() +" with " + outColor.ToString());
                mpb.SetColor("_SegmentColor", outColor);
                r.SetPropertyBlock(mpb);
            }
            // else if (r.name.ToString() == "Tile")// attempting to hack this so the tiles all have the map color
            // {
            //     segmentDict.TryGetValue("map", out Color32 TColor);
            //     mpb.SetColor("_SegmentColor", TColor);
            //     r.SetPropertyBlock(mpb);
            // }
        }

        // Finally set the Segment shader as replacement shader
        
        ColorMapMesh();
        
        segmentCamera.SetReplacementShader(segmentShader, "RenderType"); 
        

    }
}