using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{

    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        #region Variables: Skills
        [SerializeField] private Transform _skillsBar;
        private Transform[] _skillSlots;
        private Image[] _skillCooldowns;

        private Skills.SkillData[] _skills;
        #endregion

        #region Variables: Inputs
        private InputAction _castSkill1Action;
        private InputAction _castSkill2Action;
        private InputAction _castSkill3Action;
        private InputAction _castSkill4Action;
        private InputAction _castSkill5Action;
        private InputAction _castSkill6Action;
        #endregion

        void Awake()
        {
            instance = this;

            // (remove 1 to the count to ignore the "Border" child at the end
            // of the hierarchy
            int n = _skillsBar.childCount - 1;
            _skillSlots = new Transform[n];
            _skillCooldowns = new Image[n];
            for (int i = 0; i < n; i++)
            {
                _skillSlots[i] = _skillsBar.GetChild(i);
                _skillCooldowns[i] = _skillSlots[i].Find("Cooldown").GetComponent<Image>();
                _skillCooldowns[i].raycastTarget = false;
                _AddSlotListener(_skillSlots[i].Find("Icon").GetComponent<Button>(), i);
            }

            _skills = new Skills.SkillData[n];
        }

        private void OnEnable()
        {
            _castSkill1Action = Inputs.InputManager.InputActions.Player.CastSkill1;
            _castSkill1Action.performed += _OnCastSkill1;
            _castSkill1Action.Enable();
            _castSkill2Action = Inputs.InputManager.InputActions.Player.CastSkill2;
            _castSkill2Action.performed += _OnCastSkill2;
            _castSkill2Action.Enable();
            _castSkill3Action = Inputs.InputManager.InputActions.Player.CastSkill3;
            _castSkill3Action.performed += _OnCastSkill3;
            _castSkill3Action.Enable();
            _castSkill4Action = Inputs.InputManager.InputActions.Player.CastSkill4;
            _castSkill4Action.performed += _OnCastSkill4;
            _castSkill4Action.Enable();
            _castSkill5Action = Inputs.InputManager.InputActions.Player.CastSkill5;
            _castSkill5Action.performed += _OnCastSkill5;
            _castSkill5Action.Enable();
            _castSkill6Action = Inputs.InputManager.InputActions.Player.CastSkill6;
            _castSkill6Action.performed += _OnCastSkill6;
            _castSkill6Action.Enable();
        }

        private void OnDisable()
        {
            _castSkill1Action.Disable();
            _castSkill2Action.Disable();
            _castSkill3Action.Disable();
            _castSkill4Action.Disable();
            _castSkill5Action.Disable();
            _castSkill6Action.Disable();
        }

        private void _OnCastSkill1(InputAction.CallbackContext obj) { _CastSlot(0); }
        private void _OnCastSkill2(InputAction.CallbackContext obj) { _CastSlot(1); }
        private void _OnCastSkill3(InputAction.CallbackContext obj) { _CastSlot(2); }
        private void _OnCastSkill4(InputAction.CallbackContext obj) { _CastSlot(3); }
        private void _OnCastSkill5(InputAction.CallbackContext obj) { _CastSlot(4); }
        private void _OnCastSkill6(InputAction.CallbackContext obj) { _CastSlot(5); }

        private void _AddSlotListener(Button b, int i)
        {
            b.onClick.AddListener(() => _CastSlot(i));
        }

        public void SetSlot(int i, Skills.SkillData data)
        {
            Transform slot = _skillSlots[i];
            slot.Find("Icon").GetComponent<Image>().sprite = data.icon;
            slot.Find("Icon").gameObject.SetActive(true);
            _skillCooldowns[i].fillAmount = 0;

            _skills[i] = data;
        }

        public void _CastSlot(int i)
        {
            bool cast = _skills[i].Cast();
            if (cast)
                StartCoroutine(_ShowingCooldown(i, _skills[i].cooldown));
        }

        private IEnumerator _ShowingCooldown(int i, float cooldown)
        {
            float t = 0;
            _skillCooldowns[i].raycastTarget = true;
            while (t < cooldown)
            {
                _skillCooldowns[i].fillAmount = 1f - t / cooldown;
                t += Time.deltaTime;
                yield return null;
            }
            _skillCooldowns[i].fillAmount = 0;
            _skillCooldowns[i].raycastTarget = false;
        }
    }

}