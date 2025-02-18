using DaggerfallWorkshop.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Minimap
{
    public class MiniMapClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool overMap;
        private MeshRenderer mouseOverIconMesh;
        private GameObject mouseOverIcon;
        private GameObject mouseOverLabel;
        public static GameObject cylinder;
        public static GameObject cylinderDetector;
        public static GameObject beaconSphere;
        public static MeshRenderer cylinderMesh;
        public static MeshRenderer cylinderDetectorMesh;
        public static MeshRenderer sphereMesh;
        private float bob = 1;
        private float bobscale = .01f;

        void Start()
        {
            //setup all properties for mouse over icon obect. Will be used below when player drags mouse over icons in full screen mode.
            if (!mouseOverIcon)
            {
                mouseOverIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
                mouseOverIcon.name = "Mouse Over Icon";
                mouseOverIcon.transform.Rotate(0, 180, 0);
                mouseOverIcon.layer = Minimap.layerMinimap;
                mouseOverIconMesh = mouseOverIcon.GetComponent<MeshRenderer>();
                mouseOverIconMesh.material = Minimap.iconMarkerMaterial;
                mouseOverIconMesh.material.color = Color.white;
                mouseOverIconMesh.shadowCastingMode = 0;
                Destroy(mouseOverIcon.GetComponent<Collider>());
            }

            //setup all properties for mouse over label obect. Will be used below.
            if (!mouseOverLabel)
            {
                mouseOverLabel = new GameObject();
                mouseOverLabel.name = "Mouse Over Label";
                TextMeshPro labelutility = mouseOverLabel.AddComponent<TMPro.TextMeshPro>();
                mouseOverLabel.layer = Minimap.layerMinimap;
                RectTransform textboxRect = mouseOverLabel.GetComponent<RectTransform>();
                labelutility.enableAutoSizing = true;
                textboxRect.sizeDelta = new Vector2(500, 500);
                labelutility.isOrthographic = true;
                labelutility.fontMaterial = Minimap.labelMaterial;
                labelutility.alignment = TextAlignmentOptions.CenterGeoAligned;
                labelutility.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.0f, 0.0f, 0.0f));
                //labelutility.material.enableInstancing = true;
                labelutility.characterSpacing = 2;
                labelutility.fontSizeMin = 26;
                labelutility.enableWordWrapping = true;
                labelutility.fontStyle = TMPro.FontStyles.Bold;
                labelutility.outlineColor = new Color32(0, 0, 0, 255);
                labelutility.outlineWidth = .33f;
                labelutility.color = Color.magenta;
                //remove collider from mes.
                Destroy(mouseOverLabel.GetComponent<Collider>());
            }

        }

        void Update()
        {
            //if in full map mode and the player is pressing down on mouse, begin moving minimap around with player mouse.
            //this is the dynamic full screen map feature.
            if (Minimap.MinimapInstance.FullMinimapMode)
            {
                GameManager.Instance.PlayerMouseLook.cursorActive = true;
                if (overMap)
                {
                    RaycastHit hit;
                    GameManager.Instance.PlayerMouseLook.sensitivityScale = 0;
                    BuildingMarker hoverOverBuilding = null;
                    IconController hoverIcon = null;
                    //if a sphere cast hits a collider, do the following.
                    RaycastHit hitSpot = CastMiniMapRayToWorld(localCursorPoint(Minimap.MinimapInstance.minimapRectTransform, Input.mousePosition));

                    //grab the building marker for the building being hovered over.
                    if (hoverOverBuilding == null)
                        hoverOverBuilding = hitSpot.collider.GetComponentInParent<BuildingMarker>();

                    MeshRenderer hoverBuildingMesh = null;
                    //if there is an attached marker and marker icon, run code for label or icon show.
                    if (hoverOverBuilding)
                    {
                        //if hit building and label is active, pop up the icon for player.
                        if (Minimap.minimapControls.labelsActive && !Minimap.minimapControls.iconsActive)
                        {
                            Texture hoverTexture = hoverOverBuilding.marker.iconTexture;
                            mouseOverIconMesh.material.mainTexture = hoverTexture;
                            mouseOverIcon.transform.rotation = Quaternion.Euler(0, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y + 180f, 0);
                            mouseOverIcon.transform.position = hitSpot.point;
                            mouseOverIcon.transform.Translate(new Vector3(5f, 8f, 5f));
                            mouseOverIcon.transform.localScale = new Vector3(Minimap.minimapCamera.orthographicSize * .0175f, Minimap.minimapCamera.orthographicSize * .0175f, Minimap.minimapCamera.orthographicSize * .0175f);
                            mouseOverIcon.SetActive(true);
                        }
                        //if the icon is active and player his building, pop up label on building.
                        else if (Minimap.minimapControls.iconsActive && !Minimap.minimapControls.labelsActive)
                        {
                            mouseOverLabel.transform.position = hitSpot.point;
                            mouseOverLabel.transform.Translate(new Vector3(12f, 8f, -30f));
                            mouseOverLabel.transform.rotation = Quaternion.Euler(90f, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);

                            mouseOverLabel.transform.localScale = new Vector3(Minimap.minimapCamera.orthographicSize * .001f, Minimap.minimapCamera.orthographicSize * .001f, Minimap.minimapCamera.orthographicSize * .001f);

                            mouseOverLabel.GetComponent<TextMeshPro>().text = hoverOverBuilding.marker.dynamicBuildingName;

                            mouseOverLabel.SetActive(true);
                        }
                        //if neither are true, but a building is still hit, pop up the label and icon.
                        else if (!Minimap.minimapControls.iconsActive && !Minimap.minimapControls.labelsActive)
                        {
                            mouseOverLabel.transform.position = hitSpot.point;
                            mouseOverLabel.transform.Translate(new Vector3(0, Mathf.Clamp(Minimap.minimapCamera.orthographicSize * .11f, 0, 17), -8f));
                            mouseOverLabel.transform.rotation = Quaternion.Euler(90f, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);

                            mouseOverLabel.transform.localScale = new Vector3(Mathf.Clamp(Minimap.minimapCamera.orthographicSize * .0011f, 0, .16f), Mathf.Clamp(Minimap.minimapCamera.orthographicSize * .001f, 0, .16f), Mathf.Clamp(Minimap.minimapCamera.orthographicSize * .001f, 0, .16f));

                            mouseOverLabel.GetComponent<TextMeshPro>().text = hoverOverBuilding.marker.dynamicBuildingName;

                            Texture hoverTexture = hoverOverBuilding.marker.iconTexture;
                            mouseOverIconMesh.material.mainTexture = hoverTexture;
                            mouseOverIcon.transform.rotation = Quaternion.Euler(0, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y + 180f, 0);
                            mouseOverIcon.transform.position = hitSpot.point;
                            mouseOverIcon.transform.Translate(new Vector3(Mathf.Clamp(Minimap.minimapCamera.orthographicSize * .0011f, 0, 17), 8f, Mathf.Clamp(-Minimap.minimapCamera.orthographicSize * .0014f, -20, 0)));
                            mouseOverIcon.transform.localScale = new Vector3(Mathf.Clamp(Minimap.minimapCamera.orthographicSize * .0175f, 0, 2.7f), Mathf.Clamp(Minimap.minimapCamera.orthographicSize * .0175f, 0, 2.7f), Mathf.Clamp(Minimap.minimapCamera.orthographicSize * .0175f, 0, 2.7f));

                            mouseOverIcon.SetActive(true);
                            mouseOverLabel.SetActive(true);
                        }
                    }
                    //if nothing is hit hide/disable label and icon.
                    else
                    {
                        mouseOverLabel.SetActive(false);
                        mouseOverIcon.SetActive(false);
                    }
                }
            }
            //if not in drag mode, center camera, hide mouseover label, and icon and enable player mouse look again.
            else if (!Minimap.MinimapInstance.FullMinimapMode)
            {
                Minimap.MinimapInstance.minimapCameraX = 0;
                Minimap.MinimapInstance.minimapCameraZ = 0;
                mouseOverLabel.SetActive(false);
                mouseOverIcon.SetActive(false);
                GameManager.Instance.PlayerMouseLook.sensitivityScale = Minimap.MinimapInstance.playerDefaultMouseSensitivity;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //If your mouse hovers over the GameObject with the script attached, output this message
            Debug.Log("Mouse is over GameObject.");
            overMap = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameManager.Instance.PlayerMouseLook.cursorActive = false;
            //If your mouse hovers over the GameObject with the script attached, output this message
            Debug.Log("Mouse is over GameObject.");
            overMap = false;
        }
        //Detect if the Cursor starts to pass over the GameObject
        public void OnPointerClick(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Minimap.MinimapInstance.minimapRectTransform, eventData.pressPosition, eventData.pressEventCamera, out Vector2 localCursorPoint))
            {

                Rect imageRectSize = Minimap.MinimapInstance.minimapRectTransform.rect;

                //localCursorPoint is the distance on x and y axis from the rect center point
                //off we add the imageRectSize (by substracting because it's negative) which is the half size
                //the rectangle so we can get the local coordinates x and y inside the rectangle
                //then we divide them by the rectSize so we can get their ratios (between 0.0 - 1.0)
                localCursorPoint.x = (localCursorPoint.x - imageRectSize.x) / imageRectSize.width;
                localCursorPoint.y = (localCursorPoint.y - imageRectSize.y) / imageRectSize.height;
                RaycastHit hitSpot = CastMiniMapRayToWorld(localCursorPoint);
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    Minimap.minimapCamera.transform.position = new Vector3(hitSpot.point.x, Minimap.minimapCamera.transform.position.y, hitSpot.point.z);
                }

                if (eventData.button == PointerEventData.InputButton.Right && !GameManager.Instance.IsPlayerInside)
                {
                    Debug.LogError(hitSpot.collider.gameObject.name);
                    if(hitSpot.collider.gameObject.name == "Beacon Detector")
                    {
                        Destroy(cylinder);
                        Destroy(cylinderDetector);
                        Destroy(beaconSphere);
                        return;
                    }

                    if (cylinder == null)
                    {
                        cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        cylinderDetector = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        beaconSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        cylinder.name = "Beacon";
                        cylinderDetector.name = "Beacon Detector";
                        cylinder.layer = (1 << LayerMask.NameToLayer("Default"));
                        beaconSphere.layer = (1 << LayerMask.NameToLayer("Default"));
                        cylinderDetector.layer = 31;
                        Destroy(cylinder.GetComponent<Collider>());
                        Destroy(beaconSphere.GetComponent<Collider>());
                        cylinderMesh = cylinder.GetComponent<MeshRenderer>();
                        cylinderDetectorMesh = cylinderDetector.GetComponent<MeshRenderer>();
                        sphereMesh = beaconSphere.GetComponent<MeshRenderer>();
                        cylinderMesh.material = Minimap.beaconMaterial;
                        cylinderDetector.GetComponent<MeshRenderer>().material = Minimap.MinimapInstance.playerArrowMaterial;
                        beaconSphere.GetComponent<MeshRenderer>().material = cylinderMesh.material;
                        cylinderMesh.material.color = Minimap.iconGroupColors[Minimap.MarkerGroups.Beacon];
                        cylinderDetectorMesh.material.color = Minimap.iconGroupColors[Minimap.MarkerGroups.Beacon];
                        cylinder.transform.localScale = new Vector3(1.5f * Minimap.iconSizes[Minimap.MarkerGroups.Beacon], 300, 1.5f * Minimap.iconSizes[Minimap.MarkerGroups.Beacon]);
                        beaconSphere.transform.localScale = new Vector3(4 * Minimap.iconSizes[Minimap.MarkerGroups.Beacon], 4 * Minimap.iconSizes[Minimap.MarkerGroups.Beacon], 4 * Minimap.iconSizes[Minimap.MarkerGroups.Beacon]);
                        cylinderDetector.transform.localScale = new Vector3(15 * Minimap.iconSizes[Minimap.MarkerGroups.Beacon], .5f, 15 * Minimap.iconSizes[Minimap.MarkerGroups.Beacon]);
                    }
                    cylinder.transform.position = hitSpot.point;
                    beaconSphere.transform.position = hitSpot.point;
                    cylinderDetector.transform.position = new Vector3(hitSpot.point.x, hitSpot.point.y + 300f, hitSpot.point.z);
                }
            }
        }

        public static Vector2 localCursorPoint(RectTransform canvasRectTransform, Vector2 MousePosition)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, MousePosition, null, out Vector2 localCursorPoint))
            {
                Rect imageRectSize = canvasRectTransform.rect;

                //localCursorPoint is the distance on x and y axis from the rect center point
                //off we add the imageRectSize (by substracting because it's negative) which is the half size
                //the rectangle so we can get the local coordinates x and y inside the rectangle
                //then we divide them by the rectSize so we can get their ratios (between 0.0 - 1.0)
                localCursorPoint.x = (localCursorPoint.x - imageRectSize.x) / imageRectSize.width;
                localCursorPoint.y = (localCursorPoint.y - imageRectSize.y) / imageRectSize.height;
            }
            return localCursorPoint;
        }

        public static RaycastHit CastMiniMapRayToWorld(Vector2 localCursor)
        {
            //we multiply the local ratios inside the minimap image rect with the minimap camera's pixelWidth so we can get the right pixel coordinates for the ray
            Ray miniMapRay = Minimap.minimapCamera.ScreenPointToRay(new Vector2(localCursor.x * Minimap.minimapCamera.pixelWidth, localCursor.y * Minimap.minimapCamera.pixelHeight));

            //we cast the ray through the minimap camera, which will hit the world point that it pointed towards
            Physics.Raycast(miniMapRay, out RaycastHit miniMapHit, Mathf.Infinity);
            return miniMapHit;


        }
    }
}