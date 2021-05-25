using DaggerfallConnect;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using Wenzil.Console;

namespace DaggerfallWorkshop.Game.Minimap
{
    public class Minimap : MonoBehaviour, IHasModSaveData
    {
        [FullSerializer.fsObject("v1")]
        public class MyModSaveData
        {
            public Dictionary<MarkerGroups, Color> IconGroupColors;
            public Dictionary<MarkerGroups, float> IconGroupTransperency;
            public Dictionary<MarkerGroups, bool> IconGroupActive;
            public float MinimapSizeMult;
            public float MinimapViewSize;
            public float MinimapRotationValue;
            public float MinimapCameraHeight;
            public float AlphaValue;
            public float MinimapSensingRadius;
            public bool LabelIndicatorActive;
            public bool SmartViewActive;
            public bool IconsIndicatorActive;
            public bool RealDetectionEnabled;
            public bool CameraDetectionEnabled;

        }

        //classes for setup and use.
        public static npcMarker npcMarkerInstance;
        public static MinimapGUI minimapControls;
        public static BuildingMarker BuildingMarker;
        private static Mod mod;
        public static Minimap MinimapInstance;
        private static ModSettings settings;
        private ConsoleController consoleController;
        public static int playerLayerMask;
        public RenderTexture minimapTexture;
        UserInterfaceManager uiManager = new UserInterfaceManager();
        private DaggerfallAutomapWindow dfAutomapWindow;
        private DaggerfallExteriorAutomapWindow dfExteriorAutomapWindow;
        private string zoomInKey;
        private KeyCode zoomInKeyCode;
        private string zoomOutKey;
        private KeyCode zoomOutKeyCode;
        public BuildingDirectory buildingDirectory;
        private DaggerfallStaticBuildings staticBuildingContainer;
        private Automap automap;
        private DaggerfallRMBBlock[] blockArray;
        public Camera minimapCamera;

        //game objects for storing and manipulating.
        private GameObject gameObjectPlayerAdvanced;
        public GameObject minimapMaterialObject;
        private GameObject buildingMesh;
        public GameObject gameobjectBeaconPlayerPosition;
        public GameObject gameobjectPlayerMarkerArrow;
        public GameObject mainCamera;
        private GameObject minimapCameraObject;
        public GameObject minimapRenderTexture;
        private GameObject gameobjectAutomap;
        public GameObject hitObject;
        private GameObject lastHitObject;
        public GameObject dungeonObject;
        public GameObject interiorInstance;
        public GameObject dungeonInstance;
        public GameObject publicDirections;
        public GameObject publicMinimap;
        public GameObject publicCompass;
        public GameObject publicMinimapRender;

        //custom minimap material and shader.
        private static Material[] minimapMaterial;
        public static Material buildingMarkerMaterial;
        public static Material iconMarkerMaterial;

        //layer for automap.
        private int layerAutomap;

        //values for controlling minimap properties.
        public float PlayerHeightChanger { get; private set; }
        [SerializeField] public float minimapSize = 400;
        public float minimapAngle = 1;
        public float minimapminimapRotationZ;
        public float minimapminimapRotationY;
        public float minimapCameraHeight;
        public float minimapCameraX;
        public float minimapCameraZ;
        public float minimapViewSize;
        private float savedMinimapSize;
        private float savedMinimapViewSize;
        public float iconSize = .7f;
        public float nearClipValue;
        public float farClipValue;
        public static float minimapSensingRadius = 40;
        private float tallestSpot;
        public static float indicatorSize = 3;
        public float playerIndicatorHeight;
        private float deltaTime;
        public static float fps;
        private float timePass;
        public float minimapSizeMult = .25f;
        public float multi;

        private string currentLocationName;
        private string lastLocationName;

        private bool attackKeyPressed;
        private bool fullMinimapMode;
        private bool minimapActive = true;

        public Rect minimapControlsRect = new Rect(20, 20, 120, 50);
        public Rect indicatorControlRect = new Rect(20, 100, 120, 50);

        private new RectTransform maskRectTransform;
        private new RectTransform canvasRectTransform;
        private RectTransform minimapInterfaceRectTransform;
        private RectTransform minimapDirectionsRectTransform;

        public static List<npcMarker> currentNPCIndicatorCollection = new List<npcMarker>();
        public List<npcMarker> npcIndicatorCollection = new List<npcMarker>();
        public List<BuildingMarker> buildingInfoCollection = new List<BuildingMarker>();

        public MobilePersonNPC[] mobileNPCArray;
        public DaggerfallEnemy[] mobileEnemyArray;
        public StaticNPC[] flatNPCArray;
        public StaticBuilding[] StaticBuildingArray { get; private set; }

        Queue<int> playerInput = new Queue<int>();

        public Type SaveDataType { get { return typeof(MyModSaveData); } }

        //public List<StaticDoor> doorsOut;
        //private StaticDoor[] staticDoorArray;       
        //public List<PlayerGPS.NearbyObject> Objects;

        //dictionaries to store marker groups properties for later retrieval.
        public static Dictionary<MarkerGroups, Color> iconGroupColors = new Dictionary<MarkerGroups, Color>()
        {
            {MarkerGroups.Shops, new Color(1,.25f,0,1) },
            {MarkerGroups.Blacksmiths, new Color(0,1,1,1) },
            {MarkerGroups.Houses, new Color(.5f,.5f,.5f,1) },
            {MarkerGroups.Taverns, new Color(0,1,0,1) },
            {MarkerGroups.Utilities, new Color(1,1,0,1) },
            {MarkerGroups.Government, new Color(1,0,0,1) },
            {MarkerGroups.Friendlies, Color.green },
            {MarkerGroups.Enemies, Color.red },
            {MarkerGroups.Resident, Color.yellow },
            {MarkerGroups.None, Color.black }
        };

        public static Dictionary<MarkerGroups, float> iconGroupTransperency = new Dictionary<MarkerGroups, float>()
        {
            {MarkerGroups.Shops, 0 },
            {MarkerGroups.Blacksmiths, 0 },
            {MarkerGroups.Houses, 0 },
            {MarkerGroups.Taverns, 0 },
            {MarkerGroups.Utilities, 0 },
            {MarkerGroups.Government, 0 },
            {MarkerGroups.Friendlies, 0 },
            {MarkerGroups.Enemies, 0 },
            {MarkerGroups.Resident, 0 },
            {MarkerGroups.None, 0 }
        };

