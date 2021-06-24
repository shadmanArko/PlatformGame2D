using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CharacterController2D : MonoBehaviour {

	
	[Range(0.0f, 10.0f)] 
	public float moveSpeed = 3f;
	
	public float jumpForce = 600f;
	
	public int playerHealth = 1;
	
	public LayerMask whatIsGround;
	
	public Transform groundCheck;

	[HideInInspector]
	public bool playerCanMove = true;

	public AudioClip coinSFX;
	public AudioClip deathSFX;
	public AudioClip fallSFX;
	public AudioClip jumpSFX;
	public AudioClip victorySFX;

	
	[Header("Keyboard Control and Mobile Control Changer")]
	[Space(30)]public bool keyboardControl = true;
	
	Transform _transform;
	Rigidbody2D _rigidbody;
	Animator _animator;
	AudioSource _audio;
	
	float _velocityOnXAxis;
	float _velocityOnYAxis;

	bool _facingRight = true;
	bool _isGrounded = false;
	bool _canDoubleJump = true;
	bool _goRight = false;
	bool _goLeft = false;
	bool _idle = true;
	bool _move = false;
	bool _jump = false;
	
	int _playerLayer;
	int _platformLayer;


	public void On_ClickMoveRight()
	{
		_goRight = true;
		_idle = false;
		_move = true;
	}
	
	public void On_ClickMoveLeft()
	{
		_goLeft = true;
		_idle = false;
		_move = true;
	}
	
	public void ONPointerUp()
	{
		_goLeft = false;
		_goRight = false;
		_idle = true;
		_move = false;
	}

	public void On_ClickJump()
	{
		_jump = true;
	}

	public void On_ClickPointerUpForJump()
	{
		_jump = false;
	}

	public void On_ClickCrouch()
	{
		moveSpeed = 1f;
		_animator.SetBool("Crouch", true);
	}
	
	public void On_ClickIdle()
	{
		moveSpeed = 3f;
		_animator.SetBool("Crouch", false);
	}
	
	void Awake () 
	{
		
		_transform = GetComponent<Transform> ();
		
		_rigidbody = GetComponent<Rigidbody2D> ();
		if (_rigidbody==null) 
			Debug.LogError("Rigidbody2D component missing from this gameobject");
		
		_animator = GetComponent<Animator>();
		if (_animator==null) 
			Debug.LogError("Animator component missing from this gameobject");
		
		_audio = GetComponent<AudioSource> ();
		if (_audio==null) 
		{
			_audio = gameObject.AddComponent<AudioSource>();
		}
		
		_playerLayer = this.gameObject.layer;
		
		_platformLayer = LayerMask.NameToLayer("Platform");
	}

	
	void Update()
	{
		if (!playerCanMove || (Time.timeScale == 0f))
			return;
		
		#region KeyboardControl

		if (keyboardControl == true)
		{
			_velocityOnXAxis = Input.GetAxisRaw ("Horizontal");
			if (_velocityOnXAxis != 0) 
			{
				_move = true;
			} 
			else 
			{
				_move = false;
			}
			
			if(_isGrounded && Input.GetButtonDown("Jump")) 
			{
				DoJump();
			} 
		
			else if (_canDoubleJump && Input.GetButtonDown("Jump"))
			{
				DoJump();
				_canDoubleJump = false;
			}

			if(Input.GetButtonUp("Jump") && _velocityOnYAxis>0f)
			{
				_velocityOnYAxis = 0f;
			}
		}

		#endregion

		#region TouchControl

		if(keyboardControl == false)
		{
			if (_goRight == true)
			{
				_velocityOnXAxis = 1;
			}
		
			if (_goLeft == true)
			{
				_velocityOnXAxis = -1;
			}

			if (_idle == true)
			{
				_velocityOnXAxis = 0;
			}
			
			
			if(_isGrounded && _jump)
			{
				DoJump();
				_jump = false;
			}
			else if (_canDoubleJump && _jump)
			{
				DoJump();
				_canDoubleJump = false; 
				_jump = false;
			}
			
			if(_jump && _velocityOnYAxis>0f)
			{
				_velocityOnYAxis = 0f;
				_jump = false;
			}
		}

		#endregion
		
		_animator.SetBool("Running", _move);
		
		_velocityOnYAxis = _rigidbody.velocity.y;
		
		_isGrounded = Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround);

		if (_isGrounded)
		{
			_canDoubleJump = true;
		}
		
		_animator.SetBool("Grounded", _isGrounded);
		
		_rigidbody.velocity = new Vector2(_velocityOnXAxis * moveSpeed, _velocityOnYAxis);
		
		Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, (_velocityOnYAxis > 0.0f)); 
	}

	
	void LateUpdate()
	{
		Vector3 localScale = _transform.localScale;

		if (_velocityOnXAxis > 0) 
		{
			_facingRight = true;
		} 
		else if (_velocityOnXAxis < 0) 
		{ 
			_facingRight = false;
		}
		
		if (((_facingRight) && (localScale.x<0)) || ((!_facingRight) && (localScale.x>0))) {
			localScale.x *= -1;
		}
		
		_transform.localScale = localScale;
	}
	
	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag=="MovingPlatform")
		{
			this.transform.parent = other.transform;
		}
	}
	
	void OnCollisionExit2D(Collision2D other)
	{
		if (other.gameObject.tag=="MovingPlatform")
		{
			this.transform.parent = null;
		}
	}

	void DoJump()
	{
		_velocityOnYAxis = 0f;
		_rigidbody.AddForce (new Vector2 (0, jumpForce));
		PlaySound(jumpSFX);
	}
	
	void FreezeMotion() 
	{
		playerCanMove = false;
        _rigidbody.velocity = new Vector2(0,0);
		_rigidbody.isKinematic = true;
	}
	
	void UnFreezeMotion() 
	{
		playerCanMove = true;
		_rigidbody.isKinematic = false;
	}
	
	void PlaySound(AudioClip clip)
	{
		_audio.PlayOneShot(clip);
	}
	
	public void ApplyDamage (int damage) {
		if (playerCanMove) {
			playerHealth -= damage;

			if (playerHealth <= 0) 
			{ 
				PlaySound(deathSFX);
				StartCoroutine (KillPlayer ());
			}
		}
	}
	
	public void FallDeath () {
		if (playerCanMove) {
			playerHealth = 0;
			PlaySound(fallSFX);
			StartCoroutine (KillPlayer ());
		}
	}
	
	IEnumerator KillPlayer()
	{
		if (playerCanMove)
		{
			FreezeMotion();
			
			_animator.SetTrigger("Death");
			
			yield return new WaitForSeconds(2.0f);
			
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	public void CollectCoin(int amount)
	{
		PlaySound(coinSFX);
	}
	
	public void Victory()
	{
		PlaySound(victorySFX);
		FreezeMotion ();
		_animator.SetTrigger("Victory");
		
	}
	
	public void Respawn(Vector3 spawnloc) {
		UnFreezeMotion();
		playerHealth = 1;
		_transform.parent = null;
		_transform.position = spawnloc;
		_animator.SetTrigger("Respawn");
	}
	
	public void EnemyBounce()
	{
		DoJump();
	}
}
