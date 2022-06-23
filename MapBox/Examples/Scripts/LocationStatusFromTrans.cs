namespace Mapbox.Examples
{
    using Mapbox.Unity.Location;
    using Mapbox.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class LocationStatusFromTrans : MonoBehaviour
    {

        [SerializeField]
        Text _statusText;

        private AbstractEditorLocationProvider _locationProvider = null;
        void Start()
        {
            if (null == _locationProvider)
            {
                _locationProvider =
                    LocationProviderFactory.Instance
                        .TransformLocationProvider as AbstractEditorLocationProvider; //AbstractLocationProvider;
            }
        }


        void Update()
        {
            Location currLoc = _locationProvider.CurrentLocation;
            currLoc.IsLocationServiceEnabled = true;

            if (currLoc.IsLocationServiceInitializing)
            {
                _statusText.text = "location services are initializing";
            }
            else
            {
                if (!currLoc.IsLocationServiceEnabled)
                {
                    _statusText.text = "location services not enabled";
                }
                else
                {
                    if (currLoc.LatitudeLongitude.Equals(Vector2d.zero))
                    {
                        _statusText.text = "Waiting for location ....";
                    }
                    else
                    {
                        _statusText.text = string.Format("{0}", currLoc.LatitudeLongitude);
                    }
                }
            }

        }
    }
}