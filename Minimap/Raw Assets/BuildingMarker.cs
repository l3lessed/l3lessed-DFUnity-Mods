using DaggerfallConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaggerfallWorkshop.Game.Minimap
{
    public class BuildingMarker : MonoBehaviour
    {

        //object constructor class and properties for setting up, storing, and manipulating specific object properties.
        public class Marker
        {
            public GameObject attachedMesh;
            public GameObject attachedLabel;
            public GameObject attachedIcon;
            public Vector3 position;
            public Color iconColor;
            public Minimap.MarkerGroups iconGroup;
            public bool iconActive;
            public bool labelActive;

            public Marker()
            {
                attachedMesh = null;
                attachedLabel = null;
                attachedIcon = null;
                position = new Vector3();
                iconColor = new Color();
                iconGroup = Minimap.MarkerGroups.None;
                iconActive = false;
                labelActive = false;
            }
        }

        // Creating an Instance (an Object) of the marker class to store and update specific object properties once initiated.
        public Marker marker = new Marker();

        public StaticBuilding staticBuilding;
        public BuildingSummary buildingSummary;
        public DFLocation.BuildingTypes buildingType;
        public int buildingKey;
        public Vector3 position;
    }
}
