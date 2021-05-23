using UnityEngine;
using System;
using DaggerfallWorkshop.Game.UserInterface;
using System.Linq;
using OutfitManager;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class OutfitManagerInventoryWindow : DaggerfallInventoryWindow
    {
        //sets up all needed labels, textures, fonts, and buttons for UI system.
        public TextLabel currentoutfitLabel;
        public TextLabel BundleLabel;
        public TextLabel DismantleLabel;
        public TextLabel selectedoutfitLabel;

        public Button RebundleButton;
        public Button NextoutfitButton;
        public Button PrevoutfitButton;
        public Button BundleButton;
        public Button DismantleButton;
        public Button LoadButton;

        DaggerfallFont outfitFont = new DaggerfallFont(DaggerfallFont.FontName.FONT0000);
        DaggerfallFont buttonFont = new DaggerfallFont(DaggerfallFont.FontName.FONT0002);

        private Texture2D redUpArrow;
        private Texture2D redDownArrow;

        Rect upArrowRect = new Rect(0, 0, 9, 16);
        Rect downArrowRect = new Rect(0, 136, 9, 16);

        DFSize arrowsFullSize = new DFSize(9, 152);

        private Texture2D greenUpArrow;
        private Texture2D greenDownArrow;
        public Texture2D OutfitTex;
        public Texture2D UnbundleTex;
        public Texture2D BundleTex;

        //setup custom inventory object to inject code into base inventory.
        public OutfitManagerInventoryWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
    : base(uiManager, previous)
        {

        }

        //override, setup and inject custom code into inventory SetupActionButtions object.
        protected override void SetupActionButtons()
        {
            //runs base first, so it is setup to show up behind custom ui.
            base.SetupActionButtons();

            //setup textures using imagereader object.
            Texture2D nextArrowsTexture = ImageReader.GetTexture("TRAVCI05.IMG");
            Texture2D prevArrowsTexture = ImageReader.GetTexture("TRAVDI05.IMG");
            OutfitTex = ImageReader.GetTexture("TEXTURE.216",26,0,true,0);
            UnbundleTex = ImageReader.GetTexture("TEXTURE.216", 31, 0, true, 0);
            BundleTex = ImageReader.GetTexture("TEXTURE.204", 9, 0, true, 0);

            //setup current outfit label to render and change players current equipped outfit in ui.
            currentoutfitLabel = DaggerfallUI.AddTextLabel(outfitFont, new Vector2(DaggerfallUI.Instance.InventoryWindow.NativePanel.InteriorWidth * OutfitManager.OutfitManager.instance.currentSlider, 12), "", DaggerfallUI.Instance.InventoryWindow.NativePanel);
            currentoutfitLabel.OnMouseEnter += OnMouseEnterCurrentOutfit;
            currentoutfitLabel.OnMouseLeave += OnMouseLeaveCurrentOutfit;

            //sets up all the buttons and their properties.
            RebundleButton = DaggerfallUI.AddButton(new Rect(89, 12, 40, 10), DaggerfallUI.Instance.InventoryWindow.NativePanel);
            RebundleButton.OnMouseClick += OutfitManager.OutfitManager.instance.ResaveOutfitBundle;

            NextoutfitButton = DaggerfallUI.AddButton(new Rect(OutfitManager.OutfitManager.instance.NextX, 189, 8, 6), DaggerfallUI.Instance.InventoryWindow.NativePanel);
            NextoutfitButton.OnMouseClick += OutfitManager.OutfitManager.instance.NextOutfitClick;
            NextoutfitButton.BackgroundTexture = nextArrowsTexture;
            NextoutfitButton.Position = new Vector2(OutfitManager.OutfitManager.instance.NextX, 189);

            PrevoutfitButton = DaggerfallUI.AddButton(new Rect(OutfitManager.OutfitManager.instance.PrevX, 189, 8, 6), DaggerfallUI.Instance.InventoryWindow.NativePanel);
            PrevoutfitButton.OnMouseClick += OutfitManager.OutfitManager.instance.PreviousOutfitClick;
            PrevoutfitButton.BackgroundTexture = prevArrowsTexture;
            PrevoutfitButton.Position = new Vector2(OutfitManager.OutfitManager.instance.PrevX, 189);

            BundleButton = DaggerfallUI.AddButton(new Rect(86, 184, 36, 12), DaggerfallUI.Instance.InventoryWindow.NativePanel);
            BundleButton.OnMouseClick += OutfitManager.OutfitManager.instance.SaveOutfitClick;
            BundleButton.BackgroundTexture = BundleTex;
            BundleButton.Label.TextScale = 1.25f;
            BundleButton.Label.Text = "Bundle";

            DismantleButton = DaggerfallUI.AddButton(new Rect(86, 184, 36, 12), DaggerfallUI.Instance.InventoryWindow.NativePanel);
            DismantleButton.OnMouseClick += OutfitManager.OutfitManager.instance.DeleteOutfitClick;
            DismantleButton.BackgroundTexture = UnbundleTex;
            DismantleButton.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DismantleButton.OnMouseEnter += OnMouseEnterUnbundle;
            DismantleButton.OnMouseLeave += OnMouseLeaveUnbundle;
            DismantleButton.Label.TextScale = 1.25f;
            DismantleButton.Label.Text = "Unbundle";

            LoadButton = DaggerfallUI.AddButton(new Rect(86, 184, 36, 12), DaggerfallUI.Instance.InventoryWindow.NativePanel);
            LoadButton.OnMouseClick += OutfitManager.OutfitManager.instance.LoadOutfit;
            LoadButton.BackgroundTexture = OutfitTex;
            LoadButton.OnMouseEnter += OnMouseEnterLoadBut;
            LoadButton.OnMouseLeave += OnMouseLeaveLoadBut;
            LoadButton.Label.TextScale = 1.25f;

            //turns on or off buttons and labels for mod launch.
            currentoutfitLabel.Enabled = false;
            PrevoutfitButton.Enabled = false;
            NextoutfitButton.Enabled = false;
            BundleButton.Enabled = true;
            RebundleButton.Enabled = false;
            DismantleButton.Enabled = false;
            LoadButton.Enabled = false;
            currentoutfitLabel.Text = "Bundle";
        }

        //Sets up mouse hover routines for updating buttons/labels on mouse hover.
        void OnMouseEnterLoadBut(BaseScreenComponent sender)
        {
            LoadButton.Label.Text = "Equip " + OutfitManager.OutfitManager.instance.outfitName;
        }

        void OnMouseLeaveLoadBut(BaseScreenComponent sender)
        {
            LoadButton.Label.Text =  OutfitManager.OutfitManager.instance.outfitName;
        }

        void OnMouseEnterCurrentOutfit(BaseScreenComponent sender)
        {
            currentoutfitLabel.Text = "Rebundle " + OutfitManager.OutfitManager.instance.currentOutfit;
            currentoutfitLabel.Position = new Vector2((DaggerfallUI.Instance.InventoryWindow.NativePanel.InteriorWidth - (currentoutfitLabel.TextWidth * 1.4f)) * OutfitManager.OutfitManager.instance.selectedSlider, 13);
        }

        void OnMouseLeaveCurrentOutfit(BaseScreenComponent sender)
        {
            currentoutfitLabel.Text = OutfitManager.OutfitManager.instance.currentOutfit;
            currentoutfitLabel.Position = new Vector2((DaggerfallUI.Instance.InventoryWindow.NativePanel.InteriorWidth - (currentoutfitLabel.TextWidth * 1.4f)) * OutfitManager.OutfitManager.instance.selectedSlider, 13);
        }

        void OnMouseEnterUnbundle(BaseScreenComponent sender)
        {
            DismantleButton.Label.Text = "Unbundle " + OutfitManager.OutfitManager.instance.outfitName;
        }

        void OnMouseLeaveUnbundle(BaseScreenComponent sender)
        {
            DismantleButton.Label.Text = OutfitManager.OutfitManager.instance.outfitName;
        }

        //grabs, updates, and injects custom code into base inventory update object.    
        public override void Update()
        {
            base.Update();

            //checks current outfit dictionary for outfit total and current selected index, then turns on and off next/previous buttons.
            if (OutfitManager.OutfitManager.instance.outfitDictSerialized.Count == 0)
            {
                NextoutfitButton.Enabled = false;
                PrevoutfitButton.Enabled = false;
            }
            else if (OutfitManager.OutfitManager.instance.index == 0)
            {
                NextoutfitButton.Enabled = true;
                PrevoutfitButton.Enabled = false;
            }
            else if (OutfitManager.OutfitManager.instance.index == OutfitManager.OutfitManager.instance.outfitDictSerialized.Count)
            {
                NextoutfitButton.Enabled = false;
                PrevoutfitButton.Enabled = true;
            }
            else
            {
                NextoutfitButton.Enabled = true;
                PrevoutfitButton.Enabled = true;
            }

            //if then checks to control ui based on outfit selections. 
            if (OutfitManager.OutfitManager.instance.outfitDictSerialized.Count == 0 && OutfitManager.OutfitManager.instance.selectedIndex != OutfitManager.OutfitManager.instance.index && OutfitManager.OutfitManager.instance.outfitName == "Bundle")
            {
                BundleButton.Enabled = false;
                DismantleButton.Enabled = false;
                LoadButton.Enabled = false;
                BundleButton.Enabled = true;
                currentoutfitLabel.Enabled = false;
            }

            if (OutfitManager.OutfitManager.instance.outfitDictSerialized.Count != 0 && OutfitManager.OutfitManager.instance.selectedIndex != OutfitManager.OutfitManager.instance.index && OutfitManager.OutfitManager.instance.outfitName == "Bundle")
            {
                BundleButton.Enabled = false;
                DismantleButton.Enabled = false;
                LoadButton.Enabled = false;
                BundleButton.Enabled = true;

                if(OutfitManager.OutfitManager.instance.currentOutfit != "Bundle")
                {
                    RebundleButton.Enabled = true;
                    currentoutfitLabel.Enabled = true;
                }
                else
                {
                    RebundleButton.Enabled = false;
                    currentoutfitLabel.Enabled = false;
                }
            }

            if (OutfitManager.OutfitManager.instance.selectedIndex == OutfitManager.OutfitManager.instance.index && OutfitManager.OutfitManager.instance.outfitName != "Bundle")
            {
                BundleButton.Enabled = false;
                DismantleButton.Enabled = true;
                LoadButton.Enabled = false;
                if (OutfitManager.OutfitManager.instance.currentOutfit != "Bundle")
                {
                    RebundleButton.Enabled = true;
                    currentoutfitLabel.Enabled = true;
                }
                else
                {
                    RebundleButton.Enabled = false;
                    currentoutfitLabel.Enabled = false;
                }

                if(currentoutfitLabel.Text != "Rebundle " + OutfitManager.OutfitManager.instance.outfitName)
                {
                    currentoutfitLabel.Text = OutfitManager.OutfitManager.instance.currentOutfit;
                    currentoutfitLabel.Position = new Vector2((DaggerfallUI.Instance.InventoryWindow.NativePanel.InteriorWidth - (currentoutfitLabel.TextWidth * 1.4f)) * OutfitManager.OutfitManager.instance.selectedSlider, 13);
                }
            }

            if (OutfitManager.OutfitManager.instance.selectedIndex != OutfitManager.OutfitManager.instance.index && OutfitManager.OutfitManager.instance.outfitName != "Bundle")
            {
                BundleButton.Enabled = false;
                DismantleButton.Enabled = false;
                LoadButton.Enabled = true;
                if (OutfitManager.OutfitManager.instance.currentOutfit != "Bundle")
                {
                    RebundleButton.Enabled = true;
                    currentoutfitLabel.Enabled = true;
                }
                else
                {
                    RebundleButton.Enabled = false;
                    currentoutfitLabel.Enabled = false;
                }

                if (LoadButton.Label.Text != "Equip " + OutfitManager.OutfitManager.instance.outfitName)
                    LoadButton.Label.Text = OutfitManager.OutfitManager.instance.outfitName;
            }
        }
    }
}