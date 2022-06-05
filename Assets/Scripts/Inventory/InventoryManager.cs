using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Inventory
{

    public class InventoryManager : UI.InGameMenuPanelManager
    {
        class InventorySlot
        {
            public InventoryItemData item;
            public int amount;
        }

        public static UnityEvent<int> itemSelected;
        public static UnityEvent<bool> heroPreviewDragUpdated;

        [Header("UI References")]
        [SerializeField] private RectTransform _canvasRectTransform;
        [SerializeField] private RectTransform _grabbedItemDragIcon;
        [SerializeField] private Transform _itemsGrid;
        [SerializeField] private RectTransform _outlineGlow;
        [SerializeField] private TextMeshProUGUI _itemDetailsText;
        [SerializeField] private TextMeshProUGUI _inventoryWeightText;

        #region Base Variables
        private GameObject _firstItemCell;
        private Player.PlayerData _playerData;

        private Dictionary<int, InventorySlot> _inventory =
            new Dictionary<int, InventorySlot>();
        private int _maxNumberOfSlots;
        private float _inventoryMaxWeight;
        private float _inventoryTotalWeight = 0f;
        private int _selectedItemIndex = -1;
        private int _grabStartIndex = -1;
        private InventorySlot _grabbedItem = null;
        private bool _grabIsReplacing;
        private Vector2 _mousePositionToCanvas;
        #endregion

        #region Variables: Inputs
        private InputAction _toggleItemGrabAction;
        private InputAction _dropItemAction;
        private InputAction _dropItemStackAction;
        private InputAction _sortInventoryAction;
        #endregion

        private bool _draggingHeroPreview;

        [SerializeField] private InventoryItemData _testItem;
        [SerializeField] private InventoryItemData _testItem2;
        [SerializeField] private InventoryItemData _testItem3;
        [SerializeField] private InventoryItemData _testItem4;
        [SerializeField] private InventoryItemData _testItem5;

        void Start()
        {
            Tools.AddressablesLoader.addressablesLoaded.AddListener(
                _OnAddressablesLoaded);
            itemSelected = new UnityEvent<int>();
            itemSelected.AddListener(_OnItemSelected);

            heroPreviewDragUpdated = new UnityEvent<bool>();
            heroPreviewDragUpdated.AddListener(_OnHeroPreviewDragUpdated);

            _firstItemCell = _itemsGrid.GetChild(0).gameObject;
            _maxNumberOfSlots = _itemsGrid.childCount;

            _toggleItemGrabAction = Inputs.InputManager.InputActions.InGameMenu.ToggleItemGrab;
            _dropItemAction = Inputs.InputManager.InputActions.InGameMenu.DropItem;
            _dropItemStackAction = Inputs.InputManager.InputActions.InGameMenu.DropItemStack;
            _sortInventoryAction = Inputs.InputManager.InputActions.InGameMenu.SortInventory;
        }

        private void Update()
        {
            if (!Inputs.InputManager.UsingController && _grabStartIndex != -1)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRectTransform,
                    Mouse.current.position.ReadValue(),
                    null,
                    out _mousePositionToCanvas);
                _grabbedItemDragIcon.anchoredPosition = _mousePositionToCanvas;
            }
        }

        public override void OnEntry()
        {
            _UpdateGridItems();
            EventSystem.current.SetSelectedGameObject(_firstItemCell);
            _firstItemCell.GetComponent<Selectable>().Select();
            _OnItemSelected(0);

            _toggleItemGrabAction.performed += _OnToggleItemGrabAction;
            _toggleItemGrabAction.Enable();

            _dropItemAction.performed += _OnDropItemAction;
            _dropItemAction.Enable();

            _dropItemStackAction.performed += _OnDropItemStackAction;
            _dropItemStackAction.Enable();

            _sortInventoryAction.performed += _OnSortInventoryAction;
            _sortInventoryAction.Enable();
        }

        public override void OnExit()
        {
            _outlineGlow.gameObject.SetActive(false);

            _toggleItemGrabAction.performed -= _OnToggleItemGrabAction;
            _toggleItemGrabAction.Disable();

            _dropItemAction.performed -= _OnDropItemAction;
            _dropItemAction.Disable();

            _dropItemStackAction.performed -= _OnDropItemStackAction;
            _dropItemStackAction.Disable();

            _sortInventoryAction.performed -= _OnSortInventoryAction;
            _sortInventoryAction.Disable();
        }

        #region Event/Input Callbacks
        private void _OnAddressablesLoaded()
        {
            _playerData = Tools.AddressablesLoader.instance.playerData;
            _inventoryMaxWeight = _playerData.inventoryMaxWeight;

            AddItem(_testItem, 10);
            AddItem(_testItem2, 2);
            AddItem(_testItem5);
        }

        private void _OnItemSelected(int slotIndex)
        {
            if (_draggingHeroPreview) return;
            _selectedItemIndex = slotIndex;
            _outlineGlow.transform.SetParent(_itemsGrid.GetChild(slotIndex));
            _outlineGlow.anchoredPosition = Vector2.zero;
            _outlineGlow.gameObject.SetActive(true);
            _UpdateItemDetails(slotIndex);

            if (Inputs.InputManager.UsingController && _grabStartIndex != -1)
            {
                RectTransform rt = _itemsGrid.GetChild(_selectedItemIndex) as RectTransform;
                Vector3[] v = new Vector3[4];
                rt.GetWorldCorners(v);
                _grabbedItemDragIcon.position = v[3];
            }
        }

        private void _OnToggleItemGrabAction(InputAction.CallbackContext obj)
        {
            // grab item
            if (_grabStartIndex == -1)
            {
                // (check slot is not empty)
                if (!_inventory.ContainsKey(_selectedItemIndex)) return;

                _grabStartIndex = _selectedItemIndex;
                _grabbedItem = _inventory[_selectedItemIndex];
                _SetGridItem(_grabbedItem, _grabbedItemDragIcon);
                _itemsGrid
                   .GetChild(_grabStartIndex)
                   .Find("Icon")
                   .GetComponent<Image>()
                   .color = new Color(1f, 1f, 1f, 0.5f);
                if (Inputs.InputManager.UsingController)
                {
                    RectTransform rt = _itemsGrid.GetChild(_selectedItemIndex) as RectTransform;
                    Vector3[] v = new Vector3[4];
                    rt.GetWorldCorners(v);
                    _grabbedItemDragIcon.position = v[3];
                }
                _grabbedItemDragIcon.gameObject.SetActive(true);
                _grabIsReplacing = false;
            }
            // release item
            else
            {
                InventorySlot prevItemInSlot = null;

                bool droppingToSameSlot = _grabStartIndex == _selectedItemIndex;
                if (
                    _inventory.ContainsKey(_selectedItemIndex) &&
                    !droppingToSameSlot
                )
                    prevItemInSlot = _inventory[_selectedItemIndex];

                _inventory[_selectedItemIndex] = _grabbedItem;
                if (!_grabIsReplacing && !droppingToSameSlot)
                {
                    _inventory.Remove(_grabStartIndex);
                    _UnsetGridItem(_grabStartIndex);
                }
                _SetGridItem(_selectedItemIndex);
                _itemsGrid
                   .GetChild(_grabStartIndex)
                   .Find("Icon")
                   .GetComponent<Image>()
                   .color = new Color(1f, 1f, 1f, 1f);

                if (prevItemInSlot != null)
                {
                    _grabbedItem = prevItemInSlot;
                    _grabStartIndex = _selectedItemIndex;
                    _grabIsReplacing = true;
                    _SetGridItem(_grabbedItem, _grabbedItemDragIcon);
                }
                else
                {
                    _grabIsReplacing = false;
                    _grabStartIndex = -1;
                    _grabbedItemDragIcon.gameObject.SetActive(false);
                }
            }
        }

        private void _OnDropItemAction(InputAction.CallbackContext obj)
        {
            int selectedSlotIndex = _outlineGlow.parent.GetSiblingIndex();
            if (!_inventory.ContainsKey(selectedSlotIndex))
                return;

            RemoveItem(selectedSlotIndex);
        }

        private void _OnDropItemStackAction(InputAction.CallbackContext obj)
        {
            int selectedSlotIndex = _outlineGlow.parent.GetSiblingIndex();
            if (!_inventory.ContainsKey(selectedSlotIndex))
                return;

            RemoveItem(selectedSlotIndex, -1);
        }

        private void _OnSortInventoryAction(InputAction.CallbackContext obj)
        {
            List<InventorySlot> sortedSlots = _inventory
                .Values
                .OrderBy((InventorySlot s) => -s.item.price * s.amount)
                .ToList();

            Dictionary<int, InventorySlot> newSlots =
                new Dictionary<int, InventorySlot>();
            for (int i = 0; i < sortedSlots.Count; i++)
                newSlots.Add(i, sortedSlots[i]);

            _inventory = newSlots;
            _UpdateGridItems();
            int selectedSlotIndex = _outlineGlow.parent.GetSiblingIndex();
            _UpdateItemDetails(selectedSlotIndex);
        }

        private void _OnHeroPreviewDragUpdated(bool on)
        {
            _draggingHeroPreview = on;
        }
        #endregion

        #region Logic Methods
        public int AddItem(InventoryItemData item, int amount = 1)
        {
            int excess = 0;

            // fill stack of same item type
            Dictionary<int, InventorySlot> currentSlots =
                new Dictionary<int, InventorySlot>(_inventory);
            foreach (KeyValuePair<int, InventorySlot> p in currentSlots)
            {
                InventorySlot slot = p.Value;

                if (slot.item.code == item.code)
                {
                    if (slot.amount + amount <= slot.item.maxStackSize)
                        slot.amount += amount;
                    else
                    {
                        int consumed = slot.item.maxStackSize - slot.amount;
                        slot.amount = slot.item.maxStackSize;
                        _inventoryTotalWeight += item.weight * consumed;
                        int remaining = amount - consumed;
                        foreach (int stackCount in _DistributeItems(
                            slot.item.maxStackSize,
                            remaining))
                        {
                            excess += _AddInventorySlot(item, stackCount);
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
                    excess += _AddInventorySlot(item, stackCount);
                }
            }

            // check for encumbrance
            _playerData.overburdened = _inventoryTotalWeight > _inventoryMaxWeight;
            _inventoryWeightText.color = _playerData.overburdened
                ? Color.red : Color.white;

            return excess;
        }

        private int _AddInventorySlot(InventoryItemData item, int stackCount)
        {
            if (_inventory.Count == _maxNumberOfSlots)
                return stackCount;

            int idx;
            idx = _GetNextSlotIndex();
            _inventory.Add(idx, new InventorySlot()
            {
                item = item,
                amount = stackCount
            });
            _inventoryTotalWeight += item.weight * stackCount;
            _SetGridItem(idx);
            return 0;
        }

        public void RemoveItem(int slotIndex, int amount = 1)
        {
            bool remove = false;
            foreach (KeyValuePair<int, InventorySlot> p in _inventory)
            {
                if (p.Key == slotIndex)
                {
                    InventorySlot slot = p.Value;
                    if (amount == -1)
                        amount = slot.amount;
                    slot.amount -= amount;
                    if (slot.amount == 0)
                        remove = true;
                    _inventoryTotalWeight -= p.Value.item.weight * amount;
                    break;
                }
            }
            if (remove)
            {
                _inventory.Remove(slotIndex);
                _UnsetGridItem(slotIndex);
            }
            else
            {
                _SetGridItem(slotIndex);
            }
            _UpdateItemDetails(slotIndex);
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
        private void _UpdateGridItems()
        {
            // clean grid
            for (int i = 0; i < _itemsGrid.childCount; i++)
                _UnsetGridItem(i);

            // show icon + amount in each slot
            foreach (KeyValuePair<int, InventorySlot> p in _inventory)
                _SetGridItem(p.Key);

            _inventoryWeightText.text =
                $"{(int)_inventoryTotalWeight}/{_inventoryMaxWeight}";
        }

        private void _SetGridItem(int slotIndex)
        {
            InventorySlot slot = _inventory[slotIndex];
            Transform slotTransform = _itemsGrid.GetChild(slotIndex);
            _SetGridItem(slot, slotTransform);
        }
        private void _SetGridItem(InventorySlot slot, Transform slotTransform)
        {
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
            else
            {
                slotTransform.Find("Amount").gameObject.SetActive(false);
            }
            if (slot.item.rarity != ItemRarity.Common)
            {
                slotTransform.Find("Rarity")
                    .GetComponent<Image>()
                    .color = InventoryItemData.ITEM_RARITY_COLORS[slot.item.rarity];
                slotTransform.Find("Rarity").gameObject.SetActive(true);
            }
            else
            {
                slotTransform.Find("Rarity").gameObject.SetActive(false);
            }

            _inventoryWeightText.text =
                $"{(int)_inventoryTotalWeight}/{_inventoryMaxWeight}";
        }

        private void _UnsetGridItem(int slotIndex)
        {
            Transform slotTransform = _itemsGrid.GetChild(slotIndex);
            slotTransform.Find("Icon").gameObject.SetActive(false);
            slotTransform.Find("Rarity").gameObject.SetActive(false);
            slotTransform.Find("Amount").gameObject.SetActive(false);

            _inventoryWeightText.text =
                $"{(int)_inventoryTotalWeight}/{_inventoryMaxWeight}";
        }

        private void _UpdateItemDetails(int slotIndex)
        {
            if (!_inventory.ContainsKey(slotIndex))
            {
                _itemDetailsText.text = "";
            }
            else
            {
                InventoryItemData item = _inventory[slotIndex].item;
                _itemDetailsText.text = item.GetDetailsDisplay();
            }
        }
        #endregion
    }

}
