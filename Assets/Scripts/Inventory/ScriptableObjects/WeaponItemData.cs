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

        public WeaponItemData() : base()
        {
            type = ItemType.Weapon;
            maxStackSize = 1;
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
    }

}
