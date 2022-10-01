using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder._THMSV.RPGBuilder.Scripts.World;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class RegionManager : MonoBehaviour
    {
        public static RegionManager Instance { get; private set; }
        public RegionTemplate CurrentRegion;

        private float FogTransitionSpeed = 0.5f;
        private float LightTransitionSpeed = 0.5f;
        private float SkyboxTransitionSpeed = 0.5f;
        
        private bool isFogUpdating;
        private Color fogColorTarget;
        private float fogStartDistanceTarget, fogEndDistanceTarget, fogDensityTarget;
        
        private bool isLightUpdating;
        private Color lightColorTarget;
        private float lightIntensityTarget;
        private Light currentLight;
        
        private bool isSkyboxUpdating;
        private Material currentSkybox;
        
        private class RegionCheckingData
        {
            public Region RegionReference;
            public Collider collider;
        }
        
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        private void OnEnable()
        {
            WorldEvents.RegionEntered += EnterRegion;
            WorldEvents.RegionExit += ExitRegion;
        }

        private void OnDisable()
        {
            WorldEvents.RegionEntered -= EnterRegion;
            WorldEvents.RegionExit -= ExitRegion;
        }

        public IEnumerator InitializeDefaultRegion()
        {
            if(GameState.CurrentGameScene.DefaultRegion == null) yield break;
            yield return new WaitForSeconds(0.25f);
            RegionTemplate currentPlayerRegion = getPlayerRegion();
            if (currentPlayerRegion != null)
            {
				EnterRegion(currentPlayerRegion);
                yield break;
            }

            RegionTemplate defaultRegion = GameState.CurrentGameScene.DefaultRegion;
            if (defaultRegion == null) yield break;
            EnterRegion(defaultRegion);
        }
        
        public RegionTemplate getPlayerRegion()
        {
            List<RegionCheckingData> allRegions = new List<RegionCheckingData>();
            
            Region[] regionsRefs = FindObjectsOfType<Region>();

            foreach (var regionRef in regionsRefs)
            {
                RegionCheckingData newRegionCheckData = new RegionCheckingData
                {
                    RegionReference = regionRef, collider = regionRef.GetComponent<Collider>()
                };

                if (newRegionCheckData.collider != null && newRegionCheckData.RegionReference.RegionTemplate != null)
                {
                    allRegions.Add(newRegionCheckData);
                }
            }

            List<RegionCheckingData> regionsWithoutPlayer = new List<RegionCheckingData>();
            foreach (var regionChecked in allRegions)
            {
                if (regionChecked.RegionReference.shapeType == WorldData.RegionShapeType.Cube)
                {
                    if (!ColliderContainsPoint(regionChecked.collider.transform, GameState.playerEntity.transform.position))
                    {
                        regionsWithoutPlayer.Add(regionChecked);
                    }
                }
                else if (regionChecked.RegionReference.shapeType == WorldData.RegionShapeType.Sphere)
                {
                    if (!PointInSphere(GameState.playerEntity.transform.position, regionChecked.collider.transform.position, regionChecked.collider.transform.localScale.y))
                    {
                        regionsWithoutPlayer.Add(regionChecked);
                    }
                }
            }
            
            foreach (var t in regionsWithoutPlayer)
            {
                allRegions.Remove(t);
            }

            if (allRegions.Count == 0)
            {
                return GameState.CurrentGameScene.DefaultRegion;
            }

            RegionTemplate currentRegion = null;
            float curBiggestExtent = 0;
            foreach (var validRegion in allRegions)
            {
                var bounds = validRegion.collider.bounds;
                float extentSize = bounds.extents.x +
                                   bounds.extents.y +
                                   bounds.extents.z;
                if (!(extentSize > curBiggestExtent)) continue;
                curBiggestExtent = extentSize;
                currentRegion = validRegion.RegionReference.RegionTemplate;
            }

            return currentRegion;
        }

        private void EnterRegion(RegionTemplate regionEntered)
        {
            if (CurrentRegion != null)
            {
                if (CurrentRegion.ExitGameActionsTemplate != null)
                {
                    GameActionsManager.Instance.TriggerGameActions(GameState.playerEntity, CurrentRegion.ExitGameActionsTemplate.GameActions);
                }
            }
            CurrentRegion = regionEntered;
            ExecuteRegion(regionEntered);
        }

        private void ExecuteRegion(RegionTemplate region)
        {
            if (region.fogChange)
            {
                TriggerFogChange(region);
            }
            if (region.lightningChange)
            {
                TriggerLightningChange(region);
            }
            if (region.skyboxChange)
            {
                TriggerSkyboxChange(region);
            }
            if (region.musicChange)
            {
                TriggerMusicChange(region);
            }
                
            FadePreviousRegionCameraParticles();
            if (region.cameraParticleChange)
            {
                TriggerCameraParticleChange(region);
            }
                
            if (region.combatModeChange)
            {
                TriggerCombatModeChange(region);
            }
            else
            {
                GameState.combatEnabled = true;
            }
                
            if (region.combatStateChange)
            {
                TriggerCombatStateChange(region);
            }
                
            if (region.EnterGameActionsTemplate != null)
            {
                GameActionsManager.Instance.TriggerGameActions(GameState.playerEntity, region.EnterGameActionsTemplate.GameActions);
            }
        }
        
        public void ExitRegion(RegionTemplate region)
        {
            CurrentRegion = null;
            RegionTemplate currentPlayerRegion = getPlayerRegion();

            if (region.ExitGameActionsTemplate != null)
            {
                GameActionsManager.Instance.TriggerGameActions(GameState.playerEntity, region.ExitGameActionsTemplate.GameActions);
            }

            if (currentPlayerRegion != null)
            {
                EnterRegion(currentPlayerRegion);
            }
            else
            {
                if (GameState.CurrentGameScene.DefaultRegion != null)
                {
                    EnterRegion(GameState.CurrentGameScene.DefaultRegion);
                }
            }
        }

        private void TriggerFogChange(RegionTemplate regionData)
        {
            RenderSettings.fog = regionData.fogEnabled;
            if (!regionData.fogEnabled) return;
            FogTransitionSpeed = regionData.fogTransitionSpeed;
            RenderSettings.fogMode = regionData.fogMode;
            fogColorTarget = regionData.fogColor;
            if (regionData.fogMode == FogMode.Linear)
            {
                fogStartDistanceTarget = regionData.fogStartDistance;
                fogEndDistanceTarget = regionData.fogEndDistance;
            }
            else
            {
                fogDensityTarget = regionData.fogDensity;
            }

            isFogUpdating = true;
        }

        private void TriggerLightningChange(RegionTemplate regionData)
        {
            GameObject lightGO = GameObject.Find(regionData.lightGameobjectName);
            if (lightGO == null) return;
            Light regionLight = lightGO.GetComponent<Light>();
            if (regionLight == null) return;
            currentLight = regionLight;
            regionLight.enabled = regionData.lightEnabled;
            if (!regionData.lightEnabled) return;
            LightTransitionSpeed = regionData.lightTransitionSpeed;
            RenderSettings.fogMode = regionData.fogMode;
            lightColorTarget = regionData.lightColor;
            lightIntensityTarget = regionData.lightIntensity;
            isLightUpdating = true;
        }

        private void TriggerSkyboxChange(RegionTemplate regionData)
        {
            Material skybox = RenderSettings.skybox;
            if (skybox == null) return;
            currentSkybox = skybox;
            SkyboxTransitionSpeed = regionData.skyboxTransitionSpeed;
            if (skybox.HasProperty("_Tex_Blend"))
            {
                skybox.SetTexture("_Tex_Blend", regionData.skyboxCubemap);
                
                if (skybox.HasProperty("_CubemapTransition"))
                {
                    skybox.SetFloat("_CubemapTransition", 0);
                }
            }
            
            isSkyboxUpdating = true;
        }

        private void TriggerMusicChange(RegionTemplate regionData)
        {
            if (regionData.musicClips.Count == 0) return;
            MusicManager.Instance.HandleMusicFadeCoroutine(regionData.musicClips[MusicManager.Instance.GETRandomMusicIndex(0, regionData.musicClips.Count)]);
        }

        private void TriggerCameraParticleChange(RegionTemplate regionData)
        {
            if (Camera.main == null) return;
            BlendCameraParticle(regionData);
        }

        private IEnumerator FadeParticleSystem(ParticleSystem particle)
        {
            var particleMain = particle.main;
            particleMain.loop = false;
            yield return new WaitForSeconds(particleMain.startLifetime.constantMax + 10f);
            if(particle != null) Destroy(particle.gameObject);
        }

        private void FadePreviousRegionCameraParticles()
        {
            foreach (var particle in Camera.main.gameObject.GetComponentsInChildren<CameraParticleObject>())
            {
                StartCoroutine(FadeParticleSystem(particle.GetComponent<ParticleSystem>()));
            }
        }
        
        private void BlendCameraParticle (RegionTemplate regionData)
        {
            if (regionData.cameraParticle == null) return;
            GameObject newCameraParticle = Instantiate(regionData.cameraParticle, Vector3.zero,
                regionData.cameraParticle.transform.rotation);
            newCameraParticle.transform.SetParent(Camera.main.transform);
            newCameraParticle.transform.localPosition = Vector3.zero;
        }

        private void TriggerCombatModeChange(RegionTemplate regionData)
        {
            GameState.combatEnabled = regionData.combatEnabled;
            if(!regionData.combatEnabled) CombatManager.Instance.HandleTurnOffCombat();
        }

        private void TriggerCombatStateChange(RegionTemplate regionData)
        {
            GameState.inCombatOverriden = true;
            if (regionData.inCombat)
            {
                
                if (GameDatabase.Instance.GetCombatSettings().AutomaticCombatStates) GameState.playerEntity.EnterCombat();
            }
            else
            {
                if (GameDatabase.Instance.GetCombatSettings().AutomaticCombatStates)GameState.playerEntity.ResetCombat();
            }
        }
        
        bool ColliderContainsPoint(Transform ColliderTransform, Vector3 Point)
        {
            Vector3 localPos = ColliderTransform.InverseTransformPoint(Point);
            return Mathf.Abs(localPos.x) < 0.5f && Mathf.Abs(localPos.y) < 0.5f && Mathf.Abs(localPos.z) < 0.5f;
        }

        private bool PointInSphere(Vector3 pnt, Vector3 sphereCenter, float sphereRadius)
        {
            return (sphereCenter - pnt).magnitude < sphereRadius;
        }
        
        private void Update()
        {
            if (isFogUpdating)
            {
                HandleFogTransition();
            }
            if (isLightUpdating)
            {
                HandleLightTransition();
            }
            if (isSkyboxUpdating)
            {
                HandleSkyboxTransition();
            }
        }

        private void HandleFogTransition()
        {
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, fogColorTarget, Time.deltaTime * FogTransitionSpeed);
            if (RenderSettings.fogMode == FogMode.Linear)
            {
                RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, fogEndDistanceTarget,
                    Time.deltaTime * FogTransitionSpeed);
                RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, fogStartDistanceTarget,
                    Time.deltaTime * FogTransitionSpeed);
            }
            else
            {
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, fogDensityTarget,
                    Time.deltaTime * FogTransitionSpeed);
            }

            if (!fogTransitionIsDone()) return;
            isFogUpdating = false;
            RenderSettings.fogColor = fogColorTarget;
            RenderSettings.fogEndDistance = fogEndDistanceTarget;
            RenderSettings.fogStartDistance = fogStartDistanceTarget;
            RenderSettings.fogDensity = fogDensityTarget;
        }

        private bool fogTransitionIsDone()
        {
            if (RenderSettings.fogMode == FogMode.Linear)
            {
                if (AlmostMatching(RenderSettings.fogEndDistance, fogEndDistanceTarget, 0.00001f) &&
                    AlmostMatching(RenderSettings.fogStartDistance, fogStartDistanceTarget, 0.00001f))
                {
                    return true;
                }
            }
            else
            {
                if (AlmostMatching(RenderSettings.fogDensity, fogDensityTarget, 0.00001f))
                {
                    return true;
                }
            }

            return false;
        }
        private bool lightTransitionIsDone()
        {
            return AlmostMatching(currentLight.color.r, lightColorTarget.r, 0.001f) &&
                   AlmostMatching(currentLight.color.g, lightColorTarget.g, 0.001f) &&
                   AlmostMatching(currentLight.color.b, lightColorTarget.b, 0.001f) &&
                   AlmostMatching(currentLight.color.a, lightColorTarget.a, 0.001f);
        }
        
        private bool AlmostMatching(float value1, float value2, float treshold)
        {
            return Mathf.Abs(value1 - value2) <= treshold;
        }

        private void HandleLightTransition()
        {
            if (currentLight == null) return;
            currentLight.color = Color.Lerp(currentLight.color, lightColorTarget, Time.deltaTime * LightTransitionSpeed);
            currentLight.intensity = Mathf.Lerp(currentLight.intensity, lightIntensityTarget, Time.deltaTime * LightTransitionSpeed);

            if (!lightTransitionIsDone()) return;
            isLightUpdating = false;
            currentLight.color = lightColorTarget;
            currentLight.intensity = lightIntensityTarget;
        }

        private void HandleSkyboxTransition()
        {
            if (currentSkybox == null) return;
            if (currentSkybox.HasProperty("_CubemapTransition"))
            {
                float lerpValue = Mathf.Lerp(currentSkybox.GetFloat("_CubemapTransition"), 1f,
                    Time.deltaTime * SkyboxTransitionSpeed);
                currentSkybox.SetFloat("_CubemapTransition", lerpValue);

                if (!AlmostMatching(currentSkybox.GetFloat("_CubemapTransition"), 1, 0.05f)) return;
            }

            isSkyboxUpdating = false;
            
            if (currentSkybox.HasProperty("_Tex"))
            {
                currentSkybox.SetTexture("_Tex", currentSkybox.GetTexture("_Tex_Blend"));
            }
            if (currentSkybox.HasProperty("_CubemapTransition"))
            {
                currentSkybox.SetFloat("_CubemapTransition", 0);
            }
        }
    }
}
