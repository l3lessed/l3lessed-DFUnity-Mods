using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Minimap
{
    public class BloodEffectController : MonoBehaviour
    {

        public static Dictionary<ulong, List<BloodEffect>> compassBloodDictionary = new Dictionary<ulong, List<BloodEffect>>();
        public static Dictionary<string, Texture2D> bloodTextureDict = new Dictionary<string, Texture2D>();
        public static List<string> activeBloodTextures = new List<string>();
        public static List<BloodEffect> bloodEffectList = new List<BloodEffect>();
        private string currentBloodTextureName;
        public static bool bloodEffectTrigger;

        private void Awake()
        {
            Texture2D singleTexture = null;
            byte[] fileData;

            //begin creating texture array's using stored texture folders/texture sets.\\
            //grab directory info for blood and load pngs using a for loop.
            DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/blood");
            FileInfo[] FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/minimap/blood/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                Debug.Log("BLOOD TEXTURE ADDED: " + textureFile.Name);

                bloodTextureDict.Add(textureFile.Name, singleTexture);
            }
        }

        private void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;
            //BLOOD EFFECT\\
            //setup health damage blood layer effects. If players health changes run effect code.
            if (EffectManager.enabledBloodEffect && bloodEffectTrigger)
            {
                Debug.Log("Triggered Blood");
                int randomID = Minimap.MinimapInstance.randomNumGenerator.Next(0, bloodTextureDict.Count - 1);
                currentBloodTextureName = bloodTextureDict.ElementAt(randomID).Key;
                //loops through current effects to ensure it always generates new blood textures until they are all applied.
                foreach (BloodEffect bloodEffectInstance in bloodEffectList)
                {

                    if (bloodEffectInstance.textureName == currentBloodTextureName)
                    {
                        foreach (string texturename in bloodTextureDict.Keys)
                        {
                            if (bloodEffectInstance.textureName != texturename)
                                currentBloodTextureName = texturename;
                        }
                    }
                }

                //if all blood textures are already loaded, find the current selected texture, and remove the old effect
                if (bloodEffectList.Count == bloodTextureDict.Count)
                {
                    //cycle through effect list until finds matching effect, reset its alpha and position.
                    foreach (BloodEffect bloodEffectInstance in bloodEffectList)
                    {
                        if (bloodEffectInstance.textureName == currentBloodTextureName)
                        {
                            if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                                bloodEffectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                            else
                                bloodEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;

                            bloodEffectInstance.effectRawImage.color = new Color(1, 1, 1, .9f);
                        }
                    }
                }
                //if the list isn't full, find the first texture that doesn't match the id,
                else
                {
                    BloodEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<BloodEffect>();
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                    else
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                    effectInstance.effectType = Minimap.EffectType.Blood;
                    effectInstance.effectTexture = bloodTextureDict[currentBloodTextureName];
                    effectInstance.textureName = currentBloodTextureName;
                    bloodEffectList.Add(effectInstance);
                    if (!compassBloodDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                        compassBloodDictionary.Add(Minimap.MinimapInstance.currentEquippedCompass.UID, bloodEffectList);
                    else
                        compassBloodDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID] = bloodEffectList;
                    EffectManager.totalEffects = EffectManager.totalEffects + 1;
                }
                bloodEffectTrigger = false;
            }
        }
    }
}
