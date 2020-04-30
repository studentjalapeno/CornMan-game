using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{

    private enum State
    {
        Walking, 
        Knockback,
        Dead
    }

    private State currentState;

    [SerializeField]
    private float
        groundCheckDistance,
        wallCheckDistance,
        movementSpeed,
        maxHealth,
        knockbackDuration;

    [SerializeField]
    private Transform
        groundCheck,
        wallCheck;

    [SerializeField]
    private LayerMask whatIsGround;

    [SerializeField]
    private Vector2 knockbackSpeed;

    private float
        currentHealth,
        knockbackStartTime;

    private int
        facingDirection,
        damageDirection;


    private Vector2 movement; //applies to rigid body velocity 
    
    private bool
        groundDetected,
        wallDetected;

    private GameObject alive;
    private Rigidbody2D aliveRb;
    private Animator aliveAnim; //alive animation 

    private void Start()
    {
        alive = transform.Find("Alive").gameObject; //set alive GameObject to "Alive" gameobject in Hierarchy
        aliveRb = alive.GetComponent<Rigidbody2D>(); //set RigidBody from alive Rigidbody component
        aliveAnim = alive.GetComponent<Animator>();

        facingDirection = 1;
    }

    private void Update()
    {
        switch(currentState) //based on what current state is
        {
            case State.Walking:
                UpdateWalkingState();
                break;

            case State.Knockback:
                UpdateKnockbackState();
                break;

            case State.Dead:
                UpdateDeadState();
                break;

        }
    }

    //-WALKING STATE -------------------------------------------------------

    private void EnterWalkingState()
    {

    }

    private void UpdateWalkingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround); //points down
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if(!groundDetected || wallDetected) //if ground detected false or wall detected true
        {
            //Flip
            Flip();
        }
        else
        {
            movement.Set(movementSpeed * facingDirection, aliveRb.velocity.y);
            aliveRb.velocity = movement;
        }
    }

    private void ExitWalkingState()
    {

    }

    //-KNOCKBACK STATE --------------------------------------------

    private void EnterKnockbackState()
    {
        knockbackStartTime = Time.time; //keeps track of when knockback started
        movement.Set(knockbackSpeed.x * damageDirection, knockbackSpeed.y);
        aliveRb.velocity = movement; // set velocity of enemy 
        aliveAnim.SetBool("Knockback", true); //set knockback parameter to true 

    }

    private void UpdateKnockbackState()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration) //if knockback has gone long enough
        {
            SwitchState(State.Walking);
        }
    }

    private void ExitKnockbackState()
    {
        aliveAnim.SetBool("Knockback", false);
    }

    //-DEAD STATE ----------------------------------------------

    private void EnterDeadState()
    {
        //Spawn blood particels
        Destroy(gameObject);
    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    //-OTHER FUNCTIONS --------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="attackDetails">Sends multiple pararmets ( attack damage , x location of person attacking )  </param>
    private void Damage(float[] attackDetails) 
    {
        currentHealth -= attackDetails[0]; //attack damage first index


        //determine which direction damage is coming from in order to determine where you get knocked back
        if(attackDetails[1] > alive.transform.position.x) //if x position of player is greater than enemy ( player facing enemy ) 
        {
            damageDirection = -1;
        }
        else // x position of player is less than enemy
        {
            damageDirection = 1;
        }

        //Hit particle

        if (currentHealth > 0.0f) //if enemy is still alive after hit
        {
            SwitchState(State.Knockback);
        }
        else if(currentHealth <= 0.0f) //enemy is dead
        {
            SwitchState(State.Dead);
        }

    }

    private void Flip()
    {
        facingDirection *= -1;
        alive.transform.Rotate(0.0f, 180.0f, 0.0f); //180 flip
    }


    private void SwitchState(State state)
    {
        switch(currentState)
        {
            case State.Walking:
                ExitWalkingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;

        }

        switch (state)
        {
            case State.Walking:
                EnterWalkingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.Dead:
                EnterDeadState();
                break;

        }

        currentState = state;
    }

    /// <summary>
    /// draws gizmos that are always pickable ( helps with visual debugging )
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance)); //ground check debug
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y)); //wall check debug
    }

}
