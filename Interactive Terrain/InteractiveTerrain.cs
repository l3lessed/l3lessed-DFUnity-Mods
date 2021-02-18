using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InteractiveTerrain : MonoBehaviour
{
    private PlayerSpeedChanger playerSpeedChanger;
    private float walkspeed;
    private float runspeed;
    private int lastTile;
    private static Mod mod;
    private static ModSettings settings;
    private int currentTile;

    public static InteractiveTerrain InteractiveTerrainInstance { get; private set; }

    //starts mod manager on game begin. Grabs mod initializing paramaters.
    //ensures SateTypes is set to .Start for proper save data restore values.
    [Invoke(StateManager.StateTypes.Game, 0)]
    public static void Init(InitParams initParams)
    {
        //Below code blocks set up instances of class/script/mod.\\
        //sets up and runs this script file as the main mod file, so it can setup all the other scripts for the mod.
        GameObject InteractiveTerrain = new GameObject("InteractiveTerrain");
        InteractiveTerrainInstance = InteractiveTerrain.AddComponent<InteractiveTerrain>();
        Debug.Log("You pull all your equipment out and begin preparing for the journey ahead.");

        //initiates mod paramaters for class/script.
        mod = initParams.Mod;
        //loads mods settings.
        settings = mod.GetSettings();
        //assets = mod.LoadAllAssetsFromBundle();
        //initiates save paramaters for class/script.
        //mod.SaveDataInterface = instance;
        //after finishing, set the mod's IsReady flag to true.
        mod.IsReady = true;
        Debug.Log("You Fasten your boots and feel the ground crunch under your feet.");
    }

    // Start is called before the first frame update
    void Start()
    {
        playerSpeedChanger = GameManager.Instance.SpeedChanger;
        //sets to -2 to ensure it updates the tile object on every relaunch of mod.
        lastTile = -2;

    }

    // Update is called once per frame
    void Update()
    {
        if (OnTileChange())
        {
            playerSpeedChanger.useRunSpeedOverride = true;
            playerSpeedChanger.useWalkSpeedOverride = true;
            List<float> TileProperties = TileProperty(lastTile);
            movementModifier(TileProperties[0]);
            Debug.Log("New Ground Tile: " + lastTile.ToString() + " | " + TileProperties[0].ToString());
        }
    }

    void movementModifier(float moveModifier)
    {
        playerSpeedChanger.walkSpeedOverride = GetWalkSpeed() * moveModifier;
        playerSpeedChanger.runSpeedOverride = GetRunSpeed(GetWalkSpeed()) * moveModifier;
    }

    public float GetWalkSpeed()
    {
        float drag = 0.5f * (100 - (GameManager.Instance.PlayerEntity.Stats.LiveSpeed >= 30 ? GameManager.Instance.PlayerEntity.Stats.LiveSpeed : 30));
        return (GameManager.Instance.PlayerEntity.Stats.LiveSpeed + PlayerSpeedChanger.dfWalkBase - drag) / 39.5f;
    }

    /// <summary>
    /// Get LiveSpeed adjusted for running
    /// </summary>
    /// <param name="baseSpeed"></param>
    /// <returns></returns>
    public float GetRunSpeed(float baseSpeed)
    {
        float baseRunSpeed = (GameManager.Instance.PlayerEntity.Stats.LiveSpeed + PlayerSpeedChanger.dfWalkBase) / 39.5f;
        return baseRunSpeed * (1.35f + (GameManager.Instance.PlayerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Running) / 200f));
    }

    bool OnTileChange()
    {
        int currentTile = GameManager.Instance.StreamingWorld.PlayerTileMapIndex;
        if (currentTile != lastTile)
        {
            lastTile = currentTile;

            return true;
        }
        return false;
    }

    //input a tile and return a list of a custom property values.
    public List<float> TileProperty(int tileIndex)
    {
        //setup empty list to hold property values and tileid holder.
        List<float> tileProperty;

        //check tile to see if it isn't melee OR doesn't contain tileID then defaults to classic values if so.
        if (!tilePropertyList("TileProperties.txt").TryGetValue(tileIndex, out tileProperty))
        {
            tileProperty = new List<float>();
            tileProperty.Add(1f);
            return tileProperty;
        }
        //return custom property values.
        else
        {
            //dump stored properties into a new list.
            tilePropertyList("TileProperties.txt").TryGetValue(tileIndex, out tileProperty);
            //grab the second item on the list, as that is tile reach value.
            return tileProperty;
        }
    }

    //sets up and outputs a dictionary that contains a float list of each custom tile property, and stores those properties by item index number.
    public Dictionary<int, List<float>> tilePropertyList(string fileName)
    {
        //setup dictionaries and list to store values.
        //master dictionary to store/index tile property values based on the item index #.
        Dictionary<int, List<float>> tilePropertyList = new Dictionary<int, List<float>>();
        //stores each txt line as a list item.
        List<string> eachLine = new List<string>();
        //stores each tile property on each line in the txt file, which is used to convert to a float list and store in master dictionary.
        List<string> eachTileProperty;

        //dump the text file object data into a unassigned var for reading.
        var sr = new StreamReader(Application.dataPath + "/StreamingAssets/Mods/" + fileName);
        //read the dump contents from begginning to end and dump them into random var.
        var fileContents = sr.ReadToEnd();
        //destroy/close text file object.
        sr.Close();
        //reach contents of file, split on every new line, and dump new line into a list.
        eachLine.AddRange(fileContents.Split("\n"[0]));
        //use for loop to process each stored line to get individual content/tile properies.
        foreach (string line in eachLine)
        {
            //checks for - and skips to next itteration. Used for adding notes for players to text file.
            if (line.Contains("-"))
                continue;
            //create blank string list to store each string value/tile property before float conversion.
            eachTileProperty = new List<string>();
            //split each line by the comma and add it to the new eachTileProperty list for reading below.
            eachTileProperty.AddRange(line.Split(","[0]));
            //create float list to store converted string values/tile properties from eachtileProperty List.
            List<float> tileProperties = new List<float>();
            //use for loop to go through string list
            foreach (string tileProperty in eachTileProperty)
            {
                //add each convert tile property string value to newly created float list.
                tileProperties.Add(float.Parse(tileProperty));
            }
            //add the tile properties, using the created float list, to add the tile properties to the dictionary.
            tilePropertyList.Add((int)tileProperties[0], tileProperties.GetRange(1,1));
        }

        //return created dictionary.
        return tilePropertyList;
    }
}
