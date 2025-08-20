using UnityEngine;

namespace Drop.StateMachine
{
	public class PlayerJumpingState : PlayerBaseState
	{
		public override void Enter(PlayerController player)
		{
			_player = player;

			_player.isJumping = true;
			_player.jumpBufferTimer = 0f;
			_player.numOfJumpUsed += 1;
			_player.VerticalVelocity = _player.moveStats.InitialJumpVelocity;

			if (_player.jumpReleasedDuringBuffer)
			{
				_player.isFastFalling = true;
				_player.fastFallReleaseSpeed = _player.VerticalVelocity;
			}

			_player.JumpEffect();

			Debug.Log("Enter Jumping State.");
		}

		public override void Update()
		{
			// Handle jump released for jump cutting.
			if (_player.inputManager.JumpWasReleased && _player.VerticalVelocity > 0f)
			{
				if (_player.isPastApexThreshold)
				{
					_player.isPastApexThreshold = false;
					_player.isFastFalling = true;
					_player.fastFallTime = _player.moveStats.timeForUpwardsCancel;
					_player.VerticalVelocity = 0f;
				}
				else
				{
					_player.isFastFalling = true;
					_player.fastFallReleaseSpeed = _player.VerticalVelocity;
					_player.fastFallTime = 0f;
				}
			}

			// Double jump check.
			if (_player.jumpBufferTimer > 0f && _player.numOfJumpUsed < _player.moveStats.numberOfJumpAllowed)
			{
				_player.isFastFalling = false;
				_player.VerticalVelocity = _player.moveStats.InitialJumpVelocity;
				_player.numOfJumpUsed += 1;

				_player.JumpEffect();
			}
		}

		public override void FixedUpdate()
		{
			_player.Move(_player.moveStats.airAcceleration, _player.moveStats.airDeceleration);

			// Fast falling.
			if (_player.isFastFalling)
			{
				if (_player.fastFallTime >= _player.moveStats.timeForUpwardsCancel)
				{
					_player.VerticalVelocity += _player.moveStats.Gravity * _player.moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
				}
				else
				{
					_player.VerticalVelocity = Mathf.Lerp(_player.fastFallReleaseSpeed, 0f,
						(_player.fastFallTime / _player.moveStats.timeForUpwardsCancel));
				}
				_player.fastFallTime += Time.fixedDeltaTime;
			}
			// Ascending gravity(upward aerial).
			else if (_player.VerticalVelocity > 0f)
			{
				// Head bump check.
				if (_player.bumpedHead)
				{
					_player.isFastFalling = true;
					return;
				}

				// Apex calculations.
				_player.apexPoint = Mathf.InverseLerp(_player.moveStats.InitialJumpVelocity, 0f, _player.VerticalVelocity);

				if (_player.apexPoint > _player.moveStats.apexThreshold)
				{
					if (!_player.isPastApexThreshold)
					{
						_player.isPastApexThreshold = true;
						_player.timePastApexThreshold = 0f;
					}

					_player.timePastApexThreshold += Time.fixedDeltaTime;
					if (_player.timePastApexThreshold < _player.moveStats.apexHangTime)
					{
						_player.VerticalVelocity = 0f;
					}
					else
					{
						_player.VerticalVelocity = -0.01f;
					}
				}
				else
				{
					_player.VerticalVelocity += _player.moveStats.Gravity * Time.fixedDeltaTime;
					if (_player.isPastApexThreshold)
					{
						_player.isPastApexThreshold = false;
					}
				}
			}
			// Change to falling state when gravity is descending.
			else
			{
				_player.ChangeState(_player.fallingState);
				return;
			}

			// Apply fall speed.
			_player.rb.linearVelocity = new Vector2(_player.rb.linearVelocity.x, _player.VerticalVelocity);

			// Check for landing state transition.
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
