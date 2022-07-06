using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UI
{

    public class InputDisplayer : MonoBehaviour
    {
        private Image _icon;
        private GameObject _iconWithModifier;
        private Image _iconWithModifier1;
        private Image _iconWithModifier2;
        private TextMeshProUGUI _text;

        [SerializeField] private string _textDisplay;
        [SerializeField] private Color _textColor = Color.black;
        [SerializeField] private string _actionPath;

        private void Start()
        {
            if (_textDisplay != "" && _actionPath != "")
            {
                _InitializeReferences();
                _OnDeviceChanged(show: false);
            }

            Inputs.InputManager.deviceChanged.AddListener(_OnDeviceChanged);
        }

        public void SetDisplay(string text, string actionPath)
        {
            _InitializeReferences();

            _actionPath = actionPath;
            _textDisplay = text;
            _OnDeviceChanged(false);
        }

        private void _InitializeReferences()
        {
            if (_text != null)
                return;
            _icon = transform.Find("Icon").GetComponent<Image>();
            _iconWithModifier = transform.Find("IconWithModifier").gameObject;
            _iconWithModifier1 = transform.Find("IconWithModifier/Icon1").GetComponent<Image>();
            _iconWithModifier2 = transform.Find("IconWithModifier/Icon2").GetComponent<Image>();
            _text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _text.color = _textColor;
        }

        private void _SetupComponents(bool show, Sprite sprite1, Sprite sprite2 = null)
        {
            if (sprite1 && sprite2)
            {
                _iconWithModifier1.sprite = sprite1;
                _iconWithModifier2.sprite = sprite2;
                _icon.gameObject.SetActive(false);
                _iconWithModifier.SetActive(true);
            }
            else
            {
                _icon.sprite = sprite1;
                _icon.gameObject.SetActive(true);
                _iconWithModifier.SetActive(false);
            }
            _text.text = _textDisplay;
            if (show)
                gameObject.SetActive(true);
        }

        private void _OnDeviceChanged() { _OnDeviceChanged(true); }
        private void _OnDeviceChanged(bool show)
        {
            Inputs.InputManager.InputDeviceType idt =
                Inputs.InputManager.currentInputDeviceType;
            string control = Inputs.InputManager.ActionToControl(_actionPath);
            if (control == "")
                return;

            string c1 = "", c2 = "";
            if (control.Contains("+"))
            {
                string[] tmp = control.Split('+');
                c1 = tmp[0];
                c2 = tmp[1];
            }
            else
            {
                c1 = control;
            }

            Addressables.LoadAssetAsync<Sprite>($"Assets/InputIcons/{idt}/{c1}.png")
                .Completed += (AsyncOperationHandle<Sprite> obj1) =>
                {
                    if (obj1.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (obj1.Result != null)
                        {
                            if (c2 != "")
                            {
                                Addressables.LoadAssetAsync<Sprite>($"Assets/InputIcons/{idt}/{c2}.png")
                                    .Completed += (AsyncOperationHandle<Sprite> obj2) =>
                                    {
                                        if (obj2.Status == AsyncOperationStatus.Succeeded)
                                        {
                                            if (obj2.Result != null)
                                            {
                                                _SetupComponents(show, obj1.Result, obj2.Result);
                                            }
                                        }
                                    };
                            }
                            else
                            {
                                _SetupComponents(show, obj1.Result);
                            }
                        }
                    }
                };
        }

    }

}
