using DaggerfallWorkshop.Game;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using System.Linq;
using System.IO;
using DaggerfallWorkshop.Utility.AssetInjection;

namespace Minimap
{
    public class NPCManager : MonoBehaviour
    {
        public float npcUpdateTimer;
        public float npcUpdateInterval;
        public GameObject location;
        public List<MobilePersonNPC> mobileNPCArray = new List<MobilePersonNPC>();
        public List<DaggerfallEnemy> mobileEnemyArray = new List<DaggerfallEnemy>();
        public GameObject interiorInstance;
        public GameObject dungeonInstance;
        public List<StaticNPC> flatNPCArray = new List<StaticNPC>();
        public List<NPCMarker> currentNPCIndicatorCollection = new List<NPCMarker>();
        public int totalNPCs;

        #region GameTextureArrays
        public static int[] maleRedguardTextures = new int[] { 381, 382, 383, 384 };
        public static int[] femaleRedguardTextures = new int[] { 395, 396, 397, 398 };

        public static int[] maleNordTextures = new int[] { 387, 388, 389, 390 };
        public static int[] femaleNordTextures = new int[] { 392, 393, 451, 452 };

        public static int[] maleBretonTextures = new int[] { 385, 386, 391, 394 };
        public static int[] femaleBretonTextures = new int[] { 453, 454, 455, 456 };

        public int[] guardTextures = { 399 };
        public Dictionary<Minimap.MarkerGroups, Material> npcIconMaterialDict = new Dictionary<Minimap.MarkerGroups, Material>();
        public Texture2D npcDotTexture;
        #endregion

        private void Awake()
        {
            npcDotTexture = null;
            byte[] fileData;

            fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/npcDot.png");
            npcDotTexture = new Texture2D(2, 2);
            npcDotTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        private void Start()
        {
            Material friendliesIconMaterial = new Material(Minimap.buildingMarkerMaterial);
            Material enemiesIconMaterial = new Material(Minimap.buildingMarkerMaterial);
            Material residentIconMaterial = new Material(Minimap.buildingMarkerMaterial);

            npcIconMaterialDict = new Dictionary<Minimap.MarkerGroups, Material>()
            {
                {Minimap.MarkerGroups.Friendlies, friendliesIconMaterial },
                {Minimap.MarkerGroups.Enemies, enemiesIconMaterial },
                {Minimap.MarkerGroups.Resident, residentIconMaterial },
                {Minimap.MarkerGroups.None, residentIconMaterial }
            };
        }

        void Update()
        {           
            //stop update loop if any of the below is happening.
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            npcUpdateTimer += Time.fixedDeltaTime;
            if (npcUpdateTimer > Minimap.MinimapInstance.npcCellUpdateInterval)
            {
                npcUpdateTimer = 0;
                //set exterior indicator size and material and grab npc objects for assigning below.
                if (!GameManager.Instance.IsPlayerInside)
                {
                    location = GameManager.Instance.PlayerEnterExit.ExteriorParent;
                    mobileNPCArray = location.GetComponentsInChildren<MobilePersonNPC>().ToList();
                    mobileEnemyArray = location.GetComponentsInChildren<DaggerfallEnemy>().ToList();
                    flatNPCArray = location.GetComponentsInChildren<StaticNPC>().ToList();
                }
                //set inside building interior indicator size and material and grab npc objects for assigning below.
                else if (GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon)
                {
                    interiorInstance = GameManager.Instance.InteriorParent;
                    flatNPCArray = interiorInstance.GetComponentsInChildren<StaticNPC>().ToList();
                    mobileNPCArray = interiorInstance.GetComponentsInChildren<MobilePersonNPC>().ToList();
                    mobileEnemyArray = interiorInstance.GetComponentsInChildren<DaggerfallEnemy>().ToList();
                }

                //set dungeon interior indicator size and material and grab npc objects for assigning below.
                else if (GameManager.Instance.IsPlayerInside && GameManager.Instance.IsPlayerInsideDungeon)
                {
                    dungeonInstance = GameManager.Instance.DungeonParent;
                    flatNPCArray = dungeonInstance.GetComponentsInChildren<StaticNPC>().ToList();
                    mobileNPCArray = dungeonInstance.GetComponentsInChildren<MobilePersonNPC>().ToList();
                    mobileEnemyArray = dungeonInstance.GetComponentsInChildren<DaggerfallEnemy>().ToList();
                }

                //count all npcs in the seen to get the total amount.
                totalNPCs = flatNPCArray.Count + mobileEnemyArray.Count + mobileNPCArray.Count;
            }

            //if the total amount of npcs match the indicator collection total, stop code execution and return from routine.
            if (totalNPCs == currentNPCIndicatorCollection.Count)
                return;

            currentNPCIndicatorCollection.RemoveAll(item => item == null);

            mobileEnemyArray.RemoveAll(item => item == null);

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (DaggerfallEnemy mobileEnemy in mobileEnemyArray)
            {
                if (!mobileEnemy.GetComponent<NPCMarker>() && mobileEnemy.isActiveAndEnabled)
                {
                    NPCMarker newNPCMarker = mobileEnemy.gameObject.AddComponent<NPCMarker>();
                    currentNPCIndicatorCollection.Add(newNPCMarker);
                }
            }

            flatNPCArray.RemoveAll(item => item == null);

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (StaticNPC staticNPC in flatNPCArray)
            {

                if (!staticNPC.GetComponent<NPCMarker>())
                {
                    NPCMarker newNPCMarker = staticNPC.gameObject.AddComponent<NPCMarker>();
                    currentNPCIndicatorCollection.Add(newNPCMarker);
                }
            }

            mobileNPCArray.RemoveAll(item => item == null);

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (MobilePersonNPC mobileNPC in mobileNPCArray)
            {
                if (!mobileNPC.GetComponent<NPCMarker>())
                {
                    NPCMarker newNPCMarker = newNPCMarker = mobileNPC.gameObject.AddComponent<NPCMarker>();
                    currentNPCIndicatorCollection.Add(newNPCMarker);
                }
            }
        }
    }
}

