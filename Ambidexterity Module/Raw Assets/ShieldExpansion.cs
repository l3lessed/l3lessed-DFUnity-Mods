using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using AmbidexterityModule;
using System.IO;

public class ShieldExpansion : MonoBehaviour
{

    public static Mod ShieldExpansionMod;
    public static ShieldExpansion ShieldExpansionInstance;
    float TimeCovered;
    float fractionOfJourney;
    float totalTime;

    float height;
    float width;
    float width2;
    float screenWidth;

    bool breatheTrigger;
    bool lerpfinished;

    int state;

    DaggerfallUnityItem spellbreaker;
    Texture2D wardTex;
    Rect wardPos;
    Rect wardPos2;

    int[] baseResistances;

    //starts mod manager on game begin. Grabs mod initializing paramaters.
    //ensures SateTypes is set to .Start for proper save data restore values.
    [Invoke(StateManager.StateTypes.Game, 0)]
    public static void Init(InitParams initParams)
    {
        Debug.Log("ShieldExpansion MODULE STARTED!");
        //sets up instance of class/script/mod.
        GameObject ShieldExpansionScript = new GameObject("ShieldExpansion");
        ShieldExpansionInstance = ShieldExpansionScript.AddComponent<ShieldExpansion>();
        //initiates mod paramaters for class/script.
        ShieldExpansionMod = initParams.Mod;
    }

    // Use this for initialization
    void Awake()
    {
        Genders playerGender = GameManager.Instance.PlayerEntity.Gender;
        Races race = GameManager.Instance.PlayerEntity.Race; 
        spellbreaker = ItemBuilder.CreateArmor(playerGender, race, Armor.Kite_Shield, ArmorMaterialTypes.Daedric);
        spellbreaker.RenameItem("Spellbreaker");
        GameManager.Instance.PlayerEntity.Items.AddItem(spellbreaker);
        wardTex = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/spellbreaker/ward.png");
        Debug.Log("Spellbreaker Added!");
        wardPos = new Rect(0, 0, ((float)Screen.width / 700) * 700, ((float)Screen.height / 700) * 700);
    }


    //lerping calculator. The workhorse of the animation management system and this script. Uses a time delta calculator to figure out how much time has passed in
    //since the last frame update. It uses this number and the vars the developer inputs to figure out what the output would be based on the current percent of time
    //that has passed and the total animation time the player inputs into the animation manager through the animation ienumerator/coroutine the developer sets up.
    float LerpCalculator(out bool lerpfinished, float duration, float startTime, float startValue, float endValue, string lerpEquation, bool loop, bool breathe, int cycles = 1)
    {
        //sets lerp calculator base properties.
        float lerpvalue = 0;
        float totalDuration = 0;
        lerpfinished = false;

        //figures out total length of lerp cycle.
        totalDuration = duration * cycles;

        //counts total time of lerp cycle.
        totalTime += Time.deltaTime;

        if (loop == true)
        {
            totalTime = 0;
        }
        //returns end value and resets triggers once lerp cycle has finished its total duration.
        else if (totalTime > totalDuration)
        {
            lerpfinished = true;
            if (!breathe)
            {
                return endValue;
            }
            else
            {
                return startValue;
            }
        }

        //breath trigger to allow lerp to breath naturally back and fourth.
        if (TimeCovered >= duration && breatheTrigger)
            breatheTrigger = false;
        else if (TimeCovered <= 0 && !breatheTrigger)
            breatheTrigger = true;

        //classic animation system starts here. If enabled, timeCovered counter for animation is disabled,
        //timeCovered value is instead forced through the animation wait coroutine itself to create same 5 frame, segmented
        //animation look as classic daggerfall.
        if (breatheTrigger)
            // Distance moved equals elapsed time times speed.
            TimeCovered += Time.deltaTime;
        else
            // Distance moved equals elapsed time times speed.
            TimeCovered -= Time.deltaTime;

        //if using classic animations, timecovered is forced through the animation coroutine itself. This is to allow classic animation styles.
        fractionOfJourney = TimeCovered / duration;

        //if individual cycle is over, and breath trigger is off, reset lerp to 0 position to start from beginning all over.
        if ((fractionOfJourney < 0f || fractionOfJourney > 1f) && breathe == false)
        {
            TimeCovered = 0;
        }

        //reprocesses time passed into a sin graph function to provide a custom movement graph shapes instead of basic linear movement.
        if (lerpEquation == "linear" || lerpEquation == null || lerpEquation == "")
            ; //do nothing to keep basic linear lerping;
        else if (lerpEquation == "easeout")
            fractionOfJourney = Mathf.Sin(fractionOfJourney * Mathf.PI * 0.5f);
        else if (lerpEquation == "easein")
            fractionOfJourney = 1f - Mathf.Cos(fractionOfJourney * Mathf.PI * 0.5f);
        else if (lerpEquation == "exponential")
            fractionOfJourney = fractionOfJourney * fractionOfJourney;
        else if (lerpEquation == "smoothstep")
            fractionOfJourney = fractionOfJourney * fractionOfJourney * (3f - 2f * fractionOfJourney);
        else if (lerpEquation == "smootherstep")
            fractionOfJourney = fractionOfJourney * fractionOfJourney * fractionOfJourney * (fractionOfJourney * (6f * fractionOfJourney - 15f) + 10f);

        lerpvalue = Mathf.Lerp(startValue, endValue, fractionOfJourney);

        //return lerp value.
        return lerpvalue;
    }

