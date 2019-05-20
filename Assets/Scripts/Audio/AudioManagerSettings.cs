using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Audio Settings", menuName = "Audio")]
public class AudioManagerSettings : ScriptableObject 
{
	public Audio[] sounds;
}
