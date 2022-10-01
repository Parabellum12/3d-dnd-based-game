using BLINK.RPGBuilder.Templates;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class MinimapDisplay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private RectTransform mapContainer, playerArrow, panel;
        [SerializeField] private Image minimapImage;
        [SerializeField] private TextMeshProUGUI regionName;

        [SerializeField] private Transform playerTransform;
        [SerializeField] private float mapScale = 0.1f;

        private bool Initialized;
        private RPGGameScene curGameScene;
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += InitializeMinimap;
            WorldEvents.RegionEntered += UpdateRegionName;
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= InitializeMinimap;
            WorldEvents.RegionEntered -= UpdateRegionName;
        }
        
        private void InitializeMinimap()
        {
            RPGGameScene currentGameScene = RPGBuilderUtilities.GetGameSceneFromName(SceneManager.GetActiveScene().name);
            curGameScene = currentGameScene;
            minimapImage.sprite = currentGameScene.minimapImage;
            Initialized = true;
            regionName.text = RPGBuilderUtilities.GetGameSceneFromName(SceneManager.GetActiveScene().name).entryDisplayName;
            RPGBuilderUtilities.EnableCG(thisCG);
        }
        
        private void Update()
        {
            if (Initialized) UpdateMinimap();
        }

        private void UpdateRegionName(RegionTemplate region)
        {
            if (regionName != null)
            {
                regionName.text = region.entryDisplayName;
            }
        }

        private void UpdateMinimap()
        {
            if (playerTransform == null)
            {
                if (GameState.playerEntity != null) playerTransform = GameState.playerEntity.transform;
                return;
            }

            playerArrow.transform.rotation = Quaternion.Euler(panel.transform.eulerAngles.x, panel.transform.eulerAngles.y,
                -playerTransform.eulerAngles.y);

            var unitScale = GetMapUnitScale();
            var posOffset = curGameScene.mapBounds.center - playerTransform.position;
            var mapPos = new Vector3(posOffset.x * unitScale.x, posOffset.z * unitScale.y, 0f) * mapScale;

            mapContainer.localPosition = new Vector2(mapPos.x, mapPos.y);
            mapContainer.localScale = new Vector3(mapScale, mapScale, 1f);
        }

        private Vector2 GetMapUnitScale()
        {
            return new Vector2(curGameScene.mapSize.x / curGameScene.mapBounds.size.x,
                curGameScene.mapSize.y / curGameScene.mapBounds.size.z);
        }
    }
}