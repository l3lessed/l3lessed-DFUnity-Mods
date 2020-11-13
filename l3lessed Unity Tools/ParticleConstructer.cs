using UnityEngine;
namespace ParticleConstructorClass
{
    public class ParticleConstructor : MonoBehaviour
    {
            ParticleSystem system;
            Texture2D particleTex;
            Light lightPrefab;
            GameObject storedParticleSystem;

            //PARTICLE SYSTEM SETUP: Sets up the first/beta particle system.\\
            public void setupParticleSystem(Vector3 SystemRotation, ParticleSystem.ColorOverLifetimeModule ColorModule, ParticleSystem.MinMaxGradient ParticleColor, ParticleSystemRenderMode RenderMode, float StretchVelocity)
            {
                storedParticleSystem = new GameObject("Particle System");
                system = storedParticleSystem.AddComponent<ParticleSystem>();
                // Create a textured stretched Particle System.
                storedParticleSystem.transform.Rotate(SystemRotation); // Rotate so the system emits upwards.
                ParticleSystem.ColorOverLifetimeModule colorModule = ColorModule;
                colorModule.color = ParticleColor;
                storedParticleSystem.GetComponent<ParticleSystemRenderer>().renderMode = RenderMode;
                storedParticleSystem.GetComponent<ParticleSystemRenderer>().velocityScale = StretchVelocity;
            }

            public void setupMaterials(string ShaderName, Mesh ParticleMesh, bool MaterialEmissions, Color EmissionColor, Color MaterialColor, Texture2D ParticleTexture = null)
            {
                Material particleMaterial = new Material(Shader.Find(ShaderName));

                if(ParticleTexture != null)
                    particleMaterial.mainTexture = ParticleTexture;

                storedParticleSystem.GetComponent<ParticleSystemRenderer>().mesh = ParticleMesh;
                storedParticleSystem.GetComponent<ParticleSystemRenderer>().material = particleMaterial;

                if (MaterialEmissions)
                {
                    storedParticleSystem.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    storedParticleSystem.GetComponent<Renderer>().material.SetColor("_EMISSION", EmissionColor);
                }

                storedParticleSystem.GetComponent<Renderer>().material.SetColor("_Color", MaterialColor);
            }

            public void setupMainSystem(Color StartParticleColor, ParticleSystemEmitterVelocityMode EmitterVelocityMode, float ParticleDuration, float ParticleSize, float GravityMod, bool loop)
            {
                //sets up particle system main module.
                var mainModule = system.main;
                mainModule.startColor = StartParticleColor;
                mainModule.emitterVelocityMode = EmitterVelocityMode;
                mainModule.duration = ParticleDuration;
                mainModule.startSize = ParticleSize;
                mainModule.gravityModifier = GravityMod;
                mainModule.loop = loop;
            }

            public void setupLight(Color LightColor, float ParticleLightRation, float LightIntensity, float Range)
            {
                //setup lights for sparks.
                GameObject lightGameObject = new GameObject("The Light");
                var lights = system.lights;
                lightPrefab = lightGameObject.AddComponent<Light>();
                // Add the light component
                lights.enabled = true;
                lightPrefab.color = LightColor;
                lights.ratio = ParticleLightRation;
                lights.intensityMultiplier = LightIntensity;
                lights.rangeMultiplier = Range;
                lights.light = lightPrefab;
            }

            public void setupEmitter(ParticleSystemShapeType SystemShape, Vector3 SystemScale)
            {
                var shape = system.shape;
                shape.enabled = true;
                shape.shapeType = SystemShape;
                shape.scale = SystemScale;
                //sets up the emession burst at 0 seconds to add sparks to first particle emmission.
            }

            public void setupParticleBursts(float BurstTiming, float ParticleAmounts)
            {
                var em = system.emission;
                em.enabled = true;
                em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(BurstTiming, ParticleAmounts) });
            }

        public void setupTrails(string ShaderName,float ParticleTrailRatio, float TrailLifeTime, float TrailWidth, GradientColorKey[] ColorGradient, GradientAlphaKey[] AlphaGradient)
            {
                Gradient gradient = new Gradient();

                var trails = system.trails;
                storedParticleSystem.GetComponent<ParticleSystemRenderer>().trailMaterial = new Material(Shader.Find(ShaderName));
                trails.enabled = true;
                trails.ratio = ParticleTrailRatio;
                trails.lifetime = TrailLifeTime;
                trails.widthOverTrail = TrailWidth;
                //trails.inheritParticleColor = true;
                gradient.SetKeys(ColorGradient, AlphaGradient);
                trails.colorOverLifetime = gradient;
            }

            public ParticleSystem ReturnConstructedParticle()
            {
                Debug.Log("Returning System: " + system.ToString());
                return system;
            }
    }
}

