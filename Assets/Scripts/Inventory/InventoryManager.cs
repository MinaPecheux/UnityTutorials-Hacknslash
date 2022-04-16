using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Inventory
{

    public class InventoryManager : MonoBehaviour
    {
        class InventorySlot
        {
            public InventoryItemData item;
            public int amount;
        }

        [Header("UI References")]
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private Transform _itemsGrid;

        private Dictionary<int, InventorySlot> _inventory =
            new Dictionary<int, InventorySlot>();

        [SerializeField] private InventoryItemData _testItem;
        [SerializeField] private InventoryItemData _testItem2;
        [SerializeField] private InventoryItemData _testItem3;
        [SerializeField] private InventoryItemData _testItem4;

        void Start()
        {
            AddItem(_testItem, 10);
            AddItem(_testItem2, 2);
            AddItem(_testItem3);
            AddItem(_testItem4);
        }

        private void Update()
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
                ToggleInventoryPanel();
        }

        #region Logic Methods
        public void AddItem(InventoryItemData item, int amount = 1)
        {
            int idx;

            // fill stack of same item type
            Dictionary<int, InventorySlot> currentSlots =
                new Dictionary<int, InventorySlot>(_inventory);
            foreach (KeyValuePair<int, InventorySlot> p in currentSlots)
            {
                InventorySlot slot = p.Value;

                if (slot.amount == slot.item.maxStackSize)
                    continue;
                if (slot.item.code == item.code)
                {
                    if (slot.amount + amount <= slot.item.maxStackSize)
                        slot.amount += amount;
                    else
                    {
                        int consumed = slot.item.maxStackSize - slot.amount;
                        slot.amount = slot.item.maxStackSize;
                        int remaining = amount - consumed;
                        foreach (int stackCount in _DistributeItems(
                            slot.item.maxStackSize,
                            remaining))
                        {
                            idx = _GetNextSlotIndex();
                            _inventory.Add(idx, new InventorySlot()
                            {
                                item = item,
                                amount = stackCount
                            });
                            _SetGridItem(idx);
                        }
                    }
                    amount = 0;
                }
            }

            // create new slot(s)
            if (amount > 0)
            {
                foreach (int stackCount in _DistributeItems(
                                item.maxStackSize,
                                amount))
                {
                    _inventory.Add(_GetNextSlotIndex(), new InventorySlot()
                    {
                        item = item,
                        amount = stackCount
                    });
                }
            }
        }

        private int _GetNextSlotIndex()
        {
            if (_inventory.Count == 0) return 0;
            List<int> occupiedIndices = new List<int>(_inventory.Keys);
            occupiedIndices.Sort();
            for (int i = 1; i < occupiedIndices.Count; i++)
            {
                if (occupiedIndices[i] - occupiedIndices[i - 1] > 1)
                {
                    return occupiedIndices[i - 1] + 1;
                }
            }
            return occupiedIndices.Count;
        }

        private List<int> _DistributeItems(int maxStackSize, int amount)
        {
            List<int> stackCounts = new List<int>();

            int nStacks = amount / maxStackSize;
            for (int i = 0; i < nStacks; i++)
                stackCounts.Add(maxStackSize);

            int remaining = amount % maxStackSize;
            if (remaining > 0)
                stackCounts.Add(remaining);

            return stackCounts;
        }
        #endregion

        #region UI Methods
        public void ToggleInventoryPanel()
        {
            bool on = !_inventoryPanel.activeSelf;
            if (on) _UpdateGridItems();
            _inventoryPanel.SetActive(on);
        }

        private void _UpdateGridItems()
        {
            // clean grid
            for (int i = 0; i < _itemsGrid.childCount; i++)
                _UnsetGridItem(i);

            // show icon + amount in each slot
            foreach (KeyValuePair<int, InventorySlot> p in _inventory)
                _SetGridItem(p.Key);
        }

        private void _SetGridItem(int slotIndex)
        {
            InventorySlot slot = _inventory[slotIndex];
            Transform slotTransform = _itemsGrid.GetChild(slotIndex);
            slotTransform.Find("Icon")
                .GetComponent<Image>()
                .sprite = slot.item.icon;
            slotTransform.Find("Icon").gameObject.SetActive(true);
            if (slot.amount > 1)
            {
                slotTransform.Find("Amount")
                    .GetComponent<TextMeshProUGUI>()
                    .text = slot.amount.ToString();
                slotTransform.Find("Amount").gameObject.SetActive(true);
            }
            if (slot.item.rarity != ItemRarity.Common)
            {
                slotTransform.Find("Rarity")
                    .GetComponent<Image>()
                    .color = InventoryItemData.ITEM_RARITY_COLORS[slot.item.rarity];
                slotTransform.Find("Rarity").gameObject.SetActive(true);
            }
        }

        private void _UnsetGridItem(int slotIndex)
        {
            Transform slotTransform = _itemsGrid.GetChild(slotIndex);
            slotTransform.Find("Icon").gameObject.SetActive(false);
            slotTransform.Find("Rarity").gameObject.SetActive(false);
            slotTransform.Find("Amount").gameObject.SetActive(false);
        }
        #endregion
    }

}
