using UnityEngine;

namespace Drop
{
	[CreateAssetMenu(menuName = "Drop/PlayerMovementStats", fileName = "new PlayerMovementStats")]
	public class PlayerMovementStats : ScriptableObject
	{
		[Header("Walk")]
		[Range(1f, 100f)] public float maxWalkSpeed = 12.5f;
		[Range(0.25f, 50f)] public float groundAcceleration = 5f;
		[Range(0.25f, 50f)] public float groundDeceleration = 20f;
		[Range(0.25f, 50f)] public float airAcceleration = 5f;
		[Range(0.25f, 50f)] public float airDeceleration = 5f;

		[Header("Run")]
		[Range(1f, 100f)] public float maxRunSpeed = 20f;

		[Header("Grounded/Collision Check")]
		public LayerMask groundLayer;
		public float groundDetectionRayLength = 0.02f;
		public float headDetectionRayLength = 0.02f;
		//[Range(0f, 1f)] public float headWidth = 0.75f;

		[Header("Jump")]
		public float jumpHeight = 6.5f;
		[Range(1f, 1.1f)] public float jumpHeightCompensationFactor = 1.054f;
		public float timeTillJumpApex = 0.35f;
		[Range(0.01f, 5f)] public float gravityOnReleaseMultiplier = 2f;
		public float maxFallSpeed = 26f;
		[Range(1, 5)] public int numberOfJumpAllowed = 2;

		[Header("Jump Cut")]
		[Range(0.02f, 0.3f)] public float timeForUpwardsCancel = 0.027f;

		[Header("Jump Apex")]
		[Range(0.5f, 1f)] public float apexThreshold = 0.97f;
		[Range(0.01f, 1f)] public float apexHangTime = 0.075f;

		[Header("Jump Buffer")]
		[Range(0f, 1f)] public float jumpBufferTime = 0.125f;

		[Header("Jump Coyote Time")]
		[Range(0f, 1f)] public float jumpCoyoteTime = 0.1f;

		public float Gravity { get; private set; }
		public float InitialJumpVelocity { get; private set; }
		public float AdjustedJumpHeight { get; private set; }

		private void OnValidate()
		{
			CalculateValues();
		}

		private void OnEnable()
		{
			CalculateValues();
		}

		private void CalculateValues()
		{
			AdjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;
			Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(timeTillJumpApex, 2f);
			InitialJumpVelocity = Mathf.Abs(Gravity) * timeTillJumpApex;
		}
	}
}
