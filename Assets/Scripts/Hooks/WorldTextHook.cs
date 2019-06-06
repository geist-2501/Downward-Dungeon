using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTextHook : MonoBehaviour
{

    private enum Type
    {
        Start,
        Quit,
        ToOptions,
        ToMain
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
                case Type.ToOptions:
                    StartCoroutine(gm.LoadLevelFade(2, 0.5f)); //Load options menu.
                    AudioManager.instance.Play("Open Door");
                    break;
                case Type.ToMain:
                    StartCoroutine(gm.LoadLevelFade(1, 0.5f)); //Load main menu.
                    OptionManager opt = FindObjectOfType<OptionManager>();
                    opt.UpdateSettings();
                    AudioManager.instance.Play("Open Door");
                    break;
            }
        }
    }
}
