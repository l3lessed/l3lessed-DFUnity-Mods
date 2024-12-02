using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class ItemRepairKit : DaggerfallUnityItem
    {
        public const int templateIndex = 723;

        public ItemRepairKit() : base(ItemGroups.MiscItems, templateIndex)
        {
        }

        // Always use same archive for both genders as the same image set is used
        public override int InventoryTextureArchive
        {
            get { return templateIndex; }
        }

        public override int InventoryTextureRecord
        {
            get { return 0; }
        }

        public override bool UseItem(ItemCollection collection)
        {
            List<DaggerfallUnityItem> dwemerGearsList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, ItemDwemerGears.templateIndex);
            List<DaggerfallUnityItem> cutGlassList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, ItemCutGlass.templateIndex);

            if(Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage >= 81)
            {

                DaggerfallMessageBox confirmBox = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallMessageBox.CommonMessageBoxButtons.Nothing, "Your compass is in fine shape");            
                confirmBox.Show();
                return false;
            }

            RepairController.tempBrokenGlassEffect.GetComponent<RawImage>().texture = Minimap.MinimapInstance.LoadPNG(Application.streamingAssetsPath + "/Textures/Minimap/damage/" + DamageEffectController.damageGlassEffectInstance.textureName);
            RepairController.tempGlassTexture = Minimap.MinimapInstance.cleanGlass;

            if (dwemerGearsList.Count != 0 && cutGlassList.Count != 0)
            {
                Minimap.dfInventoryWindow.CloseWindow();
                Minimap.dfSheetWindow.CloseWindow();
                DaggerfallMessageBox confirmBox = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallMessageBox.CommonMessageBoxButtons.Nothing, "You steady your hands and concentrate on repairing the compass. Don't move or you'll drop something.");
                confirmBox.Show();
                if (EffectManager.compassDirty)
                {
                    EffectManager.cleaningCompassTrigger = true;
                }

                EffectManager.CompassState = 0;
                EffectManager.repairingCompass = true;

                RepairController.startRepairCondition = Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage;

                //Minimap.MinimapInstance.currentEquippedCompass.currentCondition = 52;
                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 50)
                    Minimap.MinimapInstance.currentEquippedCompass.currentCondition = 0;
                else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 51)
                    Minimap.MinimapInstance.currentEquippedCompass.currentCondition = 51;
            }
            else
            {
                string missingItems = "";

                if (cutGlassList.Count == 0)
                    missingItems = "Cut Glass";
                if (dwemerGearsList.Count == 0)
                    missingItems = "Dwemer Gears";
                else
                    missingItems = missingItems + " and Dwemer Gears";

                DaggerfallUI.MessageBox("You do not have " + missingItems + " to fix your compass");
            }

            return true;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemRepairKit).ToString();
            return data;
        }
    }
}

