// Convert the 2D position of the mouse into a
// 3D position.  Display these on the game window.

using Mapbox.Map;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;

public class GetCorner : MonoBehaviour
{
    private Camera cam;
    private Corners corns;
    public AbstractMap map;

    void Start()
    {
        cam = Camera.main;
        corns = new Corners(cam,map);
    }

    void OnGUI()
    {
        Vector3 point = new Vector3();
        Vector3 worldPoint = new Vector3();
        Event   currentEvent = Event.current;
        Vector2 mousePos = new Vector2();
        float depth = 0f;
        string hitting = "none";

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        
        
        // basically the plan is to do this for each of the 4 corners using the  Camera.main.ViewportToWorldPoint(new Vector3(...)); to get each corner 
        // the hope is that this would let us hit more API eventually 
        mousePos.x = currentEvent.mousePosition.x;
        mousePos.y = cam.pixelHeight - currentEvent.mousePosition.y;

        point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit)) {
            Transform objectHit = hit.transform;

            // Do something with the object that was hit by the raycast.
            // just assuming z for simplicity now this will probably break if camera angle change too much
            depth = objectHit.forward.y;
            worldPoint = hit.point;
            depth = worldPoint[1];
            hitting = hit.collider.name;
        }


        GUILayout.BeginArea(new Rect(50, 80, 250, 250));
        GUI.contentColor = Color.black;
        // GUILayout.Label("Screen pixels: " + cam.pixelWidth + ":" + cam.pixelHeight);
        // GUILayout.Label("Mouse position pxl Cord: " + mousePos);
        // GUILayout.Label("Position cam plane: " + point.ToString("F3"));
        // GUILayout.Label("World position " + worldPoint.ToString("F3") + "at depth: " + depth.ToString() + " hitting: " + hitting);
        
        GUILayout.Label("Top Left World Pos: " + corns.getTopLeftWorldPoint().ToString() + "\n Lat , Long: [" + corns.getTopLeftLatLong().ToString() + "]\n" );
        GUILayout.Label("Top Right World Pos: " + corns.getTopRightWorldPoint().ToString()+ "\n Lat , Long: [" + corns.getTopRightLatLong().ToString() + "]\n");
        GUILayout.Label("Bot Right World Pos: " + corns.getBotRightWorldPoint().ToString()+ "\n Lat , Long: [" + corns.getBotRightLatLong().ToString() + "]\n");
        GUILayout.Label("Bot Left World Pos: " + corns.getBotLeftWorldPoint().ToString()+ "\n Lat , Long: [" + corns.getBotLeftLatLong().ToString() + "]\n");
        
        GUILayout.EndArea();
    }

    internal class Corners
    {
        private Camera cam;
        private AbstractMap map;
        
        private Ray topLeft;
        private Ray topRight;
        private Ray botRight;
        private Ray botLeft;
        
        private Vector3 topLeftWorldPoint = new Vector3();
        private Vector3 topRightWorldPoint = new Vector3();
        private Vector3 botRightWorldPoint = new Vector3();
        private Vector3 botLeftWorldPoint = new Vector3();

        private Vector2 topLeftLatLong;
        private Vector2 topRightLatLong;
        private Vector2 botRightLatLong;
        private Vector2 botLeftLatLong;

        internal Corners(Camera cam, AbstractMap map)
        {
            this.cam = cam;
            this.map = map;

            this.botLeft = cam.ScreenPointToRay(new Vector3(0, 0, 0));
            this.botRight = cam.ScreenPointToRay(new Vector3(Screen.width-1, 0, 0));
            this.topRight = cam.ScreenPointToRay(new Vector3(Screen.width-1, Screen.height-1, 0));
            this.topLeft = cam.ScreenPointToRay(new Vector3(0, Screen.height-1, 0));
            
        }

        private void setPoint(ref Vector3 point,Ray ray)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                point = hit.point;
            }
        }

        public Vector3 getTopLeftWorldPoint()
        {
            topLeft = cam.ScreenPointToRay(new Vector3(0, Screen.height-1, 0));
            setPoint(ref topLeftWorldPoint,topLeft);
            return topLeftWorldPoint;
        }
        
        public Vector3 getBotLeftWorldPoint()
        {
            botLeft = cam.ScreenPointToRay(new Vector3(0, 0, 0));
            setPoint(ref botLeftWorldPoint,botLeft);
            return botLeftWorldPoint;
        }
        
        public Vector3 getTopRightWorldPoint()
        {
            topRight = cam.ScreenPointToRay(new Vector3(Screen.width-1, Screen.height-1, 0));
            setPoint(ref topRightWorldPoint,topRight);
            return topRightWorldPoint;
        }
        
        public Vector3 getBotRightWorldPoint()
        {
            botRight = cam.ScreenPointToRay(new Vector3(Screen.width-1, 0, 0));
            setPoint(ref botRightWorldPoint,botRight);
            return botRightWorldPoint;
        }

        public Vector2d getTopLeftLatLong()
        {
            return this.map.WorldToGeoPosition(getTopLeftWorldPoint());
        }
        
        public Vector2d getTopRightLatLong()
        {
            return this.map.WorldToGeoPosition(getTopRightWorldPoint());
        }
        
        public Vector2d getBotRightLatLong()
        {
            return this.map.WorldToGeoPosition(getBotRightWorldPoint());
        }
        
        public Vector2d getBotLeftLatLong()
        {
            return this.map.WorldToGeoPosition(getBotLeftWorldPoint());
        }

    }

}