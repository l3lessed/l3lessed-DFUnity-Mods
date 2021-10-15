using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DaggerfallWorkshop.Game.RandomVariations
{
    [ImportedComponent]
    public class PigController : MonoBehaviour
    {
        [SerializeField]
        private float min = 1f;

        [SerializeField]
        private float max = 1.25f;

        [SerializeField]
        public Vector3 spawnPosition;
        public Vector3 currentPosition;
        private float timer;
        public Vector3 movePosition;
        public Vector3 roamingForwardLimit;
        public Vector3 roamingBackwardLimit;
        public Vector3 roamingLeftLimit;
        public Vector3 roamingRightLimit;
        public List<Vector3> roamingLimiteArray = new List<Vector3>();
        public bool setupRoamingComplete;
        public Vector3 moveBoundCheck;
        public long horseTexturesTotal;
        public SkinnedMeshRenderer[] animalSkinnedMeshArray;

        private void Awake()
        {
            float scale = UnityEngine.Random.Range(min, max);
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(scale, scale, scale));

        }

        private void Start()
        {
            animalSkinnedMeshArray = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer animalSkinnedMesh in animalSkinnedMeshArray)
            {
                if (animalSkinnedMesh == null)
                    return;

                animalSkinnedMesh.material.mainTexture = VariedAnimalsControls.VariedAnimalsControlsInstance.PigTextureList[UnityEngine.Random.Range(0, VariedAnimalsControls.VariedAnimalsControlsInstance.PigTextureList.Count - 1)];
                Debug.Log("APPLY PIG TEXTURE TO: " + transform.gameObject.name + transform.gameObject.GetInstanceID() + " | " + animalSkinnedMesh.name + " | " + animalSkinnedMesh.sortingLayerID);
            }

        }

        private void Update()
        {
            Roaming();
        }

        void Roaming()
        {
            if (!setupRoamingComplete)
            {
                Ray ray = new Ray();
                RaycastHit parameterCast;
                roamingLimiteArray = new List<Vector3>();

                ray = new Ray(transform.position, transform.forward);
                if (Physics.SphereCast(ray, transform.localScale.x, out parameterCast, 15))
                    roamingLimiteArray.Add(parameterCast.point);
                else
                    roamingLimiteArray.Add(ray.GetPoint(15));

                Debug.DrawRay(ray.origin, ray.direction, Color.red, 2000);
                GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = roamingLimiteArray[0];

                ray = new Ray(transform.position, -transform.forward);
                if (Physics.SphereCast(ray, transform.localScale.x, out parameterCast, 15))
                    roamingLimiteArray.Add(parameterCast.point);
                else
                    roamingLimiteArray.Add(ray.GetPoint(15));
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 2000);
                GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = roamingLimiteArray[1];

                ray = new Ray(transform.position, transform.right);
                if (Physics.SphereCast(ray, transform.localScale.x, out parameterCast, 15))
                    roamingLimiteArray.Add(parameterCast.point);
                else
                    roamingLimiteArray.Add(ray.GetPoint(15));
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 2000);
                GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = roamingLimiteArray[2];

                ray = new Ray(transform.position, -transform.right);
                if (Physics.SphereCast(ray, transform.localScale.x, out parameterCast, 15))
                    roamingLimiteArray.Add(parameterCast.point);
                else
                    roamingLimiteArray.Add(ray.GetPoint(15));
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 2000);
                GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = roamingLimiteArray[3];

                movePosition = roamingLimiteArray[UnityEngine.Random.Range(0, roamingLimiteArray.Count)];
                spawnPosition = transform.position;
                setupRoamingComplete = true;
            }


            currentPosition = transform.position;

            timer += Time.deltaTime;

            if (Vector3.Distance(transform.position, movePosition) > 3f)
            {
                Vector3 targetDirection = movePosition - transform.position;
                float step = 1f * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, movePosition, step);

                Vector3 rotationVector = Vector3.RotateTowards(transform.forward, targetDirection, step * 5, 0);
                transform.rotation = Quaternion.LookRotation(rotationVector);
                //transform.Translate(Vector3.forward * Time.deltaTime);
            }
            else if (timer > 2.0f)
            {
                timer = 0;
                Ray ray = new Ray();
                RaycastHit parameterCast;
                roamingLimiteArray = new List<Vector3>();

                ray = new Ray(transform.position, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0) * -transform.forward);

                if (Physics.SphereCast(ray, transform.localScale.x, out parameterCast, UnityEngine.Random.Range(5, 15)))
                    moveBoundCheck = parameterCast.point;
                else
                    moveBoundCheck = ray.GetPoint(UnityEngine.Random.Range(5, 15));
                Debug.DrawRay(ray.origin, ray.direction, Color.green, 2000);



                if (Vector3.Distance(moveBoundCheck, spawnPosition) > 12)
                    return;
                //GameObject castMarker;
                Destroy(GameObject.CreatePrimitive(PrimitiveType.Cube), 10f);
                //castMarker.transform.position = moveBoundCheck;
                //castMarker.name = "Path Finding Marker";

                movePosition = moveBoundCheck;
            }

            Debug.Log(transform.position + " | " + Vector3.Distance(transform.position, movePosition) + " | " + timer + " | " + Vector3.Distance(moveBoundCheck, spawnPosition));
        }
    }
}
