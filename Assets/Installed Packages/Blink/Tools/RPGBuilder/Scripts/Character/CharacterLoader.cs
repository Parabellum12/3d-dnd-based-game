using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterLoader : MonoBehaviour
{
    void Start()
    {
        if (RPGBuilderEssentials.Instance != null) return;
        StartCoroutine(LoadSelectedCharacter());
    }

    private IEnumerator LoadSelectedCharacter()
    {
        RPGBuilderEditorSettings editorSettings = Resources.Load<RPGBuilderEditorSettings>("Database/Settings/Editor_Settings");
        if(editorSettings == null) yield break;
        if (editorSettings.characterSelectedName == "") yield break;
        
        bool exist = false;
        foreach (var character in DataSavingSystem.LoadAllCharacters())
        {
            if(character.CharacterName != editorSettings.characterSelectedName) continue;
            exist = true;
        }

        if (!exist)
        {
            Debug.LogError("The character could not be loaded because it no longer exists");
            yield break;
        }
        
        RPGBuilderUISettings gameUISettings = Resources.Load<RPGBuilderUISettings>("Database/Settings/UI_Settings");
        
        if (FindObjectOfType<RPGBuilderEssentials>() == null) Instantiate(gameUISettings.RPGBuilderEssentialsPrefab, Vector3.zero, Quaternion.identity);
        if (FindObjectOfType<LoadingScreenManager>() == null) Instantiate(gameUISettings.LoadingScreenManagerPrefab, Vector3.zero, Quaternion.identity);

        yield return new WaitForFixedUpdate();
        if (Character.Instance == null) yield break;
        if (!IsCurrentSceneValid())
        {
            Debug.LogError("A VALID Game Scene is required for a Character to be loaded");
            yield break;
        }
        
        RPGBuilderJsonSaver.InitializeCharacterData(editorSettings.characterSelectedName);
        CharacterUpdater.UpdateCharacter();
        RPGBuilderEssentials.Instance.LoadCharacterInstantly();
    }

    private bool IsCurrentSceneValid()
    {
        return GameDatabase.Instance.GetGameScenes().Values.Any(gameScene => gameScene.entryName == SceneManager.GetActiveScene().name);
    }
    
    
}
