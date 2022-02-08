using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using UnityEditor.UIElements;
using UnityEngine;

public class ApplySegmentation : MonoBehaviour
{

    public Shader segmentShader;
    public Camera segmentCamera;
    public AbstractMap map;
    public MaterialModifier mod;
    public LayerModifier LayerModifierForBG;
    
    Dictionary<string, Color32> segmentDict = new Dictionary<string, Color32>();
    

    private System.Action mapInitEvent;
    private ArrayList children = new ArrayList();
    void Start()
    {
         //map.OnInitialized += doTheThing;
         
         segmentDict.Add("Building", new Color32(255, 0, 0, 255));
         segmentDict.Add("Road", new Color32(45, 45, 45, 255));
         segmentDict.Add("Parking", new Color32(255, 90, 0, 255));
         segmentDict.Add("nonFloodWater", new Color32(0, 0, 255, 255));
         segmentDict.Add("FloodWater", new Color32(114, 93, 71, 255));
         segmentDict.Add("map", new Color32(255, 255, 0, 255));
         
    }

    private bool ready = false;
    private bool done = false;
    private int count = 0;
    private void Update()
    {
        if (!ready)
        {
            ready = map != null;
        }

        if (ready)// && ! done)
        {
            count = count + 1;
            
            if (count % 100 == 0)
            {
                doTheThing();
                //map.Terrain.AddToUnityLayer(8);
                count = 0;
            }

            // doTheThing();
            // done = true;
        }
    }


    void ColorMapMesh()
    {
        
        int numKids = map.transform.childCount;
        for (int i = 1; i < numKids - 1; i++ )
        {
            GameObject c = map.transform.GetChild(i).gameObject;
            if (c.activeSelf && !c.name.Contains("Clone")) // dont clone the clones only dup og
            {
                
                //Mesh m = c.GetComponent<MeshFilter>().mesh;




                if (!children.Contains(c))
                {
                    children.Add(c);
                    GameObject cloned = Object.Instantiate(c, map.transform);
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
                    UnityTile tile = cloned.GetComponent<UnityTile>();
                    
                    mod.Run(ve, tile);
                    LayerModifierForBG.Run(ve,tile);
                    
                    var mpb = new MaterialPropertyBlock();
                    segmentDict.TryGetValue("map", out Color32 outColor);
                    mpb.SetColor("_SegmentColor", outColor);
                    r.SetPropertyBlock(mpb);
                    //r1.SetPropertyBlock(mpb);
                    
                }

                
            }
        }
    }

    void doTheThing()
    {
        Debug.Log("made it into event code");
        

        // Find all GameObjects with Mesh Renderer and add a color variable to be
        // used by the shader in it's MaterialPropertyBlock
        var renderers = FindObjectsOfType<MeshRenderer>();
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {
            Debug.Log("Attempting " + r.name.ToString()); //.transform.tag.ToString());
            
            // mpb.SetColor("_SegmentColor", new Color32(0, 0, 255, 255));
            // r.SetPropertyBlock(mpb);

            if (segmentDict.TryGetValue(r.transform.tag, out Color32 outColor))
            {
                Debug.Log("adding " + r.transform.tag.ToString() +" with " + outColor.ToString());
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