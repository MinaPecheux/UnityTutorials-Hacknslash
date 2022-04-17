using UnityEngine;

namespace Inventory
{

    [CreateAssetMenu(
        fileName = "Armor",
        menuName = "Scriptable Objects/Inventory/Armor")]
    public class ArmorItemData : InventoryItemData
    {
        [Header("Armor Data")]
        public float baseDefence;
        public float defenceVariability;

        public ArmorItemData() : base()
        {
            type = ItemType.Armor;
            maxStackSize = 1;
        }

        public (float, float) GetDefenceRange()
            => (baseDefence - defenceVariability, baseDefence + defenceVariability);

        public float GetDefence()
            => baseDefence + Random.Range(-defenceVariability, defenceVariability);

        public override string GetDetailsDisplay()
        {
            (float minDef, float maxDef) = GetDefenceRange();
            string output = base.GetDetailsDisplay();
            output += $"\n\nDefence: {minDef.ToString("0.0")} - {maxDef.ToString("0.0")}";
            return output;
        }
    }

}
