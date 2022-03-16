using UnityEngine;

namespace Player
{

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterController _controller;

        #region Variables: Movement
        private const float _MOVE_SPEED = 5f;
        private Vector3 _move;
        #endregion

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            _move = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")).normalized;
            _controller.Move(_move * Time.deltaTime * _MOVE_SPEED);
        }
    }

}
