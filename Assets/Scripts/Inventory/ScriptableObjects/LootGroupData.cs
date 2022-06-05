using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory
{

    [System.Serializable]
    public class LootGroup
    {
        public InventoryItemData item;
        public int weight = 1;
        public int minAmount;
        public int maxAmount;
    }

    [CreateAssetMenu(
        fileName = "Group",
        menuName = "Scriptable Objects/Inventory/Loot Group")]
    public class LootGroupData : ScriptableObject
    {
        public LootGroup[] groups;
        public int minItemsAmount;
        public int maxItemsAmount;

        public List<(InventoryItemData, int)> PickRandomItems()
        {
            List<(InventoryItemData, int)> items =
                new List<(InventoryItemData, int)>();

            // (do random weighted pick)
            List<LootGroup> remainingGroups = new List<LootGroup>(groups);
            System.Random random = new System.Random();
            int itemsAmount = Random.Range(minItemsAmount, maxItemsAmount + 1);
            for (int i = 0; i < itemsAmount; i++)
            {
                int totalWeight = remainingGroups.Sum(g => g.weight);
                int rnd = random.Next(totalWeight);

                LootGroup group = remainingGroups.First(g => (rnd -= g.weight) < 0);
                items.Add((
                    group.item,
                    Random.Range(group.minAmount, group.maxAmount + 1)));
                remainingGroups.Remove(group);
            }

            return items;
        }
    }

}