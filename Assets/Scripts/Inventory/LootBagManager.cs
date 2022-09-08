using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Inventory
{

    public class LootBagManager : MonoBehaviour
    {
        private GameObject _lootDisplay;

        [HideInInspector] public List<(InventoryItemData, int)> contents;

        private void Awake()
        {
            Transform camTransform = Camera.main.transform;
            _lootDisplay = transform.Find("LootDisplay").gameObject;
            _lootDisplay.transform.rotation = Quaternion.LookRotation(
                camTransform.forward,
                camTransform.up);

            _SetInputDisplay();
        }

        private void OnEnable()
        {
            Inputs.InputManager.deviceChanged.AddListener(_OnDeviceChanged);
        }

        private void OnDisable()
        {
            Inputs.InputManager.deviceChanged.RemoveListener(_OnDeviceChanged);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                InventoryManager.lootBagSighted.Invoke(transform);
                _lootDisplay.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                InventoryManager.lootBagForgotten.Invoke(transform);
                _lootDisplay.SetActive(false);
            }
        }

        private void _OnDeviceChanged()
        {
            _SetInputDisplay();
        }

        private void _SetInputDisplay()
        {
            Inputs.InputManager.InputDeviceType idt =
                Inputs.InputManager.currentInputDeviceType;
            string control = Inputs.InputManager.ActionToControl("Transitions:Loot");
            Addressables.LoadAssetAsync<Sprite>($"Assets/InputIcons/{idt}/{control}.png")
                .Completed += (AsyncOperationHandle<Sprite> obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (obj.Result != null)
                        {
                            transform.Find("LootDisplay/InputDisplay")
                                .GetComponent<SpriteRenderer>()
                                .sprite = obj.Result;
                        }
                    }
                };
        }
    }

}