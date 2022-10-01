using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Templates
{
    public class WeaponTransformTemplate : RPGBuilderDatabaseEntry
    {
        [RPGDataList] public List<WeaponTransform> WeaponTransforms = new List<WeaponTransform>();

        public void UpdateEntryData(WeaponTransformTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;

            WeaponTransforms = new List<WeaponTransform>();
            for (var index = 0; index < newEntryData.WeaponTransforms.Count; index++)
            {
                WeaponTransform newWeaponTransform = new WeaponTransform
                {
                    raceID = newEntryData.WeaponTransforms[index].raceID,
                };

                foreach (var transformValue in newEntryData.WeaponTransforms[index].transformValues)
                {
                    WeaponTransform.TransformValues trsf = new WeaponTransform.TransformValues
                    {
                        gender = transformValue.gender,
                        CombatPosition = transformValue.CombatPosition,
                        CombatRotation = transformValue.CombatRotation,
                        CombatScale = transformValue.CombatScale,
                        RestPosition = transformValue.RestPosition,
                        RestRotation = transformValue.RestRotation,
                        RestScale = transformValue.RestScale,
                        CombatPosition2 = transformValue.CombatPosition2,
                        CombatRotation2 = transformValue.CombatRotation2,
                        CombatScale2 = transformValue.CombatScale2,
                        RestPosition2 = transformValue.RestPosition2,
                        RestRotation2 = transformValue.RestRotation2,
                        RestScale2 = transformValue.RestScale2,
                    };
                    newWeaponTransform.transformValues.Add(trsf);
                }

                WeaponTransforms.Add(newWeaponTransform);
            }
        }

        public List<WeaponTransform> OverrideRequirements(List<WeaponTransform> weaponTransforms)
        {
            weaponTransforms = new List<WeaponTransform>();

            foreach (var weaponTransform in WeaponTransforms)
            {
                WeaponTransform newWeaponTransform = new WeaponTransform
                {
                    raceID = weaponTransform.raceID,
                };

                foreach (var newData in weaponTransform.transformValues)
                {
                    WeaponTransform.TransformValues trsf = new WeaponTransform.TransformValues
                    {
                        gender = newData.gender,
                        CombatPosition = newData.CombatPosition,
                        CombatRotation = newData.CombatRotation,
                        CombatScale = newData.CombatScale,
                        RestPosition = newData.RestPosition,
                        RestRotation = newData.RestRotation,
                        RestScale = newData.RestScale,
                        CombatPosition2 = newData.CombatPosition2,
                        CombatRotation2 = newData.CombatRotation2,
                        CombatScale2 = newData.CombatScale2,
                        RestPosition2 = newData.RestPosition2,
                        RestRotation2 = newData.RestRotation2,
                        RestScale2 = newData.RestScale2,
                    };
                    newWeaponTransform.transformValues.Add(trsf);
                }

                weaponTransforms.Add(newWeaponTransform);
            }
            return weaponTransforms;
        }
    }
}
