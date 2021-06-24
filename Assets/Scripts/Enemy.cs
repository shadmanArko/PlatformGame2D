using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {
	[Range(0f, 10f)]
	public float moveSpeed = 4f; 
	
	public int damageAmount = 10; 

	public GameObject stunnedCheck;

	public float stunnedTime = 3f;
	
	public string stunnedLayer = "StunnedEnemy";
	public string playerLayer = "Player";
	
	[HideInInspector]
	public bool isStunned = false; 
	
	public GameObject[] myWaypoints; 
	
	public float waitAtWaypointTime = 1f;   
	
	public bool loopWaypoints = true; 
	
	
	public AudioClip stunnedSFX;
	public AudioClip attackSFX;

	Transform _transform;
	Rigidbody2D _rigidbody;
	Animator _animator;
	AudioSource _audio;

	int _myWaypointIndex = 0; 
	float _moveTime; 
	float _vx = 0f;
	bool _moving = true;
	
	
	int _enemyLayer;

	
	int _stunnedLayer;
	
	void Awake() 
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

		if (stunnedCheck==null) {
			Debug.LogError("stunnedCheck child gameobject needs to be setup on the enemy");
		}
		
		_moveTime = 0f;
		_moving = true;
		
		_enemyLayer = this.gameObject.layer;
		
		_stunnedLayer = LayerMask.NameToLayer(stunnedLayer);
		
	}
	
	void Update () 
	{
		if (!isStunned)
		{
			if (Time.time >= _moveTime) {
				EnemyMovement();
			} else {
				_animator.SetBool("Moving", false);
			}
		}
	}
	
	void EnemyMovement() 
	{
		if ((myWaypoints.Length != 0) && (_moving)) {
			
			Flip (_vx);
			
			_vx = myWaypoints[_myWaypointIndex].transform.position.x-_transform.position.x;
			
			if (Mathf.Abs(_vx) <= 0.05f) {
				_rigidbody.velocity = new Vector2(0, 0);
				
				_myWaypointIndex++;
				
				if(_myWaypointIndex >= myWaypoints.Length) {
					if (loopWaypoints)
						_myWaypointIndex = 0;
					else
						_moving = false;
				}
				
				_moveTime = Time.time + waitAtWaypointTime;
			} else {
				
				_animator.SetBool("Moving", true);
				
				_rigidbody.velocity = new Vector2(_transform.localScale.x * moveSpeed, _rigidbody.velocity.y);
			}
			
		}
	}
	
	void Flip(float _vx) {
		
		Vector3 localScale = _transform.localScale;
		
		if ((_vx>0f)&&(localScale.x<0f))
			localScale.x*=-1;
		else if ((_vx<0f)&&(localScale.x>0f))
			localScale.x*=-1;
		
		_transform.localScale = localScale;
	}
	
	void OnTriggerEnter2D(Collider2D collision)
	{
		if ((collision.tag == "Player") && !isStunned)
		{
			CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
			if (player.playerCanMove) {
				Flip(collision.transform.position.x-_transform.position.x);
				
				playSound(attackSFX);
				
				_rigidbody.velocity = new Vector2(0, 0);
				
				player.ApplyDamage (damageAmount);
				
				_moveTime = Time.time + stunnedTime;
			}
		}
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
	
	void playSound(AudioClip clip)
	{
		_audio.PlayOneShot(clip);
	}
	
	public void Stunned()
	{
		if (!isStunned) 
		{
			isStunned = true;
			
			playSound(stunnedSFX);
			_animator.SetTrigger("Stunned");
			
			_rigidbody.velocity = new Vector2(0, 0);
			
			this.gameObject.layer = _stunnedLayer;
			stunnedCheck.layer = _stunnedLayer;
			
			StartCoroutine (Stand ());
		}
	}
	
	IEnumerator Stand()
	{
		yield return new WaitForSeconds(stunnedTime); 
		
		isStunned = false;
		
		this.gameObject.layer = _enemyLayer;
		stunnedCheck.layer = _enemyLayer;
		
		_animator.SetTrigger("Stand");
	}
}
