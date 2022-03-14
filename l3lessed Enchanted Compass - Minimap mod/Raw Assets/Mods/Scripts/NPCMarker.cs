using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;
using UnityEngine;

namespace Minimap
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
            public EnemySenses enemySenses;
            public Texture npcIconTexture;
            public MeshRenderer npcMesh;

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
                npcIconTexture = null;
                enemySenses = null;
                npcMesh = null;
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
        public MobilePersonNPC mobileNPC;
        public DaggerfallEnemy mobileEnemy;
        public StaticNPC flatNPC;
        private Texture2D markerTexture;

        void Start()
        {
            mobileNPC = GetComponentInParent<MobilePersonNPC>();
            mobileEnemy = GetComponentInParent<DaggerfallEnemy>();
            flatNPC = GetComponentInParent<StaticNPC>();

            //setup base npc marker object and properties.
            marker.markerObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.npcMesh = marker.markerObject.GetComponentInChildren<MeshRenderer>();
            marker.markerObject.name = "NPC Marker";
            //updateMaterials();
            marker.npcMesh.material = Minimap.minimapNpcManager.npcIconMaterialDict[marker.markerType];
            marker.npcMesh.material.mainTexture = Minimap.minimapNpcManager.npcDotTexture;
            Destroy(marker.markerObject.GetComponent<Collider>());
            marker.markerObject.transform.SetParent(transform, false);
            marker.markerObject.layer = Minimap.layerMinimap;
            marker.markerObject.transform.Rotate(90, 0, 0);

            marker.isActive = true;

            //check if player is inside or not, and then setup proper marker size.
            //This needs moved to update in some way, so it updates on entering a building/dungeon.
            if (GameManager.Instance.IsPlayerInside)
                markerScale = new Vector3(Minimap.indicatorSize, Minimap.indicatorSize, Minimap.indicatorSize);
            else
                markerScale = new Vector3(Minimap.indicatorSize, Minimap.indicatorSize, Minimap.indicatorSize);

            //set marker object scale.
            marker.markerObject.transform.localScale = markerScale;
            //if friendly npc present, setup flat npc marker color, type, and activate marker object so iit shows on minimap.
            if (mobileNPC != null)
            {
                if (mobileNPC.IsGuard)
                {
                    textures = Minimap.minimapNpcManager.guardTextures;
                }
                else
                {
                    switch (mobileNPC.Race)
                    {
                        case Races.Redguard:
                            textures = (mobileNPC.Gender == Genders.Male) ? NPCManager.maleRedguardTextures : NPCManager.femaleRedguardTextures;
                            break;
                        case Races.Nord:
                            textures = (mobileNPC.Gender == Genders.Male) ? NPCManager.maleNordTextures : NPCManager.femaleNordTextures;
                            break;
                        case Races.Breton:
                        default:
                            textures = (mobileNPC.Gender == Genders.Male) ? NPCManager.maleBretonTextures : NPCManager.femaleBretonTextures;
                            break;
                    }
                }

                markerTexture = ImageReader.GetTexture("TEXTURE." + textures[mobileNPC.PersonOutfitVariant], 5, 0, true, 0);
                //marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture = markerTexture;
                //marker.npcIconTexture = marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture;
                marker.npcIconTexture = markerTexture;
                marker.npcMesh.material.color = Color.green;
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
                    markerTexture = ImageReader.GetTexture("TEXTURE." + mobileUnit.Summary.Enemy.MaleTexture, 0, 0, true, 0);
                else
                    markerTexture = ImageReader.GetTexture("TEXTURE." + mobileUnit.Summary.Enemy.FemaleTexture, 0, 0, true, 0);

                //marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture = markerTexture;
                //marker.npcIconTexture = marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture;
                //marker.npcIconMaterial.color = Color.red;
                marker.npcIconTexture = markerTexture;
                marker.npcMesh.material.color = Color.red;
                marker.markerType = Minimap.MarkerGroups.Enemies;
                marker.markerObject.SetActive(false);
                marker.enemySenses = GetComponentInParent<DaggerfallEnemy>().GetComponent<EnemySenses>();
            }
            //if static npc present, setup flat npc marker color, type, and activate marker object so iit shows on minimap.
            else if (flatNPC != null)
            {
                DaggerfallBillboard flatBillboard = GetComponentInParent<DaggerfallBillboard>();
                //marker.npcIconMaterial.color = Color.yellow;
                marker.npcMesh.material.color = Color.yellow;
                markerTexture = ImageReader.GetTexture("TEXTURE." + flatBillboard.Summary.Archive, flatBillboard.Summary.Record, 0, true, 0);
                //marker.markerIcon.GetComponentInChildren<MeshRenderer>().material.mainTexture = markerTexture;
                marker.npcIconTexture = markerTexture;
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
            if (!Minimap.MinimapInstance.minimapActive)
                return;

                if (Minimap.npcFlatActive[marker.markerType] && marker.npcMesh.material.mainTexture != marker.npcIconTexture)
                    marker.npcMesh.material.mainTexture = marker.npcIconTexture;
                else if(!Minimap.npcFlatActive[marker.markerType] && marker.npcMesh.material.mainTexture != Minimap.minimapNpcManager.npcDotTexture)
                    marker.npcMesh.material.mainTexture = Minimap.minimapNpcManager.npcDotTexture;

            timePass += Time.deltaTime;
            //adjust how fast markers update to help potatoes computers. If above 60FPS, frame time to 60FPS update times. If below, knock it down to 30FPS update times.
            if(timePass > Minimap.MinimapInstance.npcMarkerUpdateInterval)
            {
                timePass = 0;
                //if the marker is turned off compleetly, turn off marker object and stop any further updates.
                marker.isActive = Minimap.iconGroupActive[marker.markerType];

                if (!marker.isActive)
                {
                    marker.markerObject.SetActive(false);
                    return;
                }
                //if player has camera detect and realistic detection off, enable npc marker. This setting turns on all markers.
                else if (!Minimap.minimapControls.cameraDetectionEnabled && !Minimap.minimapControls.realDetectionEnabled)
                {
                    marker.markerObject.SetActive(Minimap.iconGroupActive[marker.markerType]);
                    return;
                }
                //if marker is active, check if it is in view, and if so, turn on. If not, turn off.
                else if (Minimap.minimapControls.cameraDetectionEnabled && !Minimap.minimapControls.realDetectionEnabled)
                {
                    if (!ObjectInView() && marker.markerObject.activeSelf)
                    {
                            marker.markerObject.SetActive(false);
                    }
                    else if (ObjectInView() && !marker.markerObject.activeSelf)
                    {
                        marker.markerObject.SetActive(Minimap.iconGroupActive[marker.markerType]);
                    }
                    return;
                }
                //if marker is active, check if it is in view and within detection radius, and if so, turn on.
                //If not and more than half the distance away, turn off marker.
                else if (Minimap.minimapControls.realDetectionEnabled)
                {
                    if (!marker.markerObject.activeSelf && NPCinLOS() && ObjectInView())
                    {
                        marker.markerObject.SetActive(Minimap.iconGroupActive[marker.markerType]);
                    }
                    else if (marker.markerObject.activeSelf && MarkerDistanceFromPlayer() > Minimap.minimapSensingRadius / 2 && !ObjectInView())
                    {
                        marker.markerObject.SetActive(false);
                    }
                }

                //if the object isn't existing exit update for error stopping.
                if (marker.markerObject == null)
                    return;

                //update marker material color using saved dictionary.
                if(marker.npcMesh.material.color != Minimap.iconGroupColors[marker.markerType])
                    marker.npcMesh.material.color = Minimap.iconGroupColors[marker.markerType];

                //Updates indicator icon size size based on marker texture size itself.
                float size;
                //Updates indicator icon size size based on marker texture size itself for dream mod textures/
                if (Minimap.npcFlatActive[marker.markerType])
                {
                    if (Minimap.dreamModInstalled)
                        size =(marker.npcIconTexture.height + marker.npcIconTexture.width) * Minimap.iconSizes[marker.markerType] * 4f;
                    else
                        size = (marker.npcIconTexture.height + marker.npcIconTexture.width) * Minimap.iconSizes[marker.markerType] * 4f;
                }
                else
                    size = Minimap.iconSizes[marker.markerType] * Minimap.MinimapInstance.dotSizeAdjuster;

                //run updates to update icon and object.
                marker.markerObject.transform.localScale = Minimap.markerScale * size;
            }               
        }

        //updates object, as long as object has a material attached to it to update/apply shader to.
        void updateMaterials()
        {
            marker.npcMesh.material = marker.markerObject.GetComponent<MeshRenderer>().sharedMaterial;
            marker.npcMesh.material.color = Minimap.iconGroupColors[marker.markerType];
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