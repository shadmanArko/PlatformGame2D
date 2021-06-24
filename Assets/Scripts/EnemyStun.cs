using UnityEngine;

public class EnemyStun : MonoBehaviour {
	
	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			this.GetComponentInParent<Enemy>().Stunned();
			
			other.gameObject.GetComponent<CharacterController2D>().EnemyBounce();
		}
	}
}
