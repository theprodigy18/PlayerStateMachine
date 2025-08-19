

using UnityEngine;

namespace Drop.StateMachine
{
	public class PlayerGroundedState : PlayerBaseState
	{
		public override void Enter(PlayerController player)
		{
			_player = player;
			Debug.Log("Enter Grounded State.");
		}

		public override void Update()
		{
			if (_player.jumpBufferTimer > 0f && (_player.isGrounded || _player.coyoteTimer > 0f))
			{
				_player.ChangeState(_player.jumpingState);
				return;
			}

			if (!_player.isGrounded)
			{
				_player.ChangeState(_player.fallingState);
				return;
			}
		}


		public override void FixedUpdate()
		{
			_player.Move(_player.moveStats.groundAcceleration, _player.moveStats.groundDeceleration);
		}

		public override void Exit()
		{
			_player = null;	
		}
	}
}
