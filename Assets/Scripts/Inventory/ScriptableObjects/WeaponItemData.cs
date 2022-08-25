using UnityEngine;

namespace Inventory
{

    [CreateAssetMenu(
        fileName = "Weapon",
        menuName = "Scriptable Objects/Inventory/Weapon")]
    public class WeaponItemData : InventoryItemData
    {
        [Header("Weapon Data")]
        public float baseDamage;
        public float damageVariability;
        public float criticalStrikeChance;
        public float range;

        public WeaponItemData() : base()
        {
            type = ItemType.Weapon;
            maxStackSize = 1;
            equipmentSlot = EquipmentSlot.RightHand;
        }

        public (float, float) GetDamageRange()
            => (baseDamage - damageVariability, baseDamage + damageVariability);

        public float GetDamage()
        {
            float dmg =
                baseDamage +
                Random.Range(-damageVariability, damageVariability);
            return (Random.Range(0f, 1f) < criticalStrikeChance)
                ? dmg : dmg * 2;
        }

        public override string GetDetailsDisplay()
        {
            (float minDmg, float maxDmg) = GetDamageRange();
            string output = base.GetDetailsDisplay();
            output += $"\n\nDamage: {minDmg.ToString("0.0")} - {maxDmg.ToString("0.0")}";
            output += $"\nCritical Strike Chance: {(criticalStrikeChance * 100f).ToString("0.0")}%\n";
            return output;
        }
    }

}
