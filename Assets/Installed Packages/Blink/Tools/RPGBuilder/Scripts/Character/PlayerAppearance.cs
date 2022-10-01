using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BLINK.RPGBuilder.Characters
{
    public class PlayerAppearance : MonoBehaviour
    {
        public bool hasActiveWeaponAnimatorOverride;
        
        public GameObject cachedBodyParent;
        public GameObject cachedArmorsParent;
        
        public GameObject[] armorPieces;
        public GameObject weapon1GO, weapon2GO;
        public Transform HeadTransformSocket;
        private RPGItem weapon1RPGItem, weapon2RPGItem;
        
        public List<GameObject> armatureReferences = new List<GameObject>();
        public GameObject armatureParentGO;
        public Vector3 armatureParentOffset;
        
        [Serializable]
        public class WeaponSlot
        {
            public bool collapsed;
            public RPGBWeaponType WeaponType;
            public Transform RightHandCombat;
            public Transform LeftHandCombat;
            public Transform RightHandRest;
            public Transform LeftHandRest;
        }
        public List<WeaponSlot> WeaponSlots = new List<WeaponSlot>();

        public List<BodyPart> BodyParts = new List<BodyPart>();

        public void ShowArmor(RPGItem item, GameObject meshManager)
        {
            if (GameState.IsInGame() && GameState.playerEntity.IsShapeshifted()) return;
            foreach (var t in armorPieces)
            {
                if(t == null) continue;
                if (t.name != item.itemModelName) continue;
                t.SetActive(true);
                HideBodyPart(item.BodyCullingTemplate);
                if (item.modelMaterial != null)
                {
                    foreach (var Renderer in t.GetComponentsInChildren<Renderer>())
                    {
                        Renderer.material = item.modelMaterial;
                    }
                }
                if (meshManager != null) EnchantingManager.Instance.ApplyEnchantParticle(meshManager, t);
            }
        }

        public void HideArmor(RPGItem item)
        {
            foreach (var t in armorPieces)
            {
                if(t == null) continue;
                if (t.name != item.itemModelName) continue;
                MeshParticleManager[] meshManagers = t.GetComponentsInChildren<MeshParticleManager>();
                foreach (var v in meshManagers)
                {
                    Destroy(v.gameObject);
                }
                t.SetActive(false);
            }

            ShowBodyPart(item.BodyCullingTemplate);
        }
        
        
        private void HideBodyPart(BodyCullingTemplate template)
        {
            if(BodyParts.Count == 0 || template == null || template.HiddenBodyParts.Count == 0) return;
            foreach (var bodyPart in BodyParts)
            {
                if(bodyPart == null) continue;
                foreach (var hiddenPart in template.HiddenBodyParts)
                {
                    if(hiddenPart.raceID != Character.Instance.CharacterData.RaceID) continue;
                    foreach (var value in hiddenPart.Values)
                    {
                        if (value.Gender.entryName != Character.Instance.CharacterData.Gender) continue;
                        foreach (var part in value.BodyParts)
                        {
                            if (part != bodyPart.bodyPart) continue;
                            if (bodyPart.bodyRenderer != null) bodyPart.bodyRenderer.enabled = false;
                        }
                    }
                }
            }
        }

        private void ShowBodyPart(BodyCullingTemplate template)
        {
            if(BodyParts.Count == 0 || template == null || template.HiddenBodyParts.Count == 0) return;
            foreach (var bodyPart in BodyParts)
            {
                if(bodyPart == null) continue;
                foreach (var hiddenPart in template.HiddenBodyParts)
                {
                    if(hiddenPart.raceID != Character.Instance.CharacterData.RaceID) continue;
                    foreach (var value in hiddenPart.Values)
                    {
                        if (value.Gender.entryName != Character.Instance.CharacterData.Gender) continue;
                        foreach (var part in value.BodyParts)
                        {
                            if (part != bodyPart.bodyPart) continue;
                            if (bodyPart.bodyRenderer != null) bodyPart.bodyRenderer.enabled = true;
                        }
                    }
                }
            }
        }

        public void HideWeapon(int weaponID)
        {
            switch (weaponID)
            {
                case 1:
                {
                    if (weapon1GO != null)
                    {
                        Destroy(weapon1GO);
                        weapon1RPGItem = null;
                    }

                    break;
                }
                case 2:
                {
                    if (weapon2GO != null)
                    {
                        Destroy(weapon2GO);
                        weapon2RPGItem = null;
                    }

                    break;
                }
            }
        }

        public void UpdateWeaponStates(bool inCombat)
        {
            if (weapon1GO != null)
            {
                weapon1GO.transform.SetParent(GetSlot(inCombat, weapon1RPGItem, 1));
                SetWeaponPosition(weapon1GO, weapon1RPGItem, inCombat, true);
            }
            if (weapon2GO != null)
            {
                weapon2GO.transform.SetParent(GetSlot(inCombat, weapon2RPGItem, 2));
                SetWeaponPosition(weapon2GO, weapon2RPGItem, inCombat, false);
            }
        }

        private Transform GetSlot(bool InCombat, RPGItem item, int weaponNumber)
        {
            foreach (var weaponSlot in WeaponSlots)
            {
                if(weaponSlot.WeaponType != item.WeaponType) continue;
                if(InCombat) return weaponNumber == 1 ? weaponSlot.RightHandCombat : weaponSlot.LeftHandCombat;
                return weaponNumber == 1 ? weaponSlot.RightHandRest : weaponSlot.LeftHandRest;
            }

            return RPGBuilderEssentials.Instance.getCurrentScene().name == GameDatabase.Instance.GetGeneralSettings().mainMenuSceneName ? MainMenuManager.Instance.curPlayerModel.transform : GameState.playerEntity.transform;
        }

        void SetWeaponPosition(GameObject go, RPGItem weaponItem, bool inCombat, bool mainHand)
        {
            Vector3[] weaponPositionData = getWeaponPositionData(weaponItem, inCombat, mainHand);
            go.transform.localPosition = weaponPositionData[0];
            go.transform.localRotation = Quaternion.Euler(weaponPositionData[1]);
            go.transform.localScale = weaponPositionData[2];
        }

        Vector3[] getWeaponPositionData(RPGItem weaponItem, bool inCombat, bool mainHand)
        {
            Vector3[] weaponPositionData = new Vector3[3];
            weaponPositionData[2] = Vector3.one;

            foreach (var t in (weaponItem.UseWeaponTransformTemplate && weaponItem.WeaponTransformTemplate != null) ? weaponItem.WeaponTransformTemplate.WeaponTransforms : weaponItem.WeaponTransforms)
            {
                if (t.raceID != Character.Instance.CharacterData.RaceID) continue;
                foreach (var t1 in t.transformValues)
                {
                    if (t1.gender.entryName != Character.Instance.CharacterData.Gender) continue;
                    if (inCombat)
                    {
                        weaponPositionData[0] = mainHand ? t1.CombatPosition : t1.CombatPosition2;
                        weaponPositionData[1] = mainHand ? t1.CombatRotation : t1.CombatRotation2;
                        weaponPositionData[2] = mainHand ? t1.CombatScale : t1.CombatScale2;
                    }
                    else
                    {
                        weaponPositionData[0] = mainHand ? t1.RestPosition : t1.RestPosition2;
                        weaponPositionData[1] = mainHand ? t1.RestRotation : t1.RestRotation2;
                        weaponPositionData[2] = mainHand ? t1.RestScale : t1.RestScale2;
                    }
                }
            }
            return weaponPositionData;
        }

        private void ParentWeaponToSlot(GameObject weaponGO, int weaponID, RPGItem weaponItem, bool inCombat)
        {
            weaponGO.transform.SetParent(GetSlot(inCombat, weaponItem, weaponID));
            SetWeaponPosition(weaponGO, weaponItem, inCombat, weaponID == 1);
        }

        private Coroutine temporaryWeaponCoroutine;

        public void ShowWeaponsTemporarily(float duration)
        {
            if (temporaryWeaponCoroutine != null)
            {
                StopCoroutine(temporaryWeaponCoroutine);
            }
            
            temporaryWeaponCoroutine = StartCoroutine(HandleTemporaryWeapons(duration));
        }

        private IEnumerator HandleTemporaryWeapons(float duration)
        {
            UpdateWeaponStates(true);
            yield return new WaitForSeconds(duration);
            if(!GameState.playerEntity.IsInCombat()) UpdateWeaponStates(false);
        }

        public void HandleAnimatorOverride()
        {
            if (GameState.playerEntity.IsShapeshifted())
            {
                GameState.playerEntity.GetAnimator().runtimeAnimatorController = GameState.playerEntity.IsInCombat()
                    ? GameState.playerEntity.ShapeshiftedEffect.ranks[GameState.playerEntity.ShapeshiftedEffectRank].shapeshiftingAnimatorControllerCombat
                    : GameState.playerEntity.ShapeshiftedEffect.ranks[GameState.playerEntity.ShapeshiftedEffectRank].shapeshiftingAnimatorController;
            }
            else
            {
                RPGRace raceREF = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
                if (raceREF.Genders[RPGBuilderUtilities.GetGenderIndexByName(Character.Instance.CharacterData.Gender)].DynamicAnimator)
                {
                    GameState.playerEntity.GetAnimator().runtimeAnimatorController = GameState.playerEntity.IsInCombat()
                        ? raceREF.Genders[RPGBuilderUtilities.GetGenderIndexByName(Character.Instance.CharacterData.Gender)].CombatAnimatorController
                        : raceREF.Genders[RPGBuilderUtilities.GetGenderIndexByName(Character.Instance.CharacterData.Gender)].RestAnimatorController;
                }
                else
                {
                    if(GameState.playerEntity.CachedAnimatorController != null) GameState.playerEntity.GetAnimator().runtimeAnimatorController = GameState.playerEntity.CachedAnimatorController;
                }

                RuntimeAnimatorController newAnimatorController = RPGBuilderUtilities.getNewWeaponAnimatorOverride();
                if (newAnimatorController != null)
                {
                    hasActiveWeaponAnimatorOverride = true;
                    GameState.playerEntity.GetAnimator().runtimeAnimatorController = newAnimatorController;
                }
            }
        }

        public void ShowWeapon(RPGItem weaponItem, int weaponID, GameObject meshManager, bool InCombat)
        {
            if (GameState.IsInGame() && GameState.playerEntity.IsShapeshifted()) return;
            if (weaponItem == null) return;
            switch (weaponID)
            {
                case 1:
                {
                    var newWeaponGO = Instantiate(weaponItem.weaponModel, transform.position, Quaternion.identity);
                    ParentWeaponToSlot(newWeaponGO, weaponID, weaponItem, InCombat);
                    if (weapon1GO != null) Destroy(weapon1GO);

                    weapon1GO = newWeaponGO;
                    weapon1RPGItem = weaponItem;
                    if (weaponItem.modelMaterial != null)
                    {
                        foreach (var Renderer in newWeaponGO.GetComponentsInChildren<Renderer>())
                        {
                            Renderer.material = weaponItem.modelMaterial;
                        }
                    }
                    if(meshManager !=null) EnchantingManager.Instance.ApplyEnchantParticle(meshManager, newWeaponGO);
                    break;
                }
                case 2:
                {
                    var newWeaponGO = Instantiate(weaponItem.weaponModel, transform.position, Quaternion.identity);
                    ParentWeaponToSlot(newWeaponGO, weaponID, weaponItem, InCombat);
                    if (weapon2GO != null) Destroy(weapon2GO);

                    weapon2GO = newWeaponGO;
                    weapon2RPGItem = weaponItem;
                    if (weaponItem.modelMaterial != null)
                    {
                        foreach (var Renderer in newWeaponGO.GetComponentsInChildren<Renderer>())
                        {
                            Renderer.material = weaponItem.modelMaterial;
                        }
                    }
                    if(meshManager !=null) EnchantingManager.Instance.ApplyEnchantParticle(meshManager, newWeaponGO);
                    break;
                }
            }
        }
        
        public void HandleBodyScaleFromStats()
        {
            float bodyScaleModifier = 1;

            foreach (var stat in GameState.playerEntity.GetStats())
            {
                if(stat.Value.stat==null) continue;
                foreach (var bonus in stat.Value.stat.statBonuses)
                {
                    if(bonus.statType != RPGStat.STAT_TYPE.BODY_SCALE) continue;
                    bodyScaleModifier += bonus.modifyValue * stat.Value.currentValue;
                }
            }
            InitBodyScale(bodyScaleModifier);
        }

        public void InitBodyScale(float bodyScaleModifier)
        {
            if (armatureParentGO == null) return;
            foreach (var armatureRef in armatureReferences)
            {
                float bodyScale = 1 * bodyScaleModifier;
                armatureRef.transform.localScale = new Vector3(bodyScale, bodyScale, bodyScale);
            }

            armatureParentGO.transform.localPosition = SceneManager.GetActiveScene().name == GameDatabase.Instance.GetGeneralSettings().mainMenuSceneName ? new Vector3(0, bodyScaleModifier - 1, 0) : new Vector3(0+armatureParentOffset.x, (bodyScaleModifier - 1)+armatureParentOffset.y, 0+armatureParentOffset.z);
        }

    }
}
