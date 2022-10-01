using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class TimeManager : MonoBehaviour
    {
        private Light directionalLight;

        private float nextSecondUpdate;
        private float FrameTimePassed;
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += FindDirectionalLight;
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= FindDirectionalLight;
        }

        private void Update()
        {
            if (!GameState.IsInGame()) return;
            UpdateGameTime();
            
            int currentSeconds = Character.Instance.CharacterData.Time.CurrentHour * (GameDatabase.Instance.GetWorldSettings().SecondsPerMinutes * GameDatabase.Instance.GetWorldSettings().MinutesPerHour);
            currentSeconds += Character.Instance.CharacterData.Time.CurrentMinute * GameDatabase.Instance.GetWorldSettings().SecondsPerMinutes;
            currentSeconds += Character.Instance.CharacterData.Time.CurrentSecond;
            int maxSeconds = GameDatabase.Instance.GetWorldSettings().HoursPerDay * (GameDatabase.Instance.GetWorldSettings().SecondsPerMinutes * GameDatabase.Instance.GetWorldSettings().MinutesPerHour);
            float timePercent = (float)currentSeconds / (float)maxSeconds;
            
            if(GameState.CurrentGameScene.UpdateSunPosition && !RegionOverrideLightning()) UpdateSun(timePercent);
            if(GameState.CurrentGameScene.UpdateFog && !RegionOverrideFog()) UpdateFog(timePercent);
        }

        private bool RegionOverrideLightning()
        {
            return RegionManager.Instance.CurrentRegion != null && RegionManager.Instance.CurrentRegion.lightningChange;
        }

        private bool RegionOverrideFog()
        {
            return RegionManager.Instance.CurrentRegion != null && RegionManager.Instance.CurrentRegion.fogChange;
        }

        private void UpdateGameTime()
        {
            FrameTimePassed += Time.deltaTime;
            if (!(FrameTimePassed >= GetSecondDuration())) return;
            float seconds = FrameTimePassed / GetSecondDuration();
            FrameTimePassed -= GetSecondDuration() * seconds;
            Character.Instance.CharacterData.Time.CurrentSecond += (int)seconds;
            if (Character.Instance.CharacterData.Time.CurrentSecond > GameDatabase.Instance.GetWorldSettings().SecondsPerMinutes)
            {
                Character.Instance.CharacterData.Time.CurrentSecond = GameDatabase.Instance.GetWorldSettings().SecondsPerMinutes;
            }
            HandleTime();
            WorldEvents.Instance.OnTimeChange(Character.Instance.CharacterData.Time);
        }

        private float GetSecondDuration()
        {
            return GameDatabase.Instance.GetWorldSettings().SecondDuration /
                   GameDatabase.Instance.GetWorldSettings().GlobalTimeSpeed;
        }

        private void HandleTime()
        {
            RPGBuilderWorldSettings worldSettings = GameDatabase.Instance.GetWorldSettings();
            if (Character.Instance.CharacterData.Time.CurrentSecond < worldSettings.SecondsPerMinutes) return;
            Character.Instance.CharacterData.Time.CurrentSecond = 0;
            if (Character.Instance.CharacterData.Time.CurrentMinute <= (worldSettings.MinutesPerHour-2))
            {
                Character.Instance.CharacterData.Time.CurrentMinute++;
            }
            else
            {
                if (Character.Instance.CharacterData.Time.CurrentHour <= (worldSettings.HoursPerDay-2))
                {
                    Character.Instance.CharacterData.Time.CurrentHour++;
                    Character.Instance.CharacterData.Time.CurrentMinute = 0;
                }
                else
                {
                    if (Character.Instance.CharacterData.Time.CurrentDay <= (worldSettings.DaysPerWeek-2))
                    {
                        Character.Instance.CharacterData.Time.CurrentDay++;
                        Character.Instance.CharacterData.Time.CurrentHour = 0;
                        Character.Instance.CharacterData.Time.CurrentMinute = 0;
                    }
                    else
                    {
                        if (Character.Instance.CharacterData.Time.CurrentWeek <= (worldSettings.WeeksPerMonth-2))
                        {
                            Character.Instance.CharacterData.Time.CurrentWeek++;
                            Character.Instance.CharacterData.Time.CurrentDay = 0;
                        }
                        else
                        {
                            if (Character.Instance.CharacterData.Time.CurrentMonth <= (worldSettings.MonthsPerYear-2))
                            {
                                Character.Instance.CharacterData.Time.CurrentMonth++;
                                Character.Instance.CharacterData.Time.CurrentWeek = 0;
                            }
                            else
                            {
                                Character.Instance.CharacterData.Time.CurrentYear++;
                                Character.Instance.CharacterData.Time.CurrentMonth = 0;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateSun(float timePercent)
        {
            if (directionalLight == null) return;
            directionalLight.color = GameState.CurrentGameScene.SunColors.Evaluate(timePercent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent*360f) - 90f, GameState.CurrentGameScene.SunRotationAxis, 0));
        }

        private void UpdateFog(float timePercent)
        {
            RenderSettings.fogColor = GameState.CurrentGameScene.FogColors.Evaluate(timePercent);
        }

        private void FindDirectionalLight()
        {
            if (RenderSettings.sun != null) directionalLight = RenderSettings.sun;
            else
            {
                Light[] lights = FindObjectsOfType<Light>();
                foreach (var light in lights)
                {
                    if(light.type != LightType.Directional) continue;
                    directionalLight = light;
                    return;
                }
            }
        }
    }
}
