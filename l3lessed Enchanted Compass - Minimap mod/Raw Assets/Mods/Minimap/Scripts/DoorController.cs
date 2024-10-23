using DaggerfallWorkshop.Game;
using UnityEngine;

namespace Minimap
{
    public class DoorController : MonoBehaviour
    {
        private float lastRotation;
        private Material iconMaterials;
        private Renderer iconRenderer;
        private float doorSize;
        public float lastColorPercent;
        public float colorLerpPercent;
        public bool insideDoor;
        public float doorDistanceFader;
        public Vector3 SpawnPosition;

        private void Start()
        {
            if (gameObject == null)
            {
                Destroy(gameObject);
                Destroy(this);
            }

            iconMaterials = gameObject.GetComponent<MeshRenderer>().material;
            iconRenderer = gameObject.GetComponent<Renderer>();
            doorSize = 1;

            if (GameManager.Instance.IsPlayerInsideDungeon)
                doorDistanceFader = 21;
            else
                doorDistanceFader = 7;
        }

        // Update is called once per frame
        void Update()
        {
            if (!insideDoor && GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y != lastRotation && !Minimap.minimapControls.autoRotateActive)
            {
                lastRotation = Minimap.minimapControls.minimapRotationValue + 2;
                //updates rotation for each icon, if they are existing.
                gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Rotation", (Minimap.MinimapInstance.publicMinimap.transform.eulerAngles.y - GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y) * .0174f);
            }
            else if (lastRotation != 0)
            {
                lastRotation = 0;
                //updates rotation for each icon, if they are existing.
                iconMaterials.SetFloat("_Rotation", lastRotation);
            }

            if (Minimap.minimapControls.updateMinimap && Minimap.MarkerGroups.Doors == (Minimap.MarkerGroups)Minimap.minimapControls.selectedIconInt)
            {
                if (Minimap.iconGroupActive[Minimap.MarkerGroups.Doors])
                    iconRenderer.forceRenderingOff = false;
                else
                    iconRenderer.forceRenderingOff = true;

                iconMaterials.SetFloat("_LineLength", 10 * (1.1f - (Minimap.iconSizes[Minimap.MarkerGroups.Doors] + 1) / 2));
                iconMaterials.SetFloat("_Spacing", 0);

                if(!insideDoor)
                    iconMaterials.color = Minimap.iconGroupColors[Minimap.MarkerGroups.Doors];
            }

            if (insideDoor)
            {
                colorLerpPercent = (GameManager.Instance.PlayerMotor.transform.position.y - SpawnPosition.y) / doorDistanceFader;

                //if door exist and is active, update door position and color.
                if (colorLerpPercent != lastColorPercent + .025f)
                {
                    //if verticial is negative and makes negative percentage, turn positive.
                    if (colorLerpPercent < 0)
                        colorLerpPercent *= -1;

                    lastColorPercent = colorLerpPercent;
                    float lerpPercent = Mathf.Clamp(colorLerpPercent, 0, 1);

                    //update color using two step lerp in a lerp. First lerp goes from green to yellow, ending lerp goes from yellow to red. This creates a clear green to yellow to red transition color.
                    iconMaterials.color = Color.Lerp(Color.Lerp(Color.green, Color.yellow, lerpPercent), Color.Lerp(Color.yellow, Color.red, lerpPercent), lerpPercent);
                    //set door icon position.
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, GameManager.Instance.PlayerMotor.transform.position.y - .8f, gameObject.transform.position.z);
                    //sets up and adjust the distance for the color distance effect. Higher number the further up/down the shader detects and shifts.
                }
            }
        }
    }
}

