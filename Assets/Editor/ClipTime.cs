using System.Collections;
using UnityEngine;
using UnityEditor;

public class ClipTime : ScriptableWizard 
{

	public AnimationClip animationClip;
	public float animLength = 0f;
	public AudioClip soundClip;
	public float soundLength = 0f;

	[MenuItem("G2 Tools/Get Clip Length")]
	static void GetClipTime()
	{
		ScriptableWizard.DisplayWizard<ClipTime>("Get Clip Length", "Close");
	}

	void OnWizardCreate()
	{

	}

	void OnWizardUpdate()
	{
		if (soundClip)
		{
			soundLength = soundClip.length;
		}

		if (animationClip)
		{
			animLength = animationClip.length;
		}
	}

}
