using DaggerfallWorkshop.Game;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using System.Linq;

namespace Minimap
{
    public class NPCManager : MonoBehaviour
    {
        public float npcUpdateTimer;
        public float npcUpdateInterval;
        public GameObject location;
        public static List<MobilePersonNPC> mobileNPCArray = new List<MobilePersonNPC>();
        public static List<DaggerfallEnemy> mobileEnemyArray = new List<DaggerfallEnemy>();
        public GameObject interiorInstance;
        public GameObject dungeonInstance;
        public static List<StaticNPC> flatNPCArray = new List<StaticNPC>();
        public static List<npcMarker> currentNPCIndicatorCollection = new List<npcMarker>();
        public int totalNPCs;

        void Update()
        {
            //stop update loop if any of the below is happening.
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            npcUpdateTimer += Time.fixedDeltaTime;
            if (npcUpdateTimer > Minimap.MinimapInstance.npcUpdateInterval)
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
                if (!mobileEnemy.GetComponent<npcMarker>())
                {
                    float addMarkerRandomizer = UnityEngine.Random.Range(0.0f, 0.5f);
                    float time = +Time.deltaTime;
                    if (time > addMarkerRandomizer)
                    {
                        npcMarker newNPCMarker = mobileEnemy.gameObject.AddComponent<npcMarker>();
                        currentNPCIndicatorCollection.Add(newNPCMarker);
                    }
                }
            }

            flatNPCArray.RemoveAll(item => item == null);

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (StaticNPC staticNPC in flatNPCArray)
            {

                if (!staticNPC.GetComponent<npcMarker>())
                {
                    float addMarkerRandomizer = UnityEngine.Random.Range(0.0f, 0.5f);
                    float time = +Time.deltaTime;
                    if (time > addMarkerRandomizer)
                    {
                        npcMarker newNPCMarker = staticNPC.gameObject.AddComponent<npcMarker>();
                        currentNPCIndicatorCollection.Add(newNPCMarker);
                    }
                }
            }

            mobileNPCArray.RemoveAll(item => item == null);

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (MobilePersonNPC mobileNPC in mobileNPCArray)
            {
                if (!mobileNPC.GetComponent<npcMarker>())
                {
                    float addMarkerRandomizer = UnityEngine.Random.Range(0.0f, 0.5f);
                    float time = +Time.deltaTime;
                    if (time > addMarkerRandomizer)
                    {
                        npcMarker newNPCMarker = newNPCMarker = mobileNPC.gameObject.AddComponent<npcMarker>();
                        currentNPCIndicatorCollection.Add(newNPCMarker);
                    }
                }
            }
        }
    }
}

