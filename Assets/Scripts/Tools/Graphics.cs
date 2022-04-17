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

        public static Color BrightenColor(Color c, float amount = 0.35f)
        {
            float r = Mathf.Clamp01(c.r + amount);
            float g = Mathf.Clamp01(c.g + amount);
            float b = Mathf.Clamp01(c.b + amount);
            return new Color(r, g, b, c.a);
        }

        // from: https://forum.unity.com/threads/print-colors-hexadecimal-value.476170/
        static public uint ToValue(this Color32 c32, bool includeAlpha = false)
        {
            if (!includeAlpha) return ((uint)c32.r << 16) | ((uint)c32.g << 8) | (uint)c32.b;
            return ((uint)c32.r << 24) | ((uint)c32.g << 16) | ((uint)c32.b << 8) | (uint)c32.a;
        }

    }

}
