using System.Collections;
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
        private InputAction _attackAction;
        #endregion

        #region Variables: Movement
        private const float _MOVE_SPEED = 5f;
        private Vector2 _move;
        private bool _running;
        #endregion

        #region Variables: Attack
        private const float _COMBO_MIN_DELAY = 0.1f;
        private const int _COMBO_MAX_STEP = 2;
        private float _lastComboTime;
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
                    _MOVE_SPEED);
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
            float t = Time.time;
            if (_comboHitStep == _COMBO_MAX_STEP)
                return;
            if (_comboHitStep == -1 || t - _lastComboTime >= _COMBO_MIN_DELAY)
            {
                if (_comboAttackResetCoroutine != null)
                    StopCoroutine(_comboAttackResetCoroutine);
                _comboHitStep++;
                _animator.SetTrigger($"Attack{_comboHitStep}");
                _comboAttackResetCoroutine = StartCoroutine(_ResettingAttackCombo());
            }
            _lastComboTime = t;
        }

        private IEnumerator _ResettingAttackCombo()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(
                _animator.GetAnimatorTransitionInfo(0).duration);
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(
                _animator.GetCurrentAnimatorStateInfo(0).length - 0.1f);
            _comboHitStep = -1;
            _attacking = false;
        }
    }

}
