using DaggerfallWorkshop.Utility;
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
            public GameObject markerIcon;
            public Minimap.MarkerGroups markerType;
            public float markerDistance;
            public bool isActive;
            public bool inVision;
            public bool inLOS;
            public LayerMask markerLayerMask;
            public Material npcMarkerMaterial;
            public Material npcIconMaterial;
            public EnemySenses enemySenses;
            public Texture npcIconTexture;

            public Marker()
            {
                markerObject = null;
                markerIcon = null;
                markerType = Minimap.MarkerGroups.None;
                isActive = false;
                inVision = false;
                inLOS = false;
                markerLayerMask = 10;
                markerDistance = 0;
                npcMarkerMaterial = null;
                npcIconMaterial = null;
                npcIconTexture = null;
                enemySenses = null;
            }
        }

        // Creating an Instance (an Object) of the marker class to store and update specific object properties once initiated.
        public Marker marker = new Marker();

        //object general properties.
        public Vector3 markerScale;
        public npcMarker NPCControlScript;
        public float frameTime;
        private bool startMarkerUpdates;
        private float timePass;
        private int[] textures;

        private void Start()
        {
            MobilePersonNPC mobileNPC = GetComponentInParent<MobilePersonNPC>();
            DaggerfallEnemy mobileEnemy = GetComponentInParent<DaggerfallEnemy>();
            StaticNPC flatNPC = GetComponentInParent<StaticNPC>();

            //setup base npc marker object and properties.
            marker.markerObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.markerObject.name = "NPC Marker";
            marker.markerObject.GetComponentInChildren<MeshRenderer>().material = Minimap.updateMaterials(marker.markerObject, Color.white, 0);
            marker.npcMarkerMaterial = marker.markerObject.GetComponentInChildren<MeshRenderer>().material;
            Destroy(marker.markerObject.GetComponent<Collider>());
            marker.markerObject.transform.SetParent(transform, false);
            marker.markerObject.layer = Minimap.layerMinimap;

            marker.markerIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
            marker.markerIcon.name = "NPC Icon";
            marker.markerIcon.GetComponentInChildren<MeshRenderer>().material = new Material(Minimap.iconMarkerMaterial);
            marker.npcIconMaterial = marker.markerIcon.GetComponentInChildren<MeshRenderer>().material;
            Destroy(marker.markerIcon.GetComponent<Collider>());
            marker.markerIcon.transform.SetParent(transform, false);
            marker.markerIcon.layer = Minimap.layerMinimap;
            marker.markerIcon.SetActive(false);

            marker.isActive = true;

            //check if player is inside or not, and then setup proper marker size.
            //This needs moved to update in some way, so it updates on entering a building/dungeon.
            if (GameManager.Instance.IsPlayerInside)
                markerScale = new Vector3(Minimap.indicatorSize, .01f, Minimap.indicatorSize);
            else
                markerScale = new Vector3(Minimap.indicatorSize, .01f, Minimap.indicatorSize);

            //set marker object scale.
            marker.markerObject.transform.localScale = markerScale;

            //if friendly npc present, setup flat npc marker color, type, and activate marker object so iit shows on minimap.
            if (mobileNPC != null)
            {
                if (mobileNPC.IsGuard)
                {
                    textures = Minimap.guardTextures;
                }
                else
                {
                    switch (mobileNPC.Race)
                    {
                        case Entity.Races.Redguard:
                            textures = (mobileNPC.Gender == Entity.Genders.Male) ? Minimap.maleRedguardTextures : Minimap.femaleRedguardTextures;
                            break;
                        case Entity.Races.Nord:
                            textures = (mobileNPC.Gender == Entity.Genders.Male) ? Minimap.maleNordTextures : Minimap.femaleNordTextures;
                            break;
                        case Entity.Races.Breton:
                        default:
                            textures = (mobileNPC.Gender == Entity.Genders.Male) ? Minimap.maleBretonTextures : Minimap.femaleBretonTextures;
                            break;
                    }
                }
                marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE." + textures[mobileNPC.PersonOutfitVariant], 5, 0, true, 0);
                marker.npcIconTexture = marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture;

                marker.npcIconMaterial.color = Color.green;
                marker.npcMarkerMaterial.color = Color.green;
                marker.markerType = Minimap.MarkerGroups.Friendlies;
                marker.markerObject.SetActive(false);
                marker.markerObject.name = marker.markerObject.name + " " + mobileNPC.NameNPC;
            }
            //if enemy npc present, setup flat npc marker color, type, and activate marker object so iit shows on minimap.
            else if (mobileEnemy != null)
            {
                DaggerfallMobileUnit mobileUnit = mobileEnemy.GetComponentInChildren<DaggerfallMobileUnit>();

                // Monster genders are always unspecified as there is no male/female variant
                if (mobileUnit.Summary.Enemy.Gender == MobileGender.Male || mobileUnit.Summary.Enemy.Gender == MobileGender.Unspecified)
                    marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE." + mobileUnit.Summary.Enemy.MaleTexture, 0, 0, true, 0);
                else
                    marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE." + mobileUnit.Summary.Enemy.FemaleTexture, 0, 0, true, 0);

                marker.npcIconTexture = marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture;
                marker.npcIconMaterial.color = Color.red;
                marker.npcMarkerMaterial.color = Color.red;
                marker.markerType = Minimap.MarkerGroups.Enemies;
                marker.markerObject.SetActive(false);
                marker.enemySenses = GetComponentInParent<DaggerfallEnemy>().GetComponent<EnemySenses>();
            }
            //if static npc present, setup flat npc marker color, type, and activate marker object so iit shows on minimap.
            else if (flatNPC != null)
            {
                DaggerfallBillboard flatBillboard = GetComponentInParent<DaggerfallBillboard>();

                Debug.Log(flatBillboard.Summary.Archive + " | " + flatBillboard.Summary.Record + " | " + flatBillboard.Summary.ImportedTextures);
                marker.npcIconMaterial.color = Color.yellow;
                marker.npcMarkerMaterial.color = Color.yellow;
                marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE." + flatBillboard.Summary.Archive, flatBillboard.Summary.Record, 0, true, 0);
                marker.npcIconTexture = marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture;
                marker.markerType = Minimap.MarkerGroups.Resident;
                marker.markerObject.SetActive(false);
            }
            else
            {
                marker.isActive = false;
            }
        }

        void Update()
        {
            UnityEngine.Profiling.Profiler.BeginSample("NPC Scripts");
            timePass += Time.deltaTime;

            //adjust how fast markers update to help potatoes computers. If above 60FPS, frame time to 60FPS update times. If below, knock it down to 30FPS update times.
            if(timePass > Random.Range(.33f,.99f))
            {
                //if the marker is turned off compleetly, turn off marker object and stop any further updates.
                marker.isActive = Minimap.iconGroupActive[marker.markerType];
                if (!marker.isActive)
                {
                    marker.markerObject.SetActive(false);
                    marker.markerIcon.SetActive(false);
                    return;
                }
                else if (!Minimap.npcFlatActive[marker.markerType] && marker.markerIcon.activeSelf)
                {
                    marker.markerIcon.SetActive(false);
                }
                else if (Minimap.npcFlatActive[marker.markerType] && marker.markerObject.activeSelf)
                {
                    marker.markerObject.SetActive(false);
                }
                //if player has camera detect and realistic detection off, enable npc marker. This setting turns on all markers.
                else if (!Minimap.minimapControls.cameraDetectionEnabled && !Minimap.minimapControls.realDetectionEnabled)
                {
                    if (!Minimap.npcFlatActive[(Minimap.MarkerGroups)Minimap.minimapControls.selectedIconInt])
                        marker.markerObject.SetActive(true);
                    else
                        marker.markerIcon.SetActive(true);
                    return;
                }
                //if marker is active, check if it is in view, and if so, turn on. If not, turn off.
                else if (Minimap.minimapControls.cameraDetectionEnabled && !Minimap.minimapControls.realDetectionEnabled)
                {
                    if (!ObjectInView() && (marker.markerObject.activeSelf || marker.markerIcon.activeSelf))
                    {
                        if (!Minimap.npcFlatActive[marker.markerType])
                            marker.markerObject.SetActive(false);
                        else
                            marker.markerIcon.SetActive(false);
                    }
                    else if (ObjectInView() && (!marker.markerObject.activeSelf || !marker.markerIcon.activeSelf))
                    {
                        if (!Minimap.npcFlatActive[marker.markerType])
                            marker.markerObject.SetActive(true);
                        else
                            marker.markerIcon.SetActive(true);
                    }
                    return;
                }
                //if marker is active, check if it is in view and within detection radius, and if so, turn on.
                //If not and more than half the distance away, turn off marker.
                else if (Minimap.minimapControls.realDetectionEnabled)
                {
                    if ((!marker.markerObject.activeSelf || !marker.markerIcon.activeSelf) && NPCinLOS() && ObjectInView())
                    {
                        if (!Minimap.npcFlatActive[marker.markerType])
                            marker.markerObject.SetActive(true);
                        else
                            marker.markerIcon.SetActive(true);
                    }
                    else if ((marker.markerObject.activeSelf || marker.markerIcon.activeSelf) && MarkerDistanceFromPlayer() > Minimap.minimapSensingRadius / 2 && !ObjectInView())
                    {
                        if (!Minimap.npcFlatActive[marker.markerType])
                            marker.markerObject.SetActive(false);
                        else
                            marker.markerIcon.SetActive(false);
                    }
                }

                //if the object isn't existing exit update for error stopping.
                if (marker.markerObject == null)
                    return;

                //update marker material color using saved dictionary.
                marker.npcMarkerMaterial.color = Minimap.iconGroupColors[marker.markerType];

                //if the icon isn't existing exit update for error stopping.
                if (marker.markerIcon == null || marker.npcIconTexture == null)
                    return;

                //Updates indicator icon size size based on marker texture size itself.
                float size = Minimap.indicatorSize * ((marker.npcIconTexture.height + marker.npcIconTexture.width) * .00085f) * Minimap.iconSizes[marker.markerType];

                //Updates indicator icon size size based on marker texture size itself for dream mod textures/
                if (Minimap.dreamModInstalled)
                    size = Minimap.indicatorSize * ((marker.npcIconTexture.height + marker.npcIconTexture.width) * .000085f) * Minimap.iconSizes[marker.markerType];

                //run updates to update icon and object.
                marker.markerIcon.transform.localScale = new Vector3(size, .01f, size);
                marker.markerObject.transform.localScale = Minimap.markerScale * Minimap.iconSizes[marker.markerType];

                marker.markerIcon.transform.rotation = Quaternion.Euler(marker.markerIcon.transform.rotation.x, 0, marker.markerIcon.transform.rotation.z);

                marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.color = Minimap.iconGroupColors[marker.markerType];
                timePass = 0;
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
            return GameManager.Instance.PlayerMotor.DistanceToPlayer(transform.position);
        }

        //gets npc/marker is within the camera view by using camera angle calcuations.
        public bool ObjectInView()
        {
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(GameManager.Instance.MainCamera), marker.markerObject.GetComponent<MeshRenderer>().GetComponent<Renderer>().bounds);
        }

        //forces maker off or not.
        public static bool SetActive(bool isActive)
        {
            bool markerStatus = isActive;
            return markerStatus;
        }
    }
}