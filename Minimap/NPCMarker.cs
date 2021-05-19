using System.Collections;
using UnityEngine;
namespace DaggerfallWorkshop.Game.Minimap
{
    public class npcMarker : MonoBehaviour
    {
        //object constructor class and properties for setting up, storing, and manipulating specific object properties.
        public class Marker
        {
            public GameObject markerObject;
            public Minimap.MarkerGroups markerType;
            public float markerDistance;
            public bool isActive;
            public bool inVision;
            public LayerMask markerLayerMask;
            public Material npcMarkerMaterial;

            public Marker()
            {
                markerObject = null;
                markerType = Minimap.MarkerGroups.None;
                isActive = false;
                inVision = false;
                markerLayerMask = 10;
                markerDistance = 0;
                npcMarkerMaterial = Minimap.minimapMarkerMaterial;
            }
        }

        // Creating an Instance (an Object) of the marker class to store and update specific object properties once initiated.
        public Marker marker = new Marker();

        //object general properties.
        private GameObject npcMarkerObject;
        private Material material;
        private Vector3 markerScale;
        private IEnumerator updateMarkerRoutine;
        private float frameTime;

        private void Start()
        {
            MobilePersonNPC mobileNPC = GetComponentInParent<MobilePersonNPC>();
            DaggerfallEnemy mobileEnemy = GetComponentInParent<DaggerfallEnemy>();
            StaticNPC flatNPC = GetComponentInParent<StaticNPC>();

            //setup base npc marker object and properties.
            npcMarkerObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(npcMarkerObject.GetComponent<Collider>());
            marker.npcMarkerMaterial = Minimap.updateMaterials(npcMarkerObject, Color.white, .5f);
            marker.markerObject = npcMarkerObject;
            marker.isActive = true;
            npcMarkerObject.name = "NPCMarker";
            npcMarkerObject.transform.SetParent(transform, false);
            npcMarkerObject.layer = 10;

            //check if player is inside or not, and then setup proper marker size.
            //This needs moved to update in some way, so it updates on entering a building/dungeon.
            if (GameManager.Instance.IsPlayerInside)
                markerScale = new Vector3(Minimap.indicatorSize, .01f, Minimap.indicatorSize);
            else
                markerScale = new Vector3(Minimap.indicatorSize, .01f, Minimap.indicatorSize);

            //set marker object scale.
            npcMarkerObject.transform.localScale = markerScale;

            //if friendly npc present, setup flat npc marker color, type, and activate marker object so iit shows on minimap.
            if (mobileNPC != null)
            {
                marker.markerType = Minimap.MarkerGroups.Friendlies;
                marker.npcMarkerMaterial.color = Color.green;
                npcMarkerObject.SetActive(false);
            }
            //if enemy npc present, setup flat npc marker color, type, and activate marker object so iit shows on minimap.
            else if (mobileEnemy != null)
            {
                marker.npcMarkerMaterial.color = Color.red;
                marker.markerType = Minimap.MarkerGroups.Enemies;
                npcMarkerObject.SetActive(false);
            }
            //if static npc present, setup flat npc marker color, type, and activate marker object so iit shows on minimap.
            else if (flatNPC != null)
            {
                marker.npcMarkerMaterial.color = Color.yellow;
                marker.markerType = Minimap.MarkerGroups.Resident;
                npcMarkerObject.SetActive(false);
            }
            else
            {
                marker.isActive = false;
            }

            //start efficient update routine to update markers.
            updateMarkerRoutine = UpdateMarker();
            StartCoroutine(updateMarkerRoutine);
        }

        public IEnumerator UpdateMarker()
        {
            while (true)
            {
                //adjust how fast markers update to help potatoes computers. If above 60FPS, frame time to 60FPS update times. If below, knock it down to 30FPS update times.
                if (Minimap.fps > 60)
                    frameTime = .017f;
                else
                    frameTime = .034f;

                //if the marker is turned off compleetly, turn off marker object and stop any further updates.
                if (!marker.isActive)
                {
                    yield return new WaitForSecondsRealtime(.017f);
                }
                //if player has camera detect and realistic detection off, enable npc marker. This setting turns on all markers.
                else if (!Minimap.minimapControls.cameraDetectionEnabled && !Minimap.minimapControls.realDetectionEnabled)
                {
                    marker.markerObject.SetActive(true);
                    yield return new WaitForSecondsRealtime(frameTime);
                }
                //if marker is active, check if it is in view, and if so, turn on. If not, turn off.
                else if (Minimap.minimapControls.cameraDetectionEnabled && !Minimap.minimapControls.realDetectionEnabled)
                {
                    if (!ObjectInView() && marker.markerObject.activeSelf)
                            marker.markerObject.SetActive(false);
                    else if (ObjectInView() && !marker.markerObject.activeSelf)
                            marker.markerObject.SetActive(true);
                    yield return new WaitForSecondsRealtime(frameTime);
                }
                //if marker is active, check if it is in view and within detection radius, and if so, turn on.
                //If not and more than half the distance away, turn off marker.
                else if (Minimap.minimapControls.realDetectionEnabled)
                {
                    if (marker.markerObject.activeSelf && !ObjectInView() && MarkerDistanceFromPlayer() > Minimap.minimapSensingRadius / 2)
                        marker.markerObject.SetActive(false);
                    else if(!marker.markerObject.activeSelf && ObjectInView() && MarkerDistanceFromPlayer() < Minimap.minimapSensingRadius)
                        marker.markerObject.SetActive(true);
                }

                //return and restart loop based on optimized frameTime value.
                yield return new WaitForSecondsRealtime(frameTime);
            }
        }

        //gets npc/marker distance from player.
        public float MarkerDistanceFromPlayer()
        {
            if (marker.markerObject)
                marker.markerDistance = Vector3.Distance(transform.position, GameManager.Instance.MainCamera.transform.position);
            else
                marker.markerDistance = 0;

            return marker.markerDistance;
        }

        //gets npc/marker is within the camera view by using camera angle calcuations.
        public bool ObjectInView()
        {
            if (marker.markerObject)
                marker.inVision = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(GameManager.Instance.MainCamera), marker.markerObject.GetComponent<MeshRenderer>().GetComponent<Renderer>().bounds);
            else
                marker.inVision = false;

            return marker.inVision;
        }

        //forces maker off or not.
        public static bool SetActive(bool isActive)
        {
            bool markerStatus = isActive;
            return markerStatus;
        }
    }
}