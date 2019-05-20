using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitLevel : MonoBehaviour
{

	//Data.
	private bool hasCollided = false; 

    private void OnTriggerEnter2D(Collider2D other)
    {
		if (other.gameObject.layer == 10 && !hasCollided) //layer 10 = player layer.
		{
			hasCollided = true;
			GameManager.playerScore += 10;
			GameManager.playerLifes++;
			StartCoroutine("ExitLevelDelayed");
		}
    }

	IEnumerator ExitLevelDelayed()
	{
		AudioManager.instance.Play("Open Door");
		yield return new WaitForSeconds(0.13f);
		int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
		if (nextIndex > SceneManager.sceneCountInBuildSettings - 1)
		{
			Debug.LogWarning("No more scenes to load! Reverting to main menu.");
			nextIndex = 0;
		}
		StartCoroutine(GameManager.instance.LoadLevelFade(nextIndex, 0.13f));
	}
}
