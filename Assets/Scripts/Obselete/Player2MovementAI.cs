using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class Player2MovementAI : MonoBehaviour
{
    public float speed = 5.0f;
    public float jumpForce = 2.5f;
    private float horizontalInput;
    private float horizontalMagnitude;
    private float distanceFromEnemy;

    private float speedMultiplier = 1;

    private float lastForwardInput = 0f, timeSinceLastForward;
    private float lastBackwardInput = 0f, timeSinceLastBackward;

    private int horizontalDirection = 5;

    private bool isGrounded = true;
    private bool isCrouching = false;
    private bool isFacingRight = true;
    private bool canMove = true;
    private bool isSprinting = false;

    private Rigidbody playerRb;
    public Animator anim;

    public GameObject enemy;
    public GameObject sprite;

    private Vector3 originalScale;
    private Vector3 flippedScale;
    private Vector3 startPosition = new Vector3(2, 1, 0.0f);
    private Vector3 behind;

    //dashing variables
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 7.5f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 0.5f;

    // Accept Movement Input
    private bool acceptInput = false;

    // Sliding
    private bool sliding = false;

    // AI Inputs
    private bool right = false;
    private bool left = false;
    private bool crouch = false;
    private bool jump = false;


    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        originalScale = sprite.transform.localScale;
        flippedScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDashing)
        {
            return;
        }

        if (!isGrounded)
        {
            canDash = false;
            speed = 4.0f;
        }
        else
        {
            sliding = false;
            speed = 5.0f;
        }

        distanceFromEnemy = transform.position.x - enemy.transform.position.x;

        if (distanceFromEnemy <= 0)
        {
            isFacingRight = true;
        }
        else
        {
            isFacingRight = false;
        }

        if (isFacingRight)
        {
            sprite.transform.localScale = originalScale;
            behind = Vector3.left;
        }
        else
        {
            sprite.transform.localScale = flippedScale;
            behind = Vector3.right;
        }

        // Get Player Horizontal Input
        if (isFacingRight)
        {
            if (!Input.GetKey(KeyCode.RightArrow) && isSprinting)
            {
                isSprinting = false;
                speedMultiplier = 1;
            }
        }
        else
        {
            if (!Input.GetKey(KeyCode.LeftArrow) && isSprinting)
            {
                isSprinting = false;
                speedMultiplier = 1;
            }
        }

        if (Input.GetKey(KeyCode.RightArrow) && acceptInput && !sliding)
        {
            if (distanceFromEnemy <= 7.2)
            {
                if (isFacingRight)
                {
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        timeSinceLastForward = Time.time - lastForwardInput;
                        //Debug.Log("Last Forward input: " + timeSinceLastForward);
                        if (!isSprinting && timeSinceLastForward <= 0.2f)
                        {
                            isSprinting = true;
                            speedMultiplier = 1.5f;
                        }
                        lastForwardInput = Time.time;
                    }
                    horizontalDirection = 6;
                    horizontalInput = 1;
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        timeSinceLastBackward = Time.time - lastBackwardInput;
                        //Debug.Log("Last Backward input: " + timeSinceLastBackward);
                        if (timeSinceLastBackward <= 0.2f && canDash)
                        {
                            StartCoroutine(BackDash());
                        }
                        lastBackwardInput = Time.time;
                    }
                    horizontalDirection = 4;
                    horizontalInput = 0.8f;
                }
            }
            else
            {
                horizontalDirection = 5;
                horizontalInput = 0;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && acceptInput && !sliding)
        {
            if (distanceFromEnemy >= -6.3)
            {
                if (isFacingRight)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        timeSinceLastBackward = Time.time - lastBackwardInput;
                        //Debug.Log("Last Backward input: " + timeSinceLastBackward);
                        if (timeSinceLastBackward <= 0.2f && canDash)
                        {
                            StartCoroutine(BackDash());
                        }
                        lastBackwardInput = Time.time;
                    }
                    horizontalDirection = 4;
                    horizontalInput = -0.8f;
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        timeSinceLastForward = Time.time - lastForwardInput;
                        //Debug.Log("Last Forward input: " + timeSinceLastForward);
                        if (!isSprinting && timeSinceLastForward <= 0.2f)
                        {
                            isSprinting = true;
                            speedMultiplier = 1.5f;
                        }
                        lastForwardInput = Time.time;
                    }
                    horizontalDirection = 6;
                    horizontalInput = -1;
                }
            }
            else
            {
                horizontalInput = 0;
                horizontalDirection = 5;
            }
        }
        else
        {
            horizontalDirection = 5;
            horizontalInput = 0;
        }

        // horizontalInput = Input.GetAxisRaw("Horizontal");


        // Move the Player Horizontally
        if (!isCrouching && canMove)
        {
            horizontalMagnitude = speed * speedMultiplier * horizontalInput;
            transform.Translate(Vector3.right * Time.deltaTime * horizontalMagnitude);
        }
        else
        {
            horizontalMagnitude = 0.0f;
        }

        // Jump Input
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded && !isCrouching && acceptInput && canMove)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            anim.SetTrigger("Jump");
            isGrounded = false;
        }

        // Crouch Input
        if (Input.GetKey(KeyCode.DownArrow) && isGrounded && acceptInput && canMove)
        {
            isCrouching = true;
            canDash = false;
            //Debug.Log("Crouching!");
        }
        else
        {
            isCrouching = false;
            canDash = true;
        }

        // Debugging
        //if (!isCrouching)
        //{
        //Debug.Log("Not Crouching!");
        //}

        // Movement Animations
        if (isFacingRight)
        {
            anim.SetFloat("Movement", horizontalMagnitude);
        }
        else
        {
            anim.SetFloat("Movement", -horizontalMagnitude);
        }
        anim.SetBool("Crouching", isCrouching);
        anim.SetInteger("Direction", horizontalDirection);
        anim.SetBool("Sprinting", isSprinting);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    public void changeMoveState(bool moveable)
    {
        canMove = moveable;
    }

    public void Launch(float launchForceUp, float launchForceBehind)
    {
        playerRb.AddForce(Vector3.Normalize(Vector3.up + launchForceBehind * behind) * launchForceUp, ForceMode.Impulse);
        isGrounded = false;
    }

    public bool ReturnIsGrounded()
    {
        return isGrounded;
    }

    public int ReturnDirection()
    {
        return horizontalDirection;
    }

    public bool ReturnIsCrouching()
    {
        return isCrouching;
    }

    public void SetDirection(int newDirection)
    {
        horizontalDirection = newDirection;
    }

    public void ResetPosition()
    {
        //afterStart = false;
        //startTimer = 0.0f;
        playerRb.position = startPosition;
    }
    public void AcceptInput()
    {
        acceptInput = true;
    }

    public void DenyInput()
    {
        acceptInput = false;
    }

    public void LungeMovement()
    {
        playerRb.velocity = new Vector2(-behind.x * dashingPower, 0.0f);
    }


    private IEnumerator BackDash()
    {
        canDash = false;
        isDashing = true;
        anim.SetTrigger("Backdash");
        if (isFacingRight)
        {
            playerRb.velocity = new Vector2(-1f * dashingPower, 0f);
        }
        else
        {
            playerRb.velocity = new Vector2(1f * dashingPower, 0f);
        }
        yield return new WaitForSeconds(dashingTime);
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.tag == "PlayerHitBox")
        {
            Rigidbody collisionRb = collision.gameObject.transform.parent.GetComponent<Rigidbody>();
            Debug.Log("Standing on Head!~");
            sliding = true;
            if (isFacingRight)
            {
                playerRb.velocity = new Vector2(-3, playerRb.velocity.y);
                collisionRb.velocity = new Vector2(3, collisionRb.velocity.y);
            }
            else
            {
                playerRb.velocity = new Vector2(3, playerRb.velocity.y);
                collisionRb.velocity = new Vector2(-3, collisionRb.velocity.y);
            }
        }
    }
}