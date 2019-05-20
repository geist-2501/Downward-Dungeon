using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Audio {

	//Data.
	public string name;
	public AudioClip clip;
	[Range(0, 1)] public float volume;
	[Range(0.1f, 3f)] public float pitch;
	public AudioSource src;
	
}
