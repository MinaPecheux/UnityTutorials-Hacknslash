using System.Collections.Generic;
using UnityEngine;

using Tools;

namespace Inventory
{

    public enum ItemType
    {
        Weapon,
        Armor,
        Component,
        Misc
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum EquipmentSlot
    {
        // for armors
        //   (left column)
        Helmet,
        Shoulders,
        Chest,
        Legs,
        Boots,
        //   (right column)
        Necklace,
        Belt,
        Gloves,
        Ring1,
        Ring2,
        // for weapons
        LeftHand,
        RightHand,
        // for "unequippables"
        None,
    }

    [CreateAssetMenu(
        fileName = "Item",
        menuName = "Scriptable Objects/Inventory/Item")]
    public abstract class InventoryItemData : ScriptableObject
    {
        public static Dictionary<ItemRarity, Color> ITEM_RARITY_COLORS
            = new Dictionary<ItemRarity, Color>()
            {
                { ItemRarity.Common, Color.white },
                { ItemRarity.Uncommon, new Color(0.1f, 1f, 0.1f, 1f) },
                { ItemRarity.Rare, new Color(0.1f, 0.1f, 1f, 1f) },
                { ItemRarity.Epic, new Color(0.6f, 0.1f, 0.6f, 1f) },
                { ItemRarity.Legendary, new Color(0.6f, 0.6f, 0.1f, 1f) },
            };

        public string code;
        public string itemName;
        public Sprite icon;
        public ItemRarity rarity;
        public ItemType type;
        public float weight;
        public int price;
        public int maxStackSize = 20;
        public EquipmentSlot equipmentSlot = EquipmentSlot.None;

        public virtual string GetDetailsDisplay()
        {
            Color c = Tools.Graphics.BrightenColor(ITEM_RARITY_COLORS[rarity]);
            string color = $"{((Color32) c).ToValue():X}";
            string output = $"<color=#{color}>";
            output += $"<size=34>{itemName}</size>";
            output += "</color>\n";
            output += $"<size=24>{type}\n</size>";
            output += $"<size=24><sprite=0> {price}    <sprite=1> {weight.ToString("0.0")}</size>";
            return output;
        }

    }

}
