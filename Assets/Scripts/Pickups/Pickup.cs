using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class Pickup : MonoBehaviour
{

    private Image background;
    private Text infoText;

    //Options.
    public bool optDestroyAfterUse = false;    	//Destroy gameobj after message displayed.
    public bool optHideAfterUse = false;       	//Hide visuals after message displayed.
	public bool optShowMessagePopup = true;		//Whether or not to display the poppupInfo.

    private Collider2D col;
    private SpriteRenderer render;

    [SerializeField] [TextArea(1, 3)] string popupInfo;

    private void Start()
    {
        background = GetComponentInChildren<Image>();
        infoText = background.GetComponentInChildren<Text>();
        background.gameObject.SetActive(false);
        col = GetComponent<Collider2D>();
        render = GetComponent<SpriteRenderer>();
    }

    public abstract void PickupEffects(Player _player);

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();



        if (player)
        {
            PickupEffects(player);

            if (optHideAfterUse)
            {
                col.enabled = false;
                render.enabled = false;
            }

            if (optShowMessagePopup) { StartCoroutine(DisplayInfo(player)); }
        }
    }

    private IEnumerator DisplayInfo(Player _player)
    {
        _player.isParalysed = true;
        background.gameObject.SetActive(true);
        foreach (char letter in popupInfo)
        {
            infoText.text += letter;
            AudioManager.instance.Play("Beep 2");
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(2f);
        background.gameObject.SetActive(false);
        _player.isParalysed = false;
        if (optDestroyAfterUse) { Destroy(gameObject); }
    }

}
