using DaggerfallWorkshop.Game;
using UnityEngine;

namespace Minimap
{
    public class MeshController : MonoBehaviour
    {
        private float lastRotation;
        private Color meshColor;
        private Material buildingMaterials;
        public int buildingType;

        void Start()
        {
            if (gameObject == null)
            {
                Destroy(gameObject);
                Destroy(this);
            }

            buildingMaterials = gameObject.GetComponent<MeshRenderer>().material;           
        }

            // Update is called once per frame
         void Update()
        {
            if (GameManager.Instance.IsPlayerInside || Minimap.MinimapInstance.currentEquippedCompass == null)
                return;

            if (buildingMaterials.GetColor("_Color") != Minimap.iconGroupColors[(Minimap.MarkerGroups)buildingType])
                updateMaterials();
        }

        //updates object, as long as object has a material attached to it to update/apply shader to.
        void updateMaterials()
        {
            //running through dumped material array to assign each mesh material on model the proper transperency texture.

                string textureName = buildingMaterials.name.Split(new char[] { ' ' })[0];
                if (textureName == "markerMaterial")
                    buildingMaterials.SetColor("_Color", Minimap.iconGroupColors[(Minimap.MarkerGroups)buildingType]);
        }
    }
}

