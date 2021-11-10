// Project:         RoleplayRealism:Items mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Hazelnut
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Hazelnut

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;

namespace Minimap
{
    public class ItemMagicalCompass : DaggerfallUnityItem
    {
        public const int templateIndex = 720;

        public ItemMagicalCompass() : base(ItemGroups.MagicItems, templateIndex)
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

        public override EquipSlots GetEquipSlot()
        {
            return EquipSlots.Amulet0;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemMagicalCompass).ToString();
            return data;
        }
    }
}

