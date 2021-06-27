using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* PlayerBehavior

Features: Has Five Gears of Movement Speed.

Gear 1: Neutral Speed
    - Not Dashing 
Gear 2: Warming Up.
    - Prepping the Dash
Gear 3: Speeding up
    - Dashing.  Enemies in the way may take light damage.  Walls can be climbed.
Gear 4: Max Speed
    - Super Dash.  Enemies can't defend against the super dash.
Gear 5: Ludicrous Speed
    - Faster than Gear 4.  Very fast, but REALLY hard to control.
    - AAAAHHHHHHHHHHHHH!!!!!

*/
public class PlayerBehavior : MonoBehaviour
{

    public float walkSpeed = 3.0f;
    

    public float fallMultiplier = 1.05f;

    public float lowJumpMultiplier = 1.1f;

    public float crouchMultiplier = 0.75f;

    [Header("Jumping")]
    public float jumpForce = 6.0f;

    public int defaultAdditionalJumps = 1;
    int additionalJumps = 1;


    [Header("Checking for Grounding")]
    public bool isGrounded = true;
    public Transform isGroundedChecker;
    public float checkGroundRadius;
    public LayerMask groundLayer;

    public float rememberGroundedFor;

    float lastTimeGrounded;

    
    Rigidbody2D rb;

    private PlayerActionControls playerActionControls;
    public float moveInput, jumpInput, lookInput, dashInput;


    [Header("Dashing Gear System")]

    public bool dashing = false;
    public bool turning = false;
    public int gear = 1;
    public float gear2mult = 1.00f;
    public float gear3mult = 1.75f;
    public float gear4mult = 2.30f;
    public float gear5mult = 3.00f;

    public float gear2tran=1200f;
    public float gear3tran=2800f;
    public float gear4tran=4000f;

    public double gearTranTime = 0.0;
    public double lastRecordedTime = 0.0;

    [Header("Flipping")]
    public bool left = false;

    SpriteRenderer sr;

    void Awake()
    {
        playerActionControls = new PlayerActionControls();
    }

    private void OnEnable()
    {
        playerActionControls.Enable();
    }

    private void OnDisable()
    {
        playerActionControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        moveInput = 0;
        jumpInput = 0;
        lookInput = 0;
        dashInput = 0;
        gearTranTime = lastRecordedTime = 0.0f;

        dashing = turning = false;

        rb = GetComponent<Rigidbody2D>();

        sr = transform.GetChild(1).GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForInput();

        CheckIfGrounded();

        Move();

        Jump();

        Dash();

        GearSpeed();
    }

    void CheckForInput()
    {
        moveInput = playerActionControls.Land.Run.ReadValue<float>();

        jumpInput = playerActionControls.Land.Jump.ReadValue<float>();

        lookInput = playerActionControls.Land.Look.ReadValue<float>();

        dashInput = playerActionControls.Land.Dash.ReadValue<float>();
    }

    void Move()
    {
        //If we're dashing, Dash() will take care of the "Move" key information.
        if(dashing == true)
        {
            return;
        }

        rb.velocity = new Vector2(moveInput*walkSpeed, rb.velocity.y);

        if(moveInput < 0)
        {
            left = true;
        }
        else if(moveInput > 0)
        {
            left = false;
        }


    }

    void Jump()
    {

        if (jumpInput > 0 && isGrounded) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            additionalJumps--;
            isGrounded = false;
        }
    }

    void Jump2()
    {
        if (rb.velocity.y < 0) {
            rb.velocity += Vector2.up * Physics2D.gravity * (fallMultiplier - 1) * Time.deltaTime;
        } else if (rb.velocity.y > 0 && jumpInput <= 0) {
            rb.velocity += Vector2.up * Physics2D.gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
        }  
    }

    void CheckIfGrounded()
    {
        Collider2D colliders = Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, groundLayer);

        if (colliders != null) {
            isGrounded = true;
            additionalJumps = defaultAdditionalJumps;
        } else {
            if (isGrounded) {
                lastTimeGrounded = Time.time;
            }
            isGrounded = false;
        }

        
    }

    /*
        Dash()
        This method is the cornerstone behind Pizza Tower's Design.
        When held down, the Player moves, even when they aren't holding a direction on the arrow keys.
        The player begins to move forward and accumulates speed.
        
        They accelerate, and this is reflected as different "Gears" of speed.
        After accelerating enough, the player begins to change to different Gears.

        GEAR LIST:
        Gear 1: Neutral.  Not Dashing.
        Gear 2: Startup
        Gear 3: Running Speed
        Gear 4: Max Speed
        Gear 5: Ludicrous Speed




    */
    void Dash()
    {
        if(isGrounded)
        {
            if(dashInput != 0)
            {
                dashing = true;
                gear = 2;
                double t = Time.time;

                lastRecordedTime = t - lastRecordedTime;
                gearTranTime += lastRecordedTime;
                //Now, we need to figure out what gear we are going at.

                if(gear == 2 && gearTranTime > gear2tran)
                {
                    gear = 3;
                }
                if(gear == 3 && gearTranTime > gear3tran)
                {
                    gear = 4;
                }

            }
            else 
            {
                dashing = false;
                gear = 1;
                gearTranTime = 0;
            }



        }
        else
        {
            //We can't start dashing if we're airborne.  It doesn't work that way.
            if(!dashing)
            {
                return;
            }


        }


    }

    void GearSpeed()
    {
        if(gear <= 1)
        {
            return;
        }

        int direction = left ? -1 : 1;

        if (gear == 2)
        {
            rb.velocity = new Vector2(direction * gear2mult * walkSpeed, rb.velocity.y);
        }
        else if (gear == 3)
        {
            rb.velocity = new Vector2(direction * gear3mult * walkSpeed, rb.velocity.y);
        }
        else if(gear == 4)
        {
            rb.velocity = new Vector2(direction * gear4mult * walkSpeed, rb.velocity.y);
        }
        else if(gear == 5)
        {
            rb.velocity = new Vector2(direction * gear5mult * walkSpeed, rb.velocity.y);
        }
    }



    void Turn(bool val)
    {
        left = val;

        sr.flipX = left;

    }
}
