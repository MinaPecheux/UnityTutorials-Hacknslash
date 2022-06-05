using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{

    public class LootBagManager : MonoBehaviour
    {
        private GameObject _lootText;

        [HideInInspector] public List<(InventoryItemData, int)> contents;

        private void Awake()
        {
            Transform camTransform = Camera.main.transform;
            _lootText = transform.Find("LootText").gameObject;
            _lootText.transform.rotation = Quaternion.LookRotation(
                camTransform.forward,
                camTransform.up);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                InventoryManager.lootBagSighted.Invoke(transform);
                _lootText.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                InventoryManager.lootBagForgotten.Invoke(transform);
                _lootText.SetActive(false);
            }
        }
    }

}