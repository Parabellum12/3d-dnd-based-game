using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.World
{
    public class CraftingStation : MonoBehaviour, IPlayerInteractable
    {
        public RPGCraftingStation station;
        public float useDistanceMax;
        public float interactableUIoffsetY = 2;
        
        private void OnMouseOver()
        {
            if (UIEvents.Instance.CursorHoverUI)
            {
                UIEvents.Instance.OnSetCursorToDefault();
                return;
            }
            if (Input.GetMouseButtonUp(1))
                if (Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <=
                    useDistanceMax)
                {
                    if (!UIEvents.Instance.IsPanelOpen("Crafting"))
                        InitCraftingStation();
                }
                else
                {
                    if (GameState.playerEntity.controllerEssentials.GETControllerType() ==
                        RPGBuilderGeneralSettings.ControllerTypes.TopDownClickToMove)
                    {

                    }
                    else
                    {
                        UIEvents.Instance.OnShowAlertMessage("This is too far", 3);
                    }
                }

            UIEvents.Instance.OnSetNewCursor(CursorType.CraftingStation);
        }

        private void OnMouseExit()
        {
            UIEvents.Instance.OnSetCursorToDefault();
        }

        private void InitCraftingStation()
        {
            GeneralEvents.Instance.OnInitCraftingStation(this);
        }

        public void Interact()
        {
            if (UIEvents.Instance.CursorHoverUI) return;
            if (!(Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <= useDistanceMax)) return;
            if (!UIEvents.Instance.IsPanelOpen("Crafting")) InitCraftingStation();
        }

        public void ShowInteractableUI()
        {
            var pos = transform;
            Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + interactableUIoffsetY, pos.position.z);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            WorldInteractableDisplayManager.Instance.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            
            WorldInteractableDisplayManager.Instance.Show(this);
        }

        public string getInteractableName()
        {
            return station.entryDisplayName;
        }

        public bool isReadyToInteract()
        {
            return true;
        }

        public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            return RPGCombatDATA.INTERACTABLE_TYPE.CraftingStation;
        }
    }
}