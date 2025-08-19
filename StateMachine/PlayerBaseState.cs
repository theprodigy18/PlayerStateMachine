

namespace Drop.StateMachine
{
	public abstract class PlayerBaseState
	{
		protected PlayerController _player;

		public abstract void Enter(PlayerController player);
		public abstract void Update();
		public abstract void FixedUpdate();
		public abstract void Exit();
	}
}
