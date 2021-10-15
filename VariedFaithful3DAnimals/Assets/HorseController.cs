using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DaggerfallWorkshop.Game.RandomVariations
{
    [ImportedComponent]
    public class HorseController : MonoBehaviour
    {
        [SerializeField]
        private float min = 1f;

        [SerializeField]
        private float max = 1.25f;

        [SerializeField]
        public Vector3 spawnPosition;
        public Vector3 currentPosition;
        private float timer;
        private Vector3 startPosition;
        public Vector3 movePosition;
        private Vector3 targetDirection;
        public Vector3 roamingForwardLimit;
        public Vector3 roamingBackwardLimit;
        public Vector3 roamingLeftLimit;
        public Vector3 roamingRightLimit;
        public List<Vector3> roamingLimiteArray = new List<Vector3>();
        public BehaviorState horseState = new BehaviorState();
        public bool setupRoamingComplete;
        public Vector3 moveBoundCheck;
        public Transform tail;
        public int[] idleAnimationList = new int[] {2,5,9};
        //drag the bone here in the inspector
        public SkinnedMeshRenderer[] animalSkinnedMeshArray;
        private Texture2D horseTexture;
        [SerializeField]
        public List<AnimationClip> clips = new List<AnimationClip>();
        [SerializeField]
        public Animation mainAnimation = new Animation();
        [SerializeField]
        public Dictionary<AnimationType, string> horseAnimationList;


        public Animation tailAnimation = new Animation();
        private bool walking;
        private float idleTimer;
        private float totalMoveDistance;
        private float animalDistancetoMovePosition;
        private float roamingRadius = 25;
        private float movementTimer;
        private float movementPercentage;
        public float walkSpeed = .6f;
        public float runSpeed = 1.75f;
        private CharacterController controller;
        public float currentMovementSpeed = 12;
        public float minWalk = 0;
        public float maxWalk = 1;

        public float Acceleration = .9f;
        public Vector3 velocity;
        public float rotationCheckValue;

        public enum AnimationType
        {
            idle,
            idle_bay,
            idle_bay_tail,
            idle_baytogray,
            idle_gray,
            idle_headtilt,
            tail_swoosh,
            trot,
            walk,
            head_shake
        }

        public enum BehaviorState
        {
            idle,
            trot,
            walk,
        }

        private void Awake()
        {
            float scale = UnityEngine.Random.Range(min, max);
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(scale, scale, scale));
            
        }

        private void Start()
        {
            horseState = BehaviorState.idle;
            horseAnimationList = new Dictionary<AnimationType, string>();
            mainAnimation = transform.GetComponent<Animation>();
            clips = new List<AnimationClip>(AnimationUtility.GetAnimationClips(transform.gameObject));

            foreach (AnimationClip animationClip in clips)
            {
                horseAnimationList.Add((AnimationType)clips.IndexOf(animationClip), animationClip.name);
            }

            animalSkinnedMeshArray = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            horseTexture = VariedAnimalsControls.VariedAnimalsControlsInstance.HorseTextureList[UnityEngine.Random.Range(0, VariedAnimalsControls.VariedAnimalsControlsInstance.HorseTextureList.Count - 1)];

            if (animalSkinnedMeshArray == null && horseTexture == null && transform.GetComponentInChildren<HorseController>() != null)
                return;

            foreach (SkinnedMeshRenderer animalSkinnedMesh in animalSkinnedMeshArray)
            {
                if (animalSkinnedMesh == null)
                    return;

                animalSkinnedMesh.material.mainTexture = horseTexture;
            }

            mainAnimation.Blend(horseAnimationList[AnimationType.idle_bay]);
            spawnPosition = transform.position;            

            controller = GetComponent<CharacterController>();
            bool isGrounded = controller.isGrounded;
            float verticalVelosity = 0;

            if (isGrounded)
            {
                verticalVelosity -= 0;
            }
            else
            {
                verticalVelosity -= 1;
            }

            Vector3 moveVector = new Vector3(0, verticalVelosity, 0);
            controller.Move(moveVector);
        }

        private void Update()
        {
            HorseBrain();
        }


        void HorseBrain()
        {
            if (horseState == BehaviorState.walk)
            {
                float distanceMoved = Vector3.Distance(transform.position, movePosition);
                movementPercentage = distanceMoved / totalMoveDistance;

                var forward = transform.TransformDirection(Vector3.forward);

                Debug.Log("Move Distance: " + distanceMoved + "Total Distance:  " + totalMoveDistance + " Move %: " + movementPercentage + " velocity: " + controller.velocity.magnitude);

                if (!mainAnimation.isPlaying)
                    mainAnimation.Blend(horseAnimationList[AnimationType.walk], 1f);

                if (!mainAnimation.IsPlaying(horseAnimationList[AnimationType.tail_swoosh]))
                    mainAnimation.Blend(horseAnimationList[AnimationType.idle_bay_tail], .2f);

                Vector3 rotationVector = Vector3.RotateTowards(forward.normalized, targetDirection.normalized, .6f * Time.fixedDeltaTime, 0);
                transform.rotation = Quaternion.LookRotation(rotationVector);
                rotationCheckValue = Quaternion.Dot(transform.rotation, Quaternion.LookRotation(targetDirection));
                if (rotationCheckValue >= 0.9f)
                {
                    velocity = controller.velocity;

                    velocity += forward.normalized * currentMovementSpeed;

                    velocity *= Acceleration;                    

                    Vector3.ClampMagnitude(velocity, 1);

                    controller.SimpleMove(velocity * Time.fixedDeltaTime);
                    mainAnimation[horseAnimationList[AnimationType.walk]].speed = controller.velocity.magnitude * 1.45f;
                }
                else
                {
                    controller.SimpleMove(forward.normalized * (currentMovementSpeed * .05f) * Time.fixedDeltaTime);
                    mainAnimation[horseAnimationList[AnimationType.walk]].speed = 1;
                }

                //idle horse when it reaches position.
                if (movementPercentage < .05f)
                {
                    mainAnimation.CrossFade(horseAnimationList[AnimationType.idle_bay], 1);
                    horseState = BehaviorState.idle;
                }

            }

            if(horseState == BehaviorState.trot)
            {
                //will add specialized running/trotting code here
            }

            if (horseState == BehaviorState.idle)
            {
                if (!mainAnimation.IsPlaying(horseAnimationList[AnimationType.idle_bay]))
                    mainAnimation.Blend(horseAnimationList[AnimationType.idle_bay], .65f, .5f);

                idleTimer += Time.deltaTime;
                if (idleTimer > UnityEngine.Random.Range(1.5f, 3))
                {
                    UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                    idleTimer = 0;
                    mainAnimation.Blend(horseAnimationList[(AnimationType)idleAnimationList[UnityEngine.Random.Range(0, idleAnimationList.Length)]],.65f,.5f);                 
                    mainAnimation.Blend(horseAnimationList[AnimationType.idle_bay_tail], .5f, .5f);
                }

                FindMoveDestination();
            }

            void FindMoveDestination()
            {
                timer += Time.deltaTime;
                if (timer > UnityEngine.Random.Range(10, 30))
                {
                    UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                    timer = 0;
                    Ray ray = new Ray();
                    RaycastHit parameterCast;
                    roamingLimiteArray = new List<Vector3>();

                    ray = new Ray(transform.position, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0) * -transform.forward);

                    if (Physics.SphereCast(ray, 2, out parameterCast, UnityEngine.Random.Range(10, roamingRadius)))
                        moveBoundCheck = parameterCast.point;
                    else
                        moveBoundCheck = ray.GetPoint(UnityEngine.Random.Range(10, roamingRadius));
                    Debug.DrawRay(ray.origin, ray.direction, Color.green, 2000);

                    if (Vector3.Distance(moveBoundCheck, transform.position) < 10)
                        return;

                    horseState = BehaviorState.walk;

                    startPosition = transform.position;
                    movePosition = moveBoundCheck;
                    targetDirection = movePosition - transform.position;

                    totalMoveDistance = Vector3.Distance(movePosition, startPosition);
                    mainAnimation.CrossFade(horseAnimationList[AnimationType.walk]);
                }
            }
        }
    }
}
