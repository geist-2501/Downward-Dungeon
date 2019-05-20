using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Static Data.")]
    public static int playerLifes = 3;
    public static int playerScore = 0;
    public static int playerHighScore;
    public static Player playerInstance;
    public static int furthestCheckpointProgress = 0;
    public static GameManager instance;

    [Header("Level Management.")]
    [SerializeField] float fadeOutTime;
    [SerializeField] float fadeInTime;
    [SerializeField] float splashScreenWaitTime;

    [Header("Settings")]
    [SerializeField] AudioManagerSettings amSettings;


    [Header("Data")]
    [SerializeField] [TextArea(1, 3)] string gameOverText;
    [SerializeField] GameObject deathScreenPrefab;
    private GameObject deathScreenInstance;
    private Text livesText;
    private RectTransform deathText;
    private Image fadePanel;

    private const string SCORE_KEY = "highscore";
    private const string CHECKPOINT_KEY = "checkpoint";

    private Text scoreText;
    private Image scorePanel;

    private AudioManager am;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (deathScreenInstance == null)
        {
            deathScreenInstance = Instantiate(deathScreenPrefab, Vector3.zero, Quaternion.identity, transform);
            deathText = deathScreenInstance.transform.GetChild(1) as RectTransform;
            livesText = deathText.transform.Find("Lives").GetComponent<Text>();
            fadePanel = deathScreenInstance.transform.Find("Panel").GetComponent<Image>();
            scorePanel = deathScreenInstance.transform.GetChild(2).GetComponent<Image>();
            scoreText = scorePanel.GetComponentInChildren<Text>();
        }

        //Initialize player prefs.
        playerHighScore = InitializePref(SCORE_KEY);
        furthestCheckpointProgress = InitializePref(CHECKPOINT_KEY);
        //Set the furthest checkpoint to not be the splash or main menu!
        if (furthestCheckpointProgress < 2) { furthestCheckpointProgress = 2; }
    }

    //Init prefs.
    private int InitializePref(string _key)
    {
        int i;

        if (!PlayerPrefs.HasKey(_key))
        {
            Debug.Log("Initializing player prefs for " + _key);
            PlayerPrefs.SetInt(_key, 0);
            i = 0;
        }
        else
        {
            i = PlayerPrefs.GetInt(_key);
        }

        return i;
    }

    private void Update()
    {

        string scoreString = "";
        string highScoreString = "";

        for (int i = 0; i < 4 - playerScore.ToString().Length; i++)
        {
            scoreString += "0";
        }

        for (int i = 0; i < 4 - playerHighScore.ToString().Length; i++)
        {
            highScoreString += "0";
        }

        if (playerScore > playerHighScore)
        {
            playerHighScore = playerScore;
        }

        scoreText.text = scoreString + playerScore.ToString() + "\n" + highScoreString + playerHighScore.ToString();
    }


    #region catch level loading
    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
    #endregion


    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loaded level!");

        //Create and set-up a new instance of the audio manager.
        GameObject amGameObj;
        amGameObj = new GameObject("AudioManager");
        am = amGameObj.AddComponent<AudioManager>();
        AudioManager.instance = am;
        am.sounds = amSettings.sounds;

        playerInstance = FindObjectOfType<Player>();
        if (!playerInstance)
        {
            Debug.LogWarning("Couldn't find player!");
        }

        am.RebuildAudio();

        if (scene.buildIndex == 0 || scene.buildIndex == 1) //Dont show score on splash and main.
        {
            scorePanel.gameObject.SetActive(false);
        } 
        else
        {
            scorePanel.gameObject.SetActive(true);
        }

        if (SceneManager.GetActiveScene().buildIndex == 0) //Splash.
        {
            StartCoroutine(LoadOutOfSplash());
        }
    }


    private void Start()
    {
        am = AudioManager.instance;
    }


    public void BasicLoad(int _levelIndex)
    {
        if (_levelIndex == -1)  //Invalid so load next level.
        {
            int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextLevelIndex > SceneManager.sceneCountInBuildSettings - 1)
            {
                Debug.LogWarning("No more scenes to load! Loading index 0!");
                SceneManager.LoadScene(0);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
        else
        {
            if (_levelIndex > SceneManager.sceneCountInBuildSettings - 1)
            {
                Debug.LogWarning("Attempted to load invalid scene! Loading index 0!");
                SceneManager.LoadScene(0);
            }
            else
            {
                SceneManager.LoadScene(_levelIndex);
            }
        }
    }


    public void QuitGame()
    {
        Application.Quit();
    }


    public IEnumerator LoadLevelFade(int _levelIndex, float _time)
    {

        float customFadeOutTime = (_time == 0) ? fadeOutTime : _time;

        //Fade out.
        float startTime = Time.time;
        float endTime = startTime + customFadeOutTime;
        float currentTime = startTime;
        fadePanel.gameObject.SetActive(true);

        while (currentTime <= endTime)
        {
            currentTime = Time.time;
            float a = (currentTime - startTime) / (endTime - startTime);
            fadePanel.color = new Color(0, 0, 0, a);

            yield return null;
        }

        BasicLoad(_levelIndex);

        StartCoroutine(FadeIn());
    }



    public IEnumerator ReloadLevelDeath()
    {
        yield return new WaitForSeconds(0.8f);
        am.Play("Beep");

        livesText.text = (playerLifes + 1) + " LIVES LEFT";
        fadePanel.gameObject.SetActive(true);
        scorePanel.gameObject.SetActive(false);
        fadePanel.color = Color.black;
        deathText.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.8f);
        am.Play("Crunch");

        string pluralLivesLeft = (playerLifes == 1) ? " LIFE  LEFT" : " LIVES LEFT";

        livesText.text = (playerLifes) + pluralLivesLeft;

        yield return new WaitForSeconds(0.8f);
        BasicLoad(SceneManager.GetActiveScene().buildIndex);
        deathText.gameObject.SetActive(false);
        StartCoroutine(FadeIn());

    }

    public static void UpdateCheckpoint(int _sceneIndex)
    {
        if (_sceneIndex > furthestCheckpointProgress)
        {
            furthestCheckpointProgress = _sceneIndex;
            PlayerPrefs.SetInt(CHECKPOINT_KEY, furthestCheckpointProgress);
        }
    }

    public static void DeleteProgress()
    {
        PlayerPrefs.SetInt(CHECKPOINT_KEY, 2);
        furthestCheckpointProgress = 2;
        PlayerPrefs.SetInt(SCORE_KEY, 0);
        playerHighScore = 0;
    }

    public IEnumerator EndGame()
    {

        PlayerPrefs.SetInt(SCORE_KEY, playerHighScore);
        playerScore = 0;

        yield return new WaitForSeconds(0.8f);
        am.Play("Beep");

        livesText.text = "";
        scorePanel.gameObject.SetActive(false);
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = Color.black;
        deathText.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        foreach (char letter in gameOverText)
        {
            livesText.text += letter;
            am.Play("Beep 2");
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.4f);
        BasicLoad(1);
        deathText.gameObject.SetActive(false);
        StartCoroutine(FadeIn());

        playerLifes = 3;
        Player.skillHate = false;
    }


    private IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);

        float startTime = Time.time;
        float endTime = startTime + fadeInTime;
        float currentTime = startTime;

        while (currentTime <= endTime)
        {
            currentTime = Time.time;
            float a = 1 - (currentTime - startTime) / (endTime - startTime);
            //Debug.Log(a + ", ct:" + currentTime + ", st:" + startTime + ", et:" + endTime);
            fadePanel.color = new Color(0, 0, 0, a);

            yield return null;
        }

        //fadePanel.gameObject.SetActive(false);
    }


    public void ProcessPlayerDeath()
    {
        playerLifes--;
        if (playerLifes <= 0)
        {
            StartCoroutine(EndGame());
        }
        else
        {
            StartCoroutine(ReloadLevelDeath());
        }
    }


    IEnumerator LoadOutOfSplash()
    {
        StartCoroutine(FadeIn());
        am.Play("Open Door");
        yield return new WaitForSeconds(fadeInTime + splashScreenWaitTime);

        fadePanel.gameObject.SetActive(true);
        StartCoroutine(LoadLevelFade(1, 1f));
    }
}