        public static Dictionary<MarkerGroups, bool> iconGroupActive = new Dictionary<MarkerGroups, bool>()
        {
            {MarkerGroups.Shops, true },
            {MarkerGroups.Blacksmiths, true },
            {MarkerGroups.Houses, true },
            {MarkerGroups.Taverns, true },
            {MarkerGroups.Utilities, true },
            {MarkerGroups.Government, true },
            {MarkerGroups.Friendlies, true },
            {MarkerGroups.Enemies, true },
            {MarkerGroups.Resident, true },
            {MarkerGroups.None, false}
        };
        private float lastIndicatorSize;

        //sets up marker groups to assign each marker type. This is crucial for seperating and controlling each indicator types appearance and use.
        //technically, you can add to this enum, add to the individual dictionary's, and construct your own marker group to assign to specific objects/npcs.
        public enum MarkerGroups
        {
            Shops,
            Blacksmiths,
            Houses,
            Taverns,
            Utilities,
            Government,
            Friendlies,
            Enemies,
            Resident,
            None
        }

        //starts mod manager on game begin. Grabs mod initializing paramaters.
        //ensures SateTypes is set to .Start for proper save data restore values.
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            //sets up instance of class/script/mod.
            GameObject go = new GameObject("Minimap");
            MinimapInstance = go.AddComponent<Minimap>();

            GameObject npcMarkerObject = new GameObject("npcMarkerObject");
            npcMarkerInstance = npcMarkerObject.AddComponent<npcMarker>();

            GameObject MinimapGUIObject = new GameObject("npcMarkerObject");
            minimapControls = MinimapGUIObject.AddComponent<MinimapGUI>();

            GameObject BuildingMarkerObject = new GameObject("BuildingMarker");
            BuildingMarker = BuildingMarkerObject.AddComponent<BuildingMarker>();

            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //initates mod settings
            settings = mod.GetSettings();

            mod.SaveDataInterface = MinimapInstance;
            //after finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
            Debug.Log("Minimap MOD STARTED!");
        }

        // Start is called before the first frame update
        void Start()
        {           
            //assigns console to script object, then attaches the controller object to that.
            GameObject console = GameObject.Find("Console");
            consoleController = console.GetComponent<ConsoleController>();

            //setup needed objects.
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            minimapCameraObject = mod.GetAsset<GameObject>("MinimapCamera");
            minimapMaterialObject = mod.GetAsset<GameObject>("MinimapMaterialObject");
            minimapMaterial = minimapMaterialObject.GetComponent<MeshRenderer>().sharedMaterials;
            minimapCamera = minimapCameraObject.GetComponent<Camera>();
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;

            //initiate minimap camera.
            minimapCamera = Instantiate(minimapCamera);

            //create and assigned a new render texture for passing camera view into texture.
            minimapTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
            minimapTexture.Create();

            //get minimap size based on screen width.
            minimapSize = Screen.width * minimapSizeMult;

            //sets up minimap canvas, including the screen space canvas container.
            publicMinimap = CanvasConstructor(true, "Minimap Layer", false, false, true, true, false, 1.03f, 1.03f, new Vector3((minimapSize * .455f) * -1, (minimapSize * .455f) * -1, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/MinimapMask.png"), 1);
            //sets up minimap render canvas that render camera texture it projected to.
            publicMinimapRender = CanvasConstructor(false, "Rendering Layer", false, false, true, true, false, 1, 1, new Vector3(0, 0, 0), minimapTexture, 0);
            //sets up bearing directions canvas layer.
            publicDirections = CanvasConstructor(false, "Bearing Layer", false, false, true, true, false, .7f, .7f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/DirectionalIndicatorsSmallMarkers.png"), 0);
            //sets up the golden compass canvas layer.
            publicCompass = CanvasConstructor(false, "Compass Layer", false, false, true, true, false, 1.03f, 1.13f, new Vector3((minimapSize * .46f) * -1, (minimapSize * .365f) * -1, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/pixalatedGoldCompass.png"), 1);
            //attaches rendering canvas to the main minimap mask canvas.
            publicMinimapRender.transform.SetParent(publicMinimap.transform);
            //attaches the bearing directions canvas to the minimap canvas.
            publicDirections.transform.SetParent(publicMinimap.transform);
            //attaches golden compass canvas to main screen layer canvas.
            publicCompass.transform.SetParent(GameObject.Find("Canvas Screen Space").transform);
            //zeros out bearings canvas position so it centers on its parent canvas layer.
            publicDirections.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //zeros out rendering canvas position so it centers on its parent canvas layer.
            publicMinimapRender.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //sets the golden compass canvas to the proper screen position on the main screen space layer so it sits right on top of the rendreing canvas.
            publicCompass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3((minimapSize * .46f) * -1, (minimapSize * .365f) * -1, 0);

            //assign the camera view and the render texture output.
            minimapCamera.targetTexture = minimapTexture;
            //setup the minimap material for indicator meshes.
            buildingMarkerMaterial = new Material(minimapMaterial[0]);
            iconMarkerMaterial = new Material(minimapMaterial[1]);

            //grab the mask and canvas layer rect transforms of the minimap object.
            maskRectTransform = publicMinimap.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            canvasRectTransform = publicMinimapRender.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapInterfaceRectTransform = publicCompass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapDirectionsRectTransform = publicDirections.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            dfAutomapWindow = (DaggerfallAutomapWindow)UIWindowFactory.GetInstance(UIWindowType.Automap, uiManager);
            dfExteriorAutomapWindow = (DaggerfallExteriorAutomapWindow)UIWindowFactory.GetInstance(UIWindowType.ExteriorAutomap, uiManager);

            zoomInKey = settings.GetValue<string>("CompassKeys", "ZoomIn:FullViewCompass");
            zoomInKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), zoomInKey);
            zoomOutKey = settings.GetValue<string>("CompassKeys", "ZoomOut:SettingScroll");
            zoomOutKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), zoomOutKey);

            //grab games automap layer for assigning mesh and camera layers.
            layerAutomap = LayerMask.NameToLayer("Automap");
            if (layerAutomap == -1)
            {
                DaggerfallUnity.LogMessage("Did not find Layer with name \"Automap\"! Defaulting to Layer 10\nIt is prefered that Layer \"Automap\" is set in Unity Editor under \"Edit/Project Settings/Tags and Layers!\"", true);
                layerAutomap = 10;
            }

            gameObjectPlayerAdvanced = GameObject.Find("PlayerAdvanced");
            if (!gameObjectPlayerAdvanced)
            {
                DaggerfallUnity.LogMessage("GameObject \"PlayerAdvanced\" not found! in script Automap (in function Awake())", true);
                if (Application.isEditor)
                    Debug.Break();
                else
                    Application.Quit();
            }

            playerLayerMask = ~(1 << LayerMask.NameToLayer("Player"));
        }

        //Sets up an object class to create a game object that contains the canvas screen space overlay and any sub canvas layers for things like indicators or overlays.
        GameObject CanvasConstructor(bool giveParentContainer, string canvasName, bool canvasScaler, bool canvasRenderer, bool mask, bool rawImage, bool graphicRaycaster, float width, float height, Vector3 positioning, Texture canvasTexture = null, int screenPosition = 0)
        {
            //sets up main canvas screen space overlay for containing all sub-layers.
            //this covers the full screen as an invisible layer to hold all sub ui layers.
            //creates empty objects.
            GameObject canvasContainer = new GameObject();
            if (giveParentContainer)
            {
                //names it/
                canvasContainer.name = "Canvas Screen Space";
                //grabs and adds the canvasd object from unity library.
                canvasContainer.AddComponent<Canvas>();
                //grabs and adds the canvas scaler object from unity library.
                canvasContainer.AddComponent<CanvasScaler>();
                //grabs and adds the graphic ray caster object from unity library.
                canvasContainer.AddComponent<GraphicRaycaster>();
                //grabs the canvas object.
                Canvas containerCanvas = canvasContainer.GetComponent<Canvas>();
                //sets the screen space to full screen overlay.
                containerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            else
                Destroy(canvasContainer);


            //sets up sub layer for adding actual ui and and resizing/moving it.
            GameObject newCanvasObject = new GameObject();
            newCanvasObject.name = canvasName;
            newCanvasObject.AddComponent<Canvas>();

            //sets sublayer to child of the above main container.
            if (giveParentContainer)
                newCanvasObject.transform.SetParent(canvasContainer.transform);

            //grabs canvas from child and sets it to screen overlay. It is an overlay of the main above screen overlay.
            Canvas uiCanvas = newCanvasObject.GetComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            //if then ladder for coder to decide what unity objects they want to add to the canvas for using.
            if(canvasScaler)
                newCanvasObject.AddComponent<CanvasScaler>();
            if (canvasRenderer)
                newCanvasObject.AddComponent<CanvasRenderer>();
            if (graphicRaycaster)
                newCanvasObject.AddComponent<GraphicRaycaster>();
            if (mask)
                newCanvasObject.AddComponent<Mask>();
            if (rawImage)
                newCanvasObject.AddComponent<RawImage>();
            if (canvasTexture != null)
                newCanvasObject.GetComponent<RawImage>().texture = canvasTexture;

            //custom screen positioning method. Coder chooses 0 through 1 for differing screen positions.
            //center in screen/container.
            if(screenPosition == 0)
            {
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().sizeDelta = new Vector2(minimapSize * width, minimapSize * height);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMin = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMax = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = positioning;
            }
            //top right in screen/container.
            else if (screenPosition == 1)
            {
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().sizeDelta = new Vector2(minimapSize * width, minimapSize * height);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = positioning;
            }

            //returns the objects for the cover to drop into an empty object at any place or anytime they want.
            return newCanvasObject;
        }

        void KeyPressCheck()
        {
            
            //if either attack input is press, start the system.
            if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadPlus")) || Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadMinus")))
            {
                timePass = 0;
                attackKeyPressed = true;
            }

