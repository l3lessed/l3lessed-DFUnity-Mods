using DaggerfallWorkshop.Game;
using UnityEngine;

namespace Minimap
{
    public class DoorController : MonoBehaviour
    {
        private float lastRotation;

        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y != lastRotation)
            {
                lastRotation = Minimap.minimapControls.minimapRotationValue + 2;
                //updates rotation for each icon, if they are existing.
                gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Rotation", (Minimap.MinimapInstance.publicMinimap.transform.eulerAngles.y - GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y) * .0174f);
            }
        }
    }
}

