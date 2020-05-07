
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerController : MonoBehaviour
{


    private bool isFacingRight = true;
    private bool isWalking;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canJump;
   

    private int amountOfJumpsLeft;  //amount of Jumps character has left 
    private int facingDirection = 1; // -1 = left, 1 = right

    private float movementInputDirection; //store direction player is trying to move in 

    private Rigidbody2D rb; //a reference to the RigidBody componet
    private Animator anim; // reference to Animator component

    public int amountOfJumps = 1; //amount of jumps character has (allows for double jumping or more)

    public float movementSpeed = 10.0f; //movement speed of character(default)
    public float jumpForce = 16.0f; //jump force of character (default)
    public float groundCheckRadius; //radius of Sphere
    public float wallCheckDistance; //C
    public float wallSlideSpeed; //holds speed of slding down walls 
    public float movementForceInAir; //holds force of Character when trying to move in the air
    public float airDragMultiplier = 0.95f; //holds value for when player is falling in air and not giving input 
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce; 
    public float wallJumpForce;


    /// <summary>
    /// Vector2 (2D) are models that model both a direction and magnitude (x,y)
    /// </summary>
    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck; //a reference to groundCheck gameobject
    public Transform wallCheck;   //a reference to wallCheck  gameobject

    public LayerMask whatIsGround; 

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); //looks for compoent rigidbody2d on game object
        anim = GetComponent<Animator>(); //sets animator to reference
        amountOfJumpsLeft = amountOfJumps; //set amount of jumps player has left to amount of jumps
        wallHopDirection.Normalize(); //sets vectors = 1
        wallJumpDirection.Normalize(); //sets vectors = 1
    }

    /// <summary>
    /// Called every frame
    /// Used for regualr updates such as: receiving input and moving non physics objects
    /// </summary>
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
    }


    /// <summary>
    /// Called when any physics calculations are made
    /// Anything that affects a rigidbody should be called in fixed update
    /// </summary>
    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }


    /// <summary>
    /// Sets isGrounded and isTouchingWall 
    /// </summary>
    private void CheckSurroundings()
    {

        //checks if collider falls within a circular area, 
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround); 


        //casts a ray that is fired from wallcheck gameobject
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

    }

    /// <summary>
    /// Checks if player is inputting directional keys
    /// A = left, D = right, Spacebar = jump
    /// </summary>
    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");  //if player is pressing A or D

        //if spacebar is pressed
        if (Input.GetButton("Jump"))
        {
            Jump();
        }

        //if player releases jump button (allows short hop)
        if (Input.GetButtonUp("Jump")) 
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
    }


    /// <summary>
    /// Updates animations based on conditons
    /// This function is called in Update
    /// </summary>
    private void UpdateAnimations()
    {
       anim.SetBool("isWalking", isWalking); 
       anim.SetBool("isGrounded", isGrounded);
       anim.SetFloat("yVelocity", rb.velocity.y);
       anim.SetBool("isWallSliding", isWallSliding);
    }

    /// <summary>
    /// Sets upward velocity (y direction)
    /// </summary>
    private void Jump()
    {
        //if player can jump and not wallsldiing
        if(canJump && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
        }
        
        //if player is wall sliding and not giving input (A or D) and canJump
        else if(isWallSliding && movementInputDirection == 0 && canJump) //Wall hop
        {
            isWallSliding = false;
            amountOfJumpsLeft--; //decrease amount of jumps player has
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse); //apply force to character (impulse is used for applying forces that happen instantly)
        }

        //if player is wallslding or touching wall && if player is givingInput (a & d) && if player can jump
        else if((isWallSliding || isTouchingWall) && movementInputDirection != 0 && canJump)
        {
            isWallSliding = false; 
            amountOfJumpsLeft--; //decrease amount of jumps player has
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse); //apply force to character (impulse is used for applying forces that happen instantly)
        }

    }


    /// <summary>
    /// Checks if player can jump 
    /// </summary>
    private void CheckIfCanJump()
    {
        //if player is grouned and not in air OR wallsliding
        if((isGrounded && rb.velocity.y <= 0) || isWallSliding )
        {

            amountOfJumpsLeft = amountOfJumps; //set amountOfjumps left
        }

        //if player has no jumps left
        if(amountOfJumpsLeft <= 0)
        {
            canJump = false;
        }
        //if player has jumps left
        else
        {
            canJump = true;
        }    
    }
    /// <summary>
    /// Checks is player is wallsliding
    /// </summary>
    private void CheckIfWallSliding()
    {
        //if player is touching wall AND not grounded AND if player is falling (y direction)
        if(isTouchingWall && !isGrounded && rb.velocity.y < 0)
        {
            isWallSliding = true;

        }
        else
        {
            isWallSliding = false;
        }
    }


    /// <summary>
    /// Checks whether player is moving right/facing right or moving left/facing left
    /// </summary>
    private void CheckMovementDirection()
    {

        //if player is facing right AND moving left 
        if(isFacingRight && movementInputDirection < 0) 
        {
            Flip();
        }
        //if player is not moving right AND moving right
        else if(!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        //if player is moving left or right
        if (rb.velocity.x > 0.01f || rb.velocity.x < -0.0f)
        {
            isWalking = true;
        }

        //if player is not moving
        else
        {
            isWalking = false;
        }


    }

    /// <summary>
    /// sets velocity of characters rigid body
    /// </summary>
    private void ApplyMovement()
    {

        //if player is grounded
        if (isGrounded) 
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y); //move on x axis(left - right)
        }
        //if character is falling in air AND moving in the air
        else if (!isGrounded && !isWallSliding && movementInputDirection != 0) 
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * movementInputDirection, 0);
            rb.AddForce(forceToAdd);

            //if absoulte velocity (x) is greater than movement speed
            if (Mathf.Abs(rb.velocity.x) > movementSpeed) 
            {
                rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y); //clamp x velocity
            }
        }
        //if character is not giving input in air
        else if (!isGrounded && !isWallSliding && movementInputDirection == 0) 
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }

        //if player is wall slding
        if (isWallSliding)
        {
            //if player y is less than  wall slide speed
            if(rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    /// <summary>
    /// Flips player sprite 
    /// </summary>
    private void Flip()
    {
        //if player is not wallslidng
        if(!isWallSliding)
        {
            facingDirection *= -1; //changes facing direction to 1 or -1 everytime character is flipped
            isFacingRight = !isFacingRight; //flip bool
            transform.Rotate(0.0f, 180.0f, 0.0f); //flip player sprite
        }
    
    }

    /// <summary>
    /// Used for visual debugging. 
    /// </summary>
    private void OnDrawGizmos()
    {

        //draw a sphere at the groundcheck (gameobject) position 
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        //draw a line at wallcheck (gameobject) position
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z)); ;
    }

}
