using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FootStepAnimHook : MonoBehaviour
{

    [SerializeField] float maxPitch;
    [SerializeField] float minPitch;

    private AudioSource aud;
    private Player player;

    // Use this for initialization
    void Start()
    {
        aud = GetComponent<AudioSource>();
        player = GetComponentInParent<Player>();

        if (minPitch > maxPitch)
        {
            Debug.LogError("Min pitch is more than max pitch!");
        }
    }

    public void Footstep()
    {
        if (player.isGrounded)
        {
            aud.pitch = Random.Range(minPitch, maxPitch);
            aud.Play();
        }

    }
}