            //start monitoring key input for que system.
            if (attackKeyPressed) 
            {
                timePass += Time.deltaTime;
                if (Input.GetKeyDown(zoomInKeyCode))
                    playerInput.Enqueue(0);

                if (Input.GetKeyDown(zoomOutKeyCode))
                    playerInput.Enqueue(1);
            }

            if (Input.GetKey(zoomInKeyCode) && timePass > .25f)
            {               
                minimapViewSize += 3;
                playerInput.Clear();
            }

            if (Input.GetKey(zoomOutKeyCode) && timePass > .25f)
            {                
                playerInput.Clear();
                minimapViewSize -= 3;
            }

            if (timePass > .25f)
            {
                playerInput.Clear();
                timePass = 0;
            }                

            //if the player has qued up an input routine and .16 seconds have passed, do...     
            while (playerInput.Count >= 2 && timePass < .2f)
            {
                attackKeyPressed = false;
                timePass = 0;

                //if both buttons press, clear input, and que up parry.
                if (playerInput.Contains(1) && playerInput.Contains(0))
                {
                    playerInput.Clear();
                    playerInput.Enqueue(2);
                }

                if (playerInput.Contains(2))
                {
                    if (minimapActive)
                        minimapActive = false;
                    else
                        minimapActive = true;
                }


                int count = 0;
                foreach(int input in playerInput)
                {
                    if (input == 0)
                        count += 1;

                    if (count > 1)
                    {
                        if (!fullMinimapMode)
                        {
                            savedMinimapSize = minimapSize;
                            savedMinimapViewSize = minimapViewSize;

                            fullMinimapMode = true;
                            minimapSize = Screen.width * .58f;
                            if (!GameManager.Instance.IsPlayerInside)
                                minimapViewSize = 190;
                            else
                                minimapViewSize = 20;
                        }
                        else
                        {
                            fullMinimapMode = false;
                            minimapSize = savedMinimapSize;
                            minimapViewSize = savedMinimapViewSize;
                        }
                    }

                }

                count = 0;
                foreach (int input in playerInput)
                {
                    if (input == 1)
                        count += 1;

                    if (count > 1)
                    {
                        if (!minimapControls.minimapMenuEnabled)
                        {
                            minimapControls.minimapMenuEnabled = true;
                            GameManager.Instance.PlayerMouseLook.cursorActive = true;
                        }
                        else
                        {
                            minimapControls.minimapMenuEnabled = false;
                            GameManager.Instance.PlayerMouseLook.cursorActive = false;
                        }
                    }                        
                }

                playerInput.Clear();
            }
        }

        //grabs all the buildings in the area and outputs them into a list for use.
        List<BuildingMarker> BuildingFinderCollection()
        {
            List<BuildingMarker> buildingInfoCollection = new List<BuildingMarker>();

            buildingDirectory = GameManager.Instance.StreamingWorld.GetCurrentBuildingDirectory();

            if (buildingDirectory == null)
                return buildingInfoCollection;

            DaggerfallLocation Dflocation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
            blockArray = buildingDirectory.GetComponentsInChildren<DaggerfallRMBBlock>();
            List<Vector3> buildingPositionList = new List<Vector3>();

            foreach (DaggerfallRMBBlock block in blockArray)
            {
                staticBuildingContainer = block.GetComponentInChildren<DaggerfallStaticBuildings>();

                //if there are not any buildings in this block, stop code from crashing script and return.
                if (staticBuildingContainer == null)
                    return buildingInfoCollection;

                StaticBuildingArray = staticBuildingContainer.Buildings;

                //staticDoorContainer = block.GetComponentInChildren<DaggerfallStaticDoors>();
                //staticDoorArray = staticDoorContainer.Doors;

                //runs through building array.
                foreach (StaticBuilding building in StaticBuildingArray)
                {
                    //sets up and grabes the current buildings material, summary object/info, placing/final position, game model.
                    BuildingSummary SavedBuilding = new BuildingSummary();
                    buildingDirectory.GetBuildingSummary(building.buildingKey, out SavedBuilding);

                    switch (SavedBuilding.BuildingType)
                    {
                        case DFLocation.BuildingTypes.Town23:
                        case DFLocation.BuildingTypes.Town4:
                        case DFLocation.BuildingTypes.Special1:
                        case DFLocation.BuildingTypes.Special2:
                        case DFLocation.BuildingTypes.Special3:
                        case DFLocation.BuildingTypes.Special4:
                        case DFLocation.BuildingTypes.Ship:
                        case DFLocation.BuildingTypes.None:
                            continue;
                    }

                    BuildingMarker buildingsInfo = new BuildingMarker();

                    buildingsInfo.staticBuilding = building;
                    buildingsInfo.buildingSummary = SavedBuilding;
                    buildingsInfo.buildingKey = SavedBuilding.buildingKey;
                    buildingsInfo.buildingType = SavedBuilding.BuildingType;

                    //buildingPositionList.Add(new Vector3(block.transform.position.x + SavedBuilding.Position.x, SavedBuilding.Position.y, block.transform.position.z + SavedBuilding.Position.z));
                    buildingsInfo.position = new Vector3(block.transform.position.x + SavedBuilding.Position.x, gameObjectPlayerAdvanced.transform.position.y, block.transform.position.z + SavedBuilding.Position.z);

                    buildingInfoCollection.Add(buildingsInfo);
                }
            }

            return buildingInfoCollection;
        }

        //creates indicators for all buildings using the building finder list.
        public void SetupBuildingIndicators()
        {
            if (buildingInfoCollection != null)
            {
                foreach (BuildingMarker buildingMarker in buildingInfoCollection)
                {
                    Destroy(buildingMarker.marker.attachedIcon);
                    Destroy(buildingMarker.marker.attachedLabel);
                    Destroy(buildingMarker.marker.attachedMesh);
                }

                buildingInfoCollection.Clear();
            }

            buildingInfoCollection = BuildingFinderCollection();

            if (buildingInfoCollection == null)
                return;

            //finds the tallest building height.
            foreach (BuildingMarker marker in buildingInfoCollection)
            {
                if (marker.staticBuilding.size.y > tallestSpot)
                    tallestSpot = marker.staticBuilding.size.y;
            }

            minimapCameraHeight = gameObjectPlayerAdvanced.transform.position.y + tallestSpot + 1f;

            foreach (BuildingMarker buildingInfo in buildingInfoCollection)
            {               
                //gets buildings largest side size for label multiplier.
                float sizeMultiplier;
                if (buildingInfo.staticBuilding.size.x > buildingInfo.staticBuilding.size.y)
                    sizeMultiplier = buildingInfo.staticBuilding.size.x;
                else
                    sizeMultiplier = buildingInfo.staticBuilding.size.y;

                //setup and assign the final world position and rotation using the building, block, and tallest spot cordinates. This places the indicators .2f above the original building model.
                buildingMesh = GameObjectHelper.CreateDaggerfallMeshGameObject(buildingInfo.buildingSummary.ModelID, null, false, null, false);
                buildingMesh.transform.position = new Vector3(buildingInfo.position.x, buildingInfo.position.y + tallestSpot + 10, buildingInfo.position.z);
                buildingMesh.transform.Rotate(buildingInfo.buildingSummary.Rotation);
                buildingMesh.layer = layerAutomap;
                buildingMesh.transform.localScale = new Vector3(1, 0.01f, 1);
                buildingMesh.name = buildingInfo.buildingSummary.BuildingType.ToString() + " Marker " + buildingInfo.buildingSummary.buildingKey;
                buildingMesh.GetComponent<MeshRenderer>().shadowCastingMode = 0;
                //remove collider from mes.
                Destroy(buildingMesh.GetComponent<Collider>());

                //setup icons for building.
                Material iconMaterial = new Material(iconMarkerMaterial);
                GameObject buildingIcon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                buildingIcon.name = "Building Icon";
                buildingIcon.transform.position = buildingMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, .3f, 0);
                buildingIcon.transform.localScale = new Vector3(sizeMultiplier * iconSize, 0, sizeMultiplier * iconSize);
                buildingIcon.transform.Rotate(0, 0, 180);
                buildingIcon.layer = layerAutomap;
                buildingIcon.GetComponent<MeshRenderer>().material = iconMaterial;
                buildingIcon.GetComponent<MeshRenderer>().shadowCastingMode = 0;
                //remove collider from mes.
                Destroy(buildingMesh.GetComponent<Collider>());

                //sets up text mesh pro object and settings.
                var textObject = new GameObject();
                textObject.AddComponent<TMPro.TextMeshPro>();
                textObject.layer = layerAutomap;
                RectTransform textboxRect = textObject.GetComponent<RectTransform>();
                textObject.GetComponent<TMPro.TextMeshPro>().enableAutoSizing = true;
                textboxRect.sizeDelta = new Vector2(100, 100);
                textObject.GetComponent<TMPro.TextMeshPro>().isOrthographic = true;
                textObject.GetComponent<TMPro.TextMeshPro>().material = iconMaterial;
                textObject.GetComponent<TMPro.TextMeshPro>().material.enableInstancing = true;
                textObject.GetComponent<TMPro.TextMeshPro>().characterSpacing = 5;
                textObject.GetComponent<TMPro.TextMeshPro>().fontSizeMin = 26;
                textObject.GetComponent<TMPro.TextMeshPro>().enableWordWrapping = true;
                textObject.GetComponent<TMPro.TextMeshPro>().fontStyle = TMPro.FontStyles.Bold;
                textObject.transform.position = buildingMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, .3f, 0);
                textObject.transform.localScale = new Vector3(buildingInfo.staticBuilding.size.x * .01f, buildingInfo.staticBuilding.size.x * .01f, buildingInfo.staticBuilding.size.x * .01f);
                textObject.transform.Rotate(new Vector3(90, 0, 0));
                textObject.GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.Center;
                textObject.name = buildingInfo.buildingSummary.BuildingType.ToString() + " Label " + buildingInfo.buildingSummary.buildingKey;
                textObject.GetComponent<TMPro.TextMeshPro>().text = buildingInfo.buildingSummary.BuildingType.ToString();
                textObject.GetComponent<TMPro.TextMeshPro>().color = Color.magenta;
                //remove collider from mes.
                Destroy(textObject.GetComponent<Collider>());
                textObject.SetActive(false);

                if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Tavern)
                {
                    buildingInfo.marker.iconGroup = MarkerGroups.Taverns;
                    buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.205", 0, 0, true, 0);
                    updateMaterials(buildingMesh, iconGroupColors[buildingInfo.marker.iconGroup], iconGroupTransperency[buildingInfo.marker.iconGroup]);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.ClothingStore || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.FurnitureStore || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.GemStore || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.GeneralStore || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.PawnShop || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Bookseller || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Alchemist)
                {
                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.ClothingStore)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.204", 0, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(125, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.FurnitureStore)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.200", 14, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(125, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.GeneralStore)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 24, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(125, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.PawnShop)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.208", 3, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(80, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Bookseller)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.209", 0, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(75, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Bank)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 46, 0, true, 0);
                    }

                    buildingInfo.marker.iconGroup = MarkerGroups.Shops;
                    updateMaterials(buildingMesh, iconGroupColors[buildingInfo.marker.iconGroup], iconGroupTransperency[buildingInfo.marker.iconGroup]);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.WeaponSmith || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Armorer)
                {
                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Armorer)
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.249", 30, 0, true, 0);

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.WeaponSmith)
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 29, 0, true, 0);

                    buildingInfo.marker.iconGroup = MarkerGroups.Blacksmiths;
                    updateMaterials(buildingMesh, iconGroupColors[buildingInfo.marker.iconGroup], iconGroupTransperency[buildingInfo.marker.iconGroup]);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.GuildHall || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Temple || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Library)
                {
                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Library)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.081", 0, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(75, 100);
                    }

                    buildingInfo.marker.iconGroup = MarkerGroups.Utilities;
                    updateMaterials(buildingMesh, iconGroupColors[buildingInfo.marker.iconGroup], iconGroupTransperency[buildingInfo.marker.iconGroup]);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Palace)
                {
                    buildingInfo.marker.iconGroup = MarkerGroups.Government;
                    updateMaterials(buildingMesh, iconGroupColors[buildingInfo.marker.iconGroup], iconGroupTransperency[buildingInfo.marker.iconGroup]);
                    Destroy(buildingIcon);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House1 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House2 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House3 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House4 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House5 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House6)
                {
                    buildingInfo.marker.iconGroup = MarkerGroups.Houses;
                    textObject.GetComponent<TMPro.TextMeshPro>().text = "House";
                    buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.211", 38, 0, true, 0);
                    updateMaterials(buildingMesh, iconGroupColors[buildingInfo.marker.iconGroup], iconGroupTransperency[buildingInfo.marker.iconGroup]);
                }
                else
                {
                    Destroy(buildingIcon);
                    Destroy(textObject);
                }

                buildingInfo.marker.attachedMesh = buildingMesh;
                buildingInfo.marker.attachedLabel = textObject;
                buildingInfo.marker.attachedIcon = buildingIcon;
                buildingInfo.marker.position = new Vector3(buildingInfo.position.x, buildingInfo.position.y + tallestSpot + 10, buildingInfo.position.z);

                Debug.Log("Marker: " + buildingInfo.marker.attachedIcon);

                //turn off the indicator once transperency goes below a level it isn't helpful.
                if (iconGroupTransperency[buildingInfo.marker.iconGroup] > .8f)
                    buildingMesh.SetActive(false);
                else
                    buildingMesh.SetActive(true);
            }
        }

        //updates object, as long as object has a material attached to it to update/apply shader to.
        public static Material updateMaterials(GameObject objectWithMat, Color materialColor, float DistortionBlend)
        {
            //grabbing an alpha render shader for the building material. Needed to override default textures and give solid transperent look.
            Material buildingMarkermaterial = new Material(buildingMarkerMaterial);
            buildingMarkermaterial.SetFloat("_DistortionEnabled", 1f);
            buildingMarkermaterial.SetFloat("_DistortionStrength", 1f);
            buildingMarkermaterial.SetFloat("_DistortionBlend", DistortionBlend);
            buildingMarkermaterial.SetFloat("_Mode", 3f);
            buildingMarkermaterial.SetFloat("__ColorMode", 3);
            buildingMarkermaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            buildingMarkermaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            buildingMarkermaterial.SetInt("_ZWrite", 0);
            buildingMarkermaterial.EnableKeyword("_ALPHABLEND_ON");
            buildingMarkermaterial.EnableKeyword("_COLOROVERLAY_ON");
            buildingMarkermaterial.EnableKeyword("EFFECT_BUMP");
            buildingMarkermaterial.renderQueue = 2000;
            buildingMarkermaterial.color = materialColor;
            //grabbing the individual materials within the building mesh and assigning it to mesh array.
            Material[] buildingMaterials = objectWithMat.GetComponent<Renderer>().sharedMaterials;
            //running through dumped material array to assign each mesh material on model the proper transperency texture.
            for (int i = 0; i < buildingMaterials.Length; i++)
            {
                buildingMaterials[i] = buildingMarkermaterial;
            }
            //assigns material array back to building meshes.
            objectWithMat.GetComponent<Renderer>().materials = buildingMaterials;
            objectWithMat.GetComponent<Renderer>().UpdateGIMaterials();

            return buildingMarkermaterial;
        }

        public void UpdateBuildingMarkers()
        {
            if(buildingInfoCollection == null)
                return;

            foreach(BuildingMarker buildingMarker in buildingInfoCollection)
            {
                updateMaterials(buildingMarker.marker.attachedMesh, iconGroupColors[buildingMarker.marker.iconGroup], iconGroupTransperency[buildingMarker.marker.iconGroup]);
            }
        }

        //makes quick flat 2d colored texture. Used for coloring ui components.
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        //grabs the local dungeon and reveals meshes using hitray calculator.
        void DungeonMinimapCreator()
        {
            Vector3 npcMarkerScale = new Vector3(2, .01f, 2);
            dungeonInstance = GameManager.Instance.DungeonParent;

            //checks if inside a dungeon. If so, do..
            if (dungeonInstance != null)
            {
                //grab parent dungeon object for use.
                //grab the dungeon automap geometry object that holds all the meshes for automap.
                gameobjectAutomap = GameObject.Find("Automap/InteriorAutomap");
                automap = gameobjectAutomap.GetComponent<Automap>();

                if (dungeonObject == null)
                    dungeonObject = GameManager.Instance.InteriorAutomap.transform.Find("GeometryAutomap (Dungeon)").gameObject;

                //if the object holder is not active, do...
                if (dungeonObject.activeSelf == false)
                {
                    //enable automap object and automap switch so it renders on minimap.
                    dungeonObject.SetActive(true);
                    automap.IsOpenAutomap = true;
                }

                //setup a blank raycast checkkr.
                RaycastHit? nearestHit = null;
                //grab nearest raycast hit for none enabled automap meshes and return hit.
                GetRayCastNearestHitOnAutomapLayer(out nearestHit);
                //did it hit a none enabled automap mesh. if so, do...
                if (nearestHit.HasValue)
                {
                    //grab mesh as game object.
                    hitObject = nearestHit.Value.transform.gameObject;
                    //set last hit object and enable mesh.
                    lastHitObject = hitObject;
                    hitObject.transform.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }

        //returns closes raycast hit of not enabled automap mesh layers.
        private void GetRayCastNearestHitOnAutomapLayer(out RaycastHit? nearestHit)
        {
            Ray ray = new Ray(mainCamera.transform.position, Vector3.down);

            RaycastHit[] hits = Physics.RaycastAll(ray, 10, 1 << layerAutomap);

            nearestHit = null;
            float nearestDistance = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                if ((hit.distance < nearestDistance) && (!hit.collider.gameObject.GetComponent<MeshRenderer>().enabled))
                {
                    nearestHit = hit;
                    nearestDistance = hit.distance;
                }
            }
        }

        //grabs streaming world objects, npcs in streaming world objects, and places, sizes, and colors npcs indicator meshes.
        public List<npcMarker> SetupNPCIndicators()
        {
            foreach (npcMarker marker in currentNPCIndicatorCollection)
            {
                if (!marker)
                {
                    currentNPCIndicatorCollection.Remove(marker);
                    Destroy(marker.npcMarkerObject);
                    Destroy(marker);
                }
            }

            //set exterior indicator size and material and grab npc objects for assigning below.
            if (!GameManager.Instance.IsPlayerInside)
            {
                DaggerfallLocation location = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
                mobileNPCArray = location.GetComponentsInChildren<MobilePersonNPC>();
                mobileEnemyArray = location.GetComponentsInChildren<DaggerfallEnemy>();
                flatNPCArray = location.GetComponentsInChildren<StaticNPC>();
            }

            //set inside building interior indicator size and material and grab npc objects for assigning below.
            if (GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon)
            {
                interiorInstance = GameManager.Instance.InteriorParent;
                flatNPCArray = interiorInstance.GetComponentsInChildren<StaticNPC>();
                mobileNPCArray = interiorInstance.GetComponentsInChildren<MobilePersonNPC>();
                mobileEnemyArray = interiorInstance.GetComponentsInChildren<DaggerfallEnemy>();
            }

            //set dungeon interior indicator size and material and grab npc objects for assigning below.
            if (GameManager.Instance.IsPlayerInside && GameManager.Instance.IsPlayerInsideDungeon)
            {
                interiorInstance = GameManager.Instance.InteriorParent;
                dungeonInstance = GameManager.Instance.DungeonParent;
                flatNPCArray = dungeonInstance.GetComponentsInChildren<StaticNPC>();
                mobileNPCArray = dungeonInstance.GetComponentsInChildren<MobilePersonNPC>();
                mobileEnemyArray = dungeonInstance.GetComponentsInChildren<DaggerfallEnemy>();
            }

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (MobilePersonNPC mobileNPC in mobileNPCArray)
            {
                float addMarkerRandomizer = UnityEngine.Random.Range(0.0f, 1.0f);
                float time =+ Time.deltaTime;
                npcMarker npcMarkerObject = mobileNPC.GetComponent<npcMarker>();

                if (!npcMarkerObject && time > addMarkerRandomizer)
                {
                    npcMarker newNPCMarker = mobileNPC.gameObject.AddComponent<npcMarker>();
                    newNPCMarker.name = "Minimap " + mobileNPC.NameNPC;
                    currentNPCIndicatorCollection.Add(newNPCMarker);
                }
            }

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (DaggerfallEnemy mobileEnemy in mobileEnemyArray)
            {
                float addMarkerRandomizer = UnityEngine.Random.Range(0.0f, 1.0f);
                float time = +Time.deltaTime;

                npcMarker npcMarkerObject = mobileEnemy.GetComponent<npcMarker>();

                if (!npcMarkerObject && time > addMarkerRandomizer)
                {
                    npcMarker newNPCMarker = mobileEnemy.gameObject.AddComponent<npcMarker>();
                    currentNPCIndicatorCollection.Add(newNPCMarker);
                }
            }

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (StaticNPC staticNPC in flatNPCArray)
            {
                float addMarkerRandomizer = UnityEngine.Random.Range(0.0f, 1.0f);
                float time = +Time.deltaTime;

                npcMarker npcMarkerObject = staticNPC.GetComponent<npcMarker>();

                if (!npcMarkerObject && time > addMarkerRandomizer)
                {
                    npcMarker newNPCMarker = staticNPC.gameObject.AddComponent<npcMarker>();
                    currentNPCIndicatorCollection.Add(newNPCMarker);
                }
            }

            return currentNPCIndicatorCollection;
        }

        public void UpdateNpcMarkers()
        {
            if (currentNPCIndicatorCollection == null)
                return;

            bool isInside = GameManager.Instance.IsPlayerInside;
            Vector3 markerScale = new Vector3();

            foreach (npcMarker npcMarkerObject in currentNPCIndicatorCollection)
            {
                if (npcMarkerObject.marker.markerObject == null)
                    continue;

                if (isInside)
                {
                    markerScale = new Vector3(indicatorSize, .01f, indicatorSize);
                    npcMarkerObject.marker.markerObject.transform.localScale = markerScale;
                    indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .05f, 1, 6f);
                    minimapCameraHeight = GameManager.Instance.PlayerMotor.FindGroundPosition().y + 2f;
                }
                else
                {
                    markerScale = new Vector3(indicatorSize, .01f, indicatorSize);
                    npcMarkerObject.marker.markerObject.transform.localScale = markerScale;
                    indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .06f, 1, 7);
                    minimapCameraHeight = 800;
                }

                npcMarkerObject.marker.isActive = iconGroupActive[npcMarkerObject.marker.markerType];
                npcMarkerObject.marker.npcMarkerMaterial.color = iconGroupColors[npcMarkerObject.marker.markerType];
            }
        }
        //updates the current indicator view.
        void UpdateIndicatorView(bool labelIndicatorActive, bool iconIndicatorActive)
        {
            //checks to see if there are any markers with their information in the collection.
            if (buildingInfoCollection != null)
            {
                //if there are markers, check each building/markerinfo
                foreach (BuildingMarker buildingMarker in buildingInfoCollection)
                {
                    //if the marker group type is set to active in markerGroupActive, then pass through smart view triggers.
                    if (iconGroupActive[buildingMarker.marker.iconGroup])
                    {
                        if (buildingMarker.marker.attachedLabel != null)
                            buildingMarker.marker.attachedLabel.SetActive(labelIndicatorActive);

                        if (buildingMarker.marker.attachedIcon != null)
                            buildingMarker.marker.attachedIcon.SetActive(iconIndicatorActive);
                    }
                    //if the marker group type is disabled, disable both the icon and label attached.
                    else
                    {
                        if (buildingMarker.marker.attachedLabel != null)
                            buildingMarker.marker.attachedLabel.SetActive(false);

                        if (buildingMarker.marker.attachedIcon != null)
                            buildingMarker.marker.attachedIcon.SetActive(false);
                    }
                }
            }

        }

        //sets up and updates minimap camera.
        public void SetupMinimapCameras()
        {
            var cameraPos = new Vector3(mainCamera.transform.position.x, 0, mainCamera.transform.position.z);
            //setup the minimap overhead camera position depending on if player is inside or out, and whetjer its a dungeon or not.
            if (GameManager.Instance.IsPlayerInside)
            {
                //sets minimap camera.
                cameraPos.x = mainCamera.transform.position.x + minimapCameraX;
                //finds ground position and offest camera
                cameraPos.y = GameManager.Instance.PlayerMotor.FindGroundPosition().y + 2f;
                cameraPos.z = mainCamera.transform.position.z + minimapCameraZ;

                if (GameManager.Instance.PlayerMotor.IsGrounded)
                    minimapCamera.nearClipPlane = nearClipValue - 1.2f;

                minimapCamera.farClipPlane = 2.85f + farClipValue;
                minimapCamera.orthographicSize = minimapViewSize + 13f;
                minimapCamera.cullingMask = LayerMask.NameToLayer("Everything");
                minimapCamera.renderingPath = RenderingPath.VertexLit;

                if (GameManager.Instance.IsPlayerInsideDungeon)
                {
                    minimapCamera.cullingMask = 1 << layerAutomap;
                    cameraPos.y = gameObjectPlayerAdvanced.transform.position.y + 2f;
                    minimapCamera.nearClipPlane = nearClipValue;
                    minimapCamera.farClipPlane = farClipValue + 4;
                }
            }
            else
            {
                cameraPos.x = mainCamera.transform.position.x + minimapCameraX;
                cameraPos.y = 800;
                cameraPos.z = mainCamera.transform.position.z + minimapCameraZ;
                minimapCamera.orthographicSize = minimapViewSize + 50;
                minimapCamera.nearClipPlane = 0.3f + nearClipValue;
                minimapCamera.farClipPlane = 5000 + farClipValue;
                minimapCamera.cullingMask = LayerMask.NameToLayer("Everything");
                minimapCamera.renderingPath = RenderingPath.UsePlayerSettings;
            }

            //update camera position with above calculated position.
            minimapCamera.transform.position = cameraPos;

            //setup the camera angle/view point.
            var cameraRot = transform.rotation;
            cameraRot.x = minimapAngle;
            minimapCamera.transform.rotation = cameraRot;

            //setup the minimap mask layer size/position in top right corner.
            maskRectTransform.sizeDelta = new Vector2(minimapSize * 1.03f, minimapSize*1.03f);

            //setup the minimap render layer size/position in top right corner.
            canvasRectTransform.sizeDelta = new Vector2(minimapSize, minimapSize);

            //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            minimapInterfaceRectTransform.sizeDelta = new Vector2(minimapSize * 1.03f, minimapSize * 1.13f);

            //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            minimapDirectionsRectTransform.sizeDelta = new Vector2(minimapSize * .7f, minimapSize * .7f);

            if (fullMinimapMode)
            {
                //setup the minimap mask layer size/position in top right corner.
                maskRectTransform.anchoredPosition3D = new Vector3((Screen.width * .5f) * -1, (Screen.height * .51f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapInterfaceRectTransform.anchoredPosition3D = new Vector3((Screen.width * .5f) * -1, (Screen.height * .415f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            }
            else
            {
                //setup the minimap mask layer size/position in top right corner.
                maskRectTransform.anchoredPosition3D = new Vector3((minimapSize * .455f) * -1, (minimapSize * .455f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapInterfaceRectTransform.anchoredPosition3D = new Vector3((minimapSize * .46f) * -1, (minimapSize * .365f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            }

            //tie the minimap rotation to the players view rotation using eulerAngles.
            var minimapRot = transform.eulerAngles;
            if (minimapControls.autoRotateActive)
            {
                minimapRot.z = GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y;
            }
            else
                minimapRot.z = minimapControls.minimapRotationValue;

            minimapDirectionsRectTransform.transform.eulerAngles = minimapRot;
            canvasRectTransform.transform.eulerAngles = minimapRot;

            //force transform updates.
            minimapDirectionsRectTransform.ForceUpdateRectTransforms();
            canvasRectTransform.ForceUpdateRectTransforms();
            maskRectTransform.ForceUpdateRectTransforms();
        }

        public void SetupPlayerIndicator()
        {
            //setup and place/rotate player arrow mesh for minimap. Use automap layer only.
            if (!gameobjectPlayerMarkerArrow)
            {
                gameobjectPlayerMarkerArrow = GameObjectHelper.CreateDaggerfallMeshGameObject(99900, GameManager.Instance.PlayerEntityBehaviour.transform, false, null, true);
                Destroy(gameobjectPlayerMarkerArrow.GetComponent<MeshCollider>());
                gameobjectPlayerMarkerArrow.name = "PlayerMarkerArrow";
                gameobjectPlayerMarkerArrow.layer = layerAutomap;
                updateMaterials(gameobjectPlayerMarkerArrow, Color.yellow, 0f);
            }

            //tie player arrow to player position and rotation.
            if (GameManager.Instance.PlayerMotor.IsGrounded)
                PlayerHeightChanger = GameManager.Instance.PlayerEntityBehaviour.transform.position.y + playerIndicatorHeight + .1f;           

            //adjust player marker size to make up for camera view size adjustments when inside or outside.
            if (GameManager.Instance.IsPlayerInside)
                gameobjectPlayerMarkerArrow.transform.localScale = new Vector3(indicatorSize, indicatorSize, indicatorSize);
            else
                gameobjectPlayerMarkerArrow.transform.localScale = new Vector3(indicatorSize, indicatorSize, indicatorSize);

            //continually updates player arrow marker position and rotation.
            gameobjectPlayerMarkerArrow.transform.position = new Vector3(GameManager.Instance.PlayerActivate.transform.position.x, PlayerHeightChanger, GameManager.Instance.PlayerEntityBehaviour.transform.position.z);
            gameobjectPlayerMarkerArrow.transform.rotation = GameManager.Instance.PlayerEntityBehaviour.transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            UnityEngine.Profiling.Profiler.BeginSample("Minimap Updates");
            if (consoleController.ui.isConsoleOpen || GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return;

            if (!minimapActive)
            {
                publicMinimap.SetActive(false);
                publicMinimapRender.SetActive(false);
                publicCompass.SetActive(false);
                publicDirections.SetActive(false);
            }
            else
            {
                publicMinimap.SetActive(true);
                publicMinimapRender.SetActive(true);
                publicCompass.SetActive(true);
                publicDirections.SetActive(true);
            }

            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fps = 1.0f / deltaTime;

            KeyPressCheck();
            npcIndicatorCollection = SetupNPCIndicators();

            if (lastIndicatorSize != indicatorSize || GameManager.Instance.PlayerEnterExit)
            {
                UpdateNpcMarkers();
            }


            //grab the current location name to check if locations have changed. Has to use seperate grab for every location type.
            if (!GameManager.Instance.IsPlayerInside)
            {
                currentLocationName = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.name;
            }
            else if (GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon)
            {
                currentLocationName = GameManager.Instance.InteriorParent.name;
            }
            else if (GameManager.Instance.IsPlayerInside && GameManager.Instance.IsPlayerInsideDungeon)
            {
                currentLocationName = GameManager.Instance.DungeonParent.name;
            }

            //compare current location name with last, and if not the same and if there are buildings, destroy automap, building, and labels and rebuild building gui.
            if (currentLocationName != lastLocationName)
            {
                if (!GameManager.Instance.IsPlayerInside)
                    SetupBuildingIndicators();

                lastLocationName = currentLocationName;
            }

            //if plyaer has smart view active....
            if (minimapControls.smartViewActive)
            {
                //check the camera zoom size, if greater than 50 and the size has changed, setup indicators using triggers.
                if (minimapCamera.orthographicSize > 50 && minimapCamera.orthographicSize != minimapViewSize)
                {
                    //if all icons are active, run normal smart view enabled all icons and no labels.
                    if (minimapControls.iconsIndicatorActive)
                        UpdateIndicatorView(false, minimapControls.iconsIndicatorActive);
                    //If they have all icons turned off, turn all icons and labels off.
                    else
                        UpdateIndicatorView(false, false);
                }
                else if (minimapCamera.orthographicSize < 50 && minimapCamera.orthographicSize != minimapViewSize)
                {
                    if (minimapControls.labelIndicatorActive)
                        UpdateIndicatorView(minimapControls.labelIndicatorActive, false);
                    else
                        UpdateIndicatorView(false, false);
                }

                //if smartview is off, use standard bool triggers setup in the menu to decide what is shown and not.
                else if (!minimapControls.smartViewActive && minimapCamera.orthographicSize != minimapViewSize)
                {
                    UpdateIndicatorView(minimapControls.labelIndicatorActive, minimapControls.iconsIndicatorActive);
                }
            }

            //if player is inside, this runs continually to create the minimap automap by hijacking the automap. If this doesn't update, dungeon minimap revealed geometry won't update.
            if (GameManager.Instance.IsPlayerInsideDungeon)
                DungeonMinimapCreator();

            SetupPlayerIndicator();

            //always running to check and update player minimap camera. Ens
            SetupMinimapCameras();

            UnityEngine.Profiling.Profiler.EndSample();
        }

        #region TextureLoader
        //texture loading method. Grabs the string path the developer inputs, finds the file, if exists, loads it,
        //then resizes it for use. If not, outputs error message.
        public Texture2D LoadPNG(string filePath)
        {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            else
                Debug.Log("FilePath Broken!");

            return tex;
        }
        #endregion

        public object NewSaveData()
        {
            return new MyModSaveData
            {
                IconGroupColors = new Dictionary<MarkerGroups, Color>(),
                IconGroupTransperency = new Dictionary<MarkerGroups, float>(),
                IconGroupActive = new Dictionary<MarkerGroups, bool>(),
                MinimapSizeMult = .25f,
                MinimapViewSize = 40,
                MinimapCameraHeight = 100,
                MinimapRotationValue = 0,
                AlphaValue = 0,
                MinimapSensingRadius = 35f,
                LabelIndicatorActive = true,
                SmartViewActive = true,
                IconsIndicatorActive = true,
                RealDetectionEnabled = true,
                CameraDetectionEnabled = true
            };
        }

        public object GetSaveData()
        {
            return new MyModSaveData
            {
                IconGroupColors = iconGroupColors,
                IconGroupTransperency = iconGroupTransperency,
                IconGroupActive = iconGroupActive,
                MinimapSizeMult = minimapSize,
                MinimapViewSize = minimapViewSize,
                MinimapCameraHeight = minimapCameraHeight,
                MinimapRotationValue = minimapControls.minimapRotationValue,
                AlphaValue = minimapControls.blendValue,
                MinimapSensingRadius = minimapSensingRadius,
                LabelIndicatorActive = minimapControls.labelIndicatorActive,
                SmartViewActive = minimapControls.smartViewActive,
                IconsIndicatorActive = minimapControls.iconsIndicatorActive,
                RealDetectionEnabled = minimapControls.realDetectionEnabled,
                CameraDetectionEnabled = minimapControls.cameraDetectionEnabled,
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var myModSaveData = (MyModSaveData)saveData;
            iconGroupColors = myModSaveData.IconGroupColors;
            iconGroupTransperency = myModSaveData.IconGroupTransperency;
            iconGroupActive = myModSaveData.IconGroupActive;
            minimapSize = myModSaveData.MinimapSizeMult;
            minimapViewSize = myModSaveData.MinimapViewSize;
            minimapCameraHeight = myModSaveData.MinimapCameraHeight;
            minimapControls.minimapRotationValue = myModSaveData.MinimapRotationValue;
            minimapControls.blendValue = myModSaveData.AlphaValue;
            minimapSensingRadius = myModSaveData.MinimapSensingRadius;
            minimapControls.labelIndicatorActive = myModSaveData.LabelIndicatorActive;
            minimapControls.smartViewActive = myModSaveData.SmartViewActive;
            minimapControls.iconsIndicatorActive = myModSaveData.IconsIndicatorActive;
            minimapControls.realDetectionEnabled = myModSaveData.RealDetectionEnabled;
            minimapControls.cameraDetectionEnabled = myModSaveData.CameraDetectionEnabled;
        }
    }
}