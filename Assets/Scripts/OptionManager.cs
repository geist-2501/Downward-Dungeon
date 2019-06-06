using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionManager : MonoBehaviour
{
    [Header("Option Keys.")] 
    [SerializeField] OptionWorldHook easyModeKey;

    GameManager gm;

    private void Start() 
    {
        gm = GameManager.instance;    

        //Reset all settings.

        easyModeKey.optionOn = gm.optEasyMode;
    }

    public void UpdateSettings()
    {
        gm.optEasyMode = easyModeKey.optionOn;
        gm.SetOptionsToPrefs();
    }
}
