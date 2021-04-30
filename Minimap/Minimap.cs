using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using UnityEngine.UI;
using System;

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
        public GameObject minimapCamera;
        public GameObject minimapCanvas;

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
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            minimapCamera = mod.GetAsset<GameObject>("MinimapCamera");
            minimapCanvas = mod.GetAsset<GameObject>("Canvas");
            
            minimapTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
            minimapTexture.Create();

            minimapCamera.GetComponent<Camera>().targetTexture = minimapTexture;
            minimapCanvas.GetComponentInChildren<RawImage>().texture = minimapTexture;

            minimapCamera = Instantiate(minimapCamera);
            Instantiate(minimapCanvas);            
        }

        // Update is called once per frame
        void Update()
        {
            var pos = mainCamera.transform.position;
            pos.x = mainCamera.transform.position.x;
            pos.y = mainCamera.transform.position.y + 500f;
            pos.z = mainCamera.transform.position.z;
            minimapCamera.transform.position = pos;
        }
    }
}
