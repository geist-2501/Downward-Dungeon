using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionWorldHook : MonoBehaviour
{
    public bool optionOn = false;

    [SerializeField] KeyBlock on;
    [SerializeField] KeyBlock off;
    [SerializeField] float cooldown = 0.2f;

    float cooldownStartTime;


    bool switching = false;
    bool startCooldown = false;
    SpriteRenderer ren;
    AudioManager audMan;

    private void Start() 
    {
        ren = GetComponent<SpriteRenderer>();
        audMan = AudioManager.instance;

        //Set up switch.
        if (optionOn)
        {
            //Show on.
            on.Hide();
            off.Show();
        }
        else
        {
            //Show off.
            on.Show();
            off.Hide();
        }
    }

    private void Update() 
    {
        if (switching && startCooldown)
        {
            if (Time.time - cooldownStartTime > cooldown)
            {
                //Re-activate key.
                ren.enabled = true;
                switching = false;
                startCooldown = false;
            }
        }    
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        //Stops rapid switching.
        if (switching) { return; }

		audMan.Play("Pickup");

        switching = true;

        optionOn = !optionOn;

        if (optionOn)
        {
            //Show on.
            audMan.Play("Destroy");
            on.Hide();
            off.Show();
        }
        else
        {
            //Show off.
            audMan.Play("Destroy");
            on.Show();
            off.Hide();
        }
        
        ren.enabled = false;


    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        //start timer.
        startCooldown = true;
        cooldownStartTime = Time.time;
    }
}
