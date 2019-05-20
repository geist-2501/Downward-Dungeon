using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteProgress : MonoBehaviour
{

    [SerializeField] float timeToHoldFor = 3f;
    [SerializeField] string noProgText;

    GameManager gm;
    Transform bar;
    TextMesh text;
    string origText;

    bool escHoldFlag;

    bool noProgress = false;

    float fullScale;

    float startTime;
    float endTime;
    float currentTime;

    // Use this for initialization
    void Start()
    {
        text = GetComponent<TextMesh>();
        origText = text.text;
        bar = transform.GetChild(0);
        gm = GameManager.instance;

        if (GameManager.furthestCheckpointProgress == 2 && GameManager.playerHighScore == 0)
        {
            noProgress = true;
            text.text = noProgText;
        }

        fullScale = bar.localScale.x;

        Vector3 newScale = new Vector3(0, bar.localScale.y, bar.localScale.z);
        bar.localScale = newScale;
    }

    // Update is called once per frame
    void Update()
    {

        if (!escHoldFlag && Input.GetButton("Cancel"))
        {
            startTime = Time.time;
            endTime = startTime + timeToHoldFor;
            currentTime = startTime;
        }

        escHoldFlag = Input.GetButton("Cancel");

        Vector3 newScale;

        if (escHoldFlag && !noProgress)
        {
            if (currentTime >= endTime)
            {
                GameManager.DeleteProgress();
                noProgress = true;
                StartCoroutine(PrintProgressDeleted());
            }
            currentTime = Time.time;
            float a = (currentTime - startTime) / (endTime - startTime) * fullScale;
            newScale = new Vector3(a, bar.localScale.y, bar.localScale.z);
        }
        else
        {
            newScale = new Vector3(0, bar.localScale.y, bar.localScale.z);
        }

        bar.localScale = newScale;
    }

    private IEnumerator PrintProgressDeleted()
    {
        string newText = "PROGRESS DELETED";

        text.text = "";
        foreach (char letter in newText)
        {
            text.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1f);

        text.text = "";
        foreach (char letter in noProgText)
        {
            text.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
