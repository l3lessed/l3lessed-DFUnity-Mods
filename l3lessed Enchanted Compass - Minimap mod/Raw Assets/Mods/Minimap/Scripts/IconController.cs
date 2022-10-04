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
        private float savedSize;
        public Minimap.MarkerGroups buildingtype;
        public static bool UpdateIcon;

        void Start()
        {
            iconMaterials = gameObject.GetComponent<MeshRenderer>().material;
            iconRenderer = gameObject.GetComponent<Renderer>();
            savedSize = Minimap.iconSizes[buildingtype];
            currentSize = iconMaterials.GetFloat("_LineLength");

            if (savedSize != currentSize)
                UpdateIcon = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance.IsPlayerInside || GameManager.Instance.SaveLoadManager.LoadInProgress || GameManager.Instance.StateManager.CurrentState == StateManager.StateTypes.Start || GameManager.Instance.StateManager.CurrentState == StateManager.StateTypes.Setup)
                return;

            if (GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y != lastRotation && !Minimap.minimapControls.autoRotateActive)
            {
                lastRotation = Minimap.minimapControls.minimapRotationValue + 2;
                //updates rotation for each icon, if they are existing.
                iconMaterials.SetFloat("_Rotation", ((Minimap.MinimapInstance.publicMinimap.transform.eulerAngles.y - GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y)) * .0174f);
            }
            else if(lastRotation != 0)
            {
                lastRotation = 0;
                //updates rotation for each icon, if they are existing.
                iconMaterials.SetFloat("_Rotation", lastRotation);
            }

            if (UpdateIcon)
            {
                if (Minimap.minimapControls.smartViewActive)
                {
                    if (Minimap.minimapControls.iconsActive)
                        iconRenderer.forceRenderingOff = !Minimap.iconGroupActive[buildingtype];
                    else
                        iconRenderer.forceRenderingOff = true;
                }
                else
                    iconRenderer.forceRenderingOff = !Minimap.minimapControls.iconsActive;

                if (Minimap.minimapControls.smartViewActive)
                {
                    if (iconMaterials.GetFloat("_Spacing") != 0)
                        iconMaterials.SetFloat("_Spacing", 0);

                        iconMaterials.SetFloat("_LineLength", 10 * (1.1f - (Minimap.iconSizes[buildingtype] + 1) / 2));
                }
                else if (!Minimap.minimapControls.smartViewActive)
                {
                    if (Minimap.minimapControls.labelsActive && Minimap.minimapControls.iconsActive)
                    {
                        iconMaterials.SetFloat("_LineLength", 1.5f);
                        iconMaterials.SetFloat("_Spacing", 1.5f);
                    }
                    else if (!Minimap.minimapControls.labelsActive && Minimap.minimapControls.iconsActive)
                    {
                        iconMaterials.SetFloat("_LineLength", 10 * (1.1f - (Minimap.iconSizes[buildingtype] + 1) / 2));
                        iconMaterials.SetFloat("_Spacing", 0);
                    }
                }

                UpdateIcon = false;
            }
        }
    }
}

