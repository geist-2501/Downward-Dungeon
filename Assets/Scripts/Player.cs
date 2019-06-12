using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using G2;

public class Player : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float movementSpeed = 11f;
    [SerializeField] Vector2 climbSpeed = new Vector2(8f, 8f);
    [SerializeField] float jumpForce = 7f;
    [Range(0.0f, 1.0f)] [SerializeField] float jumpCutHeightMult = 0.5f;
    [Space(5f)]
    [SerializeField] float deathKickback = 5f;
    [SerializeField] Color normalColour;
    [Space(5f)]
    [SerializeField] float jumpVolDivisionFactor = 3f;
    [SerializeField] float jumpMaxPitch = 0.7f;
    [SerializeField] float jumpMinPitch = 0.5f;
    [Space(5f)]
    [SerializeField] Color dashColour;
    [SerializeField] float dashLength;
    [SerializeField] float dashSpeed;
    [SerializeField] float shakeDuration = 0.1f;
    [SerializeField] bool canDash;

    [Space(5f)]

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
    [SerializeField] Collider2D dashCol;
    [SerializeField] ScreenShake screenShakeVCam;
    [SerializeField] PhysicsMaterial2D physicsMat;

    //Data.
    Vector2 playerInput;
    Vector2 playerInputRaw;
    bool jumpFlag;
    bool jumpHoldFlag;
    bool jumpReleaseFlag;
    bool escFlag;
    bool dashFlag;

    bool direction; // -1 left, 1 right.

    float gravScale;    

    InputType inputType;

    // Abilities.
    public bool ablRage;
    public bool ablOverjoy;
    public bool ablTerror;


    float d;  // Delta time.
    float fd; // Fixed delta time.

    //States.
    [HideInInspector] public bool isAlive = true;
    [HideInInspector] public bool isParalysed = false; //isParalysed is a less severe version of disabling isAlive. 
    [HideInInspector] public bool isGrounded = false;
    bool isClimbing = false;
    bool jumpOffClimbingFlag = false;
    bool isPhasing = false;
    bool isBusy = false;
    bool isDashing = false;
    /* 
    The difference between isBusy and isAttacking is that 
    isAttacking is used for the attack window, whereas isBusy
    is used to prevent input during the attack and its recovery 
    */

    //Cached component refs.
    Rigidbody2D rb;
    Animator anim;
    Collider2D bodyCol;
    SpriteRenderer targetSprite;

    //Layers.
    LayerMask groundLayerMask;
    LayerMask climbableLayerMask;
    LayerMask phaseableLayerMask;
    LayerMask hazardLayerMask;


    //Managers.
    AudioManager am;
    GameManager gm;

    //Messages then methods.
    void Start()
    {
        gm = GameManager.instance;
        am = AudioManager.instance;

        inputType = InputManager.GetInputMethod();

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

        ablRage = GameManager.ablRage;
        ablOverjoy = GameManager.ablOverjoy;
        ablTerror = GameManager.ablTerror;

    }


    /// <summary>
    /// Handle all non-physics related stuff.
    /// </summary>
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
            dashFlag = false;
        }

        HandleMisc(); //Quitting and stuff.

    }


    /// <summary>
    /// Handle all physics related stuff.
    /// </summary>
    private void FixedUpdate()
    {
        //Cache fixed delta time.
        fd = Time.fixedDeltaTime;

        if (!isBusy && isAlive)
        {
            DetectClimbing();
            DetectPhasing();

            if (ablRage) { DetectDash(); }

            if (isBusy) { return; }

            Move();
            Jump();
        }


        //Always reset all flags.
        jumpFlag = false;
        jumpReleaseFlag = false;
        escFlag = false;
        dashFlag = false;
    }



    /// <summary>
    /// Gets current input from player.
    /// </summary>
    private void DetectInput()
    {
        //TODO: Support controllers.

        //Combine into control vectors.
        playerInput = InputManager.Movement(inputType);
        playerInputRaw = InputManager.MovementRaw(inputType);

        // //Get flags, and hold them until fixed update receives them.
        jumpFlag = InputManager.JumpButton(Button.Tap) ? true : jumpFlag;
        jumpHoldFlag = InputManager.JumpButton(Button.Hold);
        jumpReleaseFlag = InputManager.JumpButton(Button.Up) ? true : jumpReleaseFlag;

        escFlag = InputManager.EscapeButton(Button.Tap) ? true : escFlag;
        dashFlag = InputManager.DashButton(Button.Tap) ? true : dashFlag;
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
    /// Detects and handles the player attacks.
    /// </summary>
    private void DetectDash()
    {
        if (isGrounded || isClimbing) { canDash = true; }

        if (dashFlag && !isBusy && canDash)
        {
            isBusy = true;
            canDash = false;
            StartCoroutine(DashCoRountine());
        }

        dashFlag = false;
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

        //Restrict dashing to once per 'jump'

        anim.SetBool("isGrounded", isGrounded);

        // These timers give a little give with the responsiveness of the character.
        // I.e. if the player tries to jump a split second too late, they can still jump. It
        // just feels better.

        // Deal with jump timer.
        jumpPressedRemember -= fd;
        if (jumpFlag) { jumpPressedRemember = jumpPressedRememberTime; }

        // Deal with isGrounded timer.
        isGroundedRemember -= fd;
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
        }

        if (isClimbing) { return; } //Stop the following statement changing grav scale when climbing.

        // Let the amount of time the spacebar is held for translate to the airtime. 
        if (jumpReleaseFlag)
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutHeightMult);
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
            playerVel = new Vector2(playerInput.x * climbSpeed.x * fd, playerInputRaw.y * climbSpeed.y * fd);
        }
        else
        {

            playerVel = new Vector2(playerInput.x * movementSpeed * fd, rb.velocity.y);


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


    private IEnumerator DashCoRountine()
    {
        float timeSinceDash = Time.time;

        // Put the dash collider on the corrent side depending on the direction of the dash.
        Vector3 colPos = dashCol.gameObject.transform.localPosition;
        colPos.x *= ((int) Mathf.Sign(colPos.x)) != (direction ? 1 : -1) ? -1 : 1;  
        dashCol.gameObject.transform.localPosition = colPos;

        // Check if the players path is blocked, checked every frame.
        bool blocked = dashCol.IsTouchingLayers(groundLayerMask) || dashCol.IsTouchingLayers(phaseableLayerMask);

        StartCoroutine(ChangeColour(normalColour, dashColour, 0.1f));

        rb.gravityScale = 0f;

        anim.SetBool("isDashing", true);

        // TODO: play sound
        //am.Play("")

        while (Time.time - timeSinceDash < dashLength && !blocked)
        {
            blocked = dashCol.IsTouchingLayers(groundLayerMask) || dashCol.IsTouchingLayers(phaseableLayerMask);
            if (blocked) 
            { 
                am.Play("Bump");
                if (screenShakeVCam != null) { StartCoroutine(screenShakeVCam.Shake(shakeDuration));}  
            }
            rb.velocity = Vector3.right * ((direction) ? 1 : -1) * dashSpeed * fd;
            yield return null;
        }

        rb.gravityScale = gravScale;

        anim.SetBool("isDashing", false);

        StartCoroutine(ChangeColour(dashColour, normalColour, 0.1f));

        isBusy = false;

        yield return null;
    }


    /// <summary>
    /// Changes player's colour.
    /// </summary>
    /// <param name="from">Colour to change from.</param>
    /// <param name="to">Coluor to change to.</param>
    /// <param name="time">The amount of time the transition should take.</param>
    private IEnumerator ChangeColour(Color from, Color to, float time)
    {
        float percentMultipler = 100 / time;
        float timeStart = Time.time;

        float percent = 0f;

        while ((percent = (Time.time - timeStart)) <= time)
        {
            percent *= percentMultipler / 100;
            targetSprite.color = Color.Lerp(from, to, percent);
            yield return null;
        }

        targetSprite.color = to;
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
            if (isDashing && enemy.isAlive)
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


}
