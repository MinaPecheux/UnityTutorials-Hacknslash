using UnityEngine;

namespace Player
{

    public class PlayerManager : MonoBehaviour
    {
        private Tools.ItemCache _equipmentCache;

        [SerializeField] private PlayerController _playerController;

        [Header("Equipment")]
        [SerializeField] private Transform _equipAnchorHead;
        [SerializeField] private Transform _equipAnchorLeftHand;
        [SerializeField] private Transform _equipAnchorRightHand;

        [SerializeField] private Skills.SkillData[] _skills;

        private void Awake()
        {
            _equipmentCache = new Tools.ItemCache();
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 200, 100), "Power Strike"))
            {
                _skills[0].Cast();
            }
        }

        public void Equip(Inventory.InventoryItemData item)
        {
            if (
                item.type != Inventory.ItemType.Weapon &&
                item.type != Inventory.ItemType.Armor
            )
            {
                Debug.LogWarning($"Trying to equip an invalid item: '{item.code}'");
                return;
            }

            Transform anchor = null;
            bool addWeapon = false;
            if (item.equipmentSlot == Inventory.EquipmentSlot.Helmet)
                anchor = _equipAnchorHead;
            else if (item.equipmentSlot == Inventory.EquipmentSlot.LeftHand)
            {
                anchor = _equipAnchorLeftHand;
                addWeapon = true;
            }
            else if (item.equipmentSlot == Inventory.EquipmentSlot.RightHand)
            {
                anchor = _equipAnchorRightHand;
                addWeapon = true;
            }
            _equipmentCache.Add(item.code, item.prefab, anchor);

            if (addWeapon)
                _playerController.SetAnimatorContext(AnimatorContext.OneHanded);
        }

        public void Unequip(Inventory.InventoryItemData item)
        {
            if (
                item.type != Inventory.ItemType.Weapon &&
                item.type != Inventory.ItemType.Armor
            )
            {
                Debug.LogWarning($"Trying to equip an invalid item: '{item.code}'");
                return;
            }

            _equipmentCache.Remove(item.code);

            bool removeWeapon =
                item.equipmentSlot == Inventory.EquipmentSlot.LeftHand ||
                item.equipmentSlot == Inventory.EquipmentSlot.RightHand;
            if (removeWeapon)
                _playerController.SetAnimatorContext(AnimatorContext.Base);
        }

    }

}
