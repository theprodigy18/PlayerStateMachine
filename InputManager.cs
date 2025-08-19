using UnityEngine;
using UnityEngine.InputSystem;


namespace Drop
{
	public class InputManager : MonoBehaviour
	{
		public static InputManager Instance { get; private set; }

		private PlayerInput _playerInput;

		public Vector2 Movement { get; private set; }
		public bool JumpWasPressed { get; private set; }
		public bool JumpIsHeld { get; private set; }
		public bool JumpWasReleased { get; private set; }
		public bool RunIsHeld { get; private set; }

		private InputAction _moveAction;
		private InputAction _jumpAction;
		private InputAction _runAction;

		private void Awake()
		{
			Instance = this;

			_playerInput = GetComponent<PlayerInput>();
			_moveAction = _playerInput.actions["Move"];
			_jumpAction = _playerInput.actions["Jump"];
			_runAction = _playerInput.actions["Run"];
		}

		private void Update()
		{
			Movement = _moveAction.ReadValue<Vector2>();

			JumpWasPressed = _jumpAction.WasPressedThisFrame();
			JumpIsHeld = _jumpAction.IsPressed();
			JumpWasReleased = _jumpAction.WasReleasedThisFrame();

			RunIsHeld = _runAction.IsPressed();
		}


	}
}
