using Drop.StateMachine;
using UnityEngine;

namespace Drop
{
	public class PlayerController : MonoBehaviour
	{
		[Header("References")]
		public PlayerMovementStats moveStats;
		public Collider2D bodyColl;
		public Collider2D feetColl;

		[HideInInspector] public Rigidbody2D rb;
		[HideInInspector] public Animator animator;
		[HideInInspector] public InputManager inputManager;

		[Header("Visual Effects")]
		[SerializeField] private ParticleSystem _dustParticle;
		[SerializeField] private ParticleSystem _jumpDustParticle;
		[SerializeField] private TrailRenderer _jumpTrail;

		[Header("Sound Effects")]
		[SerializeField] private AudioClip _jumpSFX;
		[SerializeField] private AudioClip _landSFX;
		private AudioSource _audioSource;
		
		// Movement variables.
		private Vector2 _moveVelocity;
		private bool _isFacingRight;

		// Collision check variables.
		private RaycastHit2D _groundHit;
		private RaycastHit2D _headHit;
		[HideInInspector] public bool isGrounded;
		[HideInInspector] public bool bumpedHead;

		// Jump variables.
		[HideInInspector] public bool isJumping;
		[HideInInspector] public bool isFastFalling;
		[HideInInspector] public bool isFalling;
		[HideInInspector] public float fastFallTime;
		[HideInInspector] public float fastFallReleaseSpeed;
		[HideInInspector] public int numOfJumpUsed;
		public float VerticalVelocity { get; set; }

		// Apex variables.
		[HideInInspector] public float apexPoint;
		[HideInInspector] public float timePastApexThreshold;
		[HideInInspector] public bool isPastApexThreshold;

		// Jump buffer variables.
		[HideInInspector] public float jumpBufferTimer;
		[HideInInspector] public bool jumpReleasedDuringBuffer;

		// Coyote time variables.
		[HideInInspector] public float coyoteTimer;

		// State machine.
		public PlayerGroundedState groundedState;
		public PlayerJumpingState jumpingState;
		public PlayerFallingState fallingState;
		public PlayerLandingState landingState;
		private PlayerBaseState _currentState;

		// Cache animator state.
		private bool _isMovingAnimator;
		private bool IsMovingAnimator
		{
			set
			{
				if (_isMovingAnimator != value)
				{
					animator.SetBool("isMoving", value);
					_isMovingAnimator = value;
				}
			}
		}
		private bool _isRunningAnimator;
		private bool IsRunningAnimator
		{
			set
			{
				if (_isRunningAnimator != value)
				{
					animator.SetBool("isRunning", value);
					_isRunningAnimator = value;
				}
			}
		}
		private bool _isGroundedAnimator;
		private bool IsGroundedAnimator
		{
			set
			{
				if (_isGroundedAnimator != value)
				{
					animator.SetBool("isGrounded", value);
					_isGroundedAnimator = value;
				}
			}
		}

		private void Awake()
		{
			_isFacingRight = true;

			rb = GetComponent<Rigidbody2D>();
			animator = GetComponent<Animator>();
			_audioSource = GetComponent<AudioSource>();

			groundedState = new PlayerGroundedState();
			jumpingState = new PlayerJumpingState();
			fallingState = new PlayerFallingState();
			landingState = new PlayerLandingState();

			ChangeState(landingState);
		}
		private void Start()
		{
			inputManager = InputManager.Instance;
		}

		private void Update()
		{
			ConsumeJumpInput();
			CountTimers();

			_currentState.Update();

			animator.SetFloat("yVelocity", VerticalVelocity);
		}

		private void FixedUpdate()
		{
			CollisionChecks();

			_currentState.FixedUpdate();
		}

