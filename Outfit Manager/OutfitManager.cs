using UnityEngine;
using System;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings; //required for modding features
using DaggerfallWorkshop;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Serialization;

namespace OutfitManager
{
    public class OutfitManager : MonoBehaviour, IHasModSaveData
    {
        //assigns serializer version to ensures mod data continuaty / debugging.
        //sets up the save data class for the seralizer to read and save to text file.
        #region Types
        [FullSerializer.fsObject("v1")]
        public class MyModSaveData
        {
            public Dictionary<int, ulong[]> OutfitDictSerialized;
            public Dictionary<int, string> OutfitDictName;
            public int Index;
        }
        #endregion

        //sets up class fields.
        #region Fields
        ulong[] currentEquippedSerialized;
        bool toggleGuiSwitch = true;
        public int index;
        public int selectedIndex;
        public string outfitName = "Bundle";
        static Mod mod;
        public static OutfitManager instance;
        static ModSettings settings;
        DaggerfallInventoryWindow DaggerfallInventory;
        ConsoleController consoleController;
        OutfitManagerInventoryWindow UIWindow;
        public static DaggerfallAudioSource dfAudioSource;
        static GameObject console;
        UserInterfaceManager uiManager = new UserInterfaceManager();
        #endregion

        #region Keybinds
        string NextOutfitKey;
        string PrevOutfitKey;
        string EquipOutfitKey;
        string ToggleGuiKey;
        string SaveOutfitKey;
        string DeleteOutfitKey;
        #endregion

        //sets ui positions and sizes that are assigned to onGui components.
        #region UI Rects
        Rect nextrect = new Rect((Screen.width * 0.5f) - (45 * 0.5f), (Screen.height * 0.5f) - (30 * 0.5f), 45, 30);
        Rect previousButton = new Rect(2, 50, 45, 30);
        Rect count = new Rect(2, 0, 90, 50);
        Rect load = new Rect(2, 80, 90, 30);
        Rect delete = new Rect(2, 110, 90, 30);
        private Button BundleButton;
        private Button DismantleButton;
        protected static TextLabel outfitNameLabel;
        private TextLabel BundleLabel;
        private TextLabel DismantleLabel;
        #endregion

        //sets up both dictionaries (Name Dictionary To name).
        #region Dictionaries
        public Dictionary<int, ulong[]> outfitDictSerialized = new Dictionary<int, ulong[]>();
        public Dictionary<int, string> outfitDictName = new Dictionary<int, string>();
        #endregion

        //sets up different class properties.
        #region Properties
        //sets up player entity class for easy retrieval and manipulation of player character.
        PlayerEntity playerEntity;
        //popup messagebox for easy use.
        static DaggerfallMessageBox messageBox;
        static DaggerfallMessageBox confirmBox;
        public float xPos;
        public float yPos;
        public Rect buttonText;
        public string currentOutfit = "Bundle";

        public float NextX = 150;
        public float PrevX = 50;

        public float currentSlider = .2325f;
        public float selectedSlider = .3135f;

        bool updatedUI = false;

        //gets save data type for use.
        public Type SaveDataType { get { return typeof(MyModSaveData); } }
        //sets up player class instance properties for manipulation.
        public PlayerEntity PlayerEntity
        {
            get { return (playerEntity != null) ? playerEntity : playerEntity = GameManager.Instance.PlayerEntity; }
        }
        #endregion

        #region Unity
        //starts mod manager on game begin. Grabs mod initializing paramaters.
        //ensures SateTypes is set to .Start for proper save data restore values.
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("OUTFIT MANAGER STARTED!");
            //sets up instance of class/script/mod.
            GameObject go = new GameObject("OutfitManager");
            instance = go.AddComponent<OutfitManager>();

            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //loads mods settings.
            settings = mod.GetSettings();
            //initiates save paramaters for class/script.
            mod.SaveDataInterface = instance;
            //after finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
        }

