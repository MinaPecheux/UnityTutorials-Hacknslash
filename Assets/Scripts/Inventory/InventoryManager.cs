using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
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

        public static UnityEvent<(int, Transform)> itemSelected;
        public static UnityEvent<bool> heroPreviewDragUpdated;
        public static UnityEvent<Transform> lootBagSighted;
        public static UnityEvent<Transform> lootBagForgotten;

        public static bool inLootPanel = false;

        [Header("UI References")]
        [SerializeField] private RectTransform _canvasRectTransform;
        [SerializeField] private RectTransform _grabbedItemDragIcon;
        [SerializeField] private Transform _itemsGrid;
        [SerializeField] private GameObject _lootPanel;
        [SerializeField] private Transform _lootSpecialItemsGrid;
        [SerializeField] private Transform _lootCommonItemsGrid;
        [SerializeField] private RectTransform _outlineGlow;
        [SerializeField] private TextMeshProUGUI _itemDetailsText;
        [SerializeField] private TextMeshProUGUI _inventoryWeightText;

        #region Base Variables
        private GameObject _firstItemCell;
        private Player.PlayerData _playerData;

        private Dictionary<int, InventorySlot> _inventory =
            new Dictionary<int, InventorySlot>();
        private Dictionary<int, InventorySlot> _lootSpecial =
            new Dictionary<int, InventorySlot>();
        private Dictionary<int, InventorySlot> _lootCommon =
            new Dictionary<int, InventorySlot>();
        private int _maxNumberOfSlots;
        private float _inventoryMaxWeight;
        private float _inventoryTotalWeight = 0f;
        private int _selectedItemIndex = -1;
        private Transform _selectedItemParent = null;
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
        private InputAction _lootAction;
        private InputAction _lootSingleItemAction;
        private InputAction _closeLootAction;
        #endregion

        private bool _draggingHeroPreview;
        private LootBagManager _closestLootBag = null;
        private List<Transform> _lootBagsInSight = new List<Transform>();

        void Start()
        {
            Tools.AddressablesLoader.addressablesLoaded.AddListener(
                _OnAddressablesLoaded);
            itemSelected = new UnityEvent<(int, Transform)>();
            itemSelected.AddListener(_OnItemSelected);

            heroPreviewDragUpdated = new UnityEvent<bool>();
            heroPreviewDragUpdated.AddListener(_OnHeroPreviewDragUpdated);

            lootBagSighted = new UnityEvent<Transform>();
            lootBagSighted.AddListener(_OnLootBagSighted);
            lootBagForgotten = new UnityEvent<Transform>();
            lootBagForgotten.AddListener(_OnLootBagForgotten);

            _firstItemCell = _itemsGrid.GetChild(0).gameObject;
            _maxNumberOfSlots = _itemsGrid.childCount;

            _toggleItemGrabAction = Inputs.InputManager.InputActions.InGameMenu.ToggleItemGrab;
            _dropItemAction = Inputs.InputManager.InputActions.InGameMenu.DropItem;
            _dropItemStackAction = Inputs.InputManager.InputActions.InGameMenu.DropItemStack;
            _sortInventoryAction = Inputs.InputManager.InputActions.InGameMenu.SortInventory;

            _lootAction = Inputs.InputManager.InputActions.Player.Loot;
            _lootAction.performed += _OnLootAction;
            _lootAction.Enable();

            _lootSingleItemAction = Inputs.InputManager.InputActions.Player.LootSingleItem;
            _lootSingleItemAction.performed += _OnLootSingleItemAction;
            _lootSingleItemAction.Enable();

            _closeLootAction = Inputs.InputManager.InputActions.Player.CloseLoot;
            _closeLootAction.performed += _OnCloseLootAction;
            _closeLootAction.Enable();
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

        private void OnDisable()
        {
            _lootAction.performed -= _OnLootAction;
            _lootAction.Disable();

            _lootSingleItemAction.performed -= _OnLootSingleItemAction;
            _lootSingleItemAction.Disable();

            _closeLootAction.performed -= _OnCloseLootAction;
            _closeLootAction.Disable();
        }

        public override void OnEntry()
        {
            _UpdateGridItems();
            EventSystem.current.SetSelectedGameObject(_firstItemCell);
            _firstItemCell.GetComponent<Selectable>().Select();
            _OnItemSelected((0, _itemsGrid));

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
        }

        private void _OnItemSelected((int, Transform) data)
        {
            (int slotIndex, Transform parent) = data;

            if (_draggingHeroPreview) return;
            _selectedItemIndex = slotIndex;
            _selectedItemParent = parent;
            _outlineGlow.transform.SetParent(parent.GetChild(slotIndex));
            _outlineGlow.anchoredPosition = Vector2.zero;
            _outlineGlow.gameObject.SetActive(true);
            _UpdateItemDetails(slotIndex);

            if (Inputs.InputManager.UsingController && _grabStartIndex != -1)
            {
                RectTransform rt = parent.GetChild(_selectedItemIndex) as RectTransform;
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
            if (Keyboard.current.shiftKey.isPressed) return;
            _DropItem(_selectedItemIndex, 1);
        }

        private void _OnDropItemStackAction(InputAction.CallbackContext obj)
        {
            _DropItem(_selectedItemIndex, -1);
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

        private void _OnLootAction(InputAction.CallbackContext obj)
        {
            // find closest loot bag
            Vector3 p = GameObject.FindGameObjectWithTag("Player").transform.position;
            _closestLootBag = _lootBagsInSight
                .OrderBy((Transform t) => (p - t.position).sqrMagnitude)
                .First()
                .GetComponent<LootBagManager>();

            _SetLoot(_closestLootBag.contents);
            _lootPanel.SetActive(true);
            inLootPanel = true;
        }

        private void _OnLootSingleItemAction(InputAction.CallbackContext obj)
        {
            if (!inLootPanel)
                return;

            // (check if inventory is full)
            if (_inventory.Count == _maxNumberOfSlots)
                return;

            Dictionary<int, InventorySlot> data =
                _selectedItemParent == _lootCommonItemsGrid
                    ? _lootCommon : _lootSpecial;
            // (check if slot is not empty)
            if (!data.ContainsKey(_selectedItemIndex))
                return;
            InventorySlot slot = data[_selectedItemIndex];
            int excess = AddItem(slot.item, slot.amount);
            if (excess <= 0)
            {
                _UnsetGridItem(_selectedItemIndex, _selectedItemParent);
                data.Remove(_selectedItemIndex);
            }
            else
            {
                data[_selectedItemIndex].amount = excess;
                _SetGridItem(
                    _selectedItemIndex,
                    true,
                    slot.item.rarity != ItemRarity.Common);
            }

            if (_lootCommon.Count == 0 && _lootSpecial.Count == 0)
            {
                if (_lootBagsInSight.Contains(_closestLootBag.transform))
                    _lootBagsInSight.Remove(_closestLootBag.transform);
                Destroy(_closestLootBag.gameObject);
                _lootPanel.SetActive(false);
            }
            else
            {
                _closestLootBag.contents =
                    _lootSpecial.Select((KeyValuePair<int, InventorySlot> p)
                        => (p.Value.item, p.Value.amount))
                    .Concat(
                        _lootCommon.Select((KeyValuePair<int, InventorySlot> p)
                            => (p.Value.item, p.Value.amount)))
                    .ToList();
            }
        }

        private void _OnCloseLootAction(InputAction.CallbackContext obj)
        {
            _lootPanel.SetActive(false);
            inLootPanel = false;
        }

        private void _OnHeroPreviewDragUpdated(bool on)
        {
            _draggingHeroPreview = on;
        }

        private void _OnLootBagSighted(Transform bag)
        {
            if (!_lootBagsInSight.Contains(bag))
                _lootBagsInSight.Add(bag);
        }

        private void _OnLootBagForgotten(Transform bag)
        {
            if (_lootBagsInSight.Contains(bag))
                _lootBagsInSight.Remove(bag);
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

        private void _DropItem(int slotIndex, int amount = 1)
        {
            if (!_inventory.ContainsKey(slotIndex))
                return;

            // drop the item as a loot bag
            Transform p = GameObject.FindGameObjectWithTag("Player").transform;
            Vector3 pos = p.position - p.GetChild(0).forward; // (just in front of the player)
            Tools.AddressablesLoader.instance.lootBagPrefab.InstantiateAsync(
                pos, Quaternion.identity).Completed +=
                (AsyncOperationHandle<GameObject> obj) =>
                {
                    GameObject g = obj.Result;
                    InventorySlot s = _inventory[slotIndex];
                    List<(InventoryItemData, int)> contents =
                        new List<(InventoryItemData, int)>()
                    {
                        (s.item, amount == -1 ? s.amount : amount),
                    };
                    g.GetComponent<LootBagManager>().contents = contents;

                    // actually remove it from the inventory
                    RemoveItem(slotIndex, amount);
                };
        }

        public void TakeAllLootItems()
        {
            int excess;
            bool inventoryIsFull = _inventory.Count == _maxNumberOfSlots;
            Dictionary<int, InventorySlot> l =
                new Dictionary<int, InventorySlot>(_lootCommon);
            foreach (KeyValuePair<int, InventorySlot> pair in l)
            {
                if (inventoryIsFull) break;
                excess = AddItem(pair.Value.item, pair.Value.amount);
                inventoryIsFull = excess > 0;
                if (!inventoryIsFull)
                {
                    _UnsetGridItem(pair.Key, _lootCommonItemsGrid);
                    _lootCommon.Remove(pair.Key);
                }
                else
                {
                    _lootCommon[pair.Key].amount = excess;
                    _SetGridItem(pair.Key, true, false);
                }
            }
            TakeSpecialLootItems();

            if (_lootCommon.Count == 0 && _lootSpecial.Count == 0)
            {
                if (_lootBagsInSight.Contains(_closestLootBag.transform))
                    _lootBagsInSight.Remove(_closestLootBag.transform);
                Destroy(_closestLootBag.gameObject);
                _lootPanel.SetActive(false);
            }
            else
            {
                _closestLootBag.contents =
                    _lootSpecial.Select((KeyValuePair<int, InventorySlot> p)
                        => (p.Value.item, p.Value.amount))
                    .Concat(
                        _lootCommon.Select((KeyValuePair<int, InventorySlot> p)
                            => (p.Value.item, p.Value.amount)))
                    .ToList();
            }
        }

        public void TakeSpecialLootItems()
        {
            int excess;
            bool inventoryIsFull = _inventory.Count == _maxNumberOfSlots;
            Dictionary<int, InventorySlot> l =
                new Dictionary<int, InventorySlot>(_lootSpecial);
            foreach (KeyValuePair<int, InventorySlot> pair in l)
            {
                if (inventoryIsFull) break;
                excess = AddItem(pair.Value.item, pair.Value.amount);
                inventoryIsFull = excess > 0;
                if (!inventoryIsFull)
                {
                    _UnsetGridItem(pair.Key, _lootSpecialItemsGrid);
                    _lootSpecial.Remove(pair.Key);
                }
                else
                {
                    _lootSpecial[pair.Key].amount = excess;
                    _SetGridItem(pair.Key, true, true);
                }
            }

            if (_lootCommon.Count == 0 && _lootSpecial.Count == 0)
            {
                if (_lootBagsInSight.Contains(_closestLootBag.transform))
                    _lootBagsInSight.Remove(_closestLootBag.transform);
                Destroy(_closestLootBag.gameObject);
                _lootPanel.SetActive(false);
            }
            else
            {
                _closestLootBag.contents =
                    _lootSpecial.Select((KeyValuePair<int, InventorySlot> p)
                        => (p.Value.item, p.Value.amount))
                    .Concat(
                        _lootCommon.Select((KeyValuePair<int, InventorySlot> p)
                            => (p.Value.item, p.Value.amount)))
                    .ToList();
            }
        }

        private void _SetLoot(List<(InventoryItemData, int)> contents)
        {
            // clean loot grids
            for (int i = 0; i < _lootSpecialItemsGrid.childCount; i++)
                _UnsetGridItem(i, _lootSpecialItemsGrid);
            for (int i = 0; i < _lootCommonItemsGrid.childCount; i++)
                _UnsetGridItem(i, _lootCommonItemsGrid);

            _lootSpecial = new Dictionary<int, InventorySlot>();
            _lootCommon = new Dictionary<int, InventorySlot>();

            // compute new loot grids
            int idx; bool itemIsSpecial;
            foreach ((InventoryItemData item, int amount) in contents)
            {
                foreach (int stackCount in _DistributeItems(
                    item.maxStackSize, amount))
                {
                    itemIsSpecial = item.rarity != ItemRarity.Common;
                    idx = _GetNextSlotIndex(
                        isLoot: true,
                        isSpecial: itemIsSpecial);
                    (itemIsSpecial ? _lootSpecial : _lootCommon)
                        .Add(idx, new InventorySlot()
                    {
                        item = item,
                        amount = stackCount
                    });
                    _SetGridItem(idx, isLoot: true, isSpecial: itemIsSpecial);
                }
            }
        }

        private int _GetNextSlotIndex(bool isLoot = false, bool isSpecial = false)
        {
            Dictionary<int, InventorySlot> data = isLoot
                ? (isSpecial ? _lootSpecial : _lootCommon) : _inventory;
            if (data.Count == 0) return 0;
            List<int> occupiedIndices = new List<int>(data.Keys);
            occupiedIndices.Sort();
            if (!occupiedIndices.Contains(0)) return 0;
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

        private void _SetGridItem(int slotIndex, bool isLoot = false, bool isSpecial = false)
        {
            InventorySlot slot = isLoot
                ? (isSpecial ? _lootSpecial[slotIndex] : _lootCommon[slotIndex])
                : _inventory[slotIndex];

            Transform slotTransform = isLoot
                ? (isSpecial
                    ? _lootSpecialItemsGrid.GetChild(slotIndex)
                    : _lootCommonItemsGrid.GetChild(slotIndex))
                : _itemsGrid.GetChild(slotIndex);
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
            _UnsetGridItem(slotIndex, _itemsGrid);
        }
        private void _UnsetGridItem(int slotIndex, Transform grid)
        {
            Transform slotTransform = grid.GetChild(slotIndex);
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
