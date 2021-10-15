using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.Collections.Generic;
using UnityEngine;

namespace DaggerfallWorkshop.Game.RandomVariations
{
    [ImportedComponent]
    public class RandomScale : MonoBehaviour
    {
        [SerializeField]
        private float min = 0.25f;

        [SerializeField]
        private float max = 5f;

        [SerializeField]
        private Vector3 spawnPosition;
        private Vector3 currentPosition;
        private Vector3 movePosition;
        private Vector3 roamingForwardLimit;
        private Vector3 roamingBackwardLimit;
        private Vector3 roamingLeftLimit;
        private Vector3 roamingRightLimit;
        private List<Vector3> roamingLimiteArray = new List<Vector3>();
        private bool setupRoamingComplete;

        private void Awake()
        {
            float scale = Random.Range(min, max);
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(scale, scale, scale));
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (!setupRoamingComplete)
            {
                spawnPosition = transform.position;
                Ray ray = new Ray();
                RaycastHit parameterCast;
                roamingLimiteArray = new List<Vector3>();

                ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out parameterCast, 20))
                    roamingForwardLimit = parameterCast.point;
                else
                    roamingForwardLimit = ray.GetPoint(20);

                ray = new Ray(transform.position, -transform.forward);
                if (Physics.Raycast(ray, out parameterCast, 20))
                    roamingBackwardLimit = parameterCast.point;
                else
                    roamingBackwardLimit = ray.GetPoint(20);

                ray = new Ray(transform.position, transform.right);
                if (Physics.Raycast(ray, out parameterCast, 20))
                    roamingRightLimit = parameterCast.point;
                else
                    roamingRightLimit = ray.GetPoint(20);

                ray = new Ray(transform.position, -transform.right);
                if (Physics.Raycast(ray, out parameterCast, 20))
                    roamingLeftLimit = parameterCast.point;
                else
                    roamingLeftLimit = ray.GetPoint(20);

                setupRoamingComplete = true;
            }

            if ((transform.position.x > roamingRightLimit.x || transform.position.x < roamingLeftLimit.x) || (transform.position.y > roamingForwardLimit.y || transform.position.y > roamingBackwardLimit.y))
                Debug.Log(transform.position + " | "  + roamingForwardLimit + " | " + roamingBackwardLimit + " | " + roamingRightLimit + " | " + roamingLeftLimit);
        }
    }
}
