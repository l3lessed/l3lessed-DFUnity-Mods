using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace Minimap
{
    public class RagClean : MonoBehaviour
    {
        Texture2D ragTexture;
        public RectTransform ragRect;
        public float testing;
        GameObject ragEffect;
        float lerpTime = 1f;
        float currentLerpTime;
        public float periodX = .5f;
        public float amplitudeX = 120f;
        public float periodY = .2f;
        public float amplitudeY = 50;
        private Vector3 lastAnchorPosition;

        private void Start()
        {
            Texture2D ragTexture = null;
            byte[] fileData;

            fileData = File.ReadAllBytes(Application.streamingAssetsPath + "/Textures/Minimap/dirtyRag.png");
            ragTexture = new Texture2D(2, 2);
            ragTexture.LoadImage(fileData);

            ragEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Rag Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize, new Vector3(0, 0, 0), ragTexture, Color.white, 1);
            ragEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            ragRect = ragEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            ragRect.localScale = new Vector3(1f, 1f, 0);
        }

        private void Update()
        {
            if(lastAnchorPosition != Minimap.MinimapInstance.publicCompass.transform.localPosition)
                ragRect.localPosition = Minimap.MinimapInstance.publicCompass.transform.localPosition;

            if (EffectManager.cleaningCompass)
            {
                ragEffect.SetActive(true);
                ragRect.localPosition = Minimap.MinimapInstance.publicCompass.transform.localPosition;
                float thetaX = Time.timeSinceLevelLoad / periodX;
                float distanceX = amplitudeX * Mathf.Sin(thetaX);
                float thetaY = Time.timeSinceLevelLoad / periodY;
                float distanceY = amplitudeY * Mathf.Sin(thetaY);
                ragRect.SetAsLastSibling();
                ragRect.localPosition = new Vector3(Minimap.MinimapInstance.publicCompass.transform.localPosition.x + Vector3.left.x * distanceX, Minimap.MinimapInstance.publicCompass.transform.localPosition.y + Vector3.up.y * distanceY);
                return;
            }
            lastAnchorPosition = Minimap.MinimapInstance.publicCompass.transform.localPosition;
            ragEffect.SetActive(false);
        }
    }
}
