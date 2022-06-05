using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EquipmentHeroPreviewer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform _previewCameraAnchor;

    private bool _dragging;
    private Vector2 _lastMousePosition;
    private Vector2 _currentMousePosition;
    private Vector2 _delta;

    private InputAction _rotatePreviewAction;
    private Vector2 _joystickDrag;

    private void OnEnable()
    {
        _rotatePreviewAction = Inputs.InputManager.InputActions.InGameMenu.RotatePreview;
        _rotatePreviewAction.Enable();
    }

    private void OnDisable()
    {
        _rotatePreviewAction.Disable();
    }

    private void Update()
    {
        // gamepad controllers
        _joystickDrag = _rotatePreviewAction.ReadValue<Vector2>();
        if (_joystickDrag.sqrMagnitude > 0.01f)
        {
            _previewCameraAnchor.Rotate(Vector3.up * Time.deltaTime * _joystickDrag.x * 180f);
        }

        // keyboard/mouse
        if (!_dragging) return;

        _currentMousePosition = Mouse.current.position.ReadValue();
        _delta = _currentMousePosition - _lastMousePosition;
        _lastMousePosition = _currentMousePosition;
        if (_delta.sqrMagnitude > Mathf.Epsilon)
        {
            _previewCameraAnchor.Rotate(Vector3.up * Time.deltaTime * _delta.x * 180f);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _lastMousePosition = Mouse.current.position.ReadValue();
        _dragging = true;
        Inventory.InventoryManager.heroPreviewDragUpdated.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;
        Inventory.InventoryManager.heroPreviewDragUpdated.Invoke(false);
    }
}
