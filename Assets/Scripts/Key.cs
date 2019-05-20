using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour {

	[Header("Objects")]
	[SerializeField] KeyBlock[] blocks;

	//Data.
	bool isActivated = false;

	//Cached component refs.
	private Collider2D col;
	private SpriteRenderer sprite;

	private AudioManager audMan;

	private void Start()
	{
		audMan = AudioManager.instance;

		col = GetComponent<Collider2D>();
		sprite = GetComponentInChildren<SpriteRenderer>();

		if (blocks.Length == 0)
		{
			Debug.LogError("Key " + gameObject.name + " has no objects to unlock!");
		}	
	}

	private void OnTriggerEnter2D(Collider2D other) 
	{
		if (other.gameObject.tag == "Player" && !isActivated)
		{
			/*
			Technically, isActivated isn't needed as the collider is turned off
			and hence no more OnTriggerEnter2Ds will get called, but I left it
			for clarity.
			*/
			isActivated = true;
			col.enabled = false;
			sprite.enabled = false;

			audMan.Play("Pickup");

			StartCoroutine("Unlock");
		}	
	}

	private IEnumerator Unlock()
	{
		foreach (KeyBlock block in blocks)
		{
			audMan.Play("Destroy");
			block.UnlockAndDestroy();
			yield return new WaitForSeconds(0.2f);
		}
	}
}
