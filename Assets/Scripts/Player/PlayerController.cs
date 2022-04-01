using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterController _controller;
        [SerializeField] private PlayerData _data;
        [SerializeField] private Animator _animator;
        private Transform _animatorTransform;

        #region Variables: Inputs
        private DefaultInputActions _inputActions;
        private InputAction _moveAction;
        private InputAction _attackAction;
        #endregion

        #region Variables: Movement
        private Vector2 _move;
        private bool _running;
        #endregion

        #region Variables: Attack
        private const int _COMBO_MAX_STEP = 2;
        private int _comboHitStep;
        private Coroutine _comboAttackResetCoroutine;
        private bool _attacking;
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
            _attacking = false;
            _animRunningParamHash = Animator.StringToHash("Running");

            _comboHitStep = -1;
            _comboAttackResetCoroutine = null;
        }

        private void OnEnable()
        {
            _moveAction = _inputActions.Player.Move;
            _moveAction.Enable();

            _attackAction = _inputActions.Player.Attack;
            _attackAction.performed += _OnAttackAction;
            _attackAction.Enable();
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _attackAction.Disable();
        }

        private void Update()
        {
            if (_attacking)
                return;

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
                    _data.moveSpeed);
            }
            else if (_running)
            {
                _running = false;
                _animator.SetBool(_animRunningParamHash, false);
            }
        }

        private void _OnAttackAction(InputAction.CallbackContext obj)
        {
            _attacking = true;
            if (_comboHitStep == _COMBO_MAX_STEP)
                return;
            float t = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (_comboHitStep == -1 || (t >= 0.1f && t <= 0.8f))
            {
                if (_comboAttackResetCoroutine != null)
                    StopCoroutine(_comboAttackResetCoroutine);
                _comboHitStep++;
                _animator.SetTrigger($"Attack{_comboHitStep}");
                _comboAttackResetCoroutine = StartCoroutine(
                    Tools.Utils.WaitingForCurrentAnimation(
                        _animator,
                        () =>
                        {
                            _comboHitStep = -1;
                            _attacking = false;
                        }));
            }
        }
    }

}
