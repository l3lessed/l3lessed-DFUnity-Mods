using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;

namespace Minimap
{
    public class ItemCutGlass : DaggerfallUnityItem
    {
        public const int templateIndex = 721;

        public ItemCutGlass() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        // Always use same archive for both genders as the same image set is used
        public override int InventoryTextureArchive
        {
            get { return templateIndex; }
        }

        public override bool IsStackable()
        {
            return true;
        }

        public override int InventoryTextureRecord
        {
            get { return 0; }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemCutGlass).ToString();
            return data;
        }
    }
}