    // Update is called once per frame
    void Update()
    {
        //isBlocking is staying false at all times for some reason. *ERROR HERE!!!*
        if (FPSShield.shieldStates == 1)
        {
            //custom reflect spell, but not really effecive way of managing this.
            //EntityEffectBundle bundle = GameManager.Instance.PlayerEffectManager.CreateSpellBundle("SpellResistance",ElementTypes.Magic,effectSettings);
            //GameManager.Instance.PlayerEffectManager.AssignBundle(bundle);

            //max out resistance to all magic types to simiulate a strong ward.
            GameManager.Instance.PlayerEntity.Resistances.SetPermanentResistanceValue(DFCareer.Elements.Magic, 100);
            GameManager.Instance.PlayerEntity.Resistances.SetPermanentResistanceValue(DFCareer.Elements.Frost, 100);
            GameManager.Instance.PlayerEntity.Resistances.SetPermanentResistanceValue(DFCareer.Elements.Fire, 100);
            GameManager.Instance.PlayerEntity.Resistances.SetPermanentResistanceValue(DFCareer.Elements.Shock, 100);
            GameManager.Instance.PlayerEntity.Resistances.SetPermanentResistanceValue(DFCareer.Elements.DiseaseOrPoison, 100);
            //width = LerpCalculator(out lerpfinished, 2, 0, 0, 350, "smootherstep", true, true, 1);

            //height = LerpCalculator(out lerpfinished, FPSShield.totalBlockTime, 0, 300, 650, "smootherstep", false, false, 1);
            width = LerpCalculator(out lerpfinished, 5, 0, 0, 400, "smootherstep", false, false, 1);

            Debug.Log("Blocked Spell!");
        }
        else if(FPSShield.shieldStates == 2)
        {
            Debug.Log("RAISED!");
            width = LerpCalculator(out lerpfinished, 1, 0, 500, 900, "smootherstep", true, false, 1);
            width2 = LerpCalculator(out lerpfinished, 1, .5f, 500, 900, "smootherstep", true, false, 1);
        }
        else
        {
            GameManager.Instance.PlayerEntity.Resistances.SetDefaults();
            totalTime = 0;
            TimeCovered = 0;
            state = 0;
            wardPos = new Rect();
        }
    }

    //draws gui shield.
    private void OnGUI()
    {
        //if shield is not equipped or console is open then....
        if (!FPSShield.shieldEquipped || !FPSShield.isBlocking)
            ; //show nothing.
              //loads shield texture if weapon is showing & shield is equipped.
        else if (GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon == true && FPSShield.equippedShield.shortName == "Spellbreaker")
        {
            wardPos = new Rect(Screen.width / 2f - (width * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleX) / 2f,
                Screen.height - (width + 200) * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleY / 2f,
                width * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleX,
                width * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleY);

            wardPos2 = new Rect(Screen.width / 2f - (width2 * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleX) / 2f,
                Screen.height - (width2 + 200) * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleY / 2f,
                width2 * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleX,
                width2 * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleY);

            Debug.Log(FPSShield.shieldPos.size.ToString() + " | " + wardPos.size.ToString());

            GUI.DrawTextureWithTexCoords(wardPos, wardTex, new Rect(0.0f, 0.0f, .99f, .99f));
            GUI.DrawTextureWithTexCoords(wardPos2, wardTex, new Rect(0.0f, 0.0f, .99f, .99f));
        }
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