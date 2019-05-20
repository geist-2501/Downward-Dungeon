using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float movementSpeed = 1f;
    [SerializeField] float edgeDetectionDistance = 1f;
    [SerializeField] float deathKickback = 1f;
	[SerializeField] float gravScale = 3f;
	[SerializeField] float deathWaitTime = 2f;
	[SerializeField] float deathFadeTime = 1f;
	

    //States.
    [HideInInspector] public bool isAlive = true;

    [Header("Objects")]
    [SerializeField] GameObject targetSpriteGameObj;

    //Data.
    private int direction = 1; //1 is right, -1 is left.

    //Cached component refs.
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer targetSprite;

    private LayerMask groundLayerMask;
    private LayerMask playerLayerMask;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = targetSpriteGameObj.GetComponent<Animator>();
        targetSprite = targetSpriteGameObj.GetComponent<SpriteRenderer>();

        groundLayerMask = LayerMask.GetMask("Ground") | LayerMask.GetMask("Phaseable");
        playerLayerMask = LayerMask.GetMask("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive) { return; }

        rb.velocity = Vector2.right * movementSpeed * direction;
        Vector2 position2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 offset = new Vector2(edgeDetectionDistance * direction, 0.4f);

        if (!Physics2D.Raycast(offset + position2D, -Vector2.up, 0.6f, groundLayerMask))
        {
            //I.e if there's no ground ahead, switch direction.
            Flip();
        }

        offset = new Vector2(0, 0.25f); //Just re-use offset again.

        if (Physics2D.Raycast(offset + position2D, Vector2.right * direction, 0.6f, playerLayerMask))
        {
            if (!GameManager.playerInstance.isAlive)
            {
				//If there is a dead player infront switch direction.
				Flip();
            }
        }
    }

    private void Flip()
    {
        direction *= -1; //Flip sign.
        targetSpriteGameObj.transform.localScale = new Vector3(direction, 1, 1);
    }

    public void Die(bool _throw, Transform _source)
    {
        isAlive = false;

        AudioManager.instance.Play("Hurt");

        if (_throw)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
			rb.gravityScale = gravScale;
            Vector3 kickback = Vector3.Normalize(transform.position - _source.position) * deathKickback;
            rb.AddForce(kickback, ForceMode2D.Impulse);
        }

        anim.SetBool("isAlive", isAlive);

        StartCoroutine(FadeDestroy(deathWaitTime, deathFadeTime));
    }

    private IEnumerator FadeDestroy(float _waitTime, float _fadeTime)
    {

        yield return new WaitForSeconds(_waitTime);


        float startTime = Time.time;
        float endTime = startTime + _fadeTime;
        float currentTime = startTime;

        while (currentTime <= endTime)
        {
            currentTime = Time.time;
            float a = 1 - (currentTime - startTime) / (endTime - startTime);
            targetSprite.color = new Color(1, 1, 1, a);

            yield return null;
        }

        Destroy(gameObject);

    }
}
