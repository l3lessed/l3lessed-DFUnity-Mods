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
            public bool inLOS;
            public LayerMask markerLayerMask;
            public Material npcMarkerMaterial;
            public EnemySenses enemySenses;

            public Marker()
            {
                markerObject = null;
                markerType = Minimap.MarkerGroups.None;
                isActive = false;
                inVision = false;
                inLOS = false;
                markerLayerMask = 10;
                markerDistance = 0;
                npcMarkerMaterial = Minimap.buildingMarkerMaterial;
                enemySenses = null;
            }
        }

        // Creating an Instance (an Object) of the marker class to store and update specific object properties once initiated.
        public Marker marker = new Marker();

        //object general properties.
        public GameObject npcMarkerObject;
        public Material material;
        public Vector3 markerScale;
        public IEnumerator updateMarkerRoutine;
        public float frameTime;
        private bool startMarkerUpdates;

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
                npcMarkerObject.name = npcMarkerObject.name + " " + mobileNPC.NameNPC;
            }
            //if enemy npc present, setup flat npc marker color, type, and activate marker object so iit shows on minimap.
            else if (mobileEnemy != null)
            {
                marker.npcMarkerMaterial.color = Color.red;
                marker.markerType = Minimap.MarkerGroups.Enemies;
                npcMarkerObject.SetActive(false);
                marker.enemySenses = GetComponentInParent<DaggerfallEnemy>().GetComponent<EnemySenses>();
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
        }

        void Update()
        {
            UnityEngine.Profiling.Profiler.BeginSample("NPC Scripts");
            float timePass =+ Time.deltaTime;

            //adjust how fast markers update to help potatoes computers. If above 60FPS, frame time to 60FPS update times. If below, knock it down to 30FPS update times.

            if (timePass > .1f)
            {
                timePass = 0;
                //if the marker is turned off compleetly, turn off marker object and stop any further updates.
                if (!marker.isActive)
                {
                    Debug.Log(npcMarkerObject.name + " | " + ObjectInView() + " | " + NPCinLOS());
                    marker.markerObject.SetActive(false);
                    return;
                }
                //if player has camera detect and realistic detection off, enable npc marker. This setting turns on all markers.
                else if (!Minimap.minimapControls.cameraDetectionEnabled && !Minimap.minimapControls.realDetectionEnabled)
                {
                    marker.markerObject.SetActive(true);
                    return;
                }
                //if marker is active, check if it is in view, and if so, turn on. If not, turn off.
                else if (Minimap.minimapControls.cameraDetectionEnabled && !Minimap.minimapControls.realDetectionEnabled)
                {
                    if (!ObjectInView() && marker.markerObject.activeSelf)
                        marker.markerObject.SetActive(false);
                    else if (ObjectInView() && !marker.markerObject.activeSelf)
                        marker.markerObject.SetActive(true);
                    return;
                }
                //if marker is active, check if it is in view and within detection radius, and if so, turn on.
                //If not and more than half the distance away, turn off marker.
                else if (Minimap.minimapControls.realDetectionEnabled)
                {
                    //if it is an active enemy, only show their icon when they are in actual line of sight of player.
                    //a hostile would actively try to mask their position until seen.                
                    if (marker.markerType == Minimap.MarkerGroups.Enemies)
                    {
                        if (marker.enemySenses.TargetInSight)
                            marker.markerObject.SetActive(true);
                        else
                            marker.markerObject.SetActive(false);
                    }
                    //else if friendly, show within radius.
                    else
                    {
                        if (NPCinLOS() && ObjectInView())
                        {
                            marker.markerObject.SetActive(true);
                        }
                        else if ((MarkerDistanceFromPlayer() > Minimap.minimapSensingRadius / 2) || !NPCinLOS())
                            marker.markerObject.SetActive(false);

                        //if (marker.markerObject.activeSelf && !ObjectInView() && MarkerDistanceFromPlayer() > Minimap.minimapSensingRadius / 2)
                        //marker.markerObject.SetActive(false);
                        //else if(!marker.markerObject.activeSelf && ObjectInView() && MarkerDistanceFromPlayer() < Minimap.minimapSensingRadius)
                        //marker.markerObject.SetActive(true);
                    }
                }
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }       

        public bool NPCinLOS()
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, GameManager.Instance.PlayerController.transform.position - transform.position);
            if (Physics.Raycast(ray, out hit, Minimap.minimapSensingRadius))
            {
                PlayerMotor playerCheck = hit.transform.GetComponent<PlayerMotor>();

                if(playerCheck != null)
                    marker.inLOS = true;
                else
                    marker.inLOS = false;
            }

            return marker.inLOS;
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