using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Inputs
{

    public class InputManager : MonoBehaviour
    {
        public enum InputDeviceType
        {
            Xbox,
            Desktop,
            Unknown,
        }

        private static DefaultInputActions _inputActions;
        public static DefaultInputActions InputActions
        {
            get
            {
                if (_inputActions == null)
                    _inputActions = new DefaultInputActions();
                return _inputActions;
            }
        }

        public static UnityEvent deviceChanged;

        private static List<InputDevice> _currentDevices;
        public static InputDeviceType currentInputDeviceType = InputDeviceType.Unknown;
        public static bool UsingController => Gamepad.current != null;

        private static Dictionary<InputDeviceType, Dictionary<string, string>> _ACTION_TO_CONTROL_OVERRIDES
            = new Dictionary<InputDeviceType, Dictionary<string, string>>()
            {
                { InputDeviceType.Desktop, new Dictionary<string, string>()
                {
                    { "UI:Navigate", "navigate" },
                    { "InGameMenu:NavigateMenu", "navigate" },
                } }
            };

        private void Awake()
        {
            deviceChanged = new UnityEvent();
            _GetCurrentDevices();
            currentInputDeviceType = _GetInputType(_currentDevices[0]);
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += InputDeviceChanged;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= InputDeviceChanged;
        }

        #region Utils
        public static void EnableActionMap(InputActionMap actionMap)
        {
            if (!actionMap.enabled)
                actionMap.Enable();
        }

        public static void DisableActionMap(InputActionMap actionMap)
        {
            if (actionMap.enabled)
                actionMap.Disable();
        }

        public static string ActionToControl(string actionPath, bool allowOverride = true)
        {
            string[] tmp = actionPath.Split(':');

            // special case: direct override available?
            if (allowOverride)
            {
                Dictionary<string, string> overrides;
                if (_ACTION_TO_CONTROL_OVERRIDES.TryGetValue(currentInputDeviceType, out overrides))
                {
                    if (overrides.ContainsKey(actionPath))
                        return overrides[actionPath];
                }
            }

            InputActionMap map = InputActions.asset.FindActionMap(tmp[0]);
            if (map == null)
                return "";

            InputAction action = map.FindAction(tmp[1]);
            if (action == null)
                return "";
            List<string> controls = new List<string>();
            foreach (InputControl c in action.controls)
                if (c.device == _currentDevices[0] || c.device == _currentDevices[1])
                    controls.Add(c.name);
            if (controls.Count == 1)
                return controls[0];
            if (controls.Count > 1)
                return $"{controls[0]}+{controls[1]}";
            return "";
        }

        private static void _GetCurrentDevices()
        {
            _currentDevices = InputSystem.devices
                .Where((InputDevice d) => d.enabled)
                .OrderBy((InputDevice d) => _GetInputType(d))
                .ToList();
        }

        private static InputDeviceType _GetInputType(InputDevice device)
        {
            if (device == null)
                return InputDeviceType.Unknown;

            if (device.name == "Mouse" || device.name == "Keyboard")
                return InputDeviceType.Desktop;
            if (device.name.Contains("Xbox") || device.name.Contains("XInput"))
                return InputDeviceType.Xbox;
            return InputDeviceType.Unknown;
        }
        #endregion

        #region Event Callbacks
        private void InputDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            InputDevice _oldDevice = _currentDevices[0];

            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    _GetCurrentDevices();
                    break;
                default:
                    break;
            }

            if (_currentDevices[0] != _oldDevice)
            {
                currentInputDeviceType = _GetInputType(_currentDevices[0]);
                deviceChanged.Invoke();
            }
        }
        #endregion

    }

}
