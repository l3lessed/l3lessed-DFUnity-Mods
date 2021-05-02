using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using UnityEngine.UI;
using System;
using System.IO;

namespace DaggerfallWorkshop.Game.Minimap
{
    public class Minimap : MonoBehaviour
    {
        private static Mod mod;
        private static Minimap instance;
        private static ModSettings settings;
        public GameObject mainCamera;
        public RenderTexture minimapTexture;

        private Transform _player;

        //Prefab objects.
        public Camera minimapCamera;
        public GameObject minimapMask;
        public GameObject minimapCanvas;
        public GameObject minimap;
        public GameObject minimapRenderTexture;
        public float minimapCameraHeight = 60;
        public float minimapCameraX;
        public float minimapCameraZ;
        public float minimapViewSize = 40;
        [SerializeField] public float minimapSize = 200;

        private new RectTransform maskRectTransform;
        private new RectTransform canvasRectTransform;
        public float minimapAngle = .95f;
        public float minimapminimapRotationZ;
        public float minimapminimapRotationY;

        //starts mod manager on game begin. Grabs mod initializing paramaters.
        //ensures SateTypes is set to .Start for proper save data restore values.
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            //sets up instance of class/script/mod.
            GameObject go = new GameObject("Minimap");
            instance = go.AddComponent<Minimap>();
            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //initates mod settings
            settings = mod.GetSettings();
            //after finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
            Debug.Log("Minimap MOD STARTED!");
        }

        // Start is called before the first frame update
        void Start()
        {
            //setup needed objects.
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            minimapCamera = mod.GetAsset<GameObject>("MinimapCamera").GetComponent<Camera>();
            minimap = mod.GetAsset<GameObject>("Minimap");

            //initiate minimap view camera and minimap canvas layer.
            minimapCamera = Instantiate(minimapCamera);
            minimap = Instantiate(minimap);

            //grab and assign the minimap canvas and mask layers.
            minimapMask = minimap.transform.Find("Minimap Mask").gameObject;
            minimapCanvas = minimapMask.transform.Find("MinimapCanvas").gameObject;

            //create and assigned a new render texture for passing camera view into texture.
            minimapTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
            minimapTexture.Create();

            //assign the camera view and the render texture output.
            minimapCamera.targetTexture = minimapTexture;
            //assign the mask layer texture to the minimap canvas mask layer.
            minimapMask.GetComponentInChildren<RawImage>().texture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/minimap/MinimapMask.png");
            //assign the canvas texture to the minimap render texture.
            minimapCanvas.GetComponentInChildren<RawImage>().texture = minimapTexture;

            //grab the mask and canvas layer rect transforms of the minimap object.
            maskRectTransform = minimapMask.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            canvasRectTransform = minimapCanvas.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            //setup the minimap overhead camera position.
            var cameraPos = mainCamera.transform.position;
            cameraPos.x = mainCamera.transform.position.x + minimapCameraX;
            cameraPos.y = mainCamera.transform.position.y + minimapCameraHeight;
            cameraPos.z = mainCamera.transform.position.z + minimapCameraZ;
            minimapCamera.transform.position = cameraPos;

            //setup the minimap zoom.
            minimapCamera.orthographicSize = minimapViewSize;

            //setup the camera rotation.
            var cameraRot = transform.rotation;
            cameraRot.x = minimapAngle;
            minimapCamera.transform.rotation = cameraRot;

            //setup the minimap mask layer size/position in top right corner.
            maskRectTransform.sizeDelta = new Vector2(minimapSize, minimapSize);
            maskRectTransform.anchoredPosition3D = new Vector3((minimapSize * .55F) * -1, (minimapSize * .55F) * -1, 0);

            //setup the minimap render layer size/position in top right corner.
            canvasRectTransform.sizeDelta = new Vector2(minimapSize, minimapSize);
            canvasRectTransform.anchoredPosition3D = new Vector3((minimapSize / 2) * -1, (minimapSize / 2) * -1, 0);

            //tie the minimap rotation to the players view rotation using eulerAngles.
            var minimapRot = transform.eulerAngles;
            minimapRot.z = GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y;
            canvasRectTransform.transform.eulerAngles = minimapRot;

            //force transform updates.
            canvasRectTransform.ForceUpdateRectTransforms();
            maskRectTransform.ForceUpdateRectTransforms();
        }

        #region TextureLoader
        //texture loading method. Grabs the string path the developer inputs, finds the file, if exists, loads it,
        //then resizes it for use. If not, outputs error message.
        public static Texture2D LoadPNG(string filePath)
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
    }
}
