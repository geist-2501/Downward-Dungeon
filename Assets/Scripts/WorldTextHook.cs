using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTextHook : MonoBehaviour
{

	private enum Type
	{
		Start,
		Quit
	}

	[Header("Data.")]
	[SerializeField] Type textType;
	private bool isTriggered = false;

	//Cached component refs.
	private GameManager gm;

	private void Start() 
	{
		gm = GameManager.instance;
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
		if (other.gameObject.layer == 10 && !isTriggered)
		{
			isTriggered = true;
			//Debug.Log("Triggered");
			switch (textType)
			{
				case Type.Start:
					StartCoroutine(gm.LoadLevelFade(GameManager.furthestCheckpointProgress, 0));
					AudioManager.instance.Play("Fall");
				break;
				case Type.Quit:
					gm.QuitGame();
				break;
			}
		}
    }
}
