using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	[Header("Data")]
	public Audio[] sounds;

	public static AudioManager instance;

	//Footsteps are handled by the player object! (FootStepAnimHook.cs).

	public void RebuildAudio()
	{
		AudioSource[] audSources = GetComponents<AudioSource>();
		foreach (AudioSource audSource in audSources)
		{
			Destroy(audSource);
		}

		foreach (Audio s in sounds)
		{
			s.src = null;

			s.src = gameObject.AddComponent<AudioSource>();
			s.src.clip = s.clip;
			s.src.volume = s.volume;
			s.src.pitch = s.pitch;
		}

		Debug.Log("Audio Rebuilt");
	}

	public void Play(string _name)
	{
		Audio s = Array.Find(sounds, sound => sound.name == _name);
		s.src.Play();
	}

	public void ChangePitch(string _name, float _newPitch)
	{
		if (_newPitch > 3 || _newPitch < -3) 
		{
			Debug.LogWarning("Pitch value out of range, ignoreing change!");
			return;
		}

		Audio s = Array.Find(sounds, sound => sound.name == _name);
		s.src.pitch = _newPitch;
	}

	public void ChangeVol(String _name, float _newVol)
	{
		if (_newVol > 1 || _newVol < 0) 
		{
			Debug.LogWarning("Volume value out of range, ignoreing change!");
			return;
		}

		Audio s = Array.Find(sounds, sound => sound.name == _name);
		//Debug.Log("Changing volume of " + s.name + " to " + _newVol);
		s.src.volume = _newVol;
	}
}
