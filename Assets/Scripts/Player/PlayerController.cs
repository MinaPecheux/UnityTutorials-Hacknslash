using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterController _controller;
        [SerializeField] private Animator _animator;
        private PlayerData _data;
        private Transform _animatorTransform;

        #region Variables: Inputs
        private InputAction _moveAction;
        private InputAction _attackAction;
        #endregion

        #region Variables: Movement
        private Vector2 _move;
        private bool _running;
        private bool _hasOverbudernedState;
        #endregion

        #region Variables: Attack
        private const int _COMBO_MAX_STEP = 2;
        private int _comboHitStep;
        private Coroutine _comboAttackResetCoroutine;
        private bool _attacking;
        #endregion

        #region Variables: Animation
        private int _animRunningParamHash;
        private int _animOverburdenedParamHash;
        private int _animAttackComboStepParamHash;
        #endregion

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animatorTransform = _animator.transform;

            _running = false;
            _attacking = false;
            _animRunningParamHash = Animator.StringToHash("Running");
            _animOverburdenedParamHash = Animator.StringToHash("Overburdened");
            _animAttackComboStepParamHash = Animator.StringToHash("AttackComboStep");

            _comboHitStep = -1;
            _comboAttackResetCoroutine = null;
        }

        private void Start()
        {
            Tools.AddressablesLoader.addressablesLoaded.AddListener(
                _OnAddressablesLoaded);
        }

        private void OnEnable()
        {
            _moveAction = Inputs.InputManager.InputActions.Player.Move;
            _moveAction.Enable();

            _attackAction = Inputs.InputManager.InputActions.Player.Attack;
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

                if (_data.overburdened && !_hasOverbudernedState)
                {
                    _hasOverbudernedState = true;
                    _animator.SetBool(_animOverburdenedParamHash, true);
                }
                else if (!_data.overburdened && _hasOverbudernedState)
                {
                    _hasOverbudernedState = false;
                    _animator.SetBool(_animOverburdenedParamHash, false);
                }

                Vector3 v = new Vector3(_move.x, 0f, _move.y);
                float s = _data.moveSpeed;
                if (_data.overburdened) s *= 0.33f;
                _animatorTransform.rotation =
                    Quaternion.LookRotation(-v, Vector3.up);
                _controller.Move(v * Time.deltaTime * s);
            }
            else if (_running)
            {
                _running = false;
                _animator.SetBool(_animRunningParamHash, false);
            }
        }

        private void _OnAddressablesLoaded()
        {
            _data = Tools.AddressablesLoader.instance.playerData;
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
                _animator.SetBool(_animRunningParamHash, false);
                _animator.SetInteger(
                    _animAttackComboStepParamHash, _comboHitStep);
                _comboAttackResetCoroutine = StartCoroutine(
                    Tools.Utils.WaitingForCurrentAnimation(
                        _animator,
                        () =>
                        {
                            _comboHitStep = -1;
                            _animator.SetInteger(
                                _animAttackComboStepParamHash, _comboHitStep);
                            _attacking = false;

                            _move = _moveAction.ReadValue<Vector2>();
                            if (_move.sqrMagnitude > 0.01f && _running)
                                _animator.SetBool(_animRunningParamHash, true);
                        },
                        stopAfterAnim: true,
                        earlyExit: 0.1f));
            }
        }
    }

}
