using UnityEngine;

namespace Inventory
{

    [CreateAssetMenu(
        fileName = "Component",
        menuName = "Scriptable Objects/Inventory/Component")]
    public class ComponentItemData : InventoryItemData
    {
        public ComponentItemData() : base()
        {
            type = ItemType.Component;
        }
    }

}
