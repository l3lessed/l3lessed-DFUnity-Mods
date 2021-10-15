using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DaggerfallWorkshop.Game.RandomVariations
{
    public class VariedAnimalsControls : MonoBehaviour
    {
        private static Mod mod;
        [SerializeField]
        public static VariedAnimalsControls VariedAnimalsControlsInstance { get; private set; }
        public List<Texture2D> HorseTextureList = new List<Texture2D>();
        public List<Texture2D> CamelTextureList = new List<Texture2D>();
        public List<Texture2D> PigTextureList = new List<Texture2D>();
        public List<Texture2D> CowTextureList = new List<Texture2D>();
        public AnimationClip[] animations = new AnimationClip[100];
        public int TexturesTotal;
        public Animation tailSwipeAnimation = new Animation();
        public GameObject horsePrefab2010;
        public string currentLocationName;
        public string lastLocationName;
        public List<GameObject> flatsList;
        public Animation[] Animations;

        //starts mod manager on game begin. Grabs mod initializing paramaters.
        //ensures SateTypes is set to .Start for proper save data restore values.
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            //Below code blocks set up instances of class/script/mod.\\
            //sets up and runs this script file as the main mod file, so it can setup all the other scripts for the mod.
            GameObject VariedAnimalsControls = new GameObject("VariedAnimalsControls");
            VariedAnimalsControlsInstance = VariedAnimalsControls.AddComponent<VariedAnimalsControls>();
            Debug.Log("Learning of Animals");

            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //assets = mod.LoadAllAssetsFromBundle();
            //initiates save paramaters for class/script.
            //mod.SaveDataInterface = instance;
            //after finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
        }

        private void Start()
        {           
            //count, loop through, and load horse textures into a list for grabbing later.
            DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/VariedFaithful3DAnimals/horse");
            FileInfo[] horseFileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in horseFileInfoArray)
            {
                Texture2D singleTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/VariedFaithful3DAnimals/horse/" + textureFile.Name);

                if (singleTexture == null)
                    return;

                HorseTextureList.Add(singleTexture);
            }

            DirectoryInfo camelDirectory = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/VariedFaithful3DAnimals/camel");
            FileInfo[] camelFileInfoArray = camelDirectory.GetFiles("*.png");
            foreach(FileInfo textureFile in camelFileInfoArray)
            {
                Texture2D singleTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/VariedFaithful3DAnimals/camel/" + textureFile.Name);

                if (singleTexture == null)
                    return;

                CamelTextureList.Add(singleTexture);
            }

            DirectoryInfo cowDirectory = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/VariedFaithful3DAnimals/cow");
            FileInfo[] cowFileInfoArray = cowDirectory.GetFiles("*.png");
            foreach (FileInfo textureFile in cowFileInfoArray)
            {
                Texture2D singleTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/VariedFaithful3DAnimals/cow/" + textureFile.Name);

                if (singleTexture == null)
                    return;

                CowTextureList.Add(singleTexture);
            }

            DirectoryInfo pigDirectory = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/VariedFaithful3DAnimals/pig");
            FileInfo[] pigFileInfoArray = pigDirectory.GetFiles("*.png");
            foreach (FileInfo textureFile in pigFileInfoArray)
            {
                Texture2D singleTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/VariedFaithful3DAnimals/pig/" + textureFile.Name);

                if (singleTexture == null)
                    return;

                PigTextureList.Add(singleTexture);
            }

            Debug.Log("Animals Analyzed, Stored, and Ready for Loading");
        }

        private void Update()
        {
            if (!GameManager.Instance.IsPlayerInside && !GameManager.Instance.StreamingWorld.IsInit && GameManager.Instance.StreamingWorld.IsReady)
            {
                if (GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject != null)
                    currentLocationName = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.Summary.LocationName + GameManager.Instance.StreamingWorld.MapPixelX.ToString() + GameManager.Instance.StreamingWorld.MapPixelY.ToString();
                else
                    currentLocationName = "Wilderness " + GameManager.Instance.StreamingWorld.MapPixelX.ToString() + GameManager.Instance.StreamingWorld.MapPixelY.ToString();

                if (currentLocationName != lastLocationName)
                {
                    SetupNPCIndicators();
                    lastLocationName = currentLocationName;
                }
            }
        }

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


        public static long DirCount(DirectoryInfo d)
        {
            long i = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                if (fi.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                    i++;
            }
            return i;
        }

        //grabs streaming world objects, npcs in streaming world objects, and places, sizes, and colors npcs indicator meshes.
        public void SetupNPCIndicators()
        {
            flatsList = new List<GameObject>(Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Flats"));

            foreach (GameObject singleObject in flatsList)
            {
                Animations = singleObject.GetComponentsInChildren<Animation>();

                foreach (Animation singleanimation in Animations)
                {
                        singleanimation.gameObject.AddComponent<HorseController>();
                }
            }
        }
    }
}
