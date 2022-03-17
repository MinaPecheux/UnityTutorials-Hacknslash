using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterController _controller;

        #region Variables: Inputs
        private DefaultInputActions _inputActions;
        private InputAction _moveAction;
        #endregion

        #region Variables: Movement
        private const float _MOVE_SPEED = 5f;
        private Vector2 _move;
        #endregion

        private void Awake()
        {
            _inputActions = new DefaultInputActions();
            _controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            _moveAction = _inputActions.Player.Move;
            _moveAction.Enable();
        }

        private void OnDisable()
        {
            _moveAction.Disable();
        }

        private void Update()
        {
            _move = _moveAction.ReadValue<Vector2>();
            _controller.Move(
                new Vector3(_move.x, 0f, _move.y) *
                Time.deltaTime *
                _MOVE_SPEED);
        }
    }

}
