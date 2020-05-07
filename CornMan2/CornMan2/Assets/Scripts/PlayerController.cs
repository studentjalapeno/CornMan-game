using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{


    public float maxSpeed = 5f;
    public float jumpHeight = 6.5f;
    public float gravityScale = 1.5f;
    public Camera mainCamera;

    bool facingRight = true;
    float moveDirection = 0;
    bool isGrounded = false;
    Vector3 cameraPos;
    Rigidbody2D r2d;
    Collider2D mainCollider;

    LayerMask layerMask = ~(1 << 2 | 1 << 8);
    Transform t;


    void Start()
    {
        t = transform;
        r2d = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = t.localScale.x > 0;
        gameObject.layer = 8;

        if (mainCamera)
            cameraPos = mainCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {


        // Movement controls
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            moveDirection = Input.GetKey(KeyCode.A) ? -1 : 1;
        }
        else
        {
            if (isGrounded || r2d.velocity.magnitude < 0.01f)
            {
                moveDirection = 0;
            }
        }

        // Change facing direction
        if (moveDirection != 0)
        {
            if (moveDirection > 0 && !facingRight)
            {
                facingRight = true;
                t.localScale = new Vector3(Mathf.Abs(t.localScale.x), t.localScale.y, transform.localScale.z);
            }
            if (moveDirection < 0 && facingRight)
            {
                facingRight = false;
                t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
            }
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            r2d.velocity = new Vector2(r2d.velocity.x, jumpHeight);
        }

        // Camera follow
        if (mainCamera)
            mainCamera.transform.position = new Vector3(t.position.x, cameraPos.y, cameraPos.z);
    }

    void FixedUpdate()
    {
        Bounds colliderBounds = mainCollider.bounds;
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, 0.1f, 0);
        // Check if player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheckPos, 0.23f, layerMask);

        // Apply movement velocity
        r2d.velocity = new Vector2((moveDirection) * maxSpeed, r2d.velocity.y);

        // Simple debug
        Debug.DrawLine(groundCheckPos, groundCheckPos - new Vector3(0, 0.23f, 0), isGrounded ? Color.green : Color.red);
    }

}