        //binds key settings to script strings for input detection in update routine.
        void Start()
        {
            NextOutfitKey = settings.GetValue<string>("Settings", "NextOutfitKey");
            PrevOutfitKey = settings.GetValue<string>("Settings", "PrevOutfitKey");
            EquipOutfitKey = settings.GetValue<string>("Settings", "EquipOutfitKey");
            ToggleGuiKey = settings.GetValue<string>("Settings", "ToggleGuiKey");
            SaveOutfitKey = settings.GetValue<string>("Settings", "SaveOutfitKey");
            DeleteOutfitKey = settings.GetValue<string>("Settings", "DeleteOutfitKey");
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(OutfitManagerInventoryWindow));
            UIWindow = (OutfitManagerInventoryWindow)UIWindowFactory.GetInstance(UIWindowType.Inventory, uiManager, null);
            dfAudioSource = GameManager.Instance.PlayerObject.AddComponent<DaggerfallAudioSource>();
            console = GameObject.Find("Console");
            consoleController = console.GetComponent<ConsoleController>();
        }

        //uses update loop to monitor for key inputs.
        private void Update()
        {
            if (consoleController.ui.isConsoleOpen || GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress || DaggerfallUI.UIManager.WindowCount != 0)
                return;
            //activates on frame of key press and player not attacking. Sets up vars for dodging routine below.
            if (Input.GetKeyDown(NextOutfitKey))
            {
                NextOutfit();

                //if then switch for smart message feedback system. Immersively informs player what is happening with the system.
                if (index == selectedIndex && outfitName != "Bundle")
                    DaggerfallUI.Instance.PopupMessage("You inspect and adjust your " + outfitDictName[index] + " outfit");
                else if (outfitName == "Bundle")
                    DaggerfallUI.Instance.PopupMessage("You grab a new bundle kit");
                else
                    DaggerfallUI.Instance.PopupMessage("You grab your " + outfitDictName[index] + " bundle from your pack");
            }
            else if (Input.GetKeyDown(PrevOutfitKey))
            {
                PreviousOutfit();

                //if then switch for smart message feedback system. Immersively informs player what is happening with the system.
                if (index == selectedIndex && outfitName != "Bundle")
                    DaggerfallUI.Instance.PopupMessage("You inspect and secure your " + outfitDictName[index] + " outfit");
                else if (outfitName == "Bundle")
                    DaggerfallUI.Instance.PopupMessage("You grab a new bundle kit");
                else
                    DaggerfallUI.Instance.PopupMessage("You grab your " + outfitDictName[index] + " bundle from your pack");

            }
            else if (Input.GetKeyDown(EquipOutfitKey))
            {
                loadCurrentOutfit();
                DaggerfallUI.Instance.PopupMessage("Your put on your " + outfitDictName[index] + " outfit");
            }
            else if (Input.GetKeyDown(ToggleGuiKey))
            {
                toggleGuiSwitch = (toggleGuiSwitch == true) ? false : true;
            }
            else if (Input.GetKeyDown(SaveOutfitKey))
            {
                SaveOutfitBundle();
            }
            else if (Input.GetKeyDown(DeleteOutfitKey))
                DeleteConfirmMessage("You pause for a moment to consider if you need this outfit anymore?\n");
        }

        //pulls players currently equipped outfit by grabbing every equipment slot.
        //it doesn't matter if the player has something equipped or not. Then, clears nulls
        //and returns list using public currentEquippedSerialized.
        public void currentEquipped()
        {
            //drops seralized equip data into dictionary.
            currentEquippedSerialized = PlayerEntity.ItemEquipTable.SerializeEquipTable();
        }

        //sets up input messagebox for user to name outfit.
        void NameOutfit()
        {
            DaggerfallInputMessageBox mb = new DaggerfallInputMessageBox(DaggerfallUI.UIManager);
            mb.SetTextBoxLabel("Bundle:");
            mb.TextPanelDistanceX = 0;
            mb.TextPanelDistanceY = 0;
            mb.InputDistanceY = 10;
            mb.InputDistanceX = -20;
            mb.TextBox.Numeric = false;
            mb.TextBox.MaxCharacters = 16;
            mb.TextBox.Text = "";
            mb.Show();
            //when input is given, it passes the input into the below method for further use.
            mb.OnGotUserInput += OutfitName_OnGotUserInput;
        }

        //translation for inputbox and save current outfit method. Bridge for transferring to script string.
        //takes the input from the message box, assigns it to object, converts to string, and executes save outfit method.
        void OutfitName_OnGotUserInput(DaggerfallInputMessageBox sender, string outfitNameInput)
        {
            object obj = outfitNameInput;
            outfitName = (string)obj;
            saveCurrentOutfit();
        }

        //saves the currently selected outfit. It grabs the seralized equip table and the
        //current outfitName (Which was just assigned by the player), and stores each into
        //their dictionaries using the same public index integer to ensure matches.
        void saveCurrentOutfit()
        {
            dfAudioSource.PlayOneShot(417, 1, .4f);
            //looks to see if outfit already exists using dict key.
            //if it finds it, updates that dictionary value.
            //if not, adds it to the list.
            if (outfitDictSerialized.ContainsKey(index))
            {
                outfitDictSerialized[index] = currentEquippedSerialized;
                outfitDictName[index] = outfitName;
            }
            else
            {
                outfitDictSerialized.Add(index, currentEquippedSerialized);
                outfitDictName.Add(index, outfitName);
            }

            outfitName = outfitDictName[index];
            currentOutfit = outfitName;
            selectedIndex = index;
        }

        //beginning of outfit load routine. Will use saved outfit lists to equipped outfits to player.
        void loadCurrentOutfit()
        {
            //Debugging message.
            dfAudioSource.PlayOneShot(418, 1, .3f);
            Debug.Log("Outfit List Size:" + outfitDictSerialized.Count);
            //creates a local empty list for the dictionary to output too.
            ulong[] outfitListulong;
            //creates blank daggerfall item instance. Used for catching last equipped item below.
            DaggerfallUnityItem lastEquippedItem = new DaggerfallUnityItem();
            //looks for selected dictionary value: if it finds it, populates outfitList for if then.
            //if can't find dictionary key, it outputs error.
            if (outfitDictSerialized.TryGetValue(index, out outfitListulong))
            {
                //Debugging message.
                Debug.Log("Outfit List Size:" + outfitDictSerialized.Count);
                //for loop to unequipped all armor slots and update armoor values.
                foreach (DaggerfallUnityItem item in PlayerEntity.ItemEquipTable.EquipTable)
                {
                    if (item != null)
                    {
                        PlayerEntity.ItemEquipTable.UnequipItem(item);
                        PlayerEntity.UpdateEquippedArmorValues(item, false);

                        //checks if item being unequipped is enchanted, if so, run PlayerEffectManager to properly remove any item enchanments.
                        GameManager.Instance.PlayerEffectManager.DoItemEnchantmentPayloads(DaggerfallWorkshop.Game.MagicAndEffects.EnchantmentPayloadFlags.Unequipped, item, GameManager.Instance.PlayerEntity.Items);
                    }
                }

                //uses the serialized item list and the players current inventory to equip saved outfit.
                PlayerEntity.ItemEquipTable.DeserializeEquipTable(outfitListulong, PlayerEntity.Items);
                //for loop to update all armor values for items deserialized and equipped above.
                foreach (DaggerfallUnityItem item in PlayerEntity.ItemEquipTable.EquipTable)
                {
                    if (item != null)
                    {
                        PlayerEntity.UpdateEquippedArmorValues(item, true);
                        lastEquippedItem = item;

                        //checks if item being equipped is enchanted, if so, run PlayerEffectManager to properly activate any item enchanments.
                        if (item.IsEnchanted)
                        {
                            GameManager.Instance.PlayerEffectManager.DoItemEnchantmentPayloads(DaggerfallWorkshop.Game.MagicAndEffects.EnchantmentPayloadFlags.Equipped, item, GameManager.Instance.PlayerEntity.Items);
                        }
                    }
                }
                //plays sound of last equipped item. Tells player the outfit bundled loaded through audio.
                DaggerfallUI.Instance.PlayOneShot(lastEquippedItem.GetEquipSound());
                DaggerfallUI.Instance.DaggerfallHUD.Update();
            }
            else
            {
                //error messsage
                Debug.Log("Coudn't Locate Outfit: Index value not found.");
                DisplayMessage("Couldn't find outfit bundle.");
            }
            //refreshes inventory window, which includes the paper doll.
            //Put outside if then to ensure UI always updates, even if bug happens.
            DaggerfallUI.Instance.InventoryWindow.Refresh();
            DaggerfallUI.Instance.DaggerfallHUD.ActiveSpells.UpdateIcons();
        }

        //general messagebox routine. Input a string, puts out a message.
        void DeleteConfirmMessage(string message)
        {
            if (messageBox != null)
            {
                confirmBox = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallMessageBox.CommonMessageBoxButtons.YesNo, message);
                confirmBox.OnButtonClick += ConfirmClassPopup_OnButtonClick;
                confirmBox.Show();
            }
        }

        void ConfirmClassPopup_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            sender.CloseWindow();
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                ;
            }
            else if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.No)
            {
                DeleteOutfit();
            }
        }

        //general messagebox routine. Input a string, puts out a message.
        public static void DisplayMessage(string message)
        {
            if (messageBox == null)
            {
                messageBox = new DaggerfallMessageBox(DaggerfallUI.UIManager);
                messageBox.AllowCancel = true;
                messageBox.ClickAnywhereToClose = true;
                messageBox.ParentPanel.BackgroundColor = Color.clear;
            }

            //sets up messagebox.
            messageBox.SetText(message);

            //pushes messagebox to ui surface.
            DaggerfallUI.UIManager.PushWindow(messageBox);
        }

        public void NextOutfitClick(BaseScreenComponent sender, Vector2 position)
        {
            NextOutfit();
        }

            public void NextOutfit()
        {
            //plays cloth equipping sound with a reduced sound level to simulate equipment rustling.
            dfAudioSource.PlayOneShot(381, 1, .4f);
            //Ensures list doesn't go 1 above current outfit list so user doesn't break dictionary.
            if (outfitDictSerialized.Count == 1 && index == 0)
            {
                index = index + 1;
                //ensures outfit name string always has a value from either dictionary or switched defined.
                if (outfitDictName.ContainsKey(index))
                    outfitName = outfitDictName[index];
                else
                {
                    outfitName = "Bundle";
                }                    
            }
            else if (index <= outfitDictSerialized.Count - 1)
            {
                //plays cloth equipping sound with a reduced sound level to simulate equipment rustling.
                dfAudioSource.PlayOneShot(417, 1, .3f);
                index = index + 1;
                //ensures outfit name string always has a value from either dictionary or switched defined.
                if (outfitDictName.ContainsKey(index))
                    outfitName = outfitDictName[index];
                else
                {
                    outfitName = "Bundle";
                }
            }
        }

        public void PreviousOutfitClick(BaseScreenComponent sender, Vector2 position)
        {
            PreviousOutfit();
        }

        public void PreviousOutfit()
        {
            dfAudioSource.PlayOneShot(381, 1, .4f);
            //Ensures list doesn't go below 0 so dictionary doesn't error out.
            if (index - 1 > -1)
            {
                index = index - 1;
                //ensures outfit name string always has a value from either dictionary or switched defined.
                if (outfitDictName.ContainsKey(index))
                    outfitName = outfitDictName[index];
                else
                {
                    outfitName = "Bundle";
                }
            }
        }

        public void LoadOutfit(BaseScreenComponent sender, Vector2 position)
        {
            selectedIndex = index;
            currentEquipped();
            loadCurrentOutfit();
            outfitName = outfitDictName[index];
            currentOutfit = outfitName;
        }

        public void DeleteOutfitClick(BaseScreenComponent sender, Vector2 position)
        {
            DeleteOutfit();
        }

        public void DeleteOutfit()
        {
            //plays cloth equipping sound with a reduced sound level to simulate equipment rustling.
            dfAudioSource.PlayOneShot(419, 1, .3f);
            //checks for the dictionary entry, finds it, deletes it. If not, tells user the slot is empty.
            if (outfitDictSerialized.ContainsKey(index))
            {
                DisplayMessage("You take apart your " + outfitDictName[index] + " bundle and save the kit.");
                outfitDictSerialized.Remove(index);
                outfitDictName.Remove(index);
                selectedIndex = outfitDictName.Count + 1;
                currentOutfit = "Bundle";
                outfitName = "Bundle";
            }
            else
            {
                DisplayMessage("Outfit slot is empty.");
            }
        }

        public void SaveOutfitClick(BaseScreenComponent sender, Vector2 position)
        {
           SaveOutfitBundle();
        }

        public void SaveOutfitBundle()
        {
            //loads up current equipment slots routine and then saves the current outfit routine.
            currentEquipped();

            //checks if already named bundle. If so, updates bundle without changing name. If not, pops message and asks for outfit name.
            if (outfitName == "Bundle")
            {
                DaggerfallUI.MessageBox("You bundle and secure your outfit");
                NameOutfit();
            }
            else
            {
                DaggerfallUI.Instance.PopupMessage("You inspect and secure your " + outfitDictName[index] + " outfit");
                DaggerfallUI.MessageBox("You inspect and secure your " + outfitDictName[index] + " outfit");
                outfitName = outfitDictName[index];
                saveCurrentOutfit();
            }
        }

        public void ResaveOutfitBundle(BaseScreenComponent sender, Vector2 position)
        {
            //loads up current equipment slots routine and then saves the current outfit routine.
            currentEquipped();
            DaggerfallUI.Instance.PopupMessage("You inspect and secure your " + outfitDictName[index] + " outfit");
            DaggerfallUI.MessageBox("You inspect and secure your " + outfitDictName[index] + " outfit");
            outfitName = outfitDictName[index];
            saveCurrentOutfit();
        }
        #endregion

        #region Public Methods
        //KeyCode toggleClosedBinding;

        public object NewSaveData()
        {
            return new MyModSaveData
            {
                OutfitDictSerialized = new Dictionary<int, ulong[]>(),
                OutfitDictName = new Dictionary<int, string>(),
                Index = 0
            };
        }

        public object GetSaveData()
        {
            return new MyModSaveData
            {
                OutfitDictSerialized = outfitDictSerialized,
                OutfitDictName = outfitDictName,
                Index = index
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var myModSaveData = (MyModSaveData)saveData;
            outfitDictSerialized = myModSaveData.OutfitDictSerialized;
            outfitDictName = myModSaveData.OutfitDictName;
            index = myModSaveData.Index;
        }
        #endregion
    }
}