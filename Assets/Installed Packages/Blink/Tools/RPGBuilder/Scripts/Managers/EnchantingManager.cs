using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class EnchantingManager : MonoBehaviour
    {
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static EnchantingManager Instance { get; private set; }

        public void EnchantItem(int itemDataID, int curTier, RPGEnchantment enchantment, RPGItem itemUsed, bool consumeItem)
        {
            CharacterEntries.ItemEntry itemEntry = RPGBuilderUtilities.GetItemDataFromDataID(itemDataID);
            if (itemEntry == null) return;
            int upcomingTier = -1;
            if (itemEntry.enchantmentID == enchantment.ID)
            {
                upcomingTier = curTier + 1;
            }
            else
            {
                upcomingTier = 0;
            }
            
            // if enchant is consumed, check if we still own at leadst 1 of its item first
            if (consumeItem)
            {
                if (RPGBuilderUtilities.getItemCount(itemUsed) > 0)
                {
                    InventoryManager.Instance.RemoveItem(itemUsed.ID, -1, 1, -1, -1, false);

                }
                else
                {
                    GeneralEvents.Instance.OnStopCurrentEnchant();
                    UIEvents.Instance.OnShowAlertMessage("The enchantment item is not owned anymore", 3);
                    return;
                }
            }

            if (enchantment.enchantmentTiers[upcomingTier].currencyCosts.Any(t =>
                !EconomyUtilities.HasEnoughCurrency(t.currencyID, t.amount)))
            {
                GeneralEvents.Instance.OnStopCurrentEnchant();
                UIEvents.Instance.OnShowAlertMessage("Not enough currency", 3);
                return;
            }

            foreach (var itemCost in enchantment.enchantmentTiers[upcomingTier].itemCosts)
            {
                int totalOfThisComponent = 0;
                foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
                {
                    if(slot.itemID == -1 || slot.itemID != itemCost.itemID) continue;
                    totalOfThisComponent += slot.itemStack;
                }

                if (totalOfThisComponent >= itemCost.itemCount) continue;
                GeneralEvents.Instance.OnStopCurrentEnchant();
                UIEvents.Instance.OnShowAlertMessage("Items required are not in bags", 3);
                return;
            }

            foreach (var t in enchantment.enchantmentTiers[upcomingTier].itemCosts)
                InventoryManager.Instance.RemoveItem(t.itemID, -1, t.itemCount, -1, -1, false);
            foreach (var t in enchantment.enchantmentTiers[upcomingTier].currencyCosts)
                EconomyUtilities.RemoveCurrency(t.currencyID, t.amount);

            var success = Random.Range(0f, 100f);
            if (!(success <= enchantment.enchantmentTiers[upcomingTier].successRate))
            {
                GeneralEvents.Instance.OnStopCurrentEnchant();
                UIEvents.Instance.OnShowAlertMessage("The enchantment failed", 3);
                return;
            }

            if (curTier == -1)
            {
                curTier = 0;
            }

            if (enchantment.enchantmentTiers[curTier].skillID != -1)
            {
                LevelingManager.Instance.AddSkillEXP(enchantment.enchantmentTiers[curTier].skillID,
                    enchantment.enchantmentTiers[curTier].skillXPAmount);
            }

            int itemDataIndex = RPGBuilderUtilities.GetItemDataIndexFromDataID(itemDataID);
            if (itemDataIndex != -1)
            {
                Character.Instance.CharacterData.ItemEntries[itemDataIndex].enchantmentID = enchantment.ID;
                Character.Instance.CharacterData.ItemEntries[itemDataIndex].enchantmentTierIndex = upcomingTier;
            }

            UIEvents.Instance.OnUpdateEnchantingPanel();

        }

        public void ApplyEnchantParticle(GameObject meshManager, GameObject target)
        {
            if (target == null) return;
            GameObject meshManagerGO = Instantiate(meshManager, target.transform);
            meshManagerGO.transform.position = Vector3.zero;
            meshManagerGO.transform.localPosition = Vector3.zero;

            MeshParticleManager meshManagerRef = meshManagerGO.GetComponent<MeshParticleManager>();
            if (meshManagerRef != null)
            {
                meshManagerRef.Init(target);
            }
        }
    }
}
