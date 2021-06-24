using UnityEngine;

public class Coin : MonoBehaviour {

	public int coinValue = 1;
	public bool taken = false;
	public GameObject explosion;
	
	void OnTriggerEnter2D (Collider2D other)
	{
		if ((other.tag == "Player" ) && (!taken) && (other.gameObject.GetComponent<CharacterController2D>().playerCanMove))
		{
			taken=true;
			
			if (explosion)
			{
				Instantiate(explosion,transform.position, transform.rotation);
			}
			
			other.gameObject.GetComponent<CharacterController2D>().CollectCoin(coinValue);
			
			Destroy(this.gameObject);
		}
	}

}
