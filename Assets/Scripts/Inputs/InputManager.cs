using UnityEngine;
using UnityEngine.InputSystem;

namespace Inputs
{

    public class InputManager : MonoBehaviour
    {
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

        public static bool UsingController => Gamepad.current != null;

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

    }

}
