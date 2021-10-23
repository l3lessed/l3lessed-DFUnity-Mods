using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections;
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
        private float followerTimer;
        private Vector3 startPosition;
        public Vector3 movePosition;
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

        [SerializeField]
        public Animation tailAnimation = new Animation();
        private bool walking;
        private float idleTimer;
        private float totalMoveDistance;
        private float animalDistancetoMovePosition;
        private float roamingRadius = 25;
        private float movementTimer;
        private float movementPercentage;
        public float walkSpeed = .4f;
        public float runSpeed = .8f;
        private CharacterController controller;
        public float currentMovementSpeed = 0;
        public float minWalk = 0;
        public float maxWalk = .8f;
        [SerializeField]
        public List<Vector3> playersLastPos = new List<Vector3>();
        public List<GameObject> followerMarkerList = new List<GameObject>();

        public float Acceleration = .9f;
        public Vector3 velocity;
        Task walkRoutine;
        public float rotationCheckValue;
        private bool moving;
        private float myAng;
        private float timer;

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
            walk,
            follow,
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
            //runs and sets object states to decide what it should do.
            HorseBrain();
            float gSpeed = 0;
            if (controller.isGrounded)
            {
                gSpeed = 0;
            }

            gSpeed -= 100f * Time.deltaTime;
            controller.Move(new Vector3(0, gSpeed, 0) * Time.deltaTime);
        }


        void HorseBrain()
        {
            if (horseState == BehaviorState.walk)
            {
                //if there is a assigned walk routine and it isn't running yet, start it.
                if (walkRoutine != null && !walkRoutine.Running)
                    walkRoutine.Start();

                //play walk animations with blended tail.
                if (!mainAnimation.isPlaying)
                    mainAnimation.Blend(horseAnimationList[AnimationType.walk], 1f);

                if (!mainAnimation.IsPlaying(horseAnimationList[AnimationType.tail_swoosh]))
                    mainAnimation.Blend(horseAnimationList[AnimationType.idle_bay_tail], .2f);
            }

            if(horseState == BehaviorState.follow)
            {
                FollowPlayer();
            }

            //if horse brian is idle, start playing random idle animations and begin looking for a new move place.
            if (horseState == BehaviorState.idle)
            {
                //set base animation for blending as idle animation.
                if (!mainAnimation.IsPlaying(horseAnimationList[AnimationType.idle_bay]))
                    mainAnimation.Blend(horseAnimationList[AnimationType.idle_bay], .65f, .5f);

                //run idle timer to set rbadom idle animation plays.
                idleTimer += Time.deltaTime;
                if (idleTimer > UnityEngine.Random.Range(1.5f, 3))
                {
                    UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                    idleTimer = 0;
                    mainAnimation.Blend(horseAnimationList[(AnimationType)idleAnimationList[UnityEngine.Random.Range(0, idleAnimationList.Length)]], .65f, .5f);
                    mainAnimation.Blend(horseAnimationList[AnimationType.idle_bay_tail], .5f, .5f);
                }

                //if found a place to move to, tell horse brain to walk so it can begin processing walk code/intelligence.
                FindMoveDestination(out movePosition);
            }

        }

        //finds a move destination and outputs it as a vector3 for further processing by brain code.
        bool FindMoveDestination(out Vector3 movePosition)
        {
            //set a move timer so it checks at set interval.
            timer += Time.deltaTime;
            //set if a spot has been found to default to false.
            bool foundMoveSpot = false;
            //set move position as empty vector3.
            movePosition = new Vector3 (0,0,0);

            //after very 10 to 30 seconds, run a find move position check.
            if (timer > UnityEngine.Random.Range(10, 30))
            {
                //reset check timer and reassign random seed.
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                timer = 0;
                //setup blank ray objects for finding new move position.
                Ray ray = new Ray();
                RaycastHit parameterCast;

                //create ray object.
                ray = new Ray(controller.transform.position, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0) * -transform.forward);

                //use spherecast to try and ensure position is a place the object can get to/fit in.
                if (Physics.SphereCast(ray, 3, out parameterCast, UnityEngine.Random.Range(5, roamingRadius)))
                    moveBoundCheck = parameterCast.point;
                //If nothing is hit, return the end point.
                else
                    moveBoundCheck = ray.GetPoint(UnityEngine.Random.Range(5, roamingRadius));

                Debug.Log(Vector3.Distance(moveBoundCheck, transform.position) + " | " + Vector3.Distance(moveBoundCheck, spawnPosition));

                //if the distance is less than 10 meters or outside return that there is no found spot to go to.
                if (Vector3.Distance(moveBoundCheck, transform.position) < 5 || Vector3.Distance(moveBoundCheck, spawnPosition) > roamingRadius)
                    foundMoveSpot = false;
                else
                {
                    //if there is a walk routine and is is running, stop it.
                    if (walkRoutine!= null && walkRoutine.Running)
                        walkRoutine.Stop();

                    //setup debug move sphere to see where npc is placing move destination.
                    Debug.Log("FOUND SPOT!!");
                    GameObject markerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    markerSphere.transform.position = moveBoundCheck;
                    Destroy(markerSphere, 10);
                    //crossfade previous animation into a walk.
                    mainAnimation.CrossFade(horseAnimationList[AnimationType.walk]);
                    //setup a new walk routine using the position data from the raycasts.
                    walkRoutine = new Task(MoveToDestination(transform.position,moveBoundCheck, 1),false);
                    //tell horse brain/code to begin walking state/code
                    horseState = BehaviorState.walk;
                    //return that we found a spot to move to as true.
                    foundMoveSpot = true;
                }
            }
            //return if spot was found or not.
            return foundMoveSpot;
        }

        //Uses inputed positions to move object to destination.
        IEnumerator MoveToDestination(Vector3 startPosition, Vector3 moveDestination, float SpeedModifier)
        {
            while (true)
            {
                Vector3 targetDirection = (moveDestination - transform.position).normalized;

                if(Vector3.Distance(transform.position, moveDestination) > 2f)
                {
                    currentMovementSpeed = currentMovementSpeed * SpeedModifier;

                    //var forward = transform.TransformDirection(Vector3.forward);

                    velocity = transform.forward * currentMovementSpeed;

                    Vector3 rotationVector = Vector3.RotateTowards(transform.forward, targetDirection, 2 * Time.deltaTime, 0);
                    transform.rotation = Quaternion.LookRotation(new Vector3(rotationVector.x, 0, rotationVector.z));

                    Debug.Log(" velocity: " + controller.velocity.magnitude + " Rotation: " + rotationCheckValue);

                    rotationCheckValue = Quaternion.Dot(transform.rotation, Quaternion.LookRotation(targetDirection));
                    if (rotationCheckValue >= 0.95f)
                    {
                        currentMovementSpeed = Mathf.Clamp(currentMovementSpeed += maxWalk * Time.deltaTime, 1, walkSpeed);
                        //Vector3.ClampMagnitude(velocity, Acceleration);
                        controller.Move(velocity * Time.deltaTime);
                        mainAnimation[horseAnimationList[AnimationType.walk]].speed = controller.velocity.magnitude * .66f;
                    }
                    else
                    {
                        if(currentMovementSpeed != 0)
                            currentMovementSpeed = Mathf.Clamp(currentMovementSpeed -= maxWalk, 2, walkSpeed);
                        else
                            currentMovementSpeed = Mathf.Clamp(currentMovementSpeed += maxWalk, 2, walkSpeed);

                        controller.Move((velocity * .5f) * Time.deltaTime);
                        mainAnimation[horseAnimationList[AnimationType.walk]].speed = 1;
                    }
                }
                else
                {
                    mainAnimation.CrossFade(horseAnimationList[AnimationType.idle_bay], 1);

                    if (horseState != BehaviorState.follow)
                        horseState = BehaviorState.idle;

                    moving = false;
                    currentMovementSpeed = 0;
                    walkRoutine.Stop();
                    yield break;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            myAng = Vector3.Angle(Vector3.up, hit.normal); //Calc angle between normal and character
        }

        void FollowPlayer()
        {
            followerTimer += Time.deltaTime;
            if(followerTimer > .008f)
            {
                RaycastHit lineOfSightHit;

               if (NPCinLOS(out lineOfSightHit))
                {
                    if(playersLastPos.Count > 1)
                    {
                        playersLastPos.Clear();

                        foreach(GameObject markerObject in followerMarkerList)
                        {
                            Destroy(markerObject);
                        }
                        followerMarkerList.Clear();
                    }                        

                    followerTimer = 0;
                    if (playersLastPos.Count == 0)
                    {
                        playersLastPos.Add(GameManager.Instance.PlayerController.transform.position);
                        GameObject markerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        markerSphere.transform.localScale = new Vector3(.35f, .35f, .35f);
                        markerSphere.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        Destroy(markerSphere.GetComponent<SphereCollider>());
                        markerSphere.name = "Follower Marker: " + (playersLastPos.Count - 1).ToString();
                        markerSphere.transform.position = GameManager.Instance.PlayerController.transform.position;
                        followerMarkerList.Add(markerSphere);
                    }
                    else if (Vector3.Distance(GameManager.Instance.PlayerController.transform.position, playersLastPos[playersLastPos.Count - 1]) > 3)
                    {
                        Destroy(followerMarkerList[0]);
                        followerMarkerList.RemoveAt(0);
                        playersLastPos.RemoveAt(0);
                        walkRoutine.Stop();
                        playersLastPos.Add(GameManager.Instance.PlayerController.transform.position);
                        GameObject markerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        markerSphere.transform.localScale = new Vector3(.35f, .35f, .35f);
                        markerSphere.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        Destroy(markerSphere.GetComponent<SphereCollider>());
                        markerSphere.name = "Follower Marker: " + (playersLastPos.Count - 1).ToString();
                        markerSphere.transform.position = GameManager.Instance.PlayerController.transform.position;
                        followerMarkerList.Add(markerSphere);
                    }
                }
                else if (!NPCinLOS(out lineOfSightHit))
                {
                    followerTimer = 0;
                    if (playersLastPos.Count > 19 && Vector3.Distance(GameManager.Instance.PlayerController.transform.position, playersLastPos[playersLastPos.Count - 1]) > 3)
                    {
                        walkRoutine.Stop();
                        playersLastPos.RemoveAt(0);
                        playersLastPos.Add(GameManager.Instance.PlayerController.transform.position);
                        GameObject markerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        markerSphere.transform.localScale = new Vector3(.35f, .35f, .35f);
                        markerSphere.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        Destroy(markerSphere.GetComponent<SphereCollider>());
                        markerSphere.name = "Follower Marker: " + (playersLastPos.Count - 1).ToString();
                        markerSphere.transform.position = GameManager.Instance.PlayerController.transform.position;
                        followerMarkerList.Add(markerSphere);
                    }
                    else if (playersLastPos.Count != 0 && Vector3.Distance(GameManager.Instance.PlayerController.transform.position, playersLastPos[playersLastPos.Count - 1]) > 3)
                    {
                        playersLastPos.Add(GameManager.Instance.PlayerController.transform.position);
                        GameObject markerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        markerSphere.transform.localScale = new Vector3(.35f, .35f, .35f);
                        markerSphere.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        Destroy(markerSphere.GetComponent<SphereCollider>());
                        markerSphere.name = "Follower Marker: " + (playersLastPos.Count - 1).ToString();
                        markerSphere.transform.position = GameManager.Instance.PlayerController.transform.position;
                        followerMarkerList.Add(markerSphere);
                    }
                    else if(playersLastPos.Count == 0)
                    {
                        playersLastPos.Add(GameManager.Instance.PlayerController.transform.position);
                        GameObject markerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        markerSphere.transform.localScale = new Vector3(.35f, .35f, .35f);
                        markerSphere.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        Destroy(markerSphere.GetComponent<SphereCollider>());
                        markerSphere.name = "Follower Marker: " + (playersLastPos.Count - 1).ToString();
                        markerSphere.transform.position = GameManager.Instance.PlayerController.transform.position;
                        followerMarkerList.Add(markerSphere);
                    }
                }
            }
 

            if (playersLastPos.Count != 0)
            {
                if(walkRoutine == null)
                {
                    walkRoutine = new Task(MoveToDestination(transform.position, playersLastPos[0], 1));
                    mainAnimation.CrossFade(horseAnimationList[AnimationType.walk]);
                }                    

                if (walkRoutine != null && !walkRoutine.Running)
                {
                    walkRoutine = new Task(MoveToDestination(transform.position, playersLastPos[0], 1));
                    mainAnimation.CrossFade(horseAnimationList[AnimationType.walk]);
                }

                if (Vector3.Distance(transform.position, playersLastPos[0]) <= 3f)
                {
                    Destroy(followerMarkerList[0]);
                    followerMarkerList.RemoveAt(0);
                    playersLastPos.RemoveAt(0);
                    walkRoutine.Stop();
                }
            }
            else
            {
                currentMovementSpeed = 0;
                mainAnimation.CrossFade(horseAnimationList[AnimationType.idle_bay], 1);
                //set base animation for blending as idle animation.
                if (!mainAnimation.IsPlaying(horseAnimationList[AnimationType.idle_bay]))
                    mainAnimation.Blend(horseAnimationList[AnimationType.idle_bay], .65f, .5f);

                //run idle timer to set rbadom idle animation plays.
                idleTimer += Time.deltaTime;
                if (idleTimer > UnityEngine.Random.Range(1.5f, 3))
                {
                    UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                    idleTimer = 0;
                    mainAnimation.Blend(horseAnimationList[(AnimationType)idleAnimationList[UnityEngine.Random.Range(0, idleAnimationList.Length)]], .65f, .5f);
                    mainAnimation.Blend(horseAnimationList[AnimationType.idle_bay_tail], .5f, .5f);
                }
            }

        }

        bool NPCinLOS(out RaycastHit hitPosition)
        {
            bool inSight = false;
            Vector3 playerDir = GameManager.Instance.PlayerController.transform.position - transform.position;           
            Ray ray = new Ray(transform.position + transform.up, playerDir * 100);
            Debug.DrawRay(transform.position + transform.up , playerDir * 100,Color.red);
            if (Physics.SphereCast(ray.origin,2,ray.direction,out hitPosition))
            {
                CharacterController playerCheck = hitPosition.transform.GetComponent<CharacterController>();

                if (playerCheck != null && playerCheck.gameObject.name == "PlayerAdvanced")
                    inSight = true;
            }

            Debug.Log(inSight);           
            return inSight;
        }    
    }
}
