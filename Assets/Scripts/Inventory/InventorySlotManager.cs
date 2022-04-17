using UnityEngine;
using UnityEngine.EventSystems;

namespace Inventory
{
    public class InventorySlotManager : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        private int _slotIndex;

        private void Awake()
        {
            _slotIndex = transform.GetSiblingIndex();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!Inputs.InputManager.UsingController)
                InventoryManager.itemSelected.Invoke(_slotIndex);
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (Inputs.InputManager.UsingController)
                InventoryManager.itemSelected.Invoke(_slotIndex);
        }
    }

}
