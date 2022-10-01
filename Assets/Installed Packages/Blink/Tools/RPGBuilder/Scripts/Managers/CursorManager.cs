using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public enum CursorType
    {
        Merchant,
        QuestGiver,
        InteractiveObject,
        CraftingStation,
        EnemyEntity
    }
    
    public class CursorManager : MonoBehaviour
    {
        private void OnEnable()
        {
            UIEvents.SetNewCursor += SetCursor;
            UIEvents.SetCursorToDefault += ResetCursor;
        }

        private void OnDisable()
        {
            UIEvents.SetNewCursor -= SetCursor;
            UIEvents.SetCursorToDefault -= ResetCursor;
        }

        private void SetCursor(CursorType type)
        {
            switch (type)
            {
                case CursorType.Merchant:
                    Cursor.SetCursor(GameDatabase.Instance.GetGeneralSettings().merchantCursor, Vector2.zero,
                        CursorMode.Auto);
                    break;
                case CursorType.QuestGiver:
                    Cursor.SetCursor(GameDatabase.Instance.GetGeneralSettings().questGiverCursor, Vector2.zero,
                        CursorMode.Auto);
                    break;
                case CursorType.InteractiveObject:
                    Cursor.SetCursor(GameDatabase.Instance.GetGeneralSettings().interactiveObjectCursor, Vector2.zero,
                        CursorMode.Auto);
                    break;
                case CursorType.CraftingStation:
                    Cursor.SetCursor(GameDatabase.Instance.GetGeneralSettings().craftingStationCursor, Vector2.zero,
                        CursorMode.Auto);
                    break;
                case CursorType.EnemyEntity:
                    Cursor.SetCursor(GameDatabase.Instance.GetGeneralSettings().enemyCursor, Vector2.zero,
                        CursorMode.Auto);
                    break;
            }
        }

        private void ResetCursor()
        {
            Cursor.SetCursor(GameDatabase.Instance.GetGeneralSettings().defaultCursor, Vector2.zero, CursorMode.Auto);
        }
    }
}