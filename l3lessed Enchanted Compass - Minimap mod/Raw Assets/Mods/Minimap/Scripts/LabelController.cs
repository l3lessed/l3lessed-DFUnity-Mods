using DaggerfallWorkshop.Game;
using UnityEngine;

namespace Minimap
{
    public class LabelController : MonoBehaviour
    {
        public float lastRotation;

        // Update is called once per frame
        void Update()
        {
            gameObject.transform.RotateAround(gameObject.transform.position, new Vector3(0, 1, 0), lastRotation);
        }
    }
}

