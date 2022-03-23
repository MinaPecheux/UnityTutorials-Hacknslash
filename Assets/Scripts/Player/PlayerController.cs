using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterController _controller;
        [SerializeField] private Animator _animator;
        private Transform _animatorTransform;

        #region Variables: Inputs
        private DefaultInputActions _inputActions;
        private InputAction _moveAction;
        #endregion

        #region Variables: Movement
        private const float _MOVE_SPEED = 5f;
        private Vector2 _move;
        private bool _running;
        #endregion

        #region Variables: Animation
        private int _animRunningParamHash;
        #endregion

        private void Awake()
        {
            _inputActions = new DefaultInputActions();
            _controller = GetComponent<CharacterController>();
            _animatorTransform = _animator.transform;

            _running = false;
            _animRunningParamHash = Animator.StringToHash("Running");
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
            if (_move.sqrMagnitude > 0.01f)
            {
                if (!_running)
                {
                    _running = true;
                    _animator.SetBool(_animRunningParamHash, true);
                }

                Vector3 v = new Vector3(_move.x, 0f, _move.y);
                _animatorTransform.rotation =
                    Quaternion.LookRotation(-v, Vector3.up);
                _controller.Move(
                    v *
                    Time.deltaTime *
                    _MOVE_SPEED);
            }
            else if (_running)
            {
                _running = false;
                _animator.SetBool(_animRunningParamHash, false);
            }
        }
    }

}
