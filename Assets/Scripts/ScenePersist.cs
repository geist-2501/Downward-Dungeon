using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePersist : MonoBehaviour
{



    public int sceneIndex;

    // Use this for initialization
    void Awake()
    {
        DontDestroyOnLoad(this);

        sceneIndex = SceneManager.GetActiveScene().buildIndex;

        ScenePersist[] otherInstances = FindObjectsOfType<ScenePersist>();
        if (otherInstances.Length > 1)
        {
            foreach (ScenePersist i in otherInstances)
            {
                if (i.sceneIndex == sceneIndex && i != this)
                {
                    Destroy(gameObject);
                }
            }
        }

    }

    #region catch level loading
    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelLoad;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelLoad;
    }
    #endregion

    void OnLevelLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != sceneIndex)
        {
            Destroy(gameObject);
        }
    }
}
