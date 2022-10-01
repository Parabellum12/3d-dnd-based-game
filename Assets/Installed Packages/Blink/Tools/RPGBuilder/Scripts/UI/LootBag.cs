using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;

namespace BLINK.RPGBuilder.UIElements
{
    public class LootBag : MonoBehaviour, IPlayerInteractable
    {
        [Serializable]
        public class Loot_Data
        {
            public RPGItem item;
            public int count;
            public bool looted;
            public int itemDataID = -1;
        }

        public List<Loot_Data> lootData = new List<Loot_Data>();
        public string lootBagName;

        public void CheckLootState()
        {
            var nonLootedItem = lootData.Count(t => !t.looted);
            if (nonLootedItem == 0)
            {
                UIEvents.Instance.OnClosePanel("Loot");
                Destroy(gameObject);
            }
            else
            {
                if (UIEvents.Instance.IsPanelOpen("Loot"))
                {
                    GameEvents.Instance.OnDisplayLootBag(this);
                }
            }
        }


        private void OnMouseOver()
        {
            if (!Input.GetMouseButtonUp(1)) return;
            if (Vector3.Distance(transform.position, GameState.playerEntity.transform.position) < 4)
            {
                GameEvents.Instance.OnDisplayLootBag(this);
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
        }
        
        public void Interact()
        {
            if (UIEvents.Instance.CursorHoverUI) return;
            if (!(Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <= 3)) return;
            GameEvents.Instance.OnDisplayLootBag(this);
        }

        public void ShowInteractableUI()
        {
            var pos = transform;
            Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + 1.5f, pos.position.z);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            WorldInteractableDisplayManager.Instance.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            
            WorldInteractableDisplayManager.Instance.Show(this);
        }

        public string getInteractableName()
        {
            return lootBagName;
        }

        public bool isReadyToInteract()
        {
            return true;
        }

        public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            return RPGCombatDATA.INTERACTABLE_TYPE.LootBag;
        }
    }
}