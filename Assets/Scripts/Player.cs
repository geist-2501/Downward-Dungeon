using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float movementSpeed = 11f;
    [SerializeField] float climbSpeedv = 8f;
    [SerializeField] float climbSpeedh = 8f;

    [SerializeField] float jumpForce = 7f;
    [Range(0.0f, 1.0f)] [SerializeField] float jumpCutHieght = 0.5f;
    
    [SerializeField] float deathKickback = 5f;
    [SerializeField] float jumpVolDivisionFactor = 3f;
    [SerializeField] AnimationCurve attackMotion; //Sets velocity and time for an attack
    [SerializeField] float attackRecovery = 0.5f;
    [SerializeField] Color normalColour;
    [SerializeField] Color attackColour;
    [SerializeField] float maxAirbornDuration;
    [SerializeField] float jumpGravScale = 0.8f;
    [SerializeField] float jumpMaxPitch = 0.7f;
    [SerializeField] float jumpMinPitch = 0.5f;
    [SerializeField] float joystickDead = 0.3f;

    // Jump timer.
    float jumpPressedRemember = 0f;
    [Range(0.0f, 1.0f)] [SerializeField] float jumpPressedRememberTime = 0.2f;

    // IsGrounded timer.
    float isGroundedRemember = 0f;
    [Range(0.0f, 1.0f)] [SerializeField] float isGroundedRememberTime = 0.1f;

    [Header("Objects")]
    [SerializeField] GameObject targetSpriteGameObj;
    [SerializeField] Collider2D feetCol;
    [SerializeField] Collider2D headCol;
    [SerializeField] Collider2D detectorCol;
    [SerializeField] PhysicsMaterial2D physicsMat;
    [SerializeField] GameObject mobileInputLayer;

    [Space(5f)]
    [Header("Settings")]

    //Data.
    Vector2 playerInput;
    Vector2 playerInputRaw;
    bool jumpFlag;
    bool jumpHoldFlag;
    bool jumpReleaseFlag;
    bool escFlag;
    bool attackFlag;
    bool direction; // -1 left, 1 right.
    float currentAirbornTime = 0f;

    bool joystickJumpFlipFlop;

    float d; // Delta time.

    //States.
    bool isClimbing = false;
    bool jumpOffClimbingFlag = false;
    public bool isAlive = true;
    public bool isParalysed = false; //isParalysed is a less severe version of disabling isAlive. 
    bool isPhasing = false;
    public bool isGrounded = false;
    bool isBusy = false;
    bool isAttacking = false;
    /* 
    The difference between isBusy and isAttacking is that 
    isAttacking is used for the attack window, whereas isBusy
    is used to prevent input during the attack and its recovery 
    */

    //Skills.
    public static bool skillHate = false;


    //Cached component refs.
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D bodyCol;
    private SpriteRenderer targetSprite;


    private LayerMask groundLayerMask;
    private LayerMask climbableLayerMask;
    private LayerMask phaseableLayerMask;
    private LayerMask hazardLayerMask;

    private float gravScale;

    private AudioManager am;
    private GameManager gm;

    //Messages then methods.
    void Start()
    {
        gm = GameManager.instance;
        am = AudioManager.instance;

        rb = GetComponent<Rigidbody2D>();
        anim = targetSpriteGameObj.GetComponent<Animator>();
        bodyCol = GetComponent<Collider2D>();
        targetSprite = targetSpriteGameObj.GetComponent<SpriteRenderer>();

        //Col reset required after changing friction.
        physicsMat.friction = 0f;
        bodyCol.enabled = false;
        bodyCol.enabled = true;

        groundLayerMask = LayerMask.GetMask("Ground");
        climbableLayerMask = LayerMask.GetMask("Climbable");
        phaseableLayerMask = LayerMask.GetMask("Phaseable");
        hazardLayerMask = LayerMask.GetMask("Hostile");

        gravScale = rb.gravityScale;

    }


    void Update()
    {
        //Cache delta time.
        d = Time.deltaTime;

        if (bodyCol.IsTouchingLayers(hazardLayerMask) && isAlive) { Die(null, false); } //Environmental death.
        //Death by enemy is handled by OnCollisionEnter2D().

        DetectInput();

        if (!isAlive) { return; }

        if (isParalysed)
        {
            playerInput = Vector3.zero;
            playerInputRaw = Vector3.zero;
            jumpFlag = false;
            escFlag = false;
            attackFlag = false;
        }

        Debug.DrawRay(targetSpriteGameObj.transform.position, rb.velocity);

        if (isBusy) { return; }


        DetectClimbing();
        DetectPhasing();

        if (skillHate) { DetectAttack(); }

        if (isBusy) { return; }

        Move();
        Jump();

        HandleMisc(); //Quitting and stuff.

    }



    /// <summary>
    /// Gets current input from player.
    /// </summary>
    private void DetectInput()
    {
        float h;
        float v;
        float hRaw;
        float vRaw;
        
        //TODO: Support controllers.

        //Get dampened axis.
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        //Get raw axis.
        hRaw = Input.GetAxisRaw("Horizontal");
        vRaw = Input.GetAxisRaw("Vertical");

        //Combine into control vectors.
        playerInput = new Vector2(h, v);
        playerInputRaw = new Vector2(hRaw, vRaw);

        //Get flags.
        jumpFlag = Input.GetButtonDown("Jump");
        jumpHoldFlag = Input.GetButton("Jump");
        jumpReleaseFlag = Input.GetButtonUp("Jump");

        escFlag = Input.GetButtonDown("Cancel");
        attackFlag = Input.GetButtonDown("Fire1");
    }



    /// <summary>
    /// Read the players actions in regard to climbing.
    /// </summary>
    private void DetectClimbing()
    {
        //Is the player trying to climb on a climbable surface?
        if (playerInputRaw.y > 0.9f && detectorCol.IsTouchingLayers(climbableLayerMask))
        {
            rb.gravityScale = 0;
            isClimbing = true;
        }

        //Is the player trying to dismount.
        if (!detectorCol.IsTouchingLayers(climbableLayerMask) || jumpFlag)
        {
            if (jumpFlag && isClimbing) { jumpOffClimbingFlag = true; }
            rb.gravityScale = gravScale;
            isClimbing = false;
        }

        anim.SetBool("isClimbing", isClimbing);
    }



    /// <summary>
    /// Detect if the player is trying to phase.
    /// </summary>
    private void DetectPhasing()
    {
        //Is the player trying to phase through a phasable surface.
        if (playerInputRaw.y < -0.9f && jumpFlag && feetCol.IsTouchingLayers(phaseableLayerMask))
        {
            jumpFlag = false;
            isPhasing = true;
        }
        else if (headCol.IsTouchingLayers(phaseableLayerMask)) //Else if jumping through a phaseable layer.
        {
            //TODO: Replace with raycasts.
            isPhasing = true;
        }

        if (isPhasing && detectorCol.IsTouchingLayers(phaseableLayerMask))
        {
            //keep phasing.
        }
        else if (isPhasing && !(detectorCol.IsTouchingLayers(phaseableLayerMask) || headCol.IsTouchingLayers(phaseableLayerMask)))
        {
            isPhasing = false;
        }

        Physics2D.IgnoreLayerCollision(10, 12, isPhasing);
    }



    /// <summary>
    /// Detects if the player is attacking.
    /// </summary>
    private void DetectAttack()
    {
        if (attackFlag)
        {
            am.Play("Attack");
            rb.velocity += Vector2.right * 2;
            isBusy = true;
            attackFlag = false;
            StartCoroutine(ExecuteAttack());
        }
    }



    /// <summary>
    /// Handle jumping.
    /// </summary>
    private void Jump()
    {
        //Play a sound everytime the player lands.
        if (!isGrounded && (feetCol.IsTouchingLayers(groundLayerMask) || feetCol.IsTouchingLayers(phaseableLayerMask)))
        {
            am.ChangeVol("Land Light", Mathf.Clamp(Mathf.Abs(rb.velocity.y) / jumpVolDivisionFactor, 0.5f, 1f));
            am.Play("Land Light");
        }

        isGrounded = feetCol.IsTouchingLayers(groundLayerMask) || feetCol.IsTouchingLayers(phaseableLayerMask);

        anim.SetBool("isGrounded", isGrounded);

        // These timers give a little give with the responsiveness of the character.
        // I.e. if the player tries to jump a split second too late, they can still jump. It
        // just feels better.

        // Deal with jump timer.
        jumpPressedRemember -= d;
        if (jumpFlag) { jumpPressedRemember = jumpPressedRememberTime; }

        // Deal with isGrounded timer.
        isGroundedRemember -= d;
        if (isGrounded) { isGroundedRemember = isGroundedRememberTime; }

        // Handle a jump.
        if ((isGroundedRemember > 0 || jumpOffClimbingFlag) && (jumpPressedRemember > 0))
        {
            //Reset timers.
            isGroundedRemember = 0;
            jumpPressedRemember = 0;

            jumpOffClimbingFlag = false;
            am.ChangePitch("Jump", Random.Range(jumpMinPitch, jumpMaxPitch));
            am.Play("Jump");
            Vector2 jumpVel = new Vector2(rb.velocity.x, jumpForce);
            rb.velocity = jumpVel;
            currentAirbornTime = 0;
        }

        if (isClimbing) { return; } //Stop the following statement changing grav scale when climbing.

        // Let the amount of time the spacebar is held for translate to the airtime. 
        if (jumpReleaseFlag)
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutHieght);
            }
        }

        jumpFlag = false;


    }



    /// <summary>
    /// Handle movement.
    /// </summary>
    private void Move()
    {
        Vector2 playerVel;

        if (isClimbing)
        {
            playerVel = new Vector2(playerInput.x * climbSpeedh, playerInputRaw.y * climbSpeedv);
        }
        else
        {

            playerVel = new Vector2(playerInput.x * movementSpeed, rb.velocity.y);            
            

            //Make sure the sprite is always facing forwards in the event of no input.
            if (playerInput.x != 0)
            {
                targetSpriteGameObj.transform.localScale = new Vector3(Mathf.Sign(playerInput.x), 1, 1);
            }
        }

        if (Mathf.Sign(targetSpriteGameObj.transform.localScale.x) == 1)
        {
            direction = true;
        }
        else
        {
            direction = false;
        }

        anim.SetBool("isRunning", playerVel.magnitude > 0);

        rb.velocity = playerVel;
    }


    private void HandleMisc()
    {
        if (escFlag && SceneManager.GetActiveScene().buildIndex != 1) //Return to main.
        {
            GameManager.playerScore = 0;
            escFlag = false;
            StartCoroutine(gm.LoadLevelFade(1, 0.4f));
        }
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();

        if (enemy)
        {
            if (isAttacking && enemy.isAlive)
            {
                enemy.Die(true, transform);
            }
            else if (enemy.isAlive && this.isAlive)
            {
                Die(other.gameObject.transform, true);
            }
        }
    }

    private void Die(Transform _source, bool _throw)
    {
        if (GameManager.playerLifes == 1)
        {
            am.Play("Big Hurt");
        }
        else
        {
            am.Play("Hurt");
        }

        isAlive = false;

        if (_throw)
        {
            Vector3 _kickback = Vector3.Normalize(transform.position - _source.position) * deathKickback;
            rb.AddForce(_kickback, ForceMode2D.Impulse);
        }

        physicsMat.friction = 1f;

        //I hate this but this is the only way to update physics settings.
        bodyCol.enabled = false;
        bodyCol.enabled = true;

        anim.SetBool("isAlive", isAlive);

        gm.ProcessPlayerDeath();
    }

    private IEnumerator ExecuteAttack()
    {
        isAttacking = true;

        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("isRunning", false); //So the player doesn't exit straight into a run anim.

        //Change colour to red.
        float startTime = Time.time;
        float endTime = startTime + attackMotion.keys[attackMotion.length - 1].time;
        float currentTime = startTime;

        while (currentTime <= endTime)
        {
            currentTime = Time.time;
            float t = (currentTime - startTime) / (endTime - startTime);

            targetSprite.color = Color.Lerp(normalColour, attackColour, t);

            Vector2 dir = (direction) ? Vector2.right : -Vector2.right;

            rb.velocity = dir * attackMotion.Evaluate(t * attackMotion.keys[attackMotion.length - 1].time);

            yield return null;
        }

        isAttacking = false;

        anim.SetBool("isAttacking", isAttacking);


        //Change it back.
        startTime = Time.time;
        endTime = startTime + attackRecovery;
        currentTime = startTime;

        while (currentTime <= endTime)
        {
            currentTime = Time.time;
            float t = 1 - (currentTime - startTime) / (endTime - startTime);

            targetSprite.color = Color.Lerp(normalColour, attackColour, t);

            yield return null;
        }

        isBusy = false;
    }

    public void AndroidTrigJump()
    {
        jumpFlag = true;
        jumpHoldFlag = true;
    }

    public void AndroidReleaseJump()
    {
        jumpHoldFlag = false;
    }
    
}
