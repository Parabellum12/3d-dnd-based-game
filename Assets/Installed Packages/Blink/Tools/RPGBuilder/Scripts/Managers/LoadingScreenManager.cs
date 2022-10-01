using System;
using System.Collections;
using BLINK.RPGBuilder.LogicMono;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class LoadingScreenManager : MonoBehaviour
    {
        public static LoadingScreenManager Instance { get; private set; }

        public Canvas loadingCanvas;
        public Image loadingBackground, loadingProgressImage;
        public TextMeshProUGUI gameSceneNameText, gameSceneDescriptionText, loadingProgressText;

        public bool isSceneLoading;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (Instance != null) return;
            Instance = this;
        }

        public void LoadGameScene(int gameSceneID)
        {
            RPGGameScene gameScene = GameDatabase.Instance.GetGameScenes()[gameSceneID];
            if (gameScene == null) return;

            GameEvents.Instance.OnStartNewGameSceneLoad();
            loadingCanvas.enabled = true;
            loadingBackground.sprite = gameScene.loadingBG;
            loadingProgressImage.fillAmount = 0;
            gameSceneNameText.text = gameScene.entryDisplayName;
            gameSceneDescriptionText.text = gameScene.entryDescription;

            loadingProgressText.text = 0 + " %";

            asyncLoad = new AsyncOperation();
            StartCoroutine(AsyncLoad(gameScene));
        }
        
        public void LoadMainMenu()
        {
            loadingCanvas.enabled = true;
            loadingBackground.sprite = GameDatabase.Instance.GetGeneralSettings().mainMenuLoadingImage;
            loadingProgressImage.fillAmount = 0;
            gameSceneNameText.text = GameDatabase.Instance.GetGeneralSettings().mainMenuLoadingName;
            gameSceneDescriptionText.text = GameDatabase.Instance.GetGeneralSettings().mainMenuLoadingDescription;

            loadingProgressText.text = 0 + " %";

            asyncLoad = new AsyncOperation();
            StartCoroutine(AsyncLoadMainMenu());
        }

        private void ResetLoadingCanvas()
        {
            asyncLoad = null;
            loadingCanvas.enabled = false;
            loadingBackground.sprite = null;
            loadingProgressImage.fillAmount = 0;
            gameSceneNameText.text = "";
            gameSceneDescriptionText.text = "";

            loadingProgressText.text = 0 + " %";
        }

        private AsyncOperation asyncLoad = null;

        IEnumerator AsyncLoad(RPGGameScene gameSscene)
        {
            asyncLoad = SceneManager.LoadSceneAsync(gameSscene.entryName);
            asyncLoad.allowSceneActivation = !GameDatabase.Instance.GetGeneralSettings().clickToLoadScene;

            isSceneLoading = true;
            
            while (!asyncLoad.isDone)
            {
                loadingProgressImage.fillAmount = asyncLoad.progress / 1f;
                int curProgress = (int) (asyncLoad.progress * 100f);
                loadingProgressText.text = curProgress + " %";

                if (!asyncLoad.allowSceneActivation && asyncLoad.progress >= 0.9f)
                {
                    loadingProgressText.text = "Click to continue";
                    loadingProgressImage.fillAmount = 1f;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                        asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.25f);
            if (GameDatabase.Instance.GetGeneralSettings().LoadingScreenEndDelay > 0)
            {
                loadingProgressText.text = "Loading World";
                loadingProgressImage.fillAmount = 1f;
                yield return new WaitForSeconds(GameDatabase.Instance.GetGeneralSettings().LoadingScreenEndDelay);
            }

            isSceneLoading = false;
            ResetLoadingCanvas();
        }
        
        IEnumerator AsyncLoadMainMenu()
        {
            asyncLoad = SceneManager.LoadSceneAsync(GameDatabase.Instance.GetGeneralSettings().mainMenuSceneName);
            asyncLoad.allowSceneActivation = true;
            
            while (!asyncLoad.isDone)
            {
                loadingProgressImage.fillAmount = asyncLoad.progress / 1f;
                int curProgress = (int) (asyncLoad.progress * 100f);
                loadingProgressText.text = curProgress + " %";
                yield return null;
            }
            ResetLoadingCanvas();
        }
    }
}