		#region MOVEMENT
		public void Move(float acceleration, float deceleration)
		{
			Vector2 moveInput = inputManager.Movement;

			if (moveInput != Vector2.zero)
			{
				// Check if need to turn.
				TurnCheck(moveInput);

				Vector2 targetVelocity = Vector2.zero;
				if (InputManager.Instance.RunIsHeld)
				{
					targetVelocity = new Vector2(moveInput.x, 0f) * moveStats.maxRunSpeed;
					IsRunningAnimator = true;
				}
				else
				{
					targetVelocity = new Vector2(moveInput.x, 0f) * moveStats.maxWalkSpeed;
					IsRunningAnimator = false;
				}

				_moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
				rb.linearVelocity = new Vector2(_moveVelocity.x, rb.linearVelocity.y);
				IsMovingAnimator = true;
			}
			else
			{
				_moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
				rb.linearVelocity = new Vector2(_moveVelocity.x, rb.linearVelocity.y);
				IsMovingAnimator = false;
			}
		}

		private void TurnCheck(Vector2 moveInput)
		{
			if (_isFacingRight && moveInput.x < 0f)
			{
				_isFacingRight = false;
				transform.Rotate(0f, -180f, 0f);
				if (isGrounded) _dustParticle.Play();
			}
			else if (!_isFacingRight && moveInput.x > 0f)
			{
				_isFacingRight = true;
				transform.Rotate(0f, 180f, 0f);
				if (isGrounded) _dustParticle.Play();
			}
		}

		#endregion

		#region COLLISION
		private void IsGrounded()
		{
			Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, feetColl.bounds.min.y);
			Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x, moveStats.groundDetectionRayLength);

			_groundHit = Physics2D.BoxCast(
				boxCastOrigin, boxCastSize, 0f, Vector2.down, moveStats.groundDetectionRayLength, moveStats.groundLayer);
			if (_groundHit.collider != null)
			{
				isGrounded = true;
			}
			else
			{
				isGrounded = false;
			}

			IsGroundedAnimator = isGrounded;
		}

		private void BumpedHead()
		{
			Vector2 boxCastOrigin = new Vector2(bodyColl.bounds.center.x, bodyColl.bounds.max.y);
			Vector2 boxCastSize = new Vector2(bodyColl.bounds.size.x, moveStats.headDetectionRayLength);

			_headHit = Physics2D.BoxCast(
				boxCastOrigin, boxCastSize, 0f, Vector2.up, moveStats.headDetectionRayLength, moveStats.groundLayer);

			if (_headHit.collider != null)
			{
				bumpedHead = true;
			}
			else
			{
				bumpedHead = false;
			}
		}

		private void CollisionChecks()
		{
			IsGrounded();
			BumpedHead();
		}

		#endregion
		private void CountTimers()
		{
			jumpBufferTimer -= Time.deltaTime;

			if (!isGrounded)
			{
				coyoteTimer -= Time.deltaTime;
			}
			else
			{
				coyoteTimer = moveStats.jumpCoyoteTime;
			}
		}

		private void ConsumeJumpInput()
		{
			if (inputManager.JumpWasPressed)
			{
				jumpBufferTimer = moveStats.jumpBufferTime;
				jumpReleasedDuringBuffer = false;
			}

			if (inputManager.JumpWasReleased)
			{
				if (jumpBufferTimer > 0f)
				{
					jumpReleasedDuringBuffer = true;
				}
			}
		}

		public void ChangeState(PlayerBaseState state)
		{
			_currentState?.Exit();

			_currentState = state;
			_currentState.Enter(this);
		}

		public void JumpEffect()
		{
			animator.SetTrigger("jump");
			_jumpDustParticle.Play();
			_jumpTrail.emitting = true;
			_audioSource.pitch = Random.Range(0.9f, 1.1f);
			_audioSource.clip = _jumpSFX;
			_audioSource.Play();
		}

		public void Landing()
		{
			_jumpDustParticle.Play();
			_jumpTrail.emitting = false;
			_audioSource.pitch = Random.Range(0.9f, 1.1f);
			_audioSource.clip = _landSFX;
			_audioSource.Play();
		}
	}
}
