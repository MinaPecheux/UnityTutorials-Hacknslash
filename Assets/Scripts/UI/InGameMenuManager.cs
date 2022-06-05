using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public abstract class InGameMenuPanelManager : MonoBehaviour
    {
        public abstract void OnEntry();
        public abstract void OnExit();
    }

    public class InGameMenuManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private Image[] _tabBackgrounds;
        [SerializeField] private GameObject[] _panels;
        [SerializeField] private InGameMenuPanelManager[] _panelManagers;
        [SerializeField] private Sprite _tabOnSprite;
        [SerializeField] private Sprite _tabOffSprite;

        [SerializeField] private GameObject _equipmentPreviewAnchor;

        #region Base Variables
        private int _prevPanelIndex;
        private int _currentPanelIndex;
        #endregion

        #region Variables: Inputs
        private InputAction _toggleMenuAction;
        private InputAction _navigateMenuAction;
        #endregion

        private void Start()
        {
            if (!Inputs.InputManager.UsingController)
            {
                int i = 0;
                foreach (Image tab in _tabBackgrounds)
                {
                    Button b = tab.gameObject.AddComponent<Button>();
                    _SetMenuNavigationTab(b, i++);
                }
            }

            _prevPanelIndex = -1;
            _currentPanelIndex = 0;

            _equipmentPreviewAnchor.SetActive(false);
        }

        private void OnEnable()
        {
            _toggleMenuAction = Inputs.InputManager.InputActions.Transitions.ToggleMenu;
            _toggleMenuAction.performed += _OnToggleMenuAction;
            _toggleMenuAction.Enable();

            _navigateMenuAction = Inputs.InputManager.InputActions.InGameMenu.NavigateMenu;
            _navigateMenuAction.performed += _OnNavigateMenuAction;
            _navigateMenuAction.Enable();
        }

        private void OnDisable()
        {
            _toggleMenuAction.Disable();
            _navigateMenuAction.Disable();
        }

        private void _OnToggleMenuAction(InputAction.CallbackContext obj)
        {
            bool on = !_menuPanel.activeSelf;
            if (on)
            {
                _UpdateMenu();
                Inputs.InputManager.DisableActionMap(
                    Inputs.InputManager.InputActions.Player);
                Inputs.InputManager.EnableActionMap(
                    Inputs.InputManager.InputActions.UI);
                Inputs.InputManager.EnableActionMap(
                    Inputs.InputManager.InputActions.InGameMenu);
            }
            else
            {
                InGameMenuPanelManager m = _panelManagers[_currentPanelIndex];
                if (m != null)
                    m.OnExit();
                Inputs.InputManager.EnableActionMap(
                    Inputs.InputManager.InputActions.Player);
                Inputs.InputManager.DisableActionMap(
                    Inputs.InputManager.InputActions.UI);
                Inputs.InputManager.DisableActionMap(
                    Inputs.InputManager.InputActions.InGameMenu);
            }
            _menuPanel.SetActive(on);
            _equipmentPreviewAnchor.SetActive(on);
        }

        private void _OnNavigateMenuAction(InputAction.CallbackContext obj)
        {
            int navDirection = (obj.ReadValue<float>() > 0) ? 1 : -1;
            _currentPanelIndex =
                (_currentPanelIndex + navDirection + _panels.Length) % _panels.Length;
            _UpdateMenu();
        }

        private void _SetMenuNavigationTab(Button b, int i)
        {
            b.onClick.AddListener(() =>
            {
                _currentPanelIndex = i;
                _UpdateMenu();
            });
        }

        private void _UpdateMenu()
        {
            // update tabs
            for (int i = 0; i < _tabBackgrounds.Length; i++)
                _tabBackgrounds[i].sprite = i == _currentPanelIndex
                    ? _tabOnSprite : _tabOffSprite;

            // update inner panels
            for (int i = 0; i < _panels.Length; i++)
                _panels[i].SetActive(i == _currentPanelIndex);

            if (_prevPanelIndex != -1)
            {
                InGameMenuPanelManager prevM = _panelManagers[_prevPanelIndex];
                if (prevM != null)
                    prevM.OnExit();
            }
            InGameMenuPanelManager m = _panelManagers[_currentPanelIndex];
            if (m != null)
                m.OnEntry();

            _prevPanelIndex = _currentPanelIndex;
        }
    }

}