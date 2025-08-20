using UnityEngine;

namespace Drop.StateMachine
{
	public class PlayerLandingState : PlayerBaseState
	{
		public override void Enter(PlayerController player)
		{
			_player = player;

			_player.isJumping = false;
			_player.isFalling = false;
			_player.isFastFalling = false;
			_player.fastFallTime = 0f;
			_player.isPastApexThreshold = false;
			_player.numOfJumpUsed = 0;

			_player.VerticalVelocity = Physics2D.gravity.y;
			_player.Landing();

			Debug.Log("Enter Landing State.");
		}

		public override void Update()
		{
			_player.ChangeState(_player.groundedState);
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