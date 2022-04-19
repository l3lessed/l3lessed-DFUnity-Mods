using DaggerfallWorkshop.Game;
using UnityEngine;

namespace Minimap
{
    public class IconController : MonoBehaviour
    {
        private float lastRotation;
        private float currentSize;
        public Minimap.MarkerGroups iconGroup;
        private Material iconMaterials;
        private Renderer iconRenderer;
        public Minimap.MarkerGroups buildingtype;

        void Start()
        {
            iconMaterials = gameObject.GetComponent<MeshRenderer>().material;
            iconRenderer = gameObject.GetComponent<MeshRenderer>();
            currentSize = Minimap.iconSizes[buildingtype];
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance.IsPlayerInside)
                return;

            if (GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y != lastRotation)
            {
                lastRotation = Minimap.minimapControls.minimapRotationValue + 2;
                //updates rotation for each icon, if they are existing.
                iconMaterials.SetFloat("_Rotation", ((Minimap.MinimapInstance.publicMinimap.transform.eulerAngles.y - GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y)) * .0174f);
            }

            if (Minimap.iconSizes[buildingtype] != currentSize)
            {
                currentSize = Minimap.minimapControls.iconSize;

                if (Minimap.minimapControls.smartViewActive)
                {
                    if (iconMaterials.GetFloat("_Spacing") != 0)
                        iconMaterials.SetFloat("_Spacing", 0);

                        iconMaterials.SetFloat("_LineLength", 10 * (1.1f - (Minimap.iconSizes[buildingtype] + 1) / 2));

                    if (Minimap.minimapControls.iconsActive)
                        iconRenderer.forceRenderingOff = !Minimap.iconGroupActive[buildingtype];
                    else
                        iconRenderer.forceRenderingOff = true;
                }
                else if (!Minimap.minimapControls.smartViewActive)
                {
                    if (Minimap.minimapControls.labelsActive && Minimap.minimapControls.iconsActive && Minimap.minimapControls.updateMinimap)
                    {
                        iconMaterials.SetFloat("_LineLength", 1.5f);
                        iconMaterials.SetFloat("_Spacing", 1.5f);
                    }
                    else if (!Minimap.minimapControls.labelsActive && Minimap.minimapControls.iconsActive && Minimap.minimapControls.updateMinimap)
                    {
                        iconMaterials.SetFloat("_LineLength", 10 * (1.1f - (Minimap.iconSizes[buildingtype] + 1) / 2));
                        iconMaterials.SetFloat("_Spacing", 0);
                    }

                    iconRenderer.forceRenderingOff = !Minimap.minimapControls.iconsActive;
                }
            }
        }
    }
}

