using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using DG.Tweening;

namespace Tools
{

    public static class Graphics
    {

        private static Transform _cameraTransform;

        #region Variables: Damage Popup
        private static readonly float _DAMAGE_POPUP_TIME = 2f;
        private static readonly float _DAMAGE_POPUP_TEXT_BASE_SIZE = 6f;
        #endregion

        public static void CreateDamagePopup(float damageAmount, Vector3 worldPosition)
        {
            if (_cameraTransform == null)
                _cameraTransform = Camera.main.transform;

            float damageMultiplier =
                damageAmount / AddressablesLoader.instance.playerData.attackDamage;

            AddressablesLoader.instance.damagePopupPrefab.InstantiateAsync(
                worldPosition, Quaternion.identity).Completed +=
                async (AsyncOperationHandle<GameObject> obj) =>
                {
                    GameObject g = obj.Result;
                    g.transform.rotation = Quaternion.LookRotation(
                        _cameraTransform.forward,
                        _cameraTransform.up);

                    TextMeshPro tmp = g.GetComponent<TextMeshPro>();
                    tmp.text = ((int)damageAmount).ToString();
                    tmp.fontSize = 1f + 2f * Mathf.Log(
                        _DAMAGE_POPUP_TEXT_BASE_SIZE * damageMultiplier);
                    tmp.color = new Color(
                        1f,
                        Mathf.Clamp01(1f - damageMultiplier * 0.1f),
                        0f,
                        1f);

                    g.transform
                        .DOMoveY(
                            (worldPosition + g.transform.up * 5f).y,
                            _DAMAGE_POPUP_TIME)
                        .SetEase(Ease.Linear);
                    DOTween.ToAlpha(
                        () => tmp.color,
                        (Color c) => { tmp.color = c; },
                        0f,
                        _DAMAGE_POPUP_TIME);

                    await System.Threading.Tasks.Task.Delay(
                        (int)(_DAMAGE_POPUP_TIME * 1000 + 100));
                    Addressables.ReleaseInstance(obj);
                };
        }

    }

}
