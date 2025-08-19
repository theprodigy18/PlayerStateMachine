using UnityEngine;

namespace Drop.StateMachine
{

	public class PlayerFallingState : PlayerBaseState
	{
		public override void Enter(PlayerController player)
		{
			_player = player;

			_player.isFalling = true;

			Debug.Log("Enter Falling State.");
		}

		public override void Update()
		{
			if (_player.numOfJumpUsed == 0 && _player.coyoteTimer <= 0f)
			{
				_player.numOfJumpUsed += 1;
			}

			if (_player.jumpBufferTimer > 0f && _player.numOfJumpUsed < _player.moveStats.numberOfJumpAllowed)
			{
				_player.ChangeState(_player.jumpingState);
				return;
			}
		}

		public override void FixedUpdate()
		{
			_player.Move(_player.moveStats.airAcceleration, _player.moveStats.airDeceleration);

			_player.VerticalVelocity += _player.moveStats.Gravity * Time.fixedDeltaTime;

			// Clamp fall speed.
			_player.VerticalVelocity = Mathf.Clamp(_player.VerticalVelocity, -_player.moveStats.maxFallSpeed, 50f);

			// Apply fall speed.
			_player.rb.linearVelocity = new Vector2(_player.rb.linearVelocity.x, _player.VerticalVelocity);

			// Check for landing.
			if (_player.isGrounded && _player.VerticalVelocity <= 0f)
			{
				_player.ChangeState(_player.landingState);
				return;
			}
		}

		public override void Exit()
		{
			_player = null;
		}
	}
}